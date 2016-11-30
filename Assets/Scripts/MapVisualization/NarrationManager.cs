using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using JsonConstructs;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(LoadXML))]
public class NarrationManager : MonoBehaviour {

	private LoadXML lxml;
    private UnityAction<string> narrationNodeSelectListener;
    private UnityAction<string> labelCollisionCheckListener;

    public List<Vector3> pastNarrationNodeTransforms = new List<Vector3>();
	//TODO: Part of hack for demo. Remove later?
	public List<timelineNode> pastNarrationNodes = new List<timelineNode>();

    private IEnumerator current_narration;
	private bool user_can_take_turn = true;
    private bool narration_reset = false;
    private bool loaded_xml_backend = false;

    public static bool progressNarrationSwitch = false;
	public static bool firstPassNarration = true;

	private static bool first_flag = true;//set to false after first node has been expanded

    public TimeLineBar tlb;

    void Awake() {
        tlb = GameObject.Find("TimeLine").GetComponent<TimeLineBar>();
        progressNarrationSwitch = false;
		narrationNodeSelectListener = delegate (string data){
			if (user_can_take_turn) {

				NodeData nd = JsonUtility.FromJson<NodeData>(data);
				user_can_take_turn = false;
				Narrate(nd.id, 9);
			}
		};

        labelCollisionCheckListener = delegate (string data) {
            labelCollisionCheckCount = 0;
        };

        lxml = GetComponent<LoadXML>();
	}

	IEnumerator Start() {
        DebugMode.startTimer("NarrationManager.Start()");

        lxml.Initialize();

        print("NarrationManager.Reset_Narration() :: started");

        Reset_Narration();
        while(!narration_reset) {
            yield return null;
        }
        narration_reset = false;

        print("NarrationManager.Reset_Narration() :: ended");
        print("NarrationManager.Load_XML() :: started");

        Load_XML();
        while (!loaded_xml_backend)
        {
            yield return null;
        }
        loaded_xml_backend = false;

        print("NarrationManager.Load_XML() :: ended");

        EventManager.StartListening(EventManager.EventType.INTERFACE_NODE_SELECT, narrationNodeSelectListener);
        EventManager.StartListening(EventManager.EventType.LABEL_COLLISION_CHECK, labelCollisionCheckListener);
        EventManager.StartListening(EventManager.EventType.INTERFACE_ZOOM_OUT, labelCollisionCheckListener);
        EventManager.StartListening(EventManager.EventType.INTERFACE_ZOOM_IN, labelCollisionCheckListener);

		//start on diocletian
		user_can_take_turn = false;

        //Bring each node out of focus.
        foreach (timelineNode tn in lxml.nodeList)
        {
            //Change its color
            //temp.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
            //Set it to not display information on mouseover
            //temp.GetComponent<timelineNode>().display_info = false;
            tn.Unfocus();
        }//end foreach

        // TODO: make this selection UI-driven
        // for now: comment out all lines exept the one you'd like to have execute
        Scene scene = SceneManager.GetActiveScene();
        Debug.Log("NarrationManager.Start() :: Active scene is '" + scene.name + "'.");
        switch (scene.name)
        {
            case "timelineTest4_RomanEmpire":
                Narrate(13, 9); // "Roman Empire: Diocletian (node 13)" narration start
                break;
            case "timelineTest4_WWII":
                Narrate(17, 9); // "WWII: American Theater (node 17)" narration start
                //Narrate(495, 9); // "WWII: Linden Cameron (node 495)" narration start
                break;
            case "timelineTest4_Roman_WWII_Analogy":
                //Narrate(1, 2); // "Roman_WWII_Analogy: WW II (node 1)" narration start
                //Narrate(44, 102); // "Roman_WWII_Analogy: Battle of Actium (node 44) :: Mediterranean and Middle East theatre of World War II (node 102)" narration start
                //Narrate(129, 33); // "Roman_WWII_Analogy: Adolf Hitler (node 129) :: Augustus (node 33)" narration start
                Narrate(1, 216); // "Roman_WWII_Analogy: WW II (node 1) :: Charles Crombie (node 216)" narration start
                break;
            default:
                Debug.Log("NarrationManager.Start() :: unhandled scene name");
                break;
        }

        DebugMode.stopTimer("NarrationManager.Start()");

    }

    public void Update() {
		if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {
			if (!user_can_take_turn) {
				progressNarration();
			}
			
		}

        // handle Data Loading keypresses
        bool shiftDown = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        // SHIFT + D + L
        bool loadDataSet = (shiftDown && Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.L));
        // map keys 1, 2, and 3 to level indexes 0, 1, and 2
        int levelID = Input.GetKey(KeyCode.Alpha1) ? 0 : Input.GetKey(KeyCode.Alpha2) ? 1 : Input.GetKey(KeyCode.Alpha3) ? 2 : -1;
        if (loadDataSet && levelID >= 0)
        {
            //TODO: could this possibly be spammed? Should we figure out a way to only do this once every x seconds?
            first_flag = true;
            SceneManager.LoadScene(levelID);
        }

