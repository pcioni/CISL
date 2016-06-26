using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HighlightWithMouseOver : MonoBehaviour {

	Color theColor;

	void Awake(){
		theColor=GetComponent<UnityEngine.UI.Text>().color;
	}

	public void Highlight(){
		GetComponent<UnityEngine.UI.Text>().color= UnityEngine.Color.black;
		//gameObject.GetComponent<Text> ().color = 0;

	}

	public void Unhighlight(){
		GetComponent<UnityEngine.UI.Text> ().color = theColor;
	}

}
