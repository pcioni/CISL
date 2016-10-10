using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonFade : MonoBehaviour {

	private Button btn;
	private IEnumerator fctr;

	public Text alertMsg;

	void Awake() {
		btn = GetComponent<Button>();
	}

	void fadeOut(string data) {
		if (fctr != null) StopCoroutine(fctr);
		fctr = _fade(1.5f, .1f);
		StartCoroutine(fctr);
		btn.interactable = false;
	}

	void fadeIn(string data) {
		if (fctr != null) StopCoroutine(fctr);
		fctr = _fade(1.5f, 1f);
		StartCoroutine(fctr);
		btn.interactable = true;
	}

	private IEnumerator _fade(float movetime, float target_alpha) {
		float t = 0f;
		while (t < 1) {
			t += Time.deltaTime / movetime;
			Color tmp = btn.image.color;
			tmp.a = Mathf.Lerp(tmp.a, target_alpha, t);
			btn.image.color = tmp;

			Color tmp2 = alertMsg.color;
			tmp2.a = 1-tmp.a;
			alertMsg.color = tmp2;
			yield return null;
		}
	}

	void Start() {
		EventManager.StartListening(EventManager.EventType.NARRATION_MACHINE_TURN, fadeIn);
		EventManager.StartListening(EventManager.EventType.NARRATION_USER_TURN, fadeOut);
	}



}
