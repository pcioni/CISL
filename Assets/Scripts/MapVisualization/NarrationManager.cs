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
    private UnityAction<string> narrationListener = null;

    private IEnumerator current_narration;
	private bool user_can_take_turn = true;

    public static bool progressNarrationSwitch = false;
    public static bool firstPassNarration = true;

    void Awake() {
        progressNarrationSwitch = false;
        listener = delegate (string data){
			if (user_can_take_turn) {
				user_can_take_turn = false;
				int node_id = int.Parse(data);
				lxml.idMap[node_id].Focus();
				Narrate(node_id, 5);
			}
		};

        narrationListener = delegate (string data) {
            progressNarration();
        };

        lxml = GetComponent<LoadXML>();
	}

	void Start() {
		OSCHandler.Instance.Init(); //init OSC
		lxml.Initialize();
		listener("13");
	    narrationListener("progNarration");
		Reset_Narration();
        EventManager.StartListening(EventManager.EventType.INTERFACE_NODE_SELECT, listener);
        EventManager.StartListening(EventManager.EventType.INTERFACE_NODE_SELECT, narrationListener);
    }

    public void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            EventManager.TriggerEvent(EventManager.EventType.INTERFACE_NODE_SELECT, "progNarration");
        }
    }

	public void Reset_Narration() {
		//resets narration history
		StartCoroutine(_Reset_Narration());
	}

    //Call this to progress the story turn
    public static void progressNarration(bool firstPass = false) {
        //Assigning the delegate calls the function, so dont assign the listener until we do the first expansion.
        if (!firstPassNarration) {
            progressNarrationSwitch = true;
        }
        else {
            firstPassNarration = false;
        }
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
		StartCoroutine(_Narrate(node_id, turns));
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
		//The sequence of nodes we want to narrate, by name
		List<string> sequence_by_name = new List<string>();

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
		timelineNode temp_node = null;
		//foreach (string name in sequence_by_name)

		foreach (StoryNode sn in response.StorySequence) {
			int id = sn.graph_node_id;
			temp_node = null;
			lxml.idMap.TryGetValue(id, out temp_node);
			sequence_by_node.Add(new KeyValuePair<GameObject, string>(temp_node.gameObject,sn.text));
		}//end foreach

		//Bring each node out of focus.
		foreach (timelineNode tn in lxml.nodeList) {
			//Change its color
			//temp.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
			//Set it to not display information on mouseover
			//temp.GetComponent<timelineNode>().display_info = false;
			tn.Unfocus();
		}//end foreach

		List<GameObject> node_history = new List<GameObject>();
		foreach (KeyValuePair<GameObject,string> kvp in sequence_by_node) {
			GameObject node_to_present = kvp.Key;
			//Bring the previous node into past-focus
			if (node_history.Count >= 1)
				node_history[node_history.Count - 1].GetComponent<timelineNode>().PastFocus();
			//Present this node
			fNode = node_to_present;
			Present(node_to_present, node_history);
			EventManager.TriggerEvent(EventManager.EventType.NARRATION_MACHINE_TURN,kvp.Value);
			//Add it to the history
			node_history.Add(node_to_present);
			//Wait for spacebar before presenting the next.

			yield return StartCoroutine(WaitForKeyDown());
		    progressNarrationSwitch = false;
			Color tmp = new Color(0,1,1,.05f);
			fNode.GetComponent<LineRenderer>().SetColors(tmp, tmp);
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
