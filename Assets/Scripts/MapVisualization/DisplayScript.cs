using UnityEngine;
using System.Collections;

public class DisplayScript : MonoBehaviour {

	public enum displayState {single,multi}

	public displayState display_mode;

	public LayerMask single_mode;
	public LayerMask multi_mode;

	public Camera leftcam;
	public Camera rightcam;
	

	private Camera cam;

	void Awake() {
		cam = GetComponent<Camera>();
		if (display_mode == displayState.single) {
			cam.cullingMask = single_mode;
			leftcam.gameObject.SetActive(false);
			rightcam.gameObject.SetActive(false);
		}
		else if(display_mode == displayState.multi) {
			cam.cullingMask = multi_mode;
			leftcam.gameObject.SetActive(true);
			rightcam.gameObject.SetActive(true);
		}

	}


	// Use this for initialization
	void Start() {
		Debug.Log("displays connected: " + Display.displays.Length);
		// Display.displays[0] is the primary, default display and is always ON.
		// Check if additional displays are available and activate each.

		if(display_mode == displayState.multi) {
			if (Display.displays.Length > 1) {
				Display.displays[1].Activate();
			}
			else {
				Debug.LogWarning("WARNING: multi-display mode selected with less than 3 displays");
				return;
			}
			if (Display.displays.Length > 2) {
				Display.displays[2].Activate();
			}
			else {
				Debug.LogWarning("WARNING: multi-display mode selected with less than 3 displays");
			}
		}
	}

}