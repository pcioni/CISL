using UnityEngine;
using System.Collections;

public class QuitOnEscape : MonoBehaviour {

	// Simple script to quit when press escape
	// Need for standalone

	void Update () {

		if (Input.GetKeyDown(KeyCode.Escape)) {
			Application.Quit();
		}
	}
}