using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using JsonConstructs;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;



public class NarrationJournal : MonoBehaviour {

	public Camera main_camera;

	public Texture2D default_image;
	public Text journaltext;
	public Text pagenumber;
	public List<RawImage> imageboxes;
	private UnityAction<string> listener;
	private UnityAction<string> listener2;
	public List<Text> imageLabels;
	private List<List<Texture2D>> prev_images;

	public AudioClip clip_17;
	public AudioClip clip_22;
	public AudioClip clip_41;
	//public List<AudioClip> voice_clips;
	public Dictionary<int, AudioClip> audio_clips_by_id;

	public GameObject ui1;
	public GameObject ui2;

	public int current_page = 0;

	public List<string> entries;

	private int numtoget; //the number of images to get to fill the boxes

	public string cachePath;

	private TextToSpeechWatson TTS;

	void Awake() {
		audio_clips_by_id = new Dictionary<int, AudioClip> ();
		audio_clips_by_id.Add (17, clip_17);
		audio_clips_by_id.Add (22, clip_22);
		audio_clips_by_id.Add (41, clip_41);

		cachePath = Path.Combine(Application.dataPath, "cache");

		TTS = GetComponent<TextToSpeechWatson>();

		if (!Directory.Exists(cachePath)) Directory.CreateDirectory(cachePath);

		entries = new List<string>();
		prev_images = new List<List<Texture2D>>();
		listener = delegate (string data) {
			NodeData nd = JsonUtility.FromJson<NodeData>(data);
			add_entry(nd.text);
			load_image(nd.imgUrl, nd.imgLabel);

			// Stop any ongoing audio.
			if (main_camera.GetComponent<AudioSource>().isPlaying)
				main_camera.GetComponent<AudioSource>().Stop();
			TTS.StopSpeaking();

			// Before reading the text, check to see if the node has an associated mp3 file.
			string potential_file_name = Application.dataPath + "/Resources/voice_files/" + nd.id.ToString() + ".mp3";
			if (System.IO.File.Exists(potential_file_name))
			{
				Debug.Log("NarrationJournal.NARRATION_MACHINE_TURN listener() :: mp3 voice file found");
				Vector3 audio_source_location = main_camera.transform.position;
				Debug.Log("NarrationJournal.NARRATION_MACHINE_TURN listener() :: playing clip for " + nd.id.ToString());
				AudioClip clip_to_play = audio_clips_by_id[nd.id];
				main_camera.GetComponent<AudioSource>().PlayOneShot(clip_to_play, 1.5f);
			}//end if
			else
				readText(nd.text);
			ui1.SetActive(true);
			ui2.SetActive(false);
		};

		listener2 = delegate (string data) {
			NodeData nd = JsonUtility.FromJson<NodeData>(data);
			set_page(nd);
		};


		numtoget = imageboxes.Count;
	}

	void Start() {
		EventManager.StartListening(EventManager.EventType.NARRATION_MACHINE_TURN, listener);
		EventManager.StartListening(EventManager.EventType.INTERFACE_NODE_SELECT, listener2);
	}

	public void page_left() {
		if(current_page > 0) current_page--;
		if(entries.Count > 0) {
			journaltext.text = entries[current_page];
		}
		for(int i=0; i < imageboxes.Count; i++) {
			imageboxes[i].texture = prev_images[current_page][i];
		}
		pagenumber.text = (current_page + 1) + "/" + entries.Count;
		EventManager.TriggerEvent(EventManager.EventType.INTERFACE_PAGE_LEFT,
								  current_page.ToString());
		ui1.SetActive(true);
		ui2.SetActive(false);
	}

	public void page_right() {
		if (current_page < entries.Count-1) current_page++;
		if (entries.Count > 0) {
			journaltext.text = entries[current_page];
		}
		for (int i = 0; i < imageboxes.Count; i++) {
			imageboxes[i].texture = prev_images[current_page][i];
		}
		pagenumber.text = (current_page + 1) + "/" + entries.Count;
		EventManager.TriggerEvent(EventManager.EventType.INTERFACE_PAGE_RIGHT,
								  current_page.ToString());
		ui1.SetActive(true);
		ui2.SetActive(false);
	}

