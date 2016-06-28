using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Xml; 
using System.Xml.Serialization; 
using System.IO;
using System.Text;
using System.Diagnostics;
//using System.Speech.Synthesis;

using System.Text.RegularExpressions;

public class Loader : MonoBehaviour {

	//The location of an XML file 
	//private string xml_location = "Assets/xml/monsters_xml.xml";
	//private string xml_location = "Assets/xml/test_xml.xml";
	public string xml_location = "";
	//private string xml_location = "Assets/xml/10.xml";
	public GameObject node_prefab;

	public GameObject message_passer;

	public GameObject node_text_prefab;

	public GameObject text_box_object;

	public GameObject additional_info_text_box_object;

	public GameObject query_field;

	//The height and width of each node, to which
	//pictures will be normalized
	public float node_height;
	public float node_width;
	//The ratio by which we increase the size of the center node
	public float center_multiplier;
	//How much the focus node and its neighbors are biased in the y direction
	public float y_bias;

	//Store all the node controllers for easy access to them
	private List<NodeController> node_controllers;
	//Store all the nodes
	private List<GameObject> nodes;
	//Store a history of previously visited nodes.
	//The TAIL will store the most recently visited node.
	//When the user presses the "go back" button or key, we will
	//pop the tail and make it the focus node.
	//Otherwise, whenever we set a focus node, we add the node
	//to the tail of the list.
	private List<GameObject> node_history;

	//Maintain a list of the nodes to be shown on the side panel
	private List<GameObject> side_panel_nodes;

	//Maintain a list of nodes that are currently active.
	public List<GameObject> active_nodes;

	private bool tell_focus_change;

	//Store all the organizational frames
	private List<GameObject> organizational_frames;

	//The node that we are currently focusing on
	public GameObject focus_node;

	private string xml_contents = "";

	//Mouse scrolling variables
	Vector3 last_mouse_position;
	public GameObject main_camera;
	public float pan_sensitivity;

	//Camera resizing variables
	private float target_camera_size;
	private float camera_size_speed;
	public float current_radius;

	public GUISkin system_skin;

	public Texture2D dividers_texture;
	public Texture2D next_best_label_texture;
	public Texture2D most_novel_label_texture;

	//Whether or not this should run in single node mode.
	//If true, then only the focus node and dialogue box
	//will be shown. If false, then the focus node, its
	//neighbors, the Next Best node list, the Novel
	//node list, and the weight sliders will be shown.
	public bool single_node_mode;

	//Test array
	private GameObject[] initial_nodes;

	private bool first_load;

	// Use this for initialization
	public void Initialize () 
	{
		first_load = true;
		//Find the Main Camera game object
		main_camera = GameObject.Find ("Main Camera");
		//Find the message passer game object
		message_passer = GameObject.Find ("message_passer");

		nodes = new List<GameObject> ();
		focus_node = null;
		organizational_frames = new List<GameObject> ();
		node_history = new List<GameObject> ();

		active_nodes = new List<GameObject> ();

		stringToEdit = "";
		tell_focus_change = true;

		side_panel_nodes = new List<GameObject> ();

		initial_nodes = GameObject.FindGameObjectsWithTag("node");

		GameObject[] org_frames = GameObject.FindGameObjectsWithTag ("organization");

		//Go through each organizational frame we found
		foreach (GameObject temp_frame in org_frames)
		{
			//Hide them
			temp_frame.GetComponent<SpriteRenderer>().enabled = false;

			//Place them at 0, 0, 0
			temp_frame.transform.position = new Vector3(0, 0, 0);

			//Add them to the list of organizational frames
			organizational_frames.Add (temp_frame);
		}//end foreach

		//Go through each node already in the scene
		foreach (GameObject initial_node in initial_nodes)
		{
			//Initialize the node
			initial_node.GetComponent<NodeController>().Initialize ();
			//Disable every node
			initial_node.SetActive (false);
			//Hide the node
			//initial_node.GetComponent<SpriteRenderer>().enabled = false;
			//Add the node to the list of nodes
			nodes.Add (initial_node);
		}//end foreach

		LoadXml ();
		///StartCoroutine (TextToSpeech (speak_text));
		//string return_message = message_passer.GetComponent<SocketListener>().ReceiveDataFromServer();
		//print ("Message from server: " + return_message);
		//Parse the message from the server
		//parseMessage (return_message, 0);

		//Initialize the weight display.
		weight_names [0] = "Previously Addressed";
		weight_names [1] = "Novelty";
		weight_names [2] = "Spatial Constraint";
		weight_names [3] = "Hierarchy Constraint";

		weight_slider_values = new float[4];
		previous_slider_values = new float[4];
		//Get the initial weights from the backend.
		print ("Message to server: " + "SET_WEIGHT:-1:-1");
		message_passer.GetComponent<SocketListener>().sendMessageToServer("SET_WEIGHT:-1:-1");
		string return_message = message_passer.GetComponent<SocketListener>().ReceiveDataFromServer();
		print ("Reponse to set weight: " + return_message);
		
		//Begin co-routine to check for weight updates.
		StartCoroutine ("MaybeUpdateWeights");
		
		language_text_mode = 0;
		language_speech_mode = 0;
		//Begin co-routine to check for language mode updates.
		StartCoroutine ("MaybeUpdateLanguage");
		
		ParseMessageWeights(return_message, true);
		
		//Initially, set previous slider values to weight slider values
		for (int k = 0; k < weight_slider_values.Length; k++)
		{
			previous_slider_values[k] = weight_slider_values[k];
		}//end for
	}//end Initialize
	
	void OnApplicationQuit()
	{
		//When the application quits, send a QUIT message to the backend.
		//This allows the backend to shut down its server gracefully.
		message_passer.GetComponent<SocketListener> ().sendMessageToServer ("QUIT");
	}//end method OnApplicationQuit

