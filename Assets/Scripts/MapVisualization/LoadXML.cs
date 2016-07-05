using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System;

public class LoadXML : MonoBehaviour {

	public string xml_location = "Assets/xml/roman_empire_500_geospatial.xml";
	public Dictionary<string, GameObject> nodeDict = new Dictionary<string, GameObject>();
	public Dictionary<int, timelineNode> idMap = new Dictionary<int, timelineNode>();
	public Sprite node_sprite;
	private List<GameObject> nodeList = new List<GameObject>();
	private float nodeSpriteWidth;
	private float cameraWidth;
	private float nodeDistanceIncrement;

	// Use this for initialization
	void Awake() {
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
					tn.date = new DateTime(year, 1, 1);
				}

			}
			try {
				tn.location = new Vector2(f.geodata[0].lat, f.geodata[0].lon);
			}
			catch {
				tn.location = new Vector2();
			}
			tn.datevalue = tn.date.ToShortDateString();
			tn.dateticks = tn.date.Ticks;
			nodeList.Add(tmp_obj);
			nodeDict[f.data] = tmp_obj;

			idMap[f.id] = tn;//map id to node

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
		foreach (GameObject node in nodeList) {
			timelineNode tn = node.GetComponent<timelineNode>();
			int totaldays = 365 * tn.date.Year + tn.date.DayOfYear;
			if (totaldays > maxdays) maxdays = totaldays;
			if (totaldays < mindays) mindays = totaldays;
		}

		float mover = 0;
		foreach (GameObject node in nodeList) {
			timelineNode tn = node.GetComponent<timelineNode>();
			int totaldays = 365 * tn.date.Year + tn.date.DayOfYear;

			//TODO: use nodeDistanceIncrement
			tn.transform.position = new Vector3(map(totaldays,mindays,maxdays,0,100) + mover, UnityEngine.Random.Range(-15, 15) , 0);
			mover += .1f;
		}

		//Begin narration
		StartCoroutine(Narrate());
		//Narrate();
	}

	long map(long x, long in_min, long in_max, long out_min, long out_max) {
		return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
	}

	//Narrate a sequence of nodes
	IEnumerator Narrate()
	{
		//The sequence of nodes we want to narrate, by name
		List<string> sequence_by_name = new List<string>();
		sequence_by_name.Add("Roman Empire");
		sequence_by_name.Add("Rome");
		sequence_by_name.Add("Italy");
		sequence_by_name.Add("Constantinople");
		sequence_by_name.Add("Byzantine Empire");
		sequence_by_name.Add("Istanbul");
		sequence_by_name.Add("Turkey");
		sequence_by_name.Add("Ankara");

		//The nodes themselves
		List<GameObject> sequence_by_node = new List<GameObject>();
		GameObject temp_node = null;
		foreach (string name in sequence_by_name)
		{
			temp_node = null;
			nodeDict.TryGetValue(name, out temp_node);
			sequence_by_node.Add(temp_node);
		}//end foreach

		//Bring each node out of focus.
		foreach (GameObject temp in nodeList)
		{
			//Change its color
			temp.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
			//Set it to not display information on mouseover
			temp.GetComponent<timelineNode>().display_info = false;
			//FOR_PHIL: Other effects?
		}//end foreach

		List<GameObject> node_history = new List<GameObject>();
		foreach (GameObject node_to_present in sequence_by_node)
		{
			//Have the previous node stop displaying information
			if (node_history.Count >= 1)
				node_history[node_history.Count - 1].GetComponent<timelineNode>().display_info = false;
			//Present this node
			Present(node_to_present, node_history);
			//Add it to the history
			node_history.Add(node_to_present);
			//Wait for spacebar before presenting the next.



			yield return StartCoroutine(WaitForKeyDown(KeyCode.Space));
		}//end foreach
	}//end method Traverse

	//Present the given node given the previous nodes presented
	void Present(GameObject node_to_present, List<GameObject> node_history)
	{
		//Bring this node into focus
		//Change its color
		node_to_present.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
		//Set it to display information on mouseover
		node_to_present.GetComponent<timelineNode>().display_info = true;
		//Bring it to the front by changing its Z
		node_to_present.transform.position = new Vector3(node_to_present.transform.position.x
			, node_to_present.transform.position.y
			, node_to_present.transform.position.z - 5);
		//FOR_PHIL: Other effects, such as bringing the map up?

		//Some nodes may be layered on top of each other. Displace this node in the y if any other
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
		}//end while

	}//end method Present

	//Wait asynchronously for a key press
	IEnumerator WaitForKeyDown(KeyCode keyCode)
	{
		do
		{
			yield return null;
		} while (!Input.GetKeyDown(keyCode));
	}

}
