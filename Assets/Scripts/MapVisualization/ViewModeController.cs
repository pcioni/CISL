using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class ViewModeController : MonoBehaviour {

	//public enum viewMode { MAP, TIMELINE}
	//public viewMode current_mode = viewMode.TIMELINE;

	public LocationMapper current_map;

	private UnityAction<string> listener;

	private LoadXML lx;

	public GameObject mapNodePrefab;
	public List<mapNode> dummynodes;
	public Dictionary<int, Vector2> dummynodemap;

	public Camera mapCam;
	public float panTime = 3f;

	void Awake() {
		listener = delegate (string data) {
			print("== loc change: " + data + " ==");
			panToPoint(dummynodemap[int.Parse(data)]);
		};

		EventManager.StartListening(EventManager.EventType.INTERFACE_NODE_SELECT, listener);
	}


	IEnumerator Start() {
		lx = GetComponent<LoadXML>();
		dummynodes = new List<mapNode>();
		dummynodemap = new Dictionary<int, Vector2>();

		//find all of the appropriate positions for the current map
		foreach (timelineNode tn in lx.nodeList) {
			if (!tn.known_location) {
				find_coordinates(tn);
			}
			GameObject dummy = Instantiate(mapNodePrefab) as GameObject;
			mapNode mn = dummy.GetComponent<mapNode>();
			mn.master = tn;
			dummy.layer = LayerMask.NameToLayer("MapLayer");
			mn.transform.SetParent(current_map.transform, false);
			
			tn.mapPosition = current_map.coord2world(tn.location);
			mn.mapPosition = current_map.coord2local(tn.location);
			mn.transform.localPosition = mn.mapPosition;

			dummynodemap[tn.node_id] = mn.transform.position;
			dummynodes.Add(mn);

			yield return null;
		}
		print("Done finding positions");
		panToPoint(dummynodemap[13]);//start off on rome
	}


	private IEnumerator currentPan = null;
	void panToPoint(Vector2 location) {
		if (currentPan != null) StopCoroutine(currentPan);
		currentPan = _pan(location);
		StartCoroutine(currentPan);
	}

	public AnimationCurve panCurve;

	IEnumerator _pan(Vector2 location) {
		float t = 0f;
		Vector2 startpos = mapCam.transform.position;
		while (t < 1) {
			t += Time.deltaTime / panTime;
			mapCam.transform.position = Vector2.Lerp(startpos, location, t);

			mapCam.orthographicSize = panCurve.Evaluate(t);
			/*if (t < .5f) {//zoom out when panning a long distance
				mapCam.orthographicSize = Mathf.Lerp(14, 40, t * 2);
			}
			else {//zoom back in at end	
				mapCam.orthographicSize = Mathf.Lerp(40, 14, t * 2 - 1);
			}*/

			yield return null;
		}
	}

	private void find_coordinates(timelineNode start) {

		List<Vector3> positions = new List<Vector3>();
		int max_depth = 3;

		Queue<KeyValuePair<int, timelineNode>> q = new Queue<KeyValuePair<int, timelineNode>>();
		q.Enqueue(new KeyValuePair<int, timelineNode>(0,start));
		while (q.Count > 0) {
			KeyValuePair<int, timelineNode> current = q.Dequeue();
			if (current.Value.known_location) {
				positions.Add(current.Value.location);
			}
			if(current.Key < max_depth) {
				foreach(KeyValuePair<string,timelineNode> kvp in current.Value.neighbors) {
					q.Enqueue(new KeyValuePair<int,timelineNode>(current.Key+1, kvp.Value));
				}
			}
		}
		if(positions.Count != 0) {
			start.location = get_centroid(positions);
			//print("location found for " + start.name + ": " + start.location);
		}
		
	}

	private Vector3 get_centroid(List<Vector3> positions) {
		float sumx = 0;
		float sumy = 0;
		float sumz = 0;
		int len = positions.Count;
		
		foreach(Vector3 v in positions) {
			sumx += v.x;
			sumy += v.y;
			sumz += v.z;
		}

		return new Vector3(sumx / len, sumy / len, sumz / len);
	}

	/*
	public void toggle_mode() {
		//switch from timeline view to map view and vice versa
		switch (current_mode) {
			case viewMode.MAP:
				current_mode = viewMode.TIMELINE;
				current_map.fadeOut();
				TimelineMode();
				break;
			case viewMode.TIMELINE:
				current_mode = viewMode.MAP;
				current_map.fadeIn();
				MapMode();
				break;
		}
	}

	public void MapMode() {
		//switch to map mode

		//fade out background nodes

		//move foreground nodes to map position
		foreach (timelineNode tn in lx.nodeList) {
			Vector3 newpos = new Vector3(tn.mapPosition.x, tn.mapPosition.y, tn.transform.position.z);
			tn.moveToPosition(newpos);
		}

	}

	public void TimelineMode() {
		//switch to timeline mode

		foreach (timelineNode tn in lx.nodeList) {
			Vector3 newpos = new Vector3(tn.timelinePosition.x, tn.timelinePosition.y, tn.transform.position.z);
			tn.moveToPosition(newpos);
		}

	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.E)) {
			toggle_mode();
		}
	}
	*/


}
