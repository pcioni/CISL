using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System;

public class LoadXML : MonoBehaviour {

	public string xml_location = "Assets/xml/roman_empire_500_geospatial.xml";

	public Dictionary<string, GameObject> nodeDict = new Dictionary<string, GameObject>();

	public Sprite node_sprite;

	private List<GameObject> nodeList = new List<GameObject>();

	// Use this for initialization
	void Awake() {
		XmlSerializer serializer = new XmlSerializer(typeof(AIMind));
		FileStream stream = new FileStream(xml_location, FileMode.Open);
		AIMind container = (AIMind)serializer.Deserialize(stream);
		stream.Close();

		foreach (Feature f in container.features) {
			GameObject tmp_obj = new GameObject(f.data);
			timelineNode tn = tmp_obj.AddComponent<timelineNode>();
			tmp_obj.AddComponent<SpriteRenderer>().sprite = node_sprite;

			if (f.speak != null) {
				tn.text = f.speak.value;
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

			nodeList.Add(tmp_obj);
			nodeDict[f.data] = tmp_obj;
		}
		long maxticks = long.MinValue;
		long minticks = long.MaxValue;
		foreach (GameObject tn in nodeList) {
			long ticks = tn.GetComponent<timelineNode>().date.Ticks;
			if (ticks > maxticks) maxticks = ticks;
			if (ticks < minticks) minticks = ticks;
		}
		foreach (GameObject tn in nodeList) {
			long ticks = tn.GetComponent<timelineNode>().date.Ticks;
			tn.transform.position = new Vector3(map(ticks,minticks,maxticks,0,100), 0, 0);
		}
	}

	long map(long x, long in_min, long in_max, long out_min, long out_max) {
		return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
	}


}