	public string stringToEdit = "";
	public string weight_string;
	public string speak_text = "";
	public string tts_text = "";
	public bool english_recognition = true;
	public bool chinese_recognition = false;
	public bool english_tts = true;
	public bool chinese_tts = false;
	public bool male_voice = true;
	public bool female_voice = false;
	public string text_to_speech = "hello";
	//An array of slider values; one for each weight.    
	//Currently there are four weights.
	public float[] weight_slider_values = new float[4];
	public float[] previous_slider_values = new float[4];
	public string[] weight_names = new string[4];
	bool previous_single_node_toggle = false;
	void OnGUI() 
	{
		GUI.skin = system_skin;

		Vector3 radius_screen_point_1 = Camera.main.WorldToScreenPoint (new Vector3 (current_radius * 1.55f, 0, 0));
		Vector3 radius_screen_point_2 = Camera.main.WorldToScreenPoint (new Vector3 (current_radius * 2.35f, 0, 0));

		//Draw the side panel node dividers
		if (!single_node_mode)
		{
		
			GUI.DrawTexture(new Rect(radius_screen_point_1.x, -50.0f, 30.0f, Screen.height * 1.2f), dividers_texture);
			/*GUI.DrawTexture (new Rect (radius_screen_point_2.x, -50.0f, 30.0f, Screen.height * 1.2f), dividers_texture);

			//Draw next best node label
			GUI.DrawTexture (new Rect (radius_screen_point_1.x + ((radius_screen_point_2.x - radius_screen_point_1.x) - 200) / 2
									  , 0 //Camera.main.pixelHeight
									  , 200
									  , 50)
							 , next_best_label_texture);*/
			//Draw the most novel node label
			GUI.DrawTexture (new Rect (radius_screen_point_2.x + ((radius_screen_point_2.x - radius_screen_point_1.x) - 200) / 2
									   , 0 //Camera.main.pixelHeight
									   , 200
									   , 50)
							 , most_novel_label_texture);

			//hSliderValue = GUI.HorizontalSlider(new Rect(25, 25, 100, 30), hSliderValue, 0.0F, 10.0F);
			for (int i = 0; i < weight_slider_values.Length; i++)
			{
				weight_slider_values[i] = GUI.HorizontalSlider (new Rect(25, 335 + i * (30 + 30), 225, 25), weight_slider_values[i], -100.0f, 100.0f);
				GUI.TextArea (new Rect (25, 305 + i * (30 + 30), 250, 25), weight_names[i] + ": " + weight_slider_values[i].ToString ("F4"));
			}//end for
			//Make sure the weight string matches the slider values.
			SetWeightStringToValues();
		}//end if

		//Single node toggle
		single_node_mode = GUI.Toggle(new Rect(10, 10, 100, 30), single_node_mode, "Single-Node View");
		if (single_node_mode != previous_single_node_toggle)
		{
			//If we have changed the toggle, reset all nodes.
			previous_single_node_toggle = single_node_mode;
			ResetNodes ();
	
			if (!single_node_mode)
			{
				//If we are now in full display mode, reset the focus node
				//to get neighbor and side panel nodes back
				//SetFocusNode(focus_node.GetComponent<NodeController>().node_feature.id, false, "", true);
			}//end if
		}//end if

		//Input text area for user.
		float response_area_width = Camera.main.pixelWidth / 4;
		float response_area_x = Camera.main.pixelWidth / 2 - response_area_width;

		float response_area_height = Camera.main.pixelHeight / 4;
		float response_area_y = Camera.main.pixelHeight - response_area_height;

		//if (stringToEdit.Equals (""))
		//	stringToEdit = "Please continue.";
		stringToEdit = GUI.TextArea(new Rect(25, 225, 225, 50), stringToEdit, 1000);
		//stringToEdit = GUI.TextArea(new Rect(response_area_x + response_area_width, response_area_y, response_area_width, response_area_height), stringToEdit, 1000);
		if (GUI.Button (new Rect (25, 280, 60, 20), "Submit"))
		{
			if (stringToEdit.Equals("Please continue."))
				stringToEdit = "";
			PlayerPrefs.SetString("Text Area", stringToEdit);
			print ("Query: " + stringToEdit);

			//Check for a set weight command
			string[] split_query = stringToEdit.Split (':');
			bool message_parsed = false;
			string return_message = "";
			if (split_query.Length != 0)
			{
				if (split_query[0].Equals ("SET_WEIGHT"))
				{
					//Parse the output as a new set of weights to display

					message_passer.GetComponent<SocketListener>().sendMessageToServer(stringToEdit);
					return_message = message_passer.GetComponent<SocketListener>().ReceiveDataFromServer();
					print ("Reponse to set weight: " + return_message);

					weight_string = ParseMessageWeights(return_message, false);
					//ParseMessageWeights (return_message, false);

					message_parsed = true;
				}//end if
			}//end if

			//If no special case parsed the message
			if (!message_parsed)
			{
				message_passer.GetComponent<SocketListener>().sendMessageToServer(stringToEdit);
				return_message = message_passer.GetComponent<SocketListener>().ReceiveDataFromServer();
				print ("Message from server: " + return_message);
				//Parse the message from the server
				parseMessage (return_message, 0);
			}//end if

			//Get a speak value from the message and place it in the text box
			string new_speak = parseMessageForSpeak(return_message);
			//text_box_object.GetComponent<TextBoxController> ().setText (new_speak);
			speak_text = new_speak;
			//StartCoroutine (TextToSpeech (speak_text));
		
			stringToEdit = "Please continue.";
		}//end if;
		//Show response text from server
		//GUI.TextArea(new Rect(response_area_x, response_area_y, response_area_width, response_area_height), speak_text, 1000);
		GUI.TextArea (new Rect (25, 25, 225, 200), speak_text, 1000);

		if(GUI.Toggle (new Rect (275, 50, 150, 25), english_recognition, "English Text")){
			english_recognition = true;
			chinese_recognition = false;
		}
		if(GUI.Toggle (new Rect (275, 75, 150, 25), chinese_recognition, "Chinese Text")){
			english_recognition = false;
			chinese_recognition = true;
		}
		if(GUI.Toggle (new Rect (425, 50, 150, 25), english_tts, "English Speech")){
			english_tts = true;
			chinese_tts = false;
		}
		if(GUI.Toggle (new Rect (425, 75, 150, 25), chinese_tts, "Chinese Speech")){
			english_tts = false;
			chinese_tts = true;
		}

		/*if(GUI.Toggle (new Rect (275, 100, 225, 25), male_voice, "Male voice")){
			male_voice = true;
			female_voice = false;
		}
		if(GUI.Toggle (new Rect (275, 125, 225, 25), female_voice, "Female voice")){
			male_voice = false;
			female_voice = true;
		}*/
	/*
		// Speech to text
		if (GUI.Button (new Rect (95, 280, 120, 20), "Start Speaking")) {
			print ("pressed start speaking button");
			message_passer.GetComponent<SocketListener>().sendMessageToServer("Start Recording");
		}
		if (GUI.Button (new Rect (215, 280, 120, 20), "Stop Speaking")) {
			print ("pressed stop speaking button");
			if(english_recognition){
				message_passer.GetComponent<SocketListener>().sendMessageToServer("Stop Recording:english");
			}
			else{
				message_passer.GetComponent<SocketListener>().sendMessageToServer("Stop Recording:chinese");
			}
			stringToEdit = message_passer.GetComponent<SocketListener>().ReceiveDataFromServer();
		}



		//tts_text = GUI.TextArea (new Rect (275, 150, 225, 50), tts_text, 1000);

		// Test Chinese/English TTS
		/*if( GUI.Button (new Rect (275, 200, 225, 25), "TTS")){
			string tts_info = "TTS#";
			print ("pressed tts button");
			if(english_tts){
				tts_info += "english#";
			}
			else{
				tts_info += "chinese#";
			}
			if(male_voice){
				tts_info += "male#";
			}
			else{
				tts_info += "female#";
			}
			tts_info += tts_text;
			message_passer.GetComponent<SocketListener>().sendMessageToServer(tts_info);
			message_passer.GetComponent<SocketListener>().ReceiveDataFromServer();
		}*/
	
	}//end OnGUI
	
