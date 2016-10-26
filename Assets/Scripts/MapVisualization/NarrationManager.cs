using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using JsonConstructs;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(LoadXML))]
public class NarrationManager : MonoBehaviour {

	private LoadXML lxml;
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

				NodeData nd = JsonUtility.FromJson<NodeData>(data);
				user_can_take_turn = false;
				Narrate(nd.id, 9);
			}
		};

		lxml = GetComponent<LoadXML>();
	}

	void Start() {
		lxml.Initialize();
		Reset_Narration();
		EventManager.StartListening(EventManager.EventType.INTERFACE_NODE_SELECT, listener);

		//start on diocletian
		user_can_take_turn = false;
		Narrate(13, 9);

	}

	public void Update() {
		if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {
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
	void Present(timelineNode node_to_present, List<timelineNode> node_history) {
		print("Current node: " + node_to_present.text);
		//Bring this node into focus
		node_to_present.Focus();

        // TODO: move map camera whenever new node is presented -- data type below isn't working
        // EventManager.TriggerEvent(EventManager.EventType.NARRATION_LOCATION_CHANGE, node_to_present.GetComponent<timelineNode>().node_id.ToString());

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

		//string url = "http://" + AppConfig.Settings.Backend.ip_address + ":" + AppConfig.Settings.Backend.port + "/test";


		//Debug.Log("request: " + data);

		WWW www = new WWW(url, Encoding.UTF8.GetBytes(data));
		//WWW www = new WWW(url);
		yield return www;

		// check for errors
		if (www.error == null) {
			Debug.Log("WWW Ok!: " + www.text);
		}
		else {
			Debug.Log("WWW Error: " + www.error);
			yield break;
		}

		//ChronologyResponse response = JsonUtility.FromJson<ChronologyResponse>(www.text);

		TestSequence response = JsonUtility.FromJson<TestSequence>(www.text);

		//The nodes themselves
		List<KeyValuePair<timelineNode, string>> sequence_by_node = new List<KeyValuePair<timelineNode, string>>();
		List<List<StoryAct>> sequence_acts = new List<List<StoryAct>>();
		timelineNode temp_node = null;

		//foreach (StoryNode sn in response.Sequence) {
		foreach (StoryNode sn in response.StorySequence[0].Sequence) {
			int id = sn.graph_node_id;
			temp_node = null;
			lxml.idMap.TryGetValue(id, out temp_node);

			//NOTE: need to use storynode text here for annotations
			sequence_by_node.Add(new KeyValuePair<timelineNode, string>(temp_node,sn.text));
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
		List<timelineNode> node_history = new List<timelineNode>();
		for(int ix=0; ix<sequence_by_node.Count; ix++) {
		//foreach (KeyValuePair<GameObject,string> kvp in sequence_by_node) {
			if (first_flag) {//wait for keypress before presenting first node
				first_flag = false;
				yield return StartCoroutine(WaitForKeyDown());
			}else if(!tmp_flag){//dont wait for keypress on first node
				yield return StartCoroutine(WaitForKeyDown());
			}
			KeyValuePair<timelineNode, string> kvp = sequence_by_node[ix];
			timelineNode node_to_present = kvp.Key;
			//Bring the previous node into past-focus
			if (node_history.Count >= 1) {
				node_history[node_history.Count - 1].PastFocus();
				node_to_present.pastStoryNodeTransform = node_history[node_history.Count - 1].transform;
			}
			//Present this node
			Present(node_to_present, node_history);

			NodeData dataObj = new NodeData(node_to_present.node_id, kvp.Value, node_to_present.pic_urls, node_to_present.pic_labels);

			string json = JsonUtility.ToJson(dataObj);

			EventManager.TriggerEvent(EventManager.EventType.NARRATION_MACHINE_TURN, json);

			//always trigger a location change
			EventManager.TriggerEvent(EventManager.EventType.NARRATION_LOCATION_CHANGE, json);

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
						//EventManager.TriggerEvent(EventManager.EventType.NARRATION_LOCATION_CHANGE, sa.Item2.ToString());
						break;
					case "hint-at":
						EventManager.TriggerEvent(EventManager.EventType.NARRATION_HINT_AT, sa.Item2.ToString());
						break;
					case "analogy":
						EventManager.TriggerEvent(EventManager.EventType.NARRATION_ANALOGY, sa.Item2.ToString());
						break;
					case "user-turn":
					case "switch-point":
						EventManager.TriggerEvent(EventManager.EventType.NARRATION_USER_TURN, sa.Item2.ToString());
						break;
				}
			}

			//Add it to the history
			node_history.Add(node_to_present);
			
			progressNarrationSwitch = false;
			tmp_flag = false;

			//Call camera collision detection method
			CameraController.CollisionDetection();
		}//end foreach
		user_can_take_turn = true;


		//need to manually yield a turn at end because backend bugged
		EventManager.TriggerEvent(EventManager.EventType.NARRATION_USER_TURN, node_id.ToString());




		print("STORY ARC COMPLETE");
	}

	//Wait asynchronously for a key press
	IEnumerator WaitForKeyDown() {
		do {
			yield return null;
		} while (!progressNarrationSwitch);

	}
}
