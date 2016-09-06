using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;


public class NarrationJournal : MonoBehaviour {

	public Text journaltext;
	public Text pagenumber;
	private List<string> narration_history;
	private UnityAction<string> listener;

	public int current_page = 0;

	public List<string> entries;

	void Awake() {
		entries = new List<string>();
		listener = delegate (string data) {
			add_entry(data);
			readText(data);
		};
	}

	public void page_left() {
		if(current_page > 0) current_page--;
		journaltext.text = entries[current_page];
		pagenumber.text = (current_page + 1) + "/" + entries.Count;
		EventManager.TriggerEvent(EventManager.EventType.INTERFACE_PAGE_LEFT,
			                      current_page.ToString());
	}

	public void page_right() {
		if (current_page < entries.Count-1) current_page++;
		journaltext.text = entries[current_page];
		pagenumber.text = (current_page + 1) + "/" + entries.Count;
		EventManager.TriggerEvent(EventManager.EventType.INTERFACE_PAGE_RIGHT,
			                      current_page.ToString());
	}

	public void add_entry(string data) {
		entries.Add(data);
		current_page = entries.Count-1;
		pagenumber.text = (current_page + 1) + "/" + entries.Count;
		journaltext.text = entries[current_page];
	}

	public void readText(string data){
		TextToSpeechWatson TTS = new TextToSpeechWatson();
		TTS.TextToSynth = data;
		TTS.Start();
	}

	void Start() {
		EventManager.StartListening(EventManager.EventType.NARRATION_MACHINE_TURN, listener);
	}
	
}