	IEnumerator TextToSpeech (string tts_text = "")
	{
		Process p = new Process ();

		//We are starting a python script.
		p.StartInfo.FileName = "python";
		//The first command line argument is the name of the script itself.
		//The second command line argument is the text to be spoken.
		print ("tts_text = " + tts_text);
		if (tts_text.Equals (""))
			p.StartInfo.Arguments = "text_to_speech.pyw \"" + text_to_speech + "\"";
		else
			p.StartInfo.Arguments = "text_to_speech.pyw \"" + tts_text + "\"";


		//p.StartInfo.RedirectStandardError = true;
		//p.StartInfo.RedirectStandardOutput = true;
		p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
		p.StartInfo.CreateNoWindow = true;

		//This points to the directory where the script is.
		p.StartInfo.WorkingDirectory = Application.dataPath + "/Scripts/TextToSpeech/";
		//p.StartInfo.UseShellExecute = false;

		//Start the script.
		p.Start ();

		//UnityEngine.Debug.Log (p.StandardOutput.ReadToEnd ());
		//TODO: Put this in the yield statement or something?
		//Close this process once it has exited.
		//p.WaitForExit ();
		//p.Close ();

		//VoiceSpeaker.Say ("hello");
		/*SpeechSynthesizer synthesizer = new SpeechSynthesizer();

		synthesizer.Volume = 100;
		synthesizer.Rate = -2;

		synthesizer.Speak ("Hello World");*/

		yield return null;
		// Remove the "spaces" in excess
		/*Regex rgx = new Regex ("\\s+");
		// Replace the "spaces" with "% 20" for the link Can be interpreted
		string result = rgx.Replace (text_to_speech, "%20");
		string url = "http://translate.google.com/translate_tts?tl=en&q=" + result; // + ".mp3";

		//DEBUG
		print ("sending url " + url);

		WWW www = new WWW (url);
		yield return www;
		AudioClip return_clip = www.GetAudioClip(false, false, AudioType.MPEG);
		//AudioClip return_clip = www.audioClip;

		print ("return audio length " + return_clip.length);*/
	}

	//Make the weight string match the values of the weight sliders
	private void SetWeightStringToValues()
	{
		weight_string = "Weights";
		weight_string += "\nDiscuss Amount: " + weight_slider_values[0].ToString ("F4");
		weight_string += "\nNovelty: " + weight_slider_values [1].ToString ("F4");
		weight_string += "\nSpatial Constraint: " + weight_slider_values [2].ToString ("F4");
		weight_string += "\nHierarchy Constraint: " + weight_slider_values [3].ToString ("F4");
	}//end method setWeightStringToValues

	//Parse a string of weight values from the backend and return
	//a formatted string for display.
	//If reflect_weights is TRUE, then we set the weight slider values to the
	//weights from the message.
	private string ParseMessageWeights(string weight_string, bool reflect_weights)
	{
		string return_string = "";

		print ("Weight string: " + weight_string);

		string[] split_weight_string = weight_string.Split (':');

		return_string = split_weight_string [0];
		//The indices are as follows:
		//  1 - discuss amount weight
		//  2 - novelty weight
		//  3 - spatial constraint weight
		//  4 - hierarchy constraint weight
		return_string += "\nDiscuss Amount: " + split_weight_string[1];
		return_string += "\nNovelty: " + split_weight_string [2];
		return_string += "\nSpatial Constraint: " + split_weight_string [3];
		return_string += "\nHierarchy Constraint: " + split_weight_string [4];

		if (reflect_weights)
		{
			//Place the values into the local array of weight slider values.
			for (int i = 0; i < weight_slider_values.Length; i++)
			{
				weight_slider_values[i] = float.Parse (split_weight_string[i + 1]);
			}//end for
			SetWeightStringToValues ();
		}//end if

		return return_string;
	}//end method ParseMessageWeights

	//Parse a string of colon-separated calculation values
	//from the backend and return a string formatted for display.
	private string ParseMessageCalculations(string calculation_values)
	{
		//Calculation value indices are as follows:
		//0 = score
		//1 = novelty
		//2 = discussed amount
		//3 = expected dramatic value
		//4 = spatial constraint value
		//5 = hierarchy constraint value

		string[] split_values = calculation_values.Split (':');

		//Check that the split string has at least five values. If not, return an empty string.
		if (split_values.Length < 5)
						return "";

		//Create a formatted output string
		string return_string = "";
		return_string += "Score: " + split_values [0] + "\n";
		return_string += "Novelty: " + split_values [1] + "\n";
		return_string += "Discussed Amount: " + split_values [2] + "\n";
		return_string += "Expected Dramatic Value: " + split_values [3] + "\n";
		return_string += "Spatial Constraint Value: " + split_values [4] + "\n";
		return_string += "Hierarchy Constraint Value: " + split_values [5] + "\n";

		return return_string;
	}//end method ParseMessageCalculations

