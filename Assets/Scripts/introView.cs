using UnityEngine;
using System.Collections;

public class introView : MonoBehaviour {

	public Camera leftcam;
    public Camera slippyCam;
    public float movetime = 1.5f;

	// Use this for initialization
	IEnumerator begin () {
		float t = 0f;
		while (t < 1) {
			t += Time.deltaTime / movetime;
			leftcam.rect = new Rect(0, 0, Mathf.Lerp(1, 1/3f, t), 1);
            slippyCam.rect = new Rect(0, 0, Mathf.Lerp(1, 1 / 3f, t), 1);
            yield return null;
		}
	}

	void Awake() {
		leftcam.rect = new Rect(0, 0, 1, 1);
        slippyCam.rect = new Rect(0, 0, 1, 1);
    }

	private bool triggered = false;

	void Update () {
		if (!triggered && Input.GetKeyDown(KeyCode.Return)) {
			StartCoroutine(begin());
			triggered = true;
		}
	}
}
