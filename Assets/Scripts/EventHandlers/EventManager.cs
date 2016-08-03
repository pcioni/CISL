using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class EventManager : MonoBehaviour {

	private class NarrationEvent: UnityEvent<string>{}

	//event types defined here
	public enum EventType {
		//NARRATION_RELATIONSHIP,
		//NARRATION_ANALOGY,
		//NARRATION_LEAD_IN,
		//NARRATION_NOVEL_LEAD_IN,
		//NARRATION_HINT_AT,
		//NARRATION_TIE_BACK,
		NARRATION_MACHINE_TURN,
		NARRATION_USER_TURN,
		INTERFACE_NODE_SELECT
	}

	private Dictionary<EventType, NarrationEvent> eventDictionary;

	private static EventManager eventManager;

	public static EventManager instance {
		get {
			if (!eventManager) {
				eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;

				if (!eventManager) {
					Debug.LogError("There needs to be one active EventManger script on a GameObject in your scene.");
				}
				else {
					eventManager.Init();
				}
			}

			return eventManager;
		}
	}

	void Init() {
		if (eventDictionary == null) {
			eventDictionary = new Dictionary<EventType, NarrationEvent>();
		}
	}

	public static void StartListening(EventType eventName, UnityAction<string> listener) {
		NarrationEvent thisEvent = null;
		if (instance.eventDictionary.TryGetValue(eventName, out thisEvent)) {
			thisEvent.AddListener(listener);
		}
		else {
			thisEvent = new NarrationEvent();
			thisEvent.AddListener(listener);
			instance.eventDictionary.Add(eventName, thisEvent);
		}
	}

	public static void StopListening(EventType eventName, UnityAction<string> listener) {
		if (eventManager == null) return;
		NarrationEvent thisEvent = null;
		if (instance.eventDictionary.TryGetValue(eventName, out thisEvent)) {
			thisEvent.RemoveListener(listener);
		}
	}

	public static void TriggerEvent(EventType eventName, string data) {
		NarrationEvent thisEvent = null;
		if (instance.eventDictionary.TryGetValue(eventName, out thisEvent)) {
			thisEvent.Invoke(data);
		}
	}
}