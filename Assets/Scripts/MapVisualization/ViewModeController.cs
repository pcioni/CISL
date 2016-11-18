using UnityEngine;
using UnityEngine.Events;
using System;
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

    public bool reconstructGeolocationData = true;
    public bool reconstructDateData = true;

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

	enum data_type {
		LOC,
		DATE
	}

	IEnumerator Start() {
        DebugMode.startTimer("ViewModeController.Start()");
        DebugMode.startTimer("ViewModeController.Start() :: initilizing");

        lx = GetComponent<LoadXML>();
		dummynodes = new List<mapNode>();
		dummynodemap = new Dictionary<int, Vector2>();
		crossmap = new Dictionary<timelineNode, mapNode>();

		while (GoogleMap.m_maxLatitude == 0) {
			yield return new WaitForEndOfFrame ();
		}

        DebugMode.stopTimer("ViewModeController.Start() :: initilizing");
        DebugMode.startTimer("ViewModeController.Start() :: reconstructing data based on existing data");

        bool result = false;
		//find all of the appropriate positions for the current map
		foreach (timelineNode tn in lx.nodeList) {
            //DebugMode.startTimer(string.Format("ViewModeController.Start() :: reconstructed \"{0}\" data based on existing data", tn.name));

            if (reconstructGeolocationData && !tn.known_location) {
				result = reconstruct_data(tn, data_type.LOC);
				if (!result) {
					//print ("ViewModeController.Start() :: location from outgoing connection data not found for " + tn.node_name + ": location = " + tn.location);
					//print ("positions.Count: " + positions.Count);

					result = reconstruct_data(tn, data_type.LOC, false, true);
					if (!result) {
						//print ("ViewModeController.Start() :: location from incoming connection data not found for " + tn.node_name + ": location = " + tn.location);
						//print ("positions.Count: " + positions.Count);
					}
				}
			}
			if (reconstructDateData && !tn.known_date) {
                result = reconstruct_data(tn, data_type.DATE);
                if (!result) {
					//print ("ViewModeController.Start() :: date from outgoing connection data not found for " + tn.node_name + ": dateticks = " + tn.dateticks);
					//print ("dateTicks.Count: " + dateTicks.Count);

                    result = reconstruct_data(tn, data_type.DATE, false, true);
                    if (!result) {
						//print ("ViewModeController.Start() :: date from incoming connection data not found for " + tn.node_name + ": dateticks = " + tn.dateticks);
						//print ("dateTicks.Count: " + dateTicks.Count);
					}
				}
			}

            //DebugMode.stopTimer(string.Format("ViewModeController.Start() :: reconstructed \"{0}\" data based on existing data", tn.name));

        }

        DebugMode.stopTimer("ViewModeController.Start() :: reconstructing data based on existing data");
        DebugMode.startTimer("ViewModeController.Start() :: instantiating nodes and reconstructing data based on interpolated data");

        foreach (timelineNode tn in lx.nodeList)
        {
            //DebugMode.startTimer(string.Format("ViewModeController.Start() :: reconstructed map node \"{0}\" data based on interpolated data", tn.name));

            if (reconstructGeolocationData && !tn.known_location && !tn.location_interpolated)
            {
				result = reconstruct_data(tn, data_type.LOC, true);
                if (!result)
                {
                    //print("ViewModeController.Start() :: location from interpolated ooutgoing connect data not found for " + tn.node_name + ": " + tn.location);
                    //print("positions.Count: " + positions.Count);

					result = reconstruct_data(tn, data_type.LOC, true, true);
                    if (!result)
                    {
                        //print("ViewModeController.Start() :: location from interpolated incoming connection data not found for " + tn.node_name + ": " + tn.location);
                        //print("positions.Count: " + positions.Count);
                    }
                }
            }
            if (reconstructDateData && !tn.known_date && !tn.date_interpolated)
            {
                result = reconstruct_data(tn, data_type.DATE, true);
                if (!result)
                {
                    //print("ViewModeController.Start() :: date from interpolated ooutgoing connect data not found for " + tn.node_name + ": dateticks = " + tn.dateticks);
                    //print("dateTicks.Count: " + dateTicks.Count);

                    result = reconstruct_data(tn, data_type.DATE, true, true);
                    if (!result)
                    {
                        //print("ViewModeController.Start() :: date from interpolated incoming connection data not found for " + tn.node_name + ": dateticks = " + tn.dateticks);
                        //print("dateTicks.Count: " + dateTicks.Count);
                    }
                }
            }

            //DebugMode.stopTimer(string.Format("ViewModeController.Start() :: reconstructed map node \"{0}\" data based on interpolated data", tn.name));
            //DebugMode.startTimer(string.Format("ViewModeController.Start() :: instantiated map node \"{0}\" and set properties", tn.name));

            GameObject dummy = Instantiate(mapNodePrefab) as GameObject;

            mapNode mn = dummy.GetComponent<mapNode>();
            mn.master = tn;
            dummy.layer = LayerMask.NameToLayer("MapLayer");
            mn.transform.SetParent(current_map.transform, false);

            mn.transform.localPosition = current_map.coord2local(tn.location);

            dummynodemap[tn.node_id] = mn.transform.position;
            dummynodes.Add(mn);
            crossmap[tn] = mn;

            //DebugMode.stopTimer(string.Format("ViewModeController.Start() :: instantiated map node \"{0}\" and set properties", tn.name));

            yield return null;
        }

        DebugMode.stopTimer("ViewModeController.Start() :: instantiating nodes and reconstructing data based on interpolated data");
        DebugMode.startTimer("ViewModeController.Start() :: assigning neighbors");

        // TODO: optimize this
        //assign corresponding neighbors
        foreach (timelineNode tn in lx.nodeList) {
			mapNode tmp = crossmap[tn];
			foreach(KeyValuePair<string,timelineNode> tn2 in tn.neighbors) {
				tmp.neighbors.Add(crossmap[tn2.Value]);
			}
			yield return null;
		}

        DebugMode.stopTimer("ViewModeController.Start() :: assigning neighbors");
        DebugMode.stopTimer("ViewModeController.Start()");

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
    private List<timelineNode> previousNodes = new List<timelineNode>();
	private List<Vector3> positions = new List<Vector3>();
	private List<long> dateTicks = new List<long>();
	private int max_depth = 30;
    private int min_data = 10;
	private bool reconstruct_data(timelineNode start, data_type dtype, bool use_interpolated = false, bool use_incoming = false) {
        previousNodes.Clear();
        positions.Clear();
		dateTicks.Clear();
        q.Clear();
		q.Enqueue(new KeyValuePair<int, timelineNode>(0,start));
		while (q.Count > 0) {
			KeyValuePair<int, timelineNode> current = q.Dequeue();
			switch (dtype) {
			case data_type.LOC:
				if ((current.Value.known_location || use_interpolated) && current.Value.location != new Vector2(0,0)) {
					positions.Add(current.Value.location);
				}
				break;
            case data_type.DATE:
                if ((current.Value.known_date || use_interpolated) && current.Value.dateticks != 0)
                {
                    dateTicks.Add(current.Value.dateticks);
                }
                break;
            default:
				Debug.Log ("ViewModeController.find_coordinates() :: unhandled dataType : " + dtype.ToString());
                return false;
				//break;
			}
			if(current.Key < max_depth) {
                if (!use_incoming)
                {
                    foreach (KeyValuePair<string, timelineNode> kvp in current.Value.neighbors)
                    {
                        // check previously queued nodes list and ignore this node if it was already queued at some point in the past
                        if (!previousNodes.Contains(kvp.Value))
                        {
                            q.Enqueue(new KeyValuePair<int, timelineNode>(current.Key + 1, kvp.Value));
                            // store node in list of previuosly queued nodes
                            previousNodes.Add(kvp.Value);
                        }
                    }
                } else
                {
                    foreach (timelineNode tn in current.Value.neighbors_incoming)
                    {
                        // check previously queued nodes list and ignore this node if it was already queued at some point in the past
                        if (!previousNodes.Contains(tn))
                        {
                            q.Enqueue(new KeyValuePair<int, timelineNode>(current.Key + 1, tn));
                            // store node in list of previuosly queued nodes
                            previousNodes.Add(tn);
                        }
                    }
                }
                if (dtype == data_type.LOC && positions.Count >= min_data) break; // break if we found enough location data
                if (dtype == data_type.DATE && dateTicks.Count >= min_data) break; // break if we found enough date data
            }
        }
		switch (dtype) {
            case data_type.LOC:
                if (positions.Count != 0)
                {
                    start.location = get_centroid(positions);
                    start.location_interpolated = true;
                    return true;
                }
                else
                {
                    //print("ViewModeController.reconstruct_data() :: location data not found for " + start.node_name + ": " + start.location);
                    //print("positions.Count: " + positions.Count);
                    return false;
                }
                //break;
            case data_type.DATE:
                if (dateTicks.Count != 0)
                {
                    start.dateticks = get_centroid(dateTicks);
                    start.date = new DateTime(start.dateticks);
                    start.datevalue = start.date.ToShortDateString();
                    start.date_interpolated = true;
                    start.reset_timeline_position();
                    return true;
                }
                else
                {
                    //print("ViewModeController.reconstruct_data() :: date data not found; node_name = " + start.node_name + "; dateticks = " + start.dateticks);
                    //print("tickCounts.Count: " + dateTicks.Count);
                    return false;
                }
                //break;
            default:
                Debug.Log("ViewModeController.find_coordinates() :: unhandled dataType : " + dtype.ToString());
                return false;
                //break;
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

	private long get_centroid(List<long> _tickCounts) {
		long sum = 0;
		int len = _tickCounts.Count;

		foreach(long t in _tickCounts) {
			sum += t / len;
		}

		return sum;
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
