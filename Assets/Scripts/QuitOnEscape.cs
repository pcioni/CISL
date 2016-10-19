using UnityEngine;
using System.Collections;

public class QuitOnEscape : MonoBehaviour {

	// Simple script to quit when press escape
	// Need for standalone

	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape)) {
			Application.Quit();
            OSCHandler.Instance.SendMessageToClient("MaxServer", "/finalize/", 1.0f);
		}
        if (Input.GetKeyDown(KeyCode.I))
        {
            OSCHandler.Instance.SendMessageToClient("MaxServer", "/initialize/", 1.0f);
        }
	}
}
