using UnityEngine;
using System.Collections;

public class DisplayScript : MonoBehaviour {

	public enum displayState {single,multi}

	public displayState display_mode;

	public Camera leftcam;
	public Camera rightcam;
	public Camera middlecam;

	void Start() {
		

		Debug.Log("displays connected: " + Display.displays.Length);
		// Display.displays[0] is the primary, default display and is always ON.
		// Check if additional displays are available and activate each.

		if(display_mode == displayState.multi) {
			if (Display.displays.Length > 1) {
				Display.displays[0].Activate();
			}
			else {
				Debug.LogWarning("WARNING: multi-display mode selected with less than 3 displays");
				return;
			}
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
			if (Display.displays.Length > 3) {
				Display.displays[3].Activate();
			}
			else {
				Debug.LogWarning("WARNING: multi-display mode selected with less than 3 displays");
			}
		}

		if (display_mode == displayState.single) {
			middlecam.rect = new Rect (0.33333333f, 0f, .33333333f, 1f);
			leftcam.rect = new Rect (0f, 0f, .33333333f, 1f);
			rightcam.rect = new Rect (0.66666666f, 0f, .33333333f, 1f);
			middlecam.targetDisplay = 0;
			leftcam.targetDisplay = 0;
			rightcam.targetDisplay = 0;

		}
		else if(display_mode == displayState.multi) {
			middlecam.rect = new Rect (0f, 0f, 1f, 1f);
			leftcam.rect = new Rect (0f, 0f, 1f, 1f);
			rightcam.rect = new Rect (0f, 0f, 1f, 1f);
			leftcam.targetDisplay = 0;
			middlecam.targetDisplay = 2;
			rightcam.targetDisplay = 1;
		}
	}

}