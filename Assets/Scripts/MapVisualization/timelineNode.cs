using UnityEngine;
using System;
using System.Collections;

public class timelineNode : MonoBehaviour
{

    public DateTime date;
    public string datevalue;
    public long dateticks;
    public Vector2 location;
    public string text;
    public Vector3 baseSize;
    private float zeroRef = 0.0f;
    private Color baseColor;
    public timelineNode neighbour; //TODO: add this feature

    public void Start()
    {
        baseColor = GetComponent<SpriteRenderer>().color;
        baseSize = gameObject.GetComponent<RectTransform>().localScale;
    }

    public void ChangeSize(Vector3 final_size)
    {
        end_size = final_size;
        smooth_time = 0.2f;

        //Change Collider accordingly
        //DEBUG
        //print ("Changing node size for " + this.node_name);
        //END DEBUG
        if (gameObject.GetComponent<SpriteRenderer>().sprite != null)
        {
            float new_width = gameObject.GetComponent<SpriteRenderer>().sprite.bounds.size.x; //base_size.x;
            float new_height = gameObject.GetComponent<SpriteRenderer>().sprite.bounds.size.y; //base_size.y;
            float desired_collider_radius = Mathf.Min(new_width, new_height);
            gameObject.GetComponent<CircleCollider2D>().radius = desired_collider_radius*0.75f;
            //Begin the co-routine to change the node size.
            if (gameObject.active)
                StartCoroutine("ChangeNodeSize");
        } //end if
    } //end method ChangeNodeSize

    private float smooth_time;
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

            gameObject.GetComponent<RectTransform>().localScale =
                new Vector3(
                    Mathf.SmoothDamp(gameObject.GetComponent<RectTransform>().localScale.x, end_size.x, ref zeroRef,
                        smooth_time)
                    ,
                    Mathf.SmoothDamp(gameObject.GetComponent<RectTransform>().localScale.y, end_size.y, ref zeroRef,
                        smooth_time)
                    , gameObject.GetComponent<RectTransform>().localScale.z);
            yield return null;
        }
    }

    public bool display_info = false;
    private bool mouseover = false;
    public string text_to_display = "";

    void OnGUI()
    {
        // GUI box that follows the mouse; Display-info on right, mouseover info on left
        if (display_info)
        {
            GUI.TextArea(new Rect(Input.mousePosition.x + 15, Screen.height - Input.mousePosition.y, 200, 100), text,
                1000);
        }
        else if (mouseOver)
        {
            GUI.TextArea(new Rect(Input.mousePosition.x - 203, Screen.height - Input.mousePosition.y, 200, 100), text,
                1000);
        }
    }

    private bool mouseOver = false;

    public void OnMouseEnter()
    {
        mouseOver = true;
        ChangeSize(new Vector3(baseSize.x*2f, baseSize.y*2f, baseSize.z));
        ChangeColor(Color.cyan);
    }

    public void OnMouseExit()
    {
        mouseOver = false;
        ChangeSize(new Vector3(baseSize.x, baseSize.y, baseSize.z));
        ChangeColor(baseColor);
    }

    public void ChangeColor(Color newColor)
    {
        GetComponent<SpriteRenderer>().color = newColor;
    }

    public void OnMouseDrag()
    {
        transform.position = Camera.main.ScreenToWorldPoint((new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10)));
    }
}
