using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Backend;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(LoadXML))]
public class NarrationManager : MonoBehaviour {

	private LoadXML lxml;
	private GameObject fNode;
	private UnityAction<string> listener;

	private IEnumerator current_narration;
	private bool user_can_take_turn = true;

	public static bool progressNarrationSwitch = false;
	public static bool firstPassNarration = true;

	private static bool first_flag = true;//set to false after first node has been expanded

	void Awake() {
		progressNarrationSwitch = false;
		listener = delegate (string data){
			if (user_can_take_turn) {
				user_can_take_turn = false;
				int node_id = int.Parse(data);
				//lxml.idMap[node_id].Focus();
				Narrate(node_id, 9);
			}
		};

		lxml = GetComponent<LoadXML>();
	}

	void Start() {
		OSCHandler.Instance.Init(); //init OSC
		lxml.Initialize();
		Reset_Narration();
		EventManager.StartListening(EventManager.EventType.INTERFACE_NODE_SELECT, listener);
		listener("13");

	}

	public void Update() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			if (!user_can_take_turn) {
				progressNarration();
			}
			
		}
	}

	public void Reset_Narration() {
		//resets narration history
		StartCoroutine(_Reset_Narration());
	}

	//Call this to progress the story turn
	public void progressNarration() {
		progressNarrationSwitch = true;
		Debug.Log("progressing narration from event manager");
	}

	IEnumerator _Reset_Narration() {
		string url = "http://" + AppConfig.Settings.Backend.ip_address + ":" + AppConfig.Settings.Backend.port + "/chronology/reset";
		WWW www = new WWW(url);
		yield return www;
		if (www.error == null) {
			print("NARRATION RESET");
		}		
	}

	public void Narrate(int node_id, int turns) {
		//narrate about node for N turns
		print("Narrate attempt");
		if (current_narration != null) {
			StopCoroutine(current_narration);
		}
		current_narration = _Narrate(node_id, turns);
		StartCoroutine(current_narration);
	}
	
	

	//Present the given node given the previous nodes presented
	void Present(GameObject node_to_present, List<GameObject> node_history) {
		print("Current node: " + node_to_present.GetComponent<timelineNode>().text);
		//Bring this node into focus
		node_to_present.GetComponent<timelineNode>().Focus();

		/*//Some nodes may be layered on top of each other. Displace this node in the y if any other
		//nodes in the history share a position with it.
		bool layered = true;
		while (layered)
		{
			layered = false;
			//Check to see if the node we wish to present is layered on any node in the history.
			foreach (GameObject past_node in node_history)
			{
				if (past_node.transform.position.Equals(node_to_present.transform.position))
				{
					layered = true;
				}//end if
			}//end foreach

			//If it is layered, displace it in the y
			if (layered)
				node_to_present.transform.position = new Vector3(node_to_present.transform.position.x
					, node_to_present.transform.position.y + 1
					, node_to_present.transform.position.z);
		}//end while*/

	}//end method Present

	//Narrate a sequence of nodes
	IEnumerator _Narrate(int node_id, int turns) {
		print("===== NEW NARRATION =====");

		//Ask the backend for a node sequence
		string url = "http://" + AppConfig.Settings.Backend.ip_address + ":" + AppConfig.Settings.Backend.port + "/chronology";
		string data = JsonUtility.ToJson(new ChronologyRequest(node_id, turns));

		Debug.Log("request: " + data);
		WWW www = new WWW(url, Encoding.UTF8.GetBytes(data));
		yield return www;

		// check for errors
		if (www.error == null) {
			Debug.Log("WWW Ok!: " + www.text);
		}
		else {
			Debug.Log("WWW Error: " + www.error);
			yield break;
		}

		ChronologyResponse response = JsonUtility.FromJson<ChronologyResponse>(www.text);

		//The nodes themselves
		List<KeyValuePair<GameObject, string>> sequence_by_node = new List<KeyValuePair<GameObject, string>>();
		List<List<StoryAct>> sequence_acts = new List<List<StoryAct>>();
		timelineNode temp_node = null;

		foreach (StoryNode sn in response.StorySequence) {
			int id = sn.graph_node_id;
			temp_node = null;
			lxml.idMap.TryGetValue(id, out temp_node);
			sequence_by_node.Add(new KeyValuePair<GameObject, string>(temp_node.gameObject,sn.text));
			sequence_acts.Add(sn.story_acts);
		}//end foreach

		//Bring each node out of focus.
		foreach (timelineNode tn in lxml.nodeList) {
			//Change its color
			//temp.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
			//Set it to not display information on mouseover
			//temp.GetComponent<timelineNode>().display_info = false;
			if(tn.node_id != node_id) {
				tn.Unfocus();
			}
			
		}//end foreach

		bool tmp_flag = true;
		List<GameObject> node_history = new List<GameObject>();
		for(int ix=0; ix<sequence_by_node.Count; ix++) {
		//foreach (KeyValuePair<GameObject,string> kvp in sequence_by_node) {
			if (first_flag) {//wait for keypress before presenting first node
				first_flag = false;
				yield return StartCoroutine(WaitForKeyDown());
			}else if(!tmp_flag){//dont wait for keypress on first node
				yield return StartCoroutine(WaitForKeyDown());
			}
			KeyValuePair<GameObject, string> kvp = sequence_by_node[ix];
			GameObject node_to_present = kvp.Key;
			//Bring the previous node into past-focus
			if (node_history.Count >= 1) {
				node_history[node_history.Count - 1].GetComponent<timelineNode>().PastFocus();
			}
			//Present this node
			fNode = node_to_present;
			Present(node_to_present, node_history);
			EventManager.TriggerEvent(EventManager.EventType.NARRATION_MACHINE_TURN,kvp.Value);

			//trigger events for all current story acts

			foreach (StoryAct sa in sequence_acts[ix]) {
				switch (sa.Item1) {
					case "lead-in":
						EventManager.TriggerEvent(EventManager.EventType.NARRATION_LEAD_IN, sa.Item2.ToString());
						break;
					case "tie-back":
						EventManager.TriggerEvent(EventManager.EventType.NARRATION_TIE_BACK, sa.Item2.ToString());
						break;
					case "relationship":
						EventManager.TriggerEvent(EventManager.EventType.NARRATION_RELATIONSHIP, sa.Item2.ToString());
						break;
					case "novel-lead-in":
						EventManager.TriggerEvent(EventManager.EventType.NARRATION_NOVEL_LEAD_IN, sa.Item2.ToString());
						break;
					case "location-change":
						EventManager.TriggerEvent(EventManager.EventType.NARRATION_LOCATION_CHANGE, sa.Item2.ToString());
						break;
					case "hint-at":
						EventManager.TriggerEvent(EventManager.EventType.NARRATION_HINT_AT, sa.Item2.ToString());
						break;
					case "analogy":
						EventManager.TriggerEvent(EventManager.EventType.NARRATION_ANALOGY, sa.Item2.ToString());
						break;
					case "user-turn":
						EventManager.TriggerEvent(EventManager.EventType.NARRATION_USER_TURN, sa.Item2.ToString());
						break;
				}
			}

			//Add it to the history
			node_history.Add(node_to_present);
			
			progressNarrationSwitch = false;
			Color tmp = new Color(0,1,1,.05f);
			fNode.GetComponent<LineRenderer>().SetColors(tmp, tmp);
			tmp_flag = false;
		}//end foreach
		user_can_take_turn = true;
		print("STORY ARC COMPLETE");
		//EventManager.TriggerEvent(EventManager.EventType.NARRATION_USER_TURN);
	}

	//Wait asynchronously for a key press
	IEnumerator WaitForKeyDown() {
		do {
			yield return null;
		} while (!progressNarrationSwitch);

	}
}
