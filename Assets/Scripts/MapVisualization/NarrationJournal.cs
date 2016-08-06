using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Text))]
public class NarrationJournal : MonoBehaviour {

	private Text txt;
	private List<string> narration_history;
	private UnityAction<string> listener;

	void Awake() {
		txt = GetComponent<Text>();
		listener = delegate (string data) {
			txt.text += data + "\n\n";
		};
	}

	void Start() {
		EventManager.StartListening(EventManager.EventType.NARRATION_MACHINE_TURN, listener);
	}
	
}
