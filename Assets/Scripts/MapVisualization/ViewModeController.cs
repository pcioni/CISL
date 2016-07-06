using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ViewModeController : MonoBehaviour {

	public enum viewMode { MAP, TIMELINE}
	public viewMode current_mode = viewMode.TIMELINE;

	public LocationMapper current_map;

	private LoadXML lx;

	IEnumerator Start() {
		lx = GetComponent<LoadXML>();

		//find all of the appropriate positions for the current map
		foreach (timelineNode tn in lx.nodeList) {
			if (!tn.known_location) {
				find_coordinates(tn);
			}
			yield return null;
		}
		print("Done finding positions");
		foreach (timelineNode tn in lx.nodeList) {
			tn.mapPosition = current_map.coord2world(tn.location);
		}
	}


	private void find_coordinates(timelineNode start) {

		List<Vector3> positions = new List<Vector3>();
		int max_depth = 4;

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
			print("location found for " + start.name + ": " + start.location);
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
			tn.moveToPosition(tn.mapPosition);
		}

	}

	public void TimelineMode() {
		//switch to timeline mode

		foreach (timelineNode tn in lx.nodeList) {
			tn.moveToPosition(tn.timelinePosition);
		}

	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.E)) {
			toggle_mode();
		}
	}




}
