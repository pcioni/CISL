using UnityEngine;
using System;
using System.Collections;

public class timelineNode : MonoBehaviour {

	public DateTime date;
	public string datevalue;
	public long dateticks;
	public Vector2 location;
	public string text;

    public bool display_info = false;
    private bool mouseover = false;
    public string text_to_display = "";
    void OnGUI()
    {
        if (display_info)
        {
            Vector3 transform_vector = new Vector3(
                gameObject.transform.position.x
                , -gameObject.transform.position.y
                , gameObject.transform.position.z);
            Vector3 screen_pos = Camera.main.WorldToScreenPoint(transform_vector);

            string output_text = text;
            //Show a floating text area with the node's information.
            GUI.TextArea(new Rect(screen_pos.x, screen_pos.y, 200, 100), output_text, 1000);
        }//end if
    }//end method onGUI

}
