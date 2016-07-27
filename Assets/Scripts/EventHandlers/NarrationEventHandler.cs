using UnityEngine;
using UnityEngine.Events;
using System;

[Serializable]
public class NarrationEvent : UnityEvent {

}

public class NarrationEventHandler : MonoBehaviour {


	/*
	Relationship - state or show the relationship, from the knowledge base, between two nodes
	Analogy - show analogical similarities between two nodes
	Lead-In Statement - Introduce or segue into a node; the 'default' transition if nothing more interesting can be done
	Novel Lead-In - Introduce or segue into a node which is sufficiently novel/new to the user
	Hint-At - suggest briefly that information about a node will be mentioned in the future
	Tie-Back - relate to information about a node that has been mentioned in the past
	*/

	//determine what functions to call when event is invoked
	public NarrationEvent UserTurn = new NarrationEvent();
	public NarrationEvent Chronology = new NarrationEvent();
	public NarrationEvent NextNode = new NarrationEvent();


	//determine when to invoke event
	public void foo() {
		UserTurn.Invoke();
	}
}
