﻿using UnityEngine;
using System;
using System.Collections;

public class timelineNode : MonoBehaviour {

	public DateTime date;
	public string datevalue;
	public long dateticks;
	public Vector2 location;
	public string text;
    public Vector3 baseSize;

    public void Start()
    {
        baseSize = gameObject.GetComponent<RectTransform>().localScale;
    }

    public void ChangeSize(Vector3 final_size, bool change_base_size)
    {
        //Set the end_size to the final_size, too.
        end_size = final_size;
        //Initialize smooth time, x velocity, y velocity
        smooth_time = 0.2f;
        x_velocity = 0.0f;
        y_velocity = 0.0f;

        //Change Collider accordingly
        //DEBUG
        //print ("Changing node size for " + this.node_name);
        //END DEBUG
        if (gameObject.GetComponent<SpriteRenderer>().sprite != null)
        {
            float new_width = gameObject.GetComponent<SpriteRenderer>().sprite.bounds.size.x;//base_size.x;
            float new_height = gameObject.GetComponent<SpriteRenderer>().sprite.bounds.size.y;//base_size.y;
            float desired_collider_radius = Mathf.Min(new_width, new_height);
            gameObject.GetComponent<CircleCollider2D>().radius = desired_collider_radius * 0.75f;
            //Begin the co-routine to change the node size.
            if (gameObject.active)
                StartCoroutine("ChangeNodeSize");
        }//end if
    }//end method ChangeNodeSize

    private float smooth_time;
    private float x_velocity;
    private float y_velocity;
    private Vector3 end_size;
    IEnumerator ChangeNodeSize()
    {
        float distance_to_final = Vector3.Distance(gameObject.GetComponent<RectTransform>().localScale, end_size);
        while (distance_to_final > 0.01f)
        {
            distance_to_final = Vector3.Distance(gameObject.GetComponent<RectTransform>().localScale, end_size);
            if (distance_to_final <= 0.01f)
            {
                gameObject.GetComponent<RectTransform>().localScale = end_size;
                break;
            }

            gameObject.GetComponent<RectTransform>().localScale = new Vector3(
                Mathf.SmoothDamp(
                    gameObject.GetComponent<RectTransform>().localScale.x
                    , end_size.x
                    , ref x_velocity
                    , smooth_time)
                , Mathf.SmoothDamp(
                    gameObject.GetComponent<RectTransform>().localScale.y
                    , end_size.y
                    , ref y_velocity
                    , smooth_time)
                , gameObject.GetComponent<RectTransform>().localScale.z);
            yield return null;
        }
    }

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

    public void OnMouseEnter()
    {
            ChangeSize(new Vector3(baseSize.x * 1.5f, baseSize.y * 1.5f, baseSize.z), false);
    }
    public void OnMouseExit()
    {
            ChangeSize(new Vector3(baseSize.x, baseSize.y, baseSize.z), false);
    }
}
