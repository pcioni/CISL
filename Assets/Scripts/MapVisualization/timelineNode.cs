using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class timelineNode : MonoBehaviour
{

	public DateTime date;
	public string datevalue;
	public long dateticks;
	public bool known_location = false;
	public Vector2 location;
	public string text;
	public Vector3 baseSize;
	private float zeroRef = 0.0f;
	private Color baseColor;
	public List<KeyValuePair<string,timelineNode>> neighbors = new List<KeyValuePair<string, timelineNode>>();//use kvp because no tuple support in unity
	public List<timelineNode> allNodes;
	public bool active = false;	//Whether this node is active and interactable.
	public float state = 0;   //What the state of this node is.
							//0 = Out of focus. Node does not respond to mouse, is transparent, and does not draw lines to neighboring nodes.
							//1 = In focus. Node responds to mouse, is the focus color (red), always displays information, and draws lines to neighboring nodes.
							//2 = Half-Focus. Node responds to mouse, is the half-focus color (white), displays information on mouse-over, and does not draw lines to neighboring nodes.
							//3 = Past-focus. Node responds to mouse, is the past-focus color (blue), displays information on mouse-over, and draws lines to neighboring nodes.


	public Vector3 timelinePosition; //where the node should be on the timeline
	public Vector3 mapPosition; //where the node should be on the map
    private Vector3 startPosition;
    private float floatOffset;
    private bool Moveable;

	private IEnumerator moveCoroutine;//reference to movement

	public void Start() {
	    Moveable = false;
        transform.Rotate(Vector3.forward * UnityEngine.Random.Range(0f, 80f)); //add some random initial rotation to offset angle from other nodes
	    floatOffset = UnityEngine.Random.Range(0f, 3f);
	    startPosition = transform.position;
		baseColor = GetComponent<SpriteRenderer>().color;
		baseSize = gameObject.GetComponent<RectTransform>().localScale;
		//drawLines(); //draw a line between every node and every neighbour.
	}

	public void moveToPosition(Vector3 position) {
		if(moveCoroutine != null) StopCoroutine(moveCoroutine);
		moveCoroutine = _move(position, 1.5f);
		StartCoroutine(moveCoroutine);
	}
	private IEnumerator _move(Vector3 position, float movetime) {
		float movespeed = 1.0f / movetime;
		while (Vector3.Distance(transform.position, position) > 0.1f) {
			transform.position = Vector3.MoveTowards(transform.position, position, movespeed);
			yield return null;
		}
	    Moveable = true;
	}

    private void Float() {
        Vector3 newPos = new Vector3(transform.position.x, startPosition.y + 0.5f * Mathf.Sin(1 * (Time.time + floatOffset)), transform.position.z);
        transform.position = newPos;
    }

    private void rotateRight() {
        transform.Rotate(Vector3.forward * -2);
    }

	void FixedUpdate() {

	    if (active && Moveable) {
            rotateRight();
	        Float();
        }
        if (Input.GetKeyDown ("return") && mouseOver) {
			//If this node is moused over and enter is pressed, focus on it.
			//If any other node is the focus, make it a past-focus
			foreach (timelineNode tn in allNodes) {
				if (tn.state == 1){
					tn.PastFocus();
				}
			}//end foreach
			//Focus on this node.
			Focus();
		}//end if
	}

	//Bring this node into focus.
	public void Focus() {
		//Mark this node as active
		active = true;
		state = 1;
		//Draw lines from this node to its neighbors
		drawLines();
		//Have it always display information
		display_info = true;
		//Change its color
		Color focus_color = Color.red; //new Color(1f, 1f, 1f, 1f);
		baseColor = focus_color;
		ChangeColor (focus_color);
		//Bring it to the front by changing its Z
		gameObject.transform.position = new Vector3(gameObject.transform.position.x
			, gameObject.transform.position.y
			, gameObject.transform.position.z - 5);

		//Bring its neighbors into half-focus if they aren't a past-focus or a focus.
		foreach (KeyValuePair<string,timelineNode> neighbor_node in this.neighbors) {
			if (neighbor_node.Value.state != 3 && neighbor_node.Value.state != 1)
				neighbor_node.Value.HalfFocus ();
		}//end foreach
	}//end method Focus

	//Half-focus this node.
	public void HalfFocus() {
		//Mark this node as active
		active = true;
		state = 2;
		//Do not have it always display information
		display_info = false;
		//Change its color
		Color half_focus_color = Color.white;
		baseColor = half_focus_color;
		ChangeColor (half_focus_color);
		//Bring it forward by changing its Z
		gameObject.transform.position = new Vector3(gameObject.transform.position.x
			, gameObject.transform.position.y
			, gameObject.transform.position.z - 3);
	}//end method HalfFocus

	//Bring this node out of full focus, but remember it used to be the focus.
	public void PastFocus() {
		//Mark this node as active
		active = true;
		state = 3;
		//Do not have it always display information
		display_info = false;
		//Change its color
		Color past_focus_color = Color.blue;
		baseColor = past_focus_color;
		ChangeColor (past_focus_color);
		//Bring it forward by changing its Z
		gameObject.transform.position = new Vector3(gameObject.transform.position.x
			, gameObject.transform.position.y
			, gameObject.transform.position.z - 3);
	}//end method PastFocus

	//Bring this node out of focus
	public void Unfocus() {
		//Mark this node as inactive
		active = false;
		state = 0;
		display_info = false;
		//Remove lines from this node to its neighbors
		eraseLines();
		//Change its color
		Color unfocus_color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
		baseColor = unfocus_color;
		ChangeColor (unfocus_color);
		//Bring it to the back by changing its Z
		gameObject.transform.position = new Vector3(gameObject.transform.position.x
			, gameObject.transform.position.y
			, gameObject.transform.position.z + 5);
	}//end method Unfocus

	private void drawLines() {
		Vector3 centralNodePos = transform.position;
		Vector3[] points = new Vector3[Math.Max(neighbors.Count * 2, 1)];
		points[0] = centralNodePos;
		int nCount = 0;
		for (int i = 1; i < points.Length; i += 2) {
			points[i - 1] = centralNodePos;
			points[i] = neighbors[nCount].Value.transform.position;
			nCount++;
		}
		LineRenderer lr = GetComponent<LineRenderer>();

		lr.SetVertexCount(points.Length);
		lr.SetPositions(points);

		lr.SetColors(Color.blue, Color.blue);
		lr.SetWidth(0.05f, 0.05f);
		lr.material = new Material(Shader.Find("Particles/Additive"));
	}

	//Remove all lines going out of this node
	private void eraseLines() {
		//LineRenderer lr = GetComponent<LineRenderer> ();
		//lr.SetVertexCount (0);
	}//end method eraseLines

	public void ChangeSize(Vector3 final_size)
	{
		end_size = final_size;
		smooth_time = 0.2f;

		//Change Collider accordingly
		if (gameObject.GetComponent<SpriteRenderer>().sprite != null)
		{
			float new_width = gameObject.GetComponent<SpriteRenderer>().sprite.bounds.size.x; //base_size.x;
			float new_height = gameObject.GetComponent<SpriteRenderer>().sprite.bounds.size.y; //base_size.y;
			float desired_collider_radius = Mathf.Min(new_width, new_height);
			gameObject.GetComponent<CircleCollider2D>().radius = desired_collider_radius*0.75f;
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

	public void OnMouseEnter() {
	    Moveable = false;
        //Only trigger mouse effects if this node is active
        if (active) {
			mouseOver = true;
			ChangeSize (new Vector3 (baseSize.x * 2f, baseSize.y * 2f, baseSize.z));
			ChangeColor (Color.cyan);
		}//end if
	}

	public void OnMouseExit() {
	    Moveable = true;
        //Only trigger mouse effects if this node is active
        if (active) {
			mouseOver = false;
			ChangeSize (new Vector3 (baseSize.x, baseSize.y, baseSize.z));
			ChangeColor (baseColor);
		}//end if
	}

	public void ChangeColor(Color newColor) {
		GetComponent<SpriteRenderer>().color = newColor;
	}

	public void OnMouseDrag() {
		//Only trigger mouse effects if this node is active
		if (active) {
			if (state == 1 || state == 3)
				drawLines ();
			//TODO: MAKE EACH NODE ONLY REDRAW THE LINE TO THIS NODE
			foreach (timelineNode tn in allNodes) {
				//Only redraw if the other node is a focus or past-focus node (states 1 or 3)
				if (tn.state == 1 || tn.state == 3) {
					tn.drawLines();
				}
			}
			transform.position = Camera.main.ScreenToWorldPoint ((new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 10)));
		}//end if
	}
}