	//Load the XML file
	private void LoadXml()
	{
		XmlSerializer serializer = new XmlSerializer (typeof(AIMind));
		FileStream stream = new FileStream (xml_location, FileMode.Open);
		AIMind container = (AIMind)serializer.Deserialize (stream);
		stream.Close ();
		print ("stream closed");
		GameObject new_node = null;
		//int node_counter = 0;
		//int row_counter = 0;
		print ("Root: " + container.root.id);
		foreach (Feature f in container.features) 
		{
			if (f.speak != null)
				//if (f.speak.value != null)
					//print ("speak: " + f.speak.value);
			foreach (Neighbor n in f.neighbors)
			{
				//print ("neighbor dest: " + n.dest + " weight: " + n.weight);
			}//end foreach

			bool existing_node_found = false;
			//Look for an existing node that shares this feature's id
			foreach (GameObject temp_node in nodes)
			{
				if (f.id == temp_node.GetComponent<NodeController>().id)
				{
					//Set the node's feature to be this feature.
					temp_node.GetComponent<NodeController>().SetFeature (f);
					existing_node_found = true;
				}//end if
			}//end foreach
			//If no existing node is found, make a new node with this feature's id and information.
			if (!existing_node_found)
			{
				new_node = Instantiate(node_prefab);
				new_node.GetComponent<NodeController>().Initialize(f);
				new_node.SetActive(false);
				nodes.Add(new_node);
			}//end if
		}//end foreach

		//Compile the adjacency lists for each node and create the node text for each node.
		foreach (GameObject temp_node in nodes)
		{
			//Create a new node text game object
			GameObject new_node_text = (GameObject)Instantiate (node_text_prefab, new Vector3(0, 0, 0), Quaternion.identity);
			GameObject new_relationship_text = (GameObject)Instantiate(node_text_prefab, new Vector3(0, 0, 0), Quaternion.identity);
			new_node_text.name = temp_node.name + "_text";
			new_relationship_text.name = temp_node.name + "_relationship_text";
			//Give it to this node
			temp_node.GetComponent<NodeController>().node_text_object = new_node_text;
			temp_node.GetComponent<NodeController>().relationship_text_object = new_relationship_text;

			Feature temp_feature = temp_node.GetComponent<NodeController>().GetFeature();
			List<int> neighbor_ids = new List<int>();
			//Go through each neighbor in temp_node
			foreach (Neighbor temp_neighbor in temp_feature.neighbors)
			{
				//Go through all the other nodes until you find the node
				//whose id matches this neighbor's id
				foreach (GameObject potential_neighbor in nodes)
				{
					if (potential_neighbor.GetComponent<NodeController>().id == temp_neighbor.dest)
					{
						//Add this node's id to the neighbor id list
						neighbor_ids.Add(temp_neighbor.dest);
						//Add this potential neighbor to the node's adjacency list.
						temp_node.GetComponent<NodeController>().AddAdjacentNode(potential_neighbor
																				 , temp_neighbor.relationship);
					}//end if
				}//end foreach
			}//end foreach
			//Go through each parent in temp_node
			foreach (Parent temp_parent in temp_feature.parents)
			{
				//Check to see if the id is already captured as a neighbor
				if (neighbor_ids.Contains(temp_parent.dest))
				{
					//If so, skip this parent
					continue;
				}//end if
				//If not, go through all other nodes
				//until you find the node whose id
				//matches this parent's id
				foreach (GameObject potential_parent in nodes)
				{
					if (potential_parent.GetComponent<NodeController>().id == temp_parent.dest)
					{
						print ("Adding parent " + potential_parent.GetComponent<NodeController>().name);
						//Add this potential neighbor to the node's adjacency list.
						temp_node.GetComponent<NodeController>().AddAdjacentNode(potential_parent
																				 ,temp_parent.relationship);
					}//end if
				}//end foreach
			}//end foreach

		}//end foreach

		ScaleNodeImages ();

		//The default node we focus on is the node whose id matches
		//the id specified in the root XML element's id attribute.
		SetFocusNode (container.root.id, true);
		//SetFocusNode (31);
	}//end method LoadXml

	IEnumerator MaybeUpdateWeights()
	{
		while (true)
		{
			bool weights_changed = false;
			//Check to see if any of the weights have changed
			for (int i = 0; i < weight_slider_values.Length; i++)
			{
				if (weight_slider_values[i] != previous_slider_values[i])
					weights_changed = true;
				previous_slider_values[i] = weight_slider_values[i];
			}//end for

			//If any of the weights have changed, send set weight commands for every weight.
			if (weights_changed)
			{
				//Build the message
				string message_to_send = "SET_WEIGHT";
				for (int i = 0; i < weight_slider_values.Length; i++)
				{
					message_to_send += ":" + i + ":" + weight_slider_values[i];
				}//end for
				//Send the message.
				print ("Message to server: " + message_to_send);
				message_passer.GetComponent<SocketListener>().sendMessageToServer(message_to_send);
				string return_message = message_passer.GetComponent<SocketListener>().ReceiveDataFromServer();
				print ("Reponse to set weight: " + return_message);
				weight_string = ParseMessageWeights(return_message, false);

				//ZEV: Put this back in later! 5/25/2016
				SetSidePanelNodes(false);
			}//end if

			//Wait for some amount of time between checks
			yield return new WaitForSeconds(0.5f);
		}//end while
	}//end method MaybeUpdateWeights

	public int language_text_mode;
	public int language_speech_mode;
	//Checks to see if the language mode has been changed, so 
	//the backend may be informed of the new language mode.
	IEnumerator MaybeUpdateLanguage()
	{
		while (true)
		{
			bool language_changed = false;
			//print ("previous language mode: " + previous_language_mode);
			//Check to see if the language mode has changed
			if (english_recognition && language_text_mode != 0)
			{
				language_changed = true;
				language_text_mode = 0;
			}//end if
			else if (chinese_recognition && language_text_mode != 1)
			{
				language_changed = true;
				language_text_mode = 1;
			}//end else if

			if (english_tts && language_speech_mode != 0)
			{
				language_changed = true;
				language_speech_mode = 0;
			}//end if
			else if (chinese_tts && language_speech_mode != 1)
			{
				language_changed = true;
				language_speech_mode = 1;
			}//end else if

			//If the language has changed, send a SET_LANGUAGE command
			if (language_changed)
			{
				//Build the message
				string message_to_send = "SET_LANGUAGE";
				message_to_send += ":" + language_text_mode + ":" + language_speech_mode;
				//Send the message.
				print ("Message to server: " + message_to_send);
				message_passer.GetComponent<SocketListener>().sendMessageToServer(message_to_send);
				string return_message = message_passer.GetComponent<SocketListener>().ReceiveDataFromServer();
				print ("Reponse to set language: " + return_message);
			}//end if
			//Wait for some amount of time between checks
			yield return new WaitForSeconds(0.5f);
		}//end while
	}//end method MaybeUpdateLanguage

	//TODO: Finish this?
	//Scale the images of each node
	private void ScaleNodeImages()
	{
		//Go through each node
		foreach (GameObject temp_node in nodes)
		{
			//Get their rectangle
			RectTransform temp_transform = temp_node.GetComponent<RectTransform>();
		}//end foreach
	}//end method ScaleNodeImages

	void FixedUpdate()
	{
		float camera_size_tolerance = 0.1f;
		//float camera_size_speed = 0.5f;
		float current_camera_size = 0.0f;
		//If we have a non-0 target camera size
		if (target_camera_size != 0)
		{
			current_camera_size = main_camera.GetComponent<Camera>().orthographicSize;
			//If our camera is not currently the target camera size
			if (current_camera_size != target_camera_size)
			{
				//Adjust the camera size.
				//First, check if we are within the threshold
				if (Mathf.Abs (current_camera_size - target_camera_size) < camera_size_tolerance)
				{
					//If so, just set the camera size to the target camera size
					main_camera.GetComponent<Camera>().orthographicSize = target_camera_size;
					//and stop.
				}//end if
				else
				{
					//Otherwise, adjust the camera's size in the right direction
					//according to the camera size speed.
					//main_camera.GetComponent<Camera>().orthographicSize = (target_camera_size - current_camera_size) / 2;
					if (current_camera_size < target_camera_size)
						main_camera.GetComponent<Camera>().orthographicSize = current_camera_size + camera_size_speed;
					else if (current_camera_size > target_camera_size)
						main_camera.GetComponent<Camera>().orthographicSize = current_camera_size - camera_size_speed;
				}//end else
			}//end if
		}//end if
	}//end method FixedUpdate

	// Update is called once per frame
	void Update () 
	{
		//Pan camera with mouse
		if (Input.GetMouseButtonDown(1))
		{
			last_mouse_position = Input.mousePosition;
		}//end if
		if (Input.GetMouseButton (1))
		{
			Vector3 delta = last_mouse_position - Input.mousePosition;
			main_camera.transform.Translate (delta.x * pan_sensitivity, delta.y * pan_sensitivity, 0);
			last_mouse_position = Input.mousePosition;
		}//end if
		//If the user presses space
		if (Input.GetKeyDown ("space"))
		{
			print ("Space pressed, sending message");
			message_passer.GetComponent<SocketListener>().sendMessageToServer("");
			string return_message = message_passer.GetComponent<SocketListener>().ReceiveDataFromServer();
			print ("Message from server: " + return_message);
			//Parse the message from the server
			parseMessage (return_message, 0);
		}//end if
		//Return to previous node with backspace
		if (Input.GetKeyDown ("backspace"))
		{
			print ("Returning to previous node");
			ReturnToPrevious();
		}//end if
		//Enter a query with return key
		/*if (Input.GetKeyDown(KeyCode.Return))
		{
			print ("Query: " + stringToEdit);
			message_passer.GetComponent<SocketListener>().sendMessageToServer(stringToEdit);
			string return_message = message_passer.GetComponent<SocketListener>().ReceiveDataFromServer();
			print ("Message from server: " + return_message);
			//Parse the message from the server
			parseMessage (return_message, 0);
			//Blank the entry field text
			stringToEdit = "";
		}//end if*/

		//Call the reset function with the F5 key
		if (Input.GetKey (KeyCode.F5)) 
		{
			ResetNodes();
		}//end if

		//Quit the program with the escape key
		if (Input.GetKey(KeyCode.Escape)) 
		{ 
			//Send quit message to server
			message_passer.GetComponent<SocketListener>().sendMessageToServer("QUIT");
			Application.Quit(); 
		}//end if
	}//end method Update

	//TODO: Get rid of these functions
	//Message types are as follows:
	//	0: next node to traverse to according to algorithm
	//	1: new node to display on side panel
	public void parseMessage(string message, int message_type)
	{
		//Separate by ":"

		int next_focus_node_id = 0;
		string[] separated_message = message.Split (':');

		if (message_type == 0)
		{
			//First, check that the split message is long enough.
			if (separated_message.Length < 2)
				//If not, ignore it
				return;
			//The SECOND index of separated_message should contain the next node id
			next_focus_node_id = int.Parse (separated_message [1]);
			//tell_focus_change = false;
			tell_focus_change = true;
			SetFocusNode (next_focus_node_id, true, message);
		}//end if
	}//end method parseMessage

	private List<int> parseMessageForDistantNodes(string message)
	{
		List <int> return_list = new List<int> ();
		//If it's a null message, don't bother trying to split it
		if (message.Equals (""))
			return return_list;
		string[] separated_message = message.Split (':');
		string[] separated_ids = separated_message [5].ToString ().Split (' ');
		if (separated_ids.Length <= 0)
			return return_list;

		print ("Side panel node id's: ");
		foreach (string ele in separated_ids)
		{
			print (separated_ids[0]);
			return_list.Add (int.Parse (ele));
		}//end foreach

		return return_list;
	}//end method parseMessageForDistantNodes
	private string parseMessageForAdditionalInfo(string message)
	{
		string return_string = "";
		if (message.Equals (""))
			return return_string;
		string[] separated_message = message.Split (':');

		return return_string;
	}//end method parseMessageForAdditionalInfo

	//Just get the speak value from the message
	private string parseMessageForSpeak(string message)
	{
		string return_string = "";
		if (message.Equals (""))
		{
			return return_string;
		}//end if
		string[] separated_message = message.Split (':');
		//The 3rd index is the speak value
		//First, make sure that it exists
		if (separated_message.Length >= 4)
			return separated_message [3];
		else
			//If the index does not exist, assume a pure
			//response from the backend.
			return message;
	}//end method parseMessageForSpeak
	//Get the five most proximal nodes from the message
	private int[] parseMessageForSideNodes(string message, string keyword)
	{
		//There will always be exactly 5 members
		int[] return_ids = new int[5];

		//If there is no message, don't try to parse it
		if (message.Equals (""))
		{
			return null;
		}//end if
		string[] separated_message = message.Split (':');

		//Find the index of the keyword "Novelty."
		//The list of novelty values are in the index after it.
		int starting_index = 0;
		for (int i = 0; i < separated_message.Length; i++)
		{
			if (separated_message[i].Equals(keyword))
				starting_index = i + 1;
		}//end for

		//Get the list of proximal nodes
		string string_node_list = separated_message [starting_index];
		//Split again by space
		string[] separated_node_list = string_node_list.Split (' ');

		//Make sure the separated node list is populated. If not, return null.
		if (separated_node_list.Length <= 0)
			return null;

		//Go through each pair and get the FIRST member
		for (int i = 0; i < separated_node_list.Length - 1; i++)
		{
			//Ignore ODD entries, those are the calculated values
			if ((i + 1) % 2 == 0)
				continue;
			//Each EVEN entry is a node ID
			return_ids[i / 2] = int.Parse(separated_node_list[i]);
		}//end for

		return return_ids;
	}//end method parseMessageForSideNodes

