using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System;
using System.Text;

public class LoadXML : MonoBehaviour {

	public string xml_location = "Assets/xml/roman_empire_1000.xml";
	public Dictionary<string, GameObject> nodeDict = new Dictionary<string, GameObject>();
	public Dictionary<int, timelineNode> idMap = new Dictionary<int, timelineNode>();
	public Sprite node_sprite;
	public List<timelineNode> nodeList = new List<timelineNode>();
	private float nodeSpriteWidth;
	private float cameraWidth;
	private float nodeDistanceIncrement;

	public void Start() {
		Initialize();
	}

	// Use this for initialization
	public void Initialize() {
		XmlSerializer serializer = new XmlSerializer(typeof(AIMind));
		FileStream stream = new FileStream(xml_location, FileMode.Open);
		AIMind container = (AIMind)serializer.Deserialize(stream);
		stream.Close();

		nodeSpriteWidth = node_sprite.bounds.size.x;
		cameraWidth = 2f * Camera.main.orthographicSize * Camera.main.aspect;

		foreach (Feature f in container.features) {
			GameObject tmp_obj = new GameObject(f.data);
			timelineNode tn = tmp_obj.AddComponent<timelineNode>();
			tmp_obj.AddComponent<SpriteRenderer>().sprite = node_sprite;
			tmp_obj.AddComponent<RectTransform>();
			tmp_obj.AddComponent<CircleCollider2D>();
			tmp_obj.AddComponent<LineRenderer>();

			if (f.speak != null) {
				tn.text = f.speak;
			}

			string tmpdate = "1";
			try {
				foreach (Timeobj to in f.timedata) {
					if (to.relationship == "birth date" || to.relationship == "years" || to.relationship == "date") {
						tmpdate = to.value;
						break;
					}
				}
			}
			catch {

			}
			try {
				//try converting to datetime
				tn.date = Convert.ToDateTime(f.timedata[0].value);
			}
			catch {
				//else extract digits
				int year;
				if (!int.TryParse(System.Text.RegularExpressions.Regex.Match(tmpdate, @"\d+").Value, out year)) {
					tn.date = new DateTime();
				}
				else {
					if(year != 0) {
						tn.date = new DateTime(year, 1, 1);
					}else {
						tn.date = new DateTime();
					}
					
				}

			}
			try {
				//check if node has geodata associated with it
				tn.location = new Vector2(f.geodata[0].lat, f.geodata[0].lon);
				tn.known_location = true;
			}
			catch {
				//otherwise set location to unknown
				tn.location = new Vector2();
				tn.known_location = false;
			}
			tn.datevalue = tn.date.ToShortDateString();
			tn.dateticks = tn.date.Ticks;
			nodeList.Add(tn);
			nodeDict[f.data] = tmp_obj;

			idMap[f.id] = tn;//map id to node

		}
		//TODO pass this a reference to the list so we dont have to manuall assign it afterwards
		foreach (timelineNode tn in nodeList) {
			tn.allNodes = nodeList;
		}

		//second pass for assigning neighbors

		foreach (Feature f in container.features) {
			timelineNode tn = idMap[f.id];
			foreach(Neighbor n in f.neighbors) {
				tn.neighbors.Add(new KeyValuePair<string, timelineNode>(n.relationship,idMap[n.dest]));
			}
		}



		nodeDistanceIncrement = nodeList.Count;

		long maxdays = int.MinValue;
		long mindays = int.MaxValue;
		foreach (timelineNode tn in nodeList) {
			int totaldays = 365 * tn.date.Year + tn.date.DayOfYear;
			if (totaldays > maxdays) maxdays = totaldays;
			if (totaldays < mindays) mindays = totaldays;
		}

		float mover = 0;
		foreach (timelineNode tn in nodeList) {
			int totaldays = 365 * tn.date.Year + tn.date.DayOfYear;

			//TODO: use nodeDistanceIncrement
			tn.timelinePosition = new Vector3(map(totaldays, mindays, maxdays, 0, 100) + mover, UnityEngine.Random.Range(-15, 15), 0);
			tn.moveToPosition(tn.timelinePosition);
			mover += .1f;
		}

		//Begin narration
		StartCoroutine(Narrate());
		//Narrate();
	}