        labelCollisionCheck();
    }

    private int labelCollisionCheckCount = 0;
    public int labelCollisionCheckMax = 3;
    void labelCollisionCheck ()
    {
        if (labelCollisionCheckCount < 0) return;

        if (labelCollisionCheckCount < labelCollisionCheckMax)
        {
            CameraController.CollisionDetection();
            labelCollisionCheckCount += 1;
        }
        else if (labelCollisionCheckCount < labelCollisionCheckMax * 2)
        {
            labelCollisionCheckCount += 1;
        }
        else
        {
            CameraController.CollisionDetection();
            labelCollisionCheckCount = labelCollisionCheckMax;
        }
    }


    public void Reset_Narration() {
		//resets narration history
		StartCoroutine(_Reset_Narration());
	}

    public void Load_XML()
    {
        //loads a new file
        StartCoroutine(_Load_XML());
    }

    //Call this to progress the story turn
    public void progressNarration() {
		progressNarrationSwitch = true;
		Debug.Log("progressing narration from event manager");
	}

    IEnumerator _Load_XML (string url = "")
    {
        if (url == "")
        {
            //url = "http://" + AppConfig.Settings.Backend.ip_address + ":" + AppConfig.Settings.Backend.port + "/load_xml:" + lxml.xml_location;
            url = "http://" + AppConfig.Settings.Backend.ip_address + ":" + AppConfig.Settings.Backend.port + "/load_xml";
        }


        string data = JsonUtility.ToJson(new LoadXMLRequest(lxml.xml_location));

        WWW www = new WWW(url, Encoding.UTF8.GetBytes(data));

        print("NarrationManager._Load_XML() :: url = " + url);
        print("                               data = " + data);

        yield return new WaitForSeconds(5);
		print("NarrationManager._Load_XML(), done waiting for backend.");
        loaded_xml_backend = true;

		//Now that the XML is done loading in the backend, categorize nodes.
		lxml.CategorizeNodes();
		
        //yield return www;
        //if (www.error == null)
        //{
        //    print("LOADED NEW XML IN BACKEND");
        //    print(url);
        //    loaded_xml_backend = true;
        //}
    }

    IEnumerator _Reset_Narration() {
        string url = "http://" + AppConfig.Settings.Backend.ip_address + ":" + AppConfig.Settings.Backend.port + "/chronology/reset";

        print("NarrationManager._Reset_Narration() :: url =  " + url);

        WWW www = new WWW(url);
		yield return www;
		if (www.error == null) {
			print("NARRATION RESET");
            narration_reset = true;
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
				node_to_present.pastStoryNodeTransform = node_history[node_history.Count - 1].transform;
			}
			
			//Present this node
			Present(node_to_present, node_history);

			//TODO: Hack for timeline scaling during demo, remove later!
			if (node_to_present.node_name.Equals("Battle of Actium"))
			{
                float moveTime = 1.5f;
                ///*
                tlb.setData(0, 722700, 1, moveTime);
                //*/

                //TODO: create a function in TimeLineBar to animate these
                /*
				TimeLineBar.minDays = 0;
				TimeLineBar.maxDays = 722700;
				TimeLineBar.zoomDivisor = 1;

                TimeLineBar.minDaysTarget = 0;
                TimeLineBar.maxDaysTarget = 722700;
                TimeLineBar.zoomDivisorTarget = 1;

                //*/
                //Reset timeline positions of all timeline nodes and redo all past narration lines.
                this.pastNarrationNodeTransforms = new List<Vector3>();
				foreach (timelineNode tn in lxml.nodeList)
				{
					tn.pastNarrationLineRenderer.SetVertexCount(0);
					tn.reset_timeline_position(moveTime);
					//Reset past narration node positions
					//pastNarrationNodeTransforms = new List<Vector3>();
					//foreach (timelineNode tn2 in pastNarrationNodes)
					//{
					//	pastNarrationNodeTransforms.Add(tn2.transform.position);
					//}//end foreach
					
					//if (tn.state == timelineNode.focusState.PAST)
					//	tn.drawLines();
				}//end foreach
			}//end if


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

            //TODO: debug why this isn't getting the desired label placement
            // (zooming collision detection / placement seems more desired, although buggy)
            //Call camera collision detection method
            EventManager.TriggerEvent(EventManager.EventType.LABEL_COLLISION_CHECK, "");

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