	IEnumerator ServerTTS()
	{
		try
		{
			message_passer.GetComponent<SocketListener>().sendMessageToServer("BEGIN_TTS");
			string return_message = "";

			return_message = message_passer.GetComponent<SocketListener>().ReceiveDataFromServer();
			print ("Message from server: " + return_message);
		}
		catch (IOException e)
		{
			print ("Exception: " + e);
		}//end catch

		yield return null;
	}//end method ServerTTS

	string additional_info = "";
	//If reset, then we are "changing" focus node to the current focus node.
	public void SetFocusNode(int node_id, bool add_to_history, string input_message = "")
	{
		//First, check that the focus not is not already this node id.
		if (focus_node != null) {
			if (focus_node.GetComponent<NodeController> ().id == node_id) {
				print ("Already focus node");
				return;
			}//end if
			//Decrease the size of the current focus node
			/*focus_node.GetComponent<NodeController>().base_size = new Vector3(
				focus_node.GetComponent<NodeController>().base_size.x / center_multiplier
				, focus_node.GetComponent<NodeController>().base_size.y / center_multiplier
				, focus_node.GetComponent<NodeController>().base_size.z);*/
			focus_node.GetComponent<NodeController> ().base_size = focus_node.GetComponent<NodeController> ().original_size;
			focus_node.GetComponent<RectTransform> ().localScale = focus_node.GetComponent<NodeController> ().base_size;
			//Tell the current focus node that it is no longer the focus node.
			focus_node.GetComponent<NodeController> ().is_focus_node = false;
		}//end if

		//First, set each active node to be deactivated.
		if (active_nodes.Count > 0)
		{
			foreach (GameObject temp_node in active_nodes)
				temp_node.GetComponent<NodeController>().StartDisableNode();

			//Empty the active nodes list
			active_nodes.Clear();
		}//end if

		//Go through all the nodes and find the focus node
		foreach (GameObject temp_node in nodes)
		{
			if (temp_node.GetComponent<NodeController>().id == node_id)
			{
				focus_node = temp_node;
			}//end if
		}//end foreach

		//Enable the focus node
		focus_node.GetComponent<NodeController> ().EnableNode ();

		//Tell the next focus node that it IS the focus node
		focus_node.GetComponent<NodeController> ().is_focus_node = true;

		//Increase the size of the focus node
		focus_node.GetComponent<NodeController>().base_size = new Vector3(
			focus_node.GetComponent<NodeController>().original_size.x * center_multiplier
			, focus_node.GetComponent<NodeController>().original_size.y * center_multiplier
			, focus_node.GetComponent<NodeController>().original_size.z);

		//Add the focus node to the list of active nodes
		active_nodes.Add (focus_node);

		//If we are adding this to our history, add the focus node.
		if (add_to_history)
			node_history.Add (focus_node);

		//Send a message that we have changed focus nodes
		//If there's an input message when change focus node was called, use it instead.
		string return_message = "";
		if (input_message.Equals(""))
		{
			print ("Focus node changed, sending message: " + focus_node.GetComponent<NodeController>().node_feature.data);
			if (tell_focus_change)
			{
				try
				{
					message_passer.GetComponent<SocketListener>().sendMessageToServer(focus_node.GetComponent<NodeController>().node_feature.data);
					return_message = message_passer.GetComponent<SocketListener>().ReceiveDataFromServer();
					print ("Message from server: " + return_message);
				}
				catch (IOException e)
				{
					print ("Exception: " + e);
				}//end catch
			}//end if
			else if (!tell_focus_change)
				tell_focus_change = true;
		}//end if
		else
			return_message = input_message;

		additional_info = parseMessageForSpeak (return_message);
		if (additional_info.Equals(""))
			additional_info = "";

		///StartCoroutine (TextToSpeech (additional_info));

		//additional_info_text_box_object.GetComponent<AdditionalInfoTextBoxController> ().setText ();


		int neighbor_counter = 0;
		int total_number_of_neighbors = 0;
		
		float radius = 10.0f;
		float minimum_radius = 15.0f;
		float theta = 0;
		float temp_x = 0;
		float temp_y = 0;

		focus_node.GetComponent<RectTransform>().localScale = focus_node.GetComponent<NodeController>().base_size;
		
		//Send the focus node to its final position
		focus_node.GetComponent<NodeController> ().SetEndPosition (new Vector3 (0, y_bias, 0), false);

		if (!additional_info.Equals (""))
			speak_text = additional_info;

		if (!single_node_mode)
		{
			//Go through all of the focus node's adjacent nodes
			total_number_of_neighbors = focus_node.GetComponent<NodeController>().GetAdjacencyList().Count;
			//Set the radius according to the number of neighbors we'll have to fit
			radius += total_number_of_neighbors * 2.0f;
			radius *= 1.5f;
			if (radius < minimum_radius)
				radius = minimum_radius;
			current_radius = radius;

			//Adjust the camera according to the radius
			//Set a new target size for the camera
			target_camera_size = radius * 1.5f;
			camera_size_speed = Mathf.Abs (main_camera.GetComponent<Camera> ().orthographicSize - target_camera_size) / 50.0f;

			//text_box_object.GetComponent<TextBoxController> ().setText (additional_info);
			//text_to_speech = speak_text;
		//StartCoroutine (TextToSpeech (speak_text));

			//print ("Total number of neighbors: " + total_number_of_neighbors);
			foreach (GameObject neighbor in focus_node.GetComponent<NodeController>().GetAdjacencyList()) {
				//Set their position relative to the focus node.
				theta = (2 * Mathf.PI / total_number_of_neighbors) * neighbor_counter;
			
				temp_x = radius * Mathf.Cos (theta);// + focus_node.transform.position.x;
				temp_y = radius * Mathf.Sin (theta) + y_bias;// + focus_node.transform.position.y;

				//Enable the neighbor
				neighbor.GetComponent<NodeController> ().EnableNode ();
				//Add the neighbor to the list of active nodes
				active_nodes.Add (neighbor);

				neighbor.GetComponent<NodeController> ().SetEndPosition (new Vector3 (temp_x, temp_y, focus_node.transform.position.z), true);

				//Send a message to the server requesting information about this neighbor.
				//The current node is the NEIGHBOR. The old node is the FOCUS.
				try {
					string neighbor_message = "GET_NODE_VALUES:";
					neighbor_message += neighbor.GetComponent<NodeController> ().node_feature.data + ":";
					neighbor_message += focus_node.GetComponent<NodeController> ().node_feature.data;

					print ("Message to server: " + neighbor_message);
					message_passer.GetComponent<SocketListener> ().sendMessageToServer (neighbor_message);
					string neighbor_return_message = message_passer.GetComponent<SocketListener> ().ReceiveDataFromServer ();
					//print ("Message from server: " + return_message);
					neighbor.GetComponent<NodeController> ().calculation_text = ParseMessageCalculations (neighbor_return_message);
				} catch (IOException e) {
					print ("Exception: " + e);
				}//end catch

				neighbor_counter += 1;
			}//end foreach
		}//end if
	
		stringToEdit = "Please continue.";

		
		if (!single_node_mode)
			//From the message, find what nodes belong in the side panel and set them there.
			SetSidePanelNodes (true);
		
		string return_message_tts = "";
		if (language_speech_mode == 1)
			StartCoroutine ("ServerTTS");
		else if (language_speech_mode == 0)
		{
			//Send fetch_tts message to c#
			try
			{
				message_passer.GetComponent<SocketListener>().sendMessageToServer("GET_TTS");
				return_message_tts = "";
				
				return_message_tts = message_passer.GetComponent<SocketListener>().ReceiveDataFromServer();
				print ("Message from server: " + return_message_tts);
			}
			catch (IOException e)
			{
				print ("Exception: " + e);
			}//end catch
			//FIX: Python keeps crashing, enable this call later.
			//StartCoroutine (TextToSpeech(return_message_tts));
		}//end else if
	}//end method SetFocusNode

	//Based on a message from the backend, find and place the side-panel nodes.
	//Indicate whether or not this is called when the focus node is changed.
	public void SetSidePanelNodes(bool focus_changed)
	{
		//Build a message requesting new side panel nodes.
		string message_to_send = "GET_RELATED:";
		//Send the message.
		print ("Message to server: " + message_to_send);
		message_passer.GetComponent<SocketListener>().sendMessageToServer(message_to_send);
		string return_message = message_passer.GetComponent<SocketListener>().ReceiveDataFromServer();
		print ("Reponse to get related: " + return_message);

		//Parse the message from the server and get a list of novel nodes.
		int[] novel_node_ids = parseMessageForSideNodes (return_message, "Novelty");
		//Parse the message from the server and get a list of proximal nodes.
		int[] proximal_node_ids = parseMessageForSideNodes (return_message, "Proximal");

		//If there are previous side-panel nodes, disable them.
		if (side_panel_nodes.Count > 0)
		{
			foreach (GameObject temp_node in side_panel_nodes)
			{
				//First, check that the temp_node is not an existing active node (a neighbor or the focus node).
				//If so, skip it; it already has a new end position and we don't want to reset it.
				//if (active_nodes.Contains(temp_node))
				//	continue;


				//If the focus node hasn't changed and this is a neighbor of the focus node,
				//don't disable the node. Instead, try to send it back to its
				//temporary position and return it to its original size.
				if (//focus_node.GetComponent<NodeController>().GetAdjacencyList().Contains(temp_node)
					active_nodes.Contains (temp_node)
					&& !focus_changed)
				{
					temp_node.GetComponent<NodeController>().ReturnFromTemporaryPosition();
					temp_node.GetComponent<NodeController>().ChangeSize (new Vector3(
						temp_node.GetComponent<NodeController>().original_size.x
						, temp_node.GetComponent<NodeController>().original_size.y
						, temp_node.GetComponent<NodeController>().original_size.z)
																		 , true);
				}//end if
				//Don't disable it if it's a current active node (focus node or a neighbor)
				else if (!active_nodes.Contains (temp_node))
					temp_node.GetComponent<NodeController>().StartDisableNode();

				temp_node.GetComponent<NodeController>().position_temporary = false;
			}//end foreach
		}//end if

		//print ("SSPN Radius: " + input_radius + " target_camera_size " + target_camera_size);

		//Remove all old side-panel nodes.
		side_panel_nodes.Clear();
		
		float side_panel_x = current_radius * 2.8f;
		float side_panel_y = current_radius * 1.0f;

		//float side_panel_y = Camera.main.pixelHeight;
		//Check novel nodes.
		if (!(novel_node_ids == null))
		{
			foreach (int side_panel_id in novel_node_ids)
			{
				//Find the node corresponding to this id
				foreach (GameObject temp_node in nodes)
				{
					if (temp_node.GetComponent<NodeController>().id.Equals (side_panel_id))
					{
						//This node is the side panel node
						//Check to see if its a neighbor of the focus node.
						//If so, skip it.
						//if (focus_node.GetComponent<NodeController>().GetAdjacencyList().Contains(temp_node))
						//	break;
						//If this is the focus node, skip it.
						if (focus_node.GetComponent<NodeController>().id.Equals (side_panel_id))
							break;
						
						//Enable the node
						temp_node.GetComponent<NodeController>().EnableNode();
						
						//Set its end position to the side panel position
						temp_node.GetComponent<NodeController>().SetEndPosition(new Vector3(
							side_panel_x, side_panel_y, 0), false);
						//Begin changing the node's size according to the target camera size
						temp_node.GetComponent<NodeController>().ChangeSize (new Vector3(
							temp_node.GetComponent<NodeController>().original_size.x * target_camera_size / 40
							, temp_node.GetComponent<NodeController>().original_size.y * target_camera_size / 40
							, temp_node.GetComponent<NodeController>().original_size.z)
																			 , true);
						//Increase the y spacing such that they are evenly spaced
						//down the length of the screen
						side_panel_y -= (target_camera_size- (current_radius * 1.25f - current_radius * 1.0f)) * 2 / 5;
						//Add it to the list of active side-panel nodes
						side_panel_nodes.Add (temp_node);
						break;
					}//end if
				}//end foreach
			}//end foreach
		}//end if

		//ZEV:Put this back later! 5/25/2016
		/*
		side_panel_x = current_radius * 2.0f;
		side_panel_y = current_radius * 1.0f;
		//Check proximal nodes.
		if (!(proximal_node_ids == null))
		{
			foreach (int side_panel_id in proximal_node_ids)
			{
				//Find the node corresponding to this id
				foreach (GameObject temp_node in nodes)
				{
					if (temp_node.GetComponent<NodeController>().id.Equals (side_panel_id))
					{
						//
						//If this is the focus node, skip it.
						if (focus_node.GetComponent<NodeController>().id.Equals (side_panel_id))
							break;

						//Enable the node
						temp_node.GetComponent<NodeController>().EnableNode();

						//If this node is a neighbor of the focus node, give it a temporary position
						//instead of an end position.
						if (focus_node.GetComponent<NodeController>().GetAdjacencyList().Contains(temp_node))
							temp_node.GetComponent<NodeController>().SetTemporaryPosition(new Vector3(side_panel_x, side_panel_y, 0));
						else
							//Otherwise, set its end position to the side panel position
							temp_node.GetComponent<NodeController>().SetEndPosition(new Vector3(
								side_panel_x, side_panel_y, 0), false);

						//Begin changing the node's size according to the target camera size
						temp_node.GetComponent<NodeController>().ChangeSize (new Vector3(
							temp_node.GetComponent<NodeController>().original_size.x * target_camera_size / 40
							, temp_node.GetComponent<NodeController>().original_size.y * target_camera_size / 40
							, temp_node.GetComponent<NodeController>().original_size.z)
																			 , true);
						//Increase the y spacing such that they are evenly spaced
						//down the length of the screen
						side_panel_y -= (target_camera_size - (current_radius * 1.25f - current_radius * 1.0f)) * 2 / 5;
						//Add it to the list of active side-panel nodes
						side_panel_nodes.Add (temp_node);
						break;
					}//end if
				}//end foreach
			}//end foreach
		}//end if
		*/
		foreach (GameObject temp_node in side_panel_nodes) 
		{
			if (!active_nodes.Contains(temp_node))
				active_nodes.Add (temp_node);
		}//end foreach
	}//end method SetSidePanelNodes