	long map(long x, long in_min, long in_max, long out_min, long out_max) {
		return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
	}

	void FixedUpdate() {
		
	}//end method FixedUpdate

	[Serializable]
	public class ChronologyRequest {
		public ChronologyRequest(int id, int turns) {
			this.id = id;
			this.turns = turns;
		}
		public int id;
		public int turns;
	}

	[Serializable]
	public class ChronologyResponse {
		public List<string> sequence;
	}

	//Narrate a sequence of nodes
	IEnumerator Narrate()
	{
		//The sequence of nodes we want to narrate, by name
		List<string> sequence_by_name = new List<string>();
		
		//Ask the backend for a node sequence
		//gameObject.GetComponent<SocketListener>().sendMessageToServer("CHRONOLOGY:13:10");



		string url = "http://localhost:8084/chronology";
		string data = JsonUtility.ToJson(new ChronologyRequest(13,10));
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

		/*string return_message = gameObject.GetComponent<SocketListener> ().ReceiveDataFromServer ();
		print ("Backend response: " + return_message);

		//Separate the response by double-colon
		string[] delimiters = {"::"};
		string[] return_message_split = return_message.Split(delimiters, StringSplitOptions.None);
		foreach (string message_part in return_message_split) {
			sequence_by_name.Add (message_part);
		}//end foreach*/

		/*sequence_by_name.Add("Roman Empire");
		sequence_by_name.Add("Rome");
		sequence_by_name.Add("Italy");
		sequence_by_name.Add("Constantinople");
		sequence_by_name.Add("Byzantine Empire");
		sequence_by_name.Add("Istanbul");
		sequence_by_name.Add("Turkey");
		sequence_by_name.Add("Ankara");*/

		//The nodes themselves
		List<GameObject> sequence_by_node = new List<GameObject>();
		GameObject temp_node = null;
		//foreach (string name in sequence_by_name)
		foreach (string name in response.sequence) {
			temp_node = null;
			nodeDict.TryGetValue(name, out temp_node);
			sequence_by_node.Add(temp_node);
		}//end foreach

		//Bring each node out of focus.
		foreach (timelineNode tn in nodeList)
		{
			//Change its color
			//temp.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
			//Set it to not display information on mouseover
			//temp.GetComponent<timelineNode>().display_info = false;
			tn.Unfocus();
		}//end foreach

		List<GameObject> node_history = new List<GameObject>();
		foreach (GameObject node_to_present in sequence_by_node)
		{
			//Bring the previous node into past-focus
			if (node_history.Count >= 1)
				node_history [node_history.Count - 1].GetComponent<timelineNode> ().PastFocus ();
			//Present this node
			Present(node_to_present, node_history);
			//Add it to the history
			node_history.Add(node_to_present);
			//Wait for spacebar before presenting the next.

			/*bool space_bar_pressed = false;
			while (!space_bar_pressed) {
				if (Input.GetKeyDown(KeyCode.Space)) {
					space_bar_pressed = true;
					break;
				}//end if
				yield return null;
			}//end while*/

			yield return StartCoroutine(WaitForKeyDown(KeyCode.Space));
		}//end foreach
	}//end method Traverse

	//Present the given node given the previous nodes presented
	void Present(GameObject node_to_present, List<GameObject> node_history)
	{
		print ("Current node: " + node_to_present.GetComponent<timelineNode>().text);
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

	//Wait asynchronously for a key press
	IEnumerator WaitForKeyDown(KeyCode keyCode)
	{
		do
		{
			yield return null;
		} while (!Input.GetKeyDown(keyCode));
	}

	void OnApplicationQuit()
	{
		//When the application quits, send a QUIT message to the backend.
		//This allows the backend to shut down its server gracefully.
		//gameObject.GetComponent<SocketListener> ().sendMessageToServer ("QUIT");
	}//end method OnApplicationQuit
}
