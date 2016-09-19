using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;
using System;

public class SaveScreenshot : MonoBehaviour {

	// TODO: create radio list for resolution presets: 
	//        - "Web" 1.0f
	//        - "Print" 4.0f //actually 4.17, but 4.0f is cleaner
	//        - "Custom" // have it enable the "resolution" entry field which should otherwise be greyed out/disappear
	// TODO: allow file browsing for specification of directory
	// TODO: allow for camera picking (to screenshot one camera only, as opposed to entire scene with three setups)

	public float resolution = 1.0f; // "Web" 1.0f; "Print" 4.0f //actually 4.17, but 4.0f is cleaner
	public string screenshotDirectory = "Screenshots"; // at the CISL directory level

	private bool saved = false;

	// Use this for initialization
	void Start () {
	}
		
	// Update is called once per frame
	void Update () {
		// capture screenshot when SHIFT + C + S are pressed
		bool shiftDown = (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift));
		bool allDown = (shiftDown && Input.GetKey (KeyCode.S) && Input.GetKey (KeyCode.C));

		if (allDown && !saved) {
			// prepare PNG filename using date, time, and scene name
			DateTime dtUTC = System.DateTime.UtcNow;
			string fext = ".png";
			string datePatt = @"yyyyMMdd_hhmmss";
			string fname = dtUTC.ToString (datePatt) + "_" + SceneManager.GetActiveScene ().name + fext;

			// append filename to screenshot dir name
			string fpath = Path.Combine (screenshotDirectory, fname);

			// do the thing
			Application.CaptureScreenshot (fpath);
			saved = true; // to avoid saving repeatedly while keys are down

			// debug
			Debug.Log ("SaveScreenshot :: fpath = " + fpath);

		} else {
			saved = false;
		}
	}
}