	private void ReturnToPrevious()
	{
		//If there is nothing in the history list, don't try to pop anything.
		if (node_history.Count == 0)
			return;

		//If there is only ONE node in the history list, and it IS the focus node,
		//then don't pop anything.
		if (node_history.Count == 1
			&& node_history [node_history.Count - 1].GetComponent<NodeController> ().id == focus_node.GetComponent<NodeController> ().id)
			return;

		//pop the end of the history list
		GameObject previous_node = node_history [node_history.Count - 1];
		node_history.RemoveAt (node_history.Count - 1);
		//If previous node IS the current focus node, pop again.
		if (previous_node.GetComponent<NodeController>().id == focus_node.GetComponent<NodeController>().id)
		{
			previous_node = node_history[node_history.Count - 1];
			node_history.RemoveAt (node_history.Count - 1);
		}//end if

		//Make this node the focus node
		tell_focus_change = true;
		SetFocusNode (previous_node.GetComponent<NodeController> ().id, true);
	}//end method ReturnToPrevious

	private void ResetNodes()
	{
		print ("reset nodes");
		foreach (GameObject node in nodes)
		{
			if (focus_node.Equals (node))
				continue;
			if (!node.GetComponent<NodeController>().enabled)
				continue;
			node.GetComponent<NodeController>().StartDisableNode();
		}//end foreach
		active_nodes.Add (focus_node);
		if (!single_node_mode)
		{
			//Find neighbors for focus node
			int neighbor_counter = 0;
			int total_number_of_neighbors = 0;
			
			float radius = 10.0f;
			float minimum_radius = 15.0f;
			float theta = 0;
			float temp_x = 0;
			float temp_y = 0;
			
			focus_node.GetComponent<RectTransform>().localScale = focus_node.GetComponent<NodeController>().base_size;
			
			//Send the focus node to its final position
			//focus_node.GetComponent<NodeController> ().SetEndPosition (new Vector3 (0, y_bias, 0), false);

			//Go through all of the focus node's adjacent nodes
			total_number_of_neighbors = focus_node.GetComponent<NodeController>().GetAdjacencyList().Count;
			//Set the radius according to the number of neighbors we'll have to fit
			radius += total_number_of_neighbors * 2.0f;
			radius *= 1.5f;
			if (radius < minimum_radius)
				radius = minimum_radius;
			current_radius = radius;
			
			//Adjust the camera according to the radius
			//Set a new target size for the camera
			target_camera_size = radius * 1.5f;
			camera_size_speed = Mathf.Abs (main_camera.GetComponent<Camera> ().orthographicSize - target_camera_size) / 50.0f;
			
			//text_box_object.GetComponent<TextBoxController> ().setText (additional_info);
			if (!additional_info.Equals (""))
				speak_text = additional_info;
			//text_to_speech = speak_text;
			//StartCoroutine (TextToSpeech (speak_text));
			
			//print ("Total number of neighbors: " + total_number_of_neighbors);
			foreach (GameObject neighbor in focus_node.GetComponent<NodeController>().GetAdjacencyList()) {
				//Set their position relative to the focus node.
				theta = (2 * Mathf.PI / total_number_of_neighbors) * neighbor_counter;
				
				temp_x = radius * Mathf.Cos (theta);// + focus_node.transform.position.x;
				temp_y = radius * Mathf.Sin (theta) + y_bias;// + focus_node.transform.position.y;
				
				//Enable the neighbor
				neighbor.GetComponent<NodeController> ().EnableNode ();
				//Add the neighbor to the list of active nodes
				active_nodes.Add (neighbor);
				
				neighbor.GetComponent<NodeController> ().SetEndPosition (new Vector3 (temp_x, temp_y, focus_node.transform.position.z), true);
				
				//Send a message to the server requesting information about this neighbor.
				//The current node is the NEIGHBOR. The old node is the FOCUS.
				try {
					string neighbor_message = "GET_NODE_VALUES:";
					neighbor_message += neighbor.GetComponent<NodeController> ().node_feature.data + ":";
					neighbor_message += focus_node.GetComponent<NodeController> ().node_feature.data;
					
					print ("Message to server: " + neighbor_message);
					message_passer.GetComponent<SocketListener> ().sendMessageToServer (neighbor_message);
					string neighbor_return_message = message_passer.GetComponent<SocketListener> ().ReceiveDataFromServer ();
					//print ("Message from server: " + return_message);
					neighbor.GetComponent<NodeController> ().calculation_text = ParseMessageCalculations (neighbor_return_message);
				} catch (IOException e) {
					print ("Exception: " + e);
				}//end catch
				
				neighbor_counter += 1;
			}//end foreach

			//ZEV: Put this back in later! 5/25/2016
			SetSidePanelNodes (false);
		}//end if
	}//end method ResetNodes

	public void DisableNonActiveNodes()
	{
		//Go through all nodes
		foreach (GameObject temp_node in nodes)
		{
			//Check each node to see if it's on the active list.
			if (active_nodes.Contains(temp_node))
			{
				//If so, don't disable it
				continue;
			}//end if
			//Otherwise, disable it.
			else
			{
				temp_node.GetComponent<NodeController>().DisableNode();
			}//end else
		}//end foreach
	}//end method ClearnOnActiveNodes
}