	public void add_entry(string data) {
		entries.Add(data);
		current_page = entries.Count-1;
		pagenumber.text = (current_page + 1) + "/" + entries.Count;
		journaltext.text = entries[current_page];
	}



	private IEnumerator curload;
	public void load_image(List<string> urls, List<string> labels, bool save=true) {
		//set the current image to display
		//read from cache or load from url
		prev_images.Add(Enumerable.Repeat(default_image, numtoget).ToList());


		//ensure images for one node are loaded at a time
		if (curload != null) StopCoroutine(curload);
		curload = _load_images(urls, prev_images.Count - 1, labels, save);
		StartCoroutine(curload);
	}

	void cache_image(string filePath, byte[] data) {
		//Debug.Log("caching image: " + filePath);
		File.WriteAllBytes(filePath, data);
	}

	IEnumerator _load_images(List<string> urls, int index, List<string> labels, bool save) {
		//try to get several valid images from the list, otherwise use default

		//start out by assigning default image

		for (int i=0; i < numtoget; i++) {
			imageboxes[i].texture = default_image;
		}

		int numfound = 0;

		foreach (string url in urls) {
			if (numfound >= numtoget) {
				break;
			}

			WWW www;
			string urlHash = string.Format("{0:X}", url.GetHashCode());
			string filePath = Path.Combine(cachePath, urlHash);


			bool cached = false;
			if (File.Exists(filePath)) {//if file in cache, load it
				//Debug.Log("loading image from cache: " + filePath);
				string pathforwww = "file://" + filePath;
				www = new WWW(pathforwww);
				cached = true;
			}
			else {
				www = new WWW(url);
			}

			//for demo purposes
			string[] parsed_url = url.Split(':');
			if (parsed_url[0].Equals("resource")) {
				Texture2D texture = Resources.Load(parsed_url[1]) as Texture2D;
				yield return null;
				if (texture != null && texture.width > 8 && texture.height > 8) {
					if (save) {//store images in list
						prev_images[index][numfound] = texture;
					}
					imageboxes[numfound].texture = texture;
					if (numfound < imageLabels.Count) { //conditional here for now since there are not necessarily label boxes for every image
														//in the future, this should not be necessary
						imageLabels[numfound].text = labels[numfound];
					}
					numfound++;
					if (!cached) cache_image(filePath, texture.EncodeToPNG());
				}
				continue;
			}

			using (www) {
				yield return www;
				if (string.IsNullOrEmpty(www.error)) {
					Texture2D texture = www.texture;
					yield return null;
					if (texture != null && texture.width > 8 && texture.height > 8) {
						if (save) {//store images in list
							prev_images[index][numfound] = texture;
						}
						imageboxes[numfound].texture = texture;
						if (numfound < imageLabels.Count) { //conditional here for now since there are not necessarily label boxes for every image
															//in the future, this should not be necessary
							imageLabels[numfound].text = labels[numfound];
						}
						numfound++;
						if (!cached) cache_image(filePath, texture.EncodeToPNG());
					}
				}
			}
		}
	}

	void set_page(NodeData nd) {
		//sets the current journal page to some arbitrary node
		//does not affect narration history
		journaltext.text = nd.text;
		load_image(nd.imgUrl, nd.imgLabel, false);
		ui1.SetActive(false);
		ui2.SetActive(true);
	}

	public void reset_page() {
		//resets the journal page to the current narration
		journaltext.text = entries[current_page];
		for (int i = 0; i < imageboxes.Count; i++) {
			imageboxes[i].texture = prev_images[current_page][i];
		}
		pagenumber.text = (current_page + 1) + "/" + entries.Count;
		ui1.SetActive(true);
		ui2.SetActive(false);

	}


	public void readText(string data){
		//remove rich text tags
		string result = Regex.Replace(data, @"<[^>]*>", string.Empty);

		TTS.TextToSynth = result;
		TTS.Speak();
	}	
}
