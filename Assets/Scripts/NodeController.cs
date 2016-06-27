using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class NodeController : MonoBehaviour 
{
	//The id of this node's feature
	public int id;

	//The location of the image for this node.
	//public string image_location;

	//This node's name
	public string node_name;

	//The main feature for this node
	public Feature node_feature;

	public GUIStyle node_style;

	public bool display;

	public bool is_focus_node;

	//Maintain an adjacency list for fast access to neighboring nodes
	private List<GameObject> adjacency_list;
	//maintain a list of the relationship of this node to each
	//adjacency list entry
	private List<string> relationship_list;

	//Save the loader so it can be interacted with.
	private GameObject loader_object;
	private GameObject graph_loader_object;

	//Save this node's text so it can be interacted with.
	public GameObject node_text_object;
	//Node also has a text object for its relationship.
	public GameObject relationship_text_object;
	//Save this node's string of additional values, used to
	//show a piece of the calculations from the backend.
	public string calculation_text = "";

	//Animation and movement variables
	private Vector3 end_position;
	private bool move_to_new;
	private float tolerance;
	private float speed;
	private float acceleration;
	private float maximum_speed;
	//The normal size of the node
	public Vector3 base_size;
	//The original size of the node
	public Vector3 original_size;

	//If the node was given a temporary position, this is where
	//the node was before moving to its temporary position.
	private Vector3 previous_position;
	//Flag that signifies whether this node is moving to a 
	//temporary position or not.
	public bool position_temporary;

	// Whether or not this node should be disabled at the end of its movement.
	public bool to_disable;

	//	Whether the node is currently fading in
	private bool fade_in;
	private float fade_speed;
	private float fade_time;
	//If we are within the slow radius, deccelerate
	private float slow_radius;
	private bool approaching_target;
	private float approach_speed;
	private float approach_acceleration;

	private bool mouseover = false;

	public bool is_held;

	//If we are drawing a line, this is the target node
	private GameObject line_target_node;

	private bool is_graph;


	//temporary -- use one date
	public DateTime date;

	//temporary -- use one coordinate
	public Vector2 location;


	// Use this for initialization
	void Start () 
	{

	}

	//Initialization function (not called automatically)
	public void Initialize()
	{
		to_disable = false;
		is_held = false;
		is_graph = false;
		//Get the loader
		GameObject[] temp_array = GameObject.FindGameObjectsWithTag ("loader");
		loader_object = temp_array [0];

		//Have this node find out its own id
		string game_object_name = gameObject.name;
		//Split the name by undersore
		string[] split_game_object_name = game_object_name.Split ('_');
		//The second index, after the underscore, is the node's id
		id = int.Parse (split_game_object_name [1]);

		end_position = new Vector3 (0, 50, 0);
		move_to_new = false;
		approaching_target = false;
		fade_in = false;
		tolerance = 3.5f;
		speed = 0.0f;
		maximum_speed = 10.0f;
		slow_radius = 0.0f;
		approach_speed = 0.0f;
		approach_acceleration = 0.0f;
		fade_speed = 1.0f;
		//Constant acceleration
		acceleration = 0.05f;
		is_focus_node = false;
		adjacency_list = new List<GameObject> ();
		relationship_list = new List<string> ();
		node_feature = new Feature ();
		display = true;

		this.GetComponent<GUIText>().enabled = false;

		original_size = gameObject.GetComponent<RectTransform> ().localScale;
		base_size = original_size;
		//gameObject.GetComponent<Text> ().enabled = true;

		//Set this game object's collision box bounds according
		//to its renderer's bounds.
		//This ensures that mousing over/clicking the image also
		//triggers the collision box.
		//gameObject.GetComponent<BoxCollider2D> ().size = gameObject.GetComponent<Renderer> ().bounds.size;
		//gameObject.GetComponent<BoxCollider2D> ().size = original_size;
		gameObject.GetComponent<BoxCollider2D> ().size = new Vector2 (4.0f, 4.0f);
	}//end method Initialize
	public void Initialize(Feature initial_feature)
	{
		to_disable = false;
		is_held = false;
		is_graph = false;
		//Get the loader
		GameObject[] temp_array = GameObject.FindGameObjectsWithTag("loader");
		loader_object = temp_array[0];

		SetFeature(initial_feature);
		//The node's id is the feature's id.
		id = initial_feature.id;
		//The node's name is the feature's name ("data" field).
		node_name = initial_feature.data;
		//The game object's name is "node_<id number>".
		gameObject.name = "node_" + initial_feature.id.ToString();

		end_position = new Vector3(0, 50, 0);
		move_to_new = false;
		approaching_target = false;
		fade_in = false;
		tolerance = 3.5f;
		speed = 0.0f;
		maximum_speed = 10.0f;
		slow_radius = 0.0f;
		approach_speed = 0.0f;
		approach_acceleration = 0.0f;
		fade_speed = 1.0f;
		//Constant acceleration
		acceleration = 0.05f;
		is_focus_node = false;
		adjacency_list = new List<GameObject>();
		relationship_list = new List<string>();
		//node_feature = new Feature();
		display = true;
		text_to_display = "";

		original_size = gameObject.GetComponent<RectTransform>().localScale;
		base_size = original_size;
		//gameObject.GetComponent<Text> ().enabled = true;

		//Set this game object's collision box bounds according
		//to its renderer's bounds.
		//This ensures that mousing over/clicking the image also
		//triggers the collision box.
		//gameObject.GetComponent<BoxCollider2D> ().size = gameObject.GetComponent<Renderer> ().bounds.size;
		//gameObject.GetComponent<BoxCollider2D> ().size = original_size;
		gameObject.GetComponent<BoxCollider2D>().size = new Vector2(4.0f, 4.0f);




		//temporary -- initialize date and location
		string tmpdate = "1";
		try {
			foreach (Timeobj to in initial_feature.timedata) {
				if (to.relationship == "birth date" || to.relationship == "years" || to.relationship == "date") {
					tmpdate = to.value;
					break;
				}
			}
		}
		catch {

		}

		try {
			//try converting to datetime
			date = Convert.ToDateTime(initial_feature.timedata[0].value);
		}
		catch {
			//else extract digits
			int year;
			if (!int.TryParse(System.Text.RegularExpressions.Regex.Match(tmpdate, @"\d+").Value, out year)){
				date = new DateTime();
			}else {
				print(year);
				date = new DateTime(year,1,1);
			}
			
		}

		try {
			location = new Vector2(initial_feature.geodata[0].lat, initial_feature.geodata[0].lon);
		}
		catch {
			location = new Vector2();
		}

		//initial_feature.geodata








	}//end method Initialize

	//Start disabling the node. The node will be fully disabled
	//once it reaches the center of the screen.
	public void StartDisableNode()
	{
		to_disable = true;
		//Move to the center of the screen
		SetEndPosition (new Vector3 (0, 0, loader_object.GetComponent<Loader>().y_bias));
		//Remove all lines from the node
		RemoveLines ();
		//Reduce the node to its original size.
		ChangeSize (original_size, true);
	}//end method StartDisableNode

	public void EnableNode()
	{
		to_disable = false;
		gameObject.SetActive (true);
		gameObject.GetComponent<SpriteRenderer> ().enabled = true;
		display = true;
		gameObject.layer = LayerMask.NameToLayer ("Default");
		ShowText ();
	}//end method EnableNode
	public void DisableNode()
	{
		to_disable = false;
		//Disable the sprite renderer.
		gameObject.GetComponent<SpriteRenderer>().enabled = false;
		display = false;
		//Make sure the node ignores raycasts so we don't activate its mouse-over
		gameObject.layer = LayerMask.NameToLayer ("Ignore Raycast");
		//Hide the node's text
		HideText ();
		//Set the node's calculation text to nothing.
		calculation_text = "";

		//Set the size to its original size
		gameObject.GetComponent<RectTransform>().localScale = new Vector3(
			original_size.x, original_size.y, original_size.z);
		//Set its position to the center (but maintain z value)
		gameObject.transform.position = new Vector3(
			0, 0, gameObject.transform.position.z);
		move_to_new = false;

		//Completely deactivate the node.
		gameObject.SetActive (false);
	}//end method DisableNode

	//The node will move to the given temporary position. Later,
	//when signalted to, it will move back to the position it had
	//before its temporary position was set.
	public void SetTemporaryPosition(Vector3 input_position)
	{
		//Check to see that the temporary position isn't within a
		//threshold of its current position or its end position.
		//Also check that this node does not already have a temporary position.
		if ((!move_to_new && Vector3.Distance (input_position, gameObject.transform.position) < tolerance)
			|| (move_to_new && Vector3.Distance (input_position, end_position) < tolerance)
			|| position_temporary)
		{
			return;
		}//end if
		//Otherwise, save the node's previous position.
		//If the node is currently moving, the previous position is its end position. 
		if (move_to_new)
			previous_position = end_position;
		//Otherwise, its previous position is where the node currently is.
		else
			previous_position = gameObject.transform.position;

		//DEBUG
		//print ("current position " + gameObject.transform.position.ToString () + " prev position " + previous_position.ToString ());
		//END DEBUG

		//Flag that this node has a temporary position.
		position_temporary = true;
		//Set a new end position
		SetEndPosition (input_position);
	}//end method SetTemporaryPosition
	//Return to previous position from this node's temporary position.
	public void ReturnFromTemporaryPosition()
	{
		if (position_temporary)
		{
			position_temporary = false;
			SetEndPosition (previous_position);
		}//end if
	}//end method ReturnFromTemporaryPosition

	public void HideText()
	{
		//node_text_object.GetComponent<MeshRenderer> ().enabled = false;
		//gameObject.GetComponent<Text> ().text = "";
		print("Hiding text for " + gameObject.name);
		node_text_object.GetComponent<GUIText>().text = " ";
		//node_text_object.GetComponent<GUIText>().enabled = false;
		text_to_display = "";
	}//end method HideText
	public void ShowText()
	{
		//node_text_object.GetComponent<MeshRenderer> ().enabled = true;
		//node_text_object.GetComponent<TextController> ().setText (node_feature.getData ());
		//gameObject.GetComponent<Text> ().text = node_feature.getData ();
		//node_text_object.GetComponent<GUIText>().text = node_feature.getData();
		
		//Set the node text to its feature's data
		node_text_object.GetComponent<GUIText>().text = NodeLabel();
		
		//node_text_object.GetComponent<GUIText>().text = node_text_object.name;
		//node_text_object.GetComponent<GUIText>().enabled = true;
		text_to_display = node_feature.getData ();
	}//end method ShowText

	//Gets the label that should be displayed on this node whenever it is active.
	private String NodeLabel()
	{
		string return_string = "";
		return_string = node_feature.getData();
		//Check for the double hash separator, which indicates that it is a multi-lingual name.
		if (return_string.Contains("##"))
		{
			if (loader_object.GetComponent<Loader>().language_text_mode == 0)
				return_string = node_feature.getData().Split(new string[] { "##" }, StringSplitOptions.None)[0];
			else if (loader_object.GetComponent<Loader>().language_text_mode == 1)
				return_string = node_feature.getData().Split(new string[] { "##" }, StringSplitOptions.None)[1];
		}//end if

		return return_string;
	}//end method NodeLabel

	public string text_to_display = "";
	void OnGUI()
	{
		if (display)
		{
			Vector3 transform_vector = new Vector3(
				gameObject.transform.position.x
				,-gameObject.transform.position.y
				,gameObject.transform.position.z);
			Vector3 screen_pos = Camera.main.WorldToScreenPoint (transform_vector);
			//The amount we offset is decided by the radius
			//Larger radius means less offset.
			float current_radius = 0;

			current_radius = loader_object.GetComponent<Loader>().current_radius;

			//Vector3 adjusted_mouse_position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Vector3 adjusted_mouse_position = Input.mousePosition;
			if (mouseover)
			{
				string output_text = "";
				if (text_to_display.Contains("##"))
				{
					if (loader_object.GetComponent<Loader> ().language_text_mode == 0)
						output_text = node_feature.getData ().Split(new string[] { "##" }, StringSplitOptions.None)[0];
					else if (loader_object.GetComponent<Loader> ().language_text_mode == 1)
						output_text = node_feature.getData ().Split(new string[] { "##" }, StringSplitOptions.None)[1];
				}//end if
				else
				{
					output_text = node_feature.getData();
				}//end else
				//When moused over, show a floating text area with the node's information.
				if (is_focus_node)
					GUI.TextArea(new Rect(adjusted_mouse_position.x, -adjusted_mouse_position.y + Camera.main.pixelHeight + 25, 200, 30), output_text, 1000);
				else
					GUI.TextArea(new Rect(adjusted_mouse_position.x, -adjusted_mouse_position.y + Camera.main.pixelHeight + 25, 200, 150), output_text + "\n" + calculation_text, 1000);
			}//end if

			if (is_graph)
				DrawLineToNeighbors();
		}//end if
	}//end method onGUI

	public void SetFeature(Feature f)
	{
		node_feature = f;
	}//end method SetFeature
	public Feature GetFeature()
	{
		return node_feature;
	}//end method GetFeature

	public void SetEndPosition(Vector3 next_end, bool next_move = false)
	{
		end_position = next_end;
		//move_to_new = next_move;
		move_to_new = true;
	}//end method SetEndPosition

	//Starts the fade-in animation for this node
	public void StartFadeIn()
	{
		fade_in = false;
	}//end method startFadeIn

	public void AddAdjacentNode(GameObject adj, string relationship)
	{
		adjacency_list.Add (adj);
		relationship_list.Add (relationship);
	}//end method AddAdjacentNode
	public List<GameObject> GetAdjacencyList()
	{
		return adjacency_list;
	}//end method GetAdjacencyList

	public void DrawLineToNeighbors()
	{
		//Draw a line to each of this node's neighbors
		foreach (GameObject temp_node in adjacency_list)
		{
			//DrawLineToNode (temp_node, relationship_list[adjacency_list.IndexOf (temp_node)]);
			DrawLineToNode (temp_node, "");
		}//end foreach
	}//end method DrawLineToNeighbors
	public void DrawLineToNode(GameObject other_node, string relationship)
	{
		//TODO: Do different colors
		line_target_node = other_node;
		Color line_color = new Color (0, 0, 0);
		int line_type = 0;
		if (relationship.Equals ("hosted"))
			line_type = 0;
		else if (relationship.Equals (""))
			line_type = 1;
		else if (relationship.Equals ("north"))
			line_type = 2;
		else if (relationship.Equals ("northeast"))
			line_type = 3;
		else if (relationship.Equals ("east"))
			line_type = 4;
		else if (relationship.Equals ("southeast"))
			line_type = 5;
		else if (relationship.Equals ("south"))
			line_type = 6;
		else if (relationship.Equals ("southwest"))
			line_type = 7;
		else if (relationship.Equals ("west"))
			line_type = 8;
		else if (relationship.Equals ("northwest"))
			line_type = 9;

		//Draw a line to the other node, with color based on the line type
		//0, White for hosted #FFFFFF rgb(255,255,255)
		//1, Black for nothing #000000 rgb(0,0,0)
		//2, Red for north hex #FF0000 rgb(255,0,0)
		//3, Orange for northeast #FFA500 rgb(255,165,0)
		//4, Yellow for east #FFFF00 rgb(255,255,0)
		//5, Green for southeast #008000 rgb(0,128,0)
		//6, Blue for south #0000FF rgb(0,0,255)
		//7, Indigo for southwest #4B0082 rgb(75,0,130)
		//8, Violet for west #800080 rgb(128,0,128)
		//9, Magenta for northwest #FF00FF rgb(255,0,255)

		/*if (line_type == 0)
			line_color = new Color (255, 255, 255);
		else if (line_type == 1)
			line_color = new Color (0, 0, 0);
		else if (line_type == 2)
			line_color = new Color (255, 0, 0);
		else if (line_type == 3)
			line_color = new Color (255, 165, 0);
		else if (line_type == 4)
			line_color = new Color (255, 255, 0);
		else if (line_type == 5)
			line_color = new Color (0, 128, 0);
		else if (line_type == 6)
			line_color = new Color (0, 0, 255);
		else if (line_type == 7)
			line_color = new Color (75, 0, 130);
		else if (line_type == 8)
			line_color = new Color (128, 0, 128);
		else if (line_type == 9)
			line_color = new Color (255, 0, 255);*/

		node_text_object.GetComponent<LineRenderer> ().SetColors (line_color, line_color);
		node_text_object.GetComponent<LineRenderer> ().SetPosition (0, gameObject.transform.position);
		node_text_object.GetComponent<LineRenderer> ().SetPosition (1, other_node.transform.position);
		//Show the relationship between this node and the other node.
		//Relationship here is relationship FROM other node TO this node
		string relationship_text = relationship;// this.relationship_list[adjacency_list.IndexOf(other_node)];
		//Relationship here is relationship FROM this node TO the other node
		/*foreach (Neighbor temp_neighbor in other_node.GetComponent<Feature>().neighbors)
		{
			if (temp_neighbor.dest == this.node_feature.getId())
			{
				relationship_text = temp_neighbor.relationship;
			}//end if
		}//end foreach*/
		//Check for the double hash separator, which indicates that it is a multi-lingual name.
		if (relationship_text.Contains("##"))
		{
			if (loader_object.GetComponent<Loader>().language_text_mode == 0)
				relationship_text = relationship_text.Split(new string[] { "##" }, StringSplitOptions.None)[0];
			else if (loader_object.GetComponent<Loader>().language_text_mode == 1)
				relationship_text = relationship_text.Split(new string[] { "##" }, StringSplitOptions.None)[1];
		}//end if
		relationship_text_object.GetComponent<GUIText>().text = relationship_text;
	}//end method DrawLineToNode
	public void RemoveLines()
	{
		node_text_object.GetComponent<LineRenderer> ().SetPosition (0, new Vector3(100, 100, 0));
		node_text_object.GetComponent<LineRenderer> ().SetPosition (1, new Vector3(100, 100, 0));
		relationship_text_object.GetComponent<GUIText>().text = "";
	}//end method RemoveLines

	// Update is called once per frame
	void Update () 
	{
		if (Input.GetMouseButtonUp (0))
			is_held = false;
	}

	void FixedUpdate()
	{
		if (is_held)
		{
			gameObject.transform.position = new Vector3 (
				Camera.main.ScreenToWorldPoint (Input.mousePosition).x
				, Camera.main.ScreenToWorldPoint (Input.mousePosition).y
				, 0); 
		}//end if

		if (is_graph)
			return;
		//If this is the focus node, initiate line-drawing with neighbor nodes
		if (is_focus_node && !loader_object.GetComponent<Loader>().single_node_mode)
		{
			foreach (GameObject neighbor_node in adjacency_list)
			{
				neighbor_node.GetComponent<NodeController>().DrawLineToNode (gameObject, relationship_list[adjacency_list.IndexOf(neighbor_node)]);
			}//end foreach
		}//end if

		//If we are moving to a new position
		if (move_to_new)
		{
			float distance = Mathf.Abs (Vector3.Distance (gameObject.transform.position, end_position));
			//Check if we are within tolerance of the end position
			if (distance < tolerance)
			{
				//If so, stop moving, we are done.
				speed = 0;
				move_to_new = false;
				approaching_target = false;
				//Snap to correct position
				gameObject.transform.position = end_position;
				/*node_text_object.transform.position = new Vector3(
					gameObject.transform.position.x
					, gameObject.transform.position.y
					, gameObject.transform.position.z);*/
				//Dsiable it if it's going to the origin
				/*if (Vector3.Distance(end_position, new Vector3(0, loader_object.GetComponent<Loader>().y_bias, 0)) < 0.01f 
					&& !is_focus_node)*/
				if (to_disable)
					DisableNode();
				else
					//FIX: Show node text
					ShowText();
			}//end if
			else
			{
				if (!approaching_target)
				{
					//Entered slow radius for the first time
					if (distance < slow_radius)
					{
						//approaching_target = true;
						//approach_speed = speed;
						//Determine what constant negative acceleration
						//we need to reach the target in a constant time.
						//approach_acceleration = -(approach_speed / distance) * 0.1f;
					}//end if
				}//end if

				if (approaching_target)
				{
					speed += approach_acceleration;
				}//end if
				else
				{
					//Otherwise, move towards the end position.
					//maximum_speed = distance * 1.0f;
					//If we are BELOW the maximum speed, accelerate
					if (speed < maximum_speed)
					{
						speed += acceleration;
					}//end if
					//Don't overshoot the maximum speed
					if (speed > maximum_speed)
						speed = maximum_speed;
				}//end if
				//Now, get the direction in which we are moving
				Vector3 direction = end_position - gameObject.transform.position;
				direction = direction.normalized;
				//Move the node in the direction specified at the node's current speed.
				Vector3 next_position = gameObject.transform.position + direction * speed;
				gameObject.transform.position = next_position;
				//node_text_object.transform.position = gameObject.transform.position;
				/*node_text_object.transform.position = new Vector3(
					gameObject.transform.position.x
					, gameObject.transform.position.y
					, gameObject.transform.position.z);*/
			}//end else
		}//end if
		 /*else
		 {
			 //If the node is not moving, but is at the origin and is not the focus node, then disable it.
			 if (Vector3.Distance(end_position, new Vector3(0, loader_object.GetComponent<Loader>().y_bias, 0)) < 0.01f 
				 && !is_focus_node)
				 DisableNode();
		 }//end else*/
		 //Node text should be in the same position as this node
		node_text_object.transform.position = new Vector3(
			gameObject.transform.position.x
			, gameObject.transform.position.y
			, gameObject.transform.position.z - 1);

		Vector3 node_screen_point = Camera.main.WorldToViewportPoint(new Vector3(
			gameObject.transform.position.x
			, gameObject.transform.position.y
			, gameObject.transform.position.z));

		node_text_object.GetComponent<RectTransform>().position = new Vector3(
			node_screen_point.x
			, node_screen_point.y
			, 0);

		if (line_target_node != null)
		{
			Vector3 target_screen_point = Camera.main.WorldToViewportPoint(new Vector3(
				line_target_node.transform.position.x
				, line_target_node.transform.position.y
				, line_target_node.transform.position.z));

			relationship_text_object.transform.position = new Vector3(
				node_screen_point.x - (node_screen_point.x - target_screen_point.x) / 2
				, node_screen_point.y - (node_screen_point.y - target_screen_point.y) / 2
				, node_screen_point.z - (node_screen_point.z - target_screen_point.z) / 2);
		}//end if

		//If we are performing a fade-in animation
		if (fade_in)
		{
			//Set the node at its end position if it is still distant
			if (Mathf.Abs (Vector3.Distance (gameObject.transform.position, end_position)) > 0.01f)
				gameObject.transform.position = new Vector3(
					end_position.x
					, end_position.y
					, end_position.z);
			//SmoothDamp(start, end, speed, time);
			//TODO: Fade Time needs tracking of start and end times
			float fade = Mathf.SmoothStep (0f, 1f, fade_time);
			gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, fade);
		}//end if
	}//end method FixedUpdate

	//Public method to change the size of the node to the given Vector3 end-size
	public void ChangeSize(Vector3 final_size, bool change_base_size)
	{
		//Set the base size to the final size.
		if (change_base_size)
			base_size = final_size;
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
			float new_width = gameObject.GetComponent<SpriteRenderer> ().sprite.bounds.size.x;//base_size.x;
			float new_height = gameObject.GetComponent<SpriteRenderer> ().sprite.bounds.size.y;//base_size.y;
			float desired_collider_radius = Mathf.Min (new_width, new_height);
			gameObject.GetComponent<CircleCollider2D> ().radius = desired_collider_radius*0.75f;
			//Begin the co-routine to change the node size.
			if (gameObject.active)
				StartCoroutine("ChangeNodeSize");
		}//end if
	}//end method ChangeNodeSize

	private float smooth_time;
	private float x_velocity;
	private float y_velocity;
	private Vector3 end_size;
	//A co-routine to change the size of the node gradually.
	IEnumerator ChangeNodeSize()
	{
		float distance_to_final = Vector3.Distance (gameObject.GetComponent<RectTransform> ().localScale, end_size);
		//Loop until we reach some threshold from the end size
		while (distance_to_final > 0.01f)
		{
			distance_to_final = Vector3.Distance (gameObject.GetComponent<RectTransform> ().localScale, end_size);
			//redundant end condition check
			if (distance_to_final <= 0.01f)
			{
				//Snap to correct size
				gameObject.GetComponent<RectTransform> ().localScale = end_size;
				//End co-routine
				break;
			}//end if

			gameObject.GetComponent<RectTransform> ().localScale = new Vector3(
				Mathf.SmoothDamp (
					gameObject.GetComponent<RectTransform> ().localScale.x
					, end_size.x
					, ref x_velocity
					, smooth_time)
				, Mathf.SmoothDamp (
					gameObject.GetComponent<RectTransform> ().localScale.y
					, end_size.y
					, ref y_velocity
					, smooth_time)
				, gameObject.GetComponent<RectTransform> ().localScale.z);
			yield return null;
		}//end while

		/*
		 * public class ExampleClass : MonoBehaviour {
	public Transform target;
	public float smoothTime = 0.3F;
	private float yVelocity = 0.0F;
	void Update() {
		float newPosition = Mathf.SmoothDamp(transform.position.y, target.position.y, ref yVelocity, smoothTime);
		transform.position = new Vector3(transform.position.x, newPosition, transform.position.z);
	}
}*/
	}//end method GrowNode

	void OnMouseDown()
	{
		print ("Mouse down on node " + name);
		is_held = true;
		//if (is_graph)
			//graph_loader_object.GetComponent<GraphLoader> ().SetFocusNode (id, true);
		if (!is_graph)
		{
			is_focus_node = true;
			loader_object.GetComponent<Loader> ().SetFocusNode (id, true);
		}//end else
	}//end method onMouseDown

	void OnMouseEnter()
	{
		mouseover = true;
		//When moused over, increase the size of the node
		//slightly to emphasize that it is focused upon.
		//Increase the size of the node.
		if (!is_graph)
			ChangeSize (new Vector3 (
				base_size.x * 1.5f
				, base_size.y * 1.5f
				, base_size.z)
				, false);

	}//end method OnMouseOver
	void OnMouseExit()
	{
		//is_held = false;
		mouseover = false;
		//Now that the node is no longer the focus, decrease its size
		//back to normal.
		//Decrease the size of the node back to its base size
		if (!is_graph)
			ChangeSize (new Vector3(
				base_size.x
				, base_size.y
				, base_size.z)
				, false);

	}//end method OnMouseExit

}//end class NodeController