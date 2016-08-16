﻿using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System;
using Backend;

#if UNITY_EDITOR
using UnityEditor;
//using System.Runtime.InteropServices;

[CustomEditor(typeof(LoadXML))]
public class ObjectBuilderEditor : Editor {
	//[DllImport("user32.dll")]
	//private static extern void OpenFileDialog();

	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		LoadXML myScript = (LoadXML)target;
		if (GUILayout.Button("Load XML file")) {
			//System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
			//myScript.xml_location = ofd.FileName;
			myScript.xml_location = EditorUtility.OpenFilePanel("open file", Application.dataPath, "xml");
		}
		if (GUILayout.Button("Load Default XML")) {
			//System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
			//myScript.xml_location = ofd.FileName;
			myScript.xml_location = AppConfig.Settings.Frontend.xml_location;
		}
	}
}
#endif

public class LoadXML : MonoBehaviour {

	//public string xml_location = "Assets/xml/roman_empire_1000.xml";
	public string xml_location;
	public Dictionary<string, GameObject> nodeDict = new Dictionary<string, GameObject>();
	public Dictionary<int, timelineNode> idMap = new Dictionary<int, timelineNode>();
	public Sprite node_sprite;
	public List<timelineNode> nodeList = new List<timelineNode>();
	
	public GameObject timelineNodePref;

	public bool loaded = false;

	// Use this for initialization
	public void Initialize() {
		//load default file if not specified
		if (string.IsNullOrEmpty(xml_location)) {
			 xml_location = AppConfig.Settings.Frontend.xml_location;
		}
		XmlSerializer serializer = new XmlSerializer(typeof(AIMind));
		FileStream stream = new FileStream(xml_location, FileMode.Open);
		AIMind container = (AIMind)serializer.Deserialize(stream);
		stream.Close();

		GameObject timeLineNodes = new GameObject("TimeLineNodes");

		foreach (Feature f in container.features) {
			GameObject tmp_obj = (GameObject) Instantiate(timelineNodePref, transform.position, transform.rotation);
			tmp_obj.name = "Node<"+f.data+">";
			timelineNode tn = tmp_obj.GetComponent<timelineNode>();
			tn.node_id = f.id;
			tn.node_name = f.data;
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

			tmp_obj.transform.SetParent(timeLineNodes.transform);

		}
		//TODO pass this a reference to the list so we dont have to manually assign it afterwards
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

		long maxdays = int.MinValue;
		long mindays = int.MaxValue;
		foreach (timelineNode tn in nodeList) {
			int totaldays = 365 * tn.date.Year + tn.date.DayOfYear;
			if (totaldays > maxdays) maxdays = totaldays;
			if (totaldays < mindays) mindays = totaldays;
		}

		TimeLineBar.minDays = mindays;
		TimeLineBar.maxDays = maxdays;


		string url = "http://" + AppConfig.Settings.Backend.ip_address + ":" + AppConfig.Settings.Backend.port + "/getgraph";
		WWW www = new WWW(url);
		float start = Time.time;
		while (!www.isDone) {//wait for request
			if(Time.time > start + 10) {//time out after 10s
				break;
			}
		}
		if(www.error != null) {
			Debug.LogError("Error: could not connect to backend; aborting.");
			return;
		}

		GraphLight response = JsonUtility.FromJson<GraphLight>(www.text);

		foreach(GraphNodeLight gn in response.graph_nodes) {
			switch (gn.entity_type) {
				case "character":
					idMap[gn.id].category = timelineNode.nodeCategory.CHARACTER;
					break;
				case "location":
					idMap[gn.id].category = timelineNode.nodeCategory.LOCATION;
					break;
				case "event":
					idMap[gn.id].category = timelineNode.nodeCategory.EVENT;
					break;
				default:
					idMap[gn.id].category = timelineNode.nodeCategory.UNKNOWN;
					break;
			}
		}



		float mover = 0;
		foreach (timelineNode tn in nodeList) {
			int totaldays = 365 * tn.date.Year + tn.date.DayOfYear;

			float ypos = 0;
			switch (tn.category) {
				case timelineNode.nodeCategory.CHARACTER:
					ypos = 0;
					break;
				case timelineNode.nodeCategory.EVENT:
					ypos = -20;
					break;
				case timelineNode.nodeCategory.LOCATION:
					ypos = 20;
					break;
				case timelineNode.nodeCategory.UNKNOWN:
					ypos = -40;
					break;
			}

			tn.timelinePosition = new Vector3(TimeLineBar.dateToPosition(totaldays), ypos + UnityEngine.Random.Range(-5,5), 0);
			tn.moveToPosition(tn.timelinePosition);
			mover += .1f;
		}

		loaded = true;
	}

	long map(long x, long in_min, long in_max, long out_min, long out_max) {
		return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
	}

}
