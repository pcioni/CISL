﻿using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using JsonConstructs;

public class ViewModeController : MonoBehaviour {

	//public enum viewMode { MAP, TIMELINE}
	//public viewMode current_mode = viewMode.TIMELINE;

	public LocationMapper current_map;

	private UnityAction<string> listener;
	private string lastPanNodeData;

	private LoadXML lx;

	public GameObject mapNodePrefab;
	public List<mapNode> dummynodes;
	public Dictionary<int, Vector2> dummynodemap;
	public Dictionary<timelineNode, mapNode> crossmap;

	public Camera mapCam;

	void Awake() {
		listener = delegate (string data) {
			// don't respond to repeated requests to pan to the same node
			if (data == lastPanNodeData) return;

			NodeData nd = JsonUtility.FromJson<NodeData>(data);
			Vector2 mappoint;
			if(dummynodemap.TryGetValue(nd.id, out mappoint)) {
				print("== loc change: " + nd.id + " ==");
				panToPoint(mappoint);
				lastPanNodeData = data;
			}else {
				Debug.LogWarning("WARNING: attempted to pan to nonexistant map node with id: " + nd.id);
			}

			
		};

		EventManager.StartListening(EventManager.EventType.INTERFACE_NODE_SELECT, listener);
		EventManager.StartListening(EventManager.EventType.NARRATION_LOCATION_CHANGE, listener);

		minOrthoSize = mapCam.orthographicSize;
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
		if (location.Equals( mapCam.transform.position)) return;
		if (currentPan != null) StopCoroutine(currentPan);
		currentPan = _pan(location);
		StartCoroutine(currentPan);
	}
		
	public float maxPanTime = 2f; // seconds
	public float minPanTime = 0.25f; // seconds
	public float panSpeed = 0.025f; // seconds per update (i.e.: 30 fps = 0.03333333, etc.)
	private float minOrthoSize; // gets set to devault camera ortho
	public float maxOrthoSize = 25;
	public AnimationCurve panCurve;

	IEnumerator _pan(Vector2 _dest) {
		Vector2 startPos = mapCam.transform.position;
		float startOrtho = mapCam.orthographicSize;
		float t = 0f;
		float orthoT = 0f;

		// calc panDuration and orthoPeak directly proportional to distance traveled on map
		// yet clamped to min & max values
		Vector2 d = startPos - _dest;
		float dist = d.magnitude;
		float panDuration = minPanTime + (maxPanTime - minPanTime) * Mathf.Clamp01 (dist * panSpeed);
		float orthoPeak = minOrthoSize + (maxOrthoSize - minOrthoSize) * panDuration / maxPanTime; 

		while (t < 1) {
			t += Time.deltaTime / panDuration;

			if (orthoPeak > startOrtho) {
				if (t < .5f) {//zoom out when panning a long distance
					orthoT = panCurve.Evaluate (t * 2);
					mapCam.orthographicSize = startOrtho + (orthoPeak - startOrtho) * orthoT;
				} else {//zoom back in at end	
					orthoT = panCurve.Evaluate (1 - (t * 2 - 1));
					mapCam.orthographicSize = minOrthoSize + (orthoPeak - minOrthoSize) * orthoT;
				}
			} else {
				orthoT = panCurve.Evaluate (1 - t);
				mapCam.orthographicSize = minOrthoSize + (startOrtho - minOrthoSize) * orthoT;	
			}

			mapCam.transform.position = Vector2.Lerp(startPos, _dest, panCurve.Evaluate(t));


//			Debug.Log ("ViewModeController._pan()");
//			Debug.Log ("               t = " + t);
//			Debug.Log ("           halfT = " + orthoT);
//			Debug.Log ("orthographicSize = " + mapCam.orthographicSize);

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
