using UnityEngine;
using System.Collections;

public class TextBoxController : MonoBehaviour {
	public GameObject text_object;

	public string text_to_display;
	public bool display;

	public GUIStyle text_box_style;
	
	private float text_offset;

	// Use this for initialization
	void Start () 
	{
		
	}
	
	public void Initialize()
	{
		
	}//end method initialize
	
	public void setText(string text_to_set)
	{
		text_to_display = text_to_set;
		print ("text to display: " + text_to_display);
	}//end method setText

	public void setPosition(Vector3 new_position)
	{
		//gameObject.transform.position = new_position;
		//text_object.transform.position = new_position;
	}//set the text box and its text to the same position

	void OnGUI()
	{
		if (display)
		{
			GUI.Box (new Rect((Screen.width / 2) - (Screen.width / 3), Screen.height - Screen.height / 8, Screen.width / 1.5f, Screen.height / 8), text_to_display, text_box_style);
		}//end if
	}//end method onGUI

	// Update is called once per frame
	void Update () 
	{

	}
	
	void FixedUpdate()
	{
	}//end method FixedUpdate
}
