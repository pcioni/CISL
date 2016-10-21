using UnityEngine;
using System.Collections;

public class QuitOnEscape : MonoBehaviour {

	// Simple script to quit when press escape
	// Need for standalone

	private bool audioFinalized = false;
	private bool audioInitialized = false;

	void Update () {

		bool shiftDown = (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift));
		bool finalizeAudio = ( shiftDown && Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.F) ); // SHIFT + A + F
		bool initAudio = ( shiftDown && Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.I) ); // SHIFT + A + I

		if (Input.GetKeyDown(KeyCode.Escape)) {
			Application.Quit();
			finalizeAudio = true;
		}

		if (finalizeAudio) {
			OSCHandler.Instance.SendMessageToClient ("MaxServer", "/finalize/", 1.0f);
			Debug.Log ("QuitOnEscape.Update() :: finalizing audio");
			audioFinalized = true;
		} else {
			audioFinalized = false;
		}
		if (initAudio) {
			OSCHandler.Instance.SendMessageToClient ("MaxServer", "/initialize/", 1.0f);
			Debug.Log ("QuitOnEscape.Update() :: initializing audio");	
			audioInitialized = true;
		} else {
			audioInitialized = false;
		}
	}
}
