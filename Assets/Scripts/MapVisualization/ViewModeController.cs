using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using JsonConstructs;

public class ViewModeController : MonoBehaviour {

	//public enum viewMode { MAP, TIMELINE}
	//public viewMode current_mode = viewMode.TIMELINE;

	public LocationMapper current_map;

	private UnityAction<string> listener;

	private LoadXML lx;

	public GameObject mapNodePrefab;
	public List<mapNode> dummynodes;
	public Dictionary<int, Vector2> dummynodemap;
	public Dictionary<timelineNode, mapNode> crossmap;

	public Camera mapCam;
	public float panTime = 3f;

	void Awake() {
		listener = delegate (string data) {
			NodeData nd = JsonUtility.FromJson<NodeData>(data);
			Vector2 mappoint;
			if(dummynodemap.TryGetValue(nd.id, out mappoint)) {
				print("== loc change: " + nd.id + " ==");
				panToPoint(mappoint);
			}else {
				Debug.LogWarning("WARNING: attempted to pan to nonexistant map node with id: " + nd.id);
			}

			
		};

		EventManager.StartListening(EventManager.EventType.INTERFACE_NODE_SELECT, listener);
	}


	IEnumerator Start() {
		lx = GetComponent<LoadXML>();
		dummynodes = new List<mapNode>();
		dummynodemap = new Dictionary<int, Vector2>();
		crossmap = new Dictionary<timelineNode, mapNode>();

		while (GoogleMap.m_maxLatitude == 0) {
			yield return new WaitForEndOfFrame ();
		}

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
			mn.transform.localPosition = current_map.coord2local(tn.location);

			dummynodemap[tn.node_id] = mn.transform.position;
			dummynodes.Add(mn);
			crossmap[tn] = mn;

			yield return null;
		}

		//assign corresponding neighbors
		foreach (timelineNode tn in lx.nodeList) {
			mapNode tmp = crossmap[tn];
			foreach(KeyValuePair<string,timelineNode> tn2 in tn.neighbors) {
				tmp.neighbors.Add(crossmap[tn2.Value]);
			}
			yield return null;
		}


		print("Done finding positions");
		//disabling auto move because it interrupts demo
		//panToPoint(dummynodemap[13]);//start off on rome
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

	private Queue<KeyValuePair<int, timelineNode>> q = new Queue<KeyValuePair<int, timelineNode>>();
	private List<Vector3> positions = new List<Vector3>();
	private int max_depth = 3;
	private void find_coordinates(timelineNode start) {
		positions.Clear();
		q.Clear();
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
