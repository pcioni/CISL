using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using JsonConstructs;
using System.Linq;


public class NarrationJournal : MonoBehaviour {

	public Texture2D default_image;
	public Text journaltext;
	public Text pagenumber;
	public List<RawImage> imageboxes;
	private UnityAction<string> listener;

	private List<List<Texture2D>> prev_images;

	public int current_page = 0;

	public List<string> entries;

	private int numtoget; //the number of images to get to fill the boxes

	void Awake() {
		entries = new List<string>();
		prev_images = new List<List<Texture2D>>();
		listener = delegate (string data) {
			DataConstruct1 dc1 = JsonUtility.FromJson<DataConstruct1>(data);
			add_entry(dc1.text);
			load_image(dc1.imgurls);
		};

		numtoget = imageboxes.Count;
	}

	void Start() {
		EventManager.StartListening(EventManager.EventType.NARRATION_MACHINE_TURN, listener);
	}

	public void page_left() {
		if(current_page > 0) current_page--;
		if(entries.Count > 0) {
			journaltext.text = entries[current_page];
		}
		if (current_page < prev_images.Count) {
			for(int i=0; i < imageboxes.Count; i++) {
				imageboxes[i].texture = prev_images[current_page][i];
			}
		}
		pagenumber.text = (current_page + 1) + "/" + entries.Count;
		EventManager.TriggerEvent(EventManager.EventType.INTERFACE_PAGE_LEFT,
			                      current_page.ToString());
	}

	public void page_right() {
		if (current_page < entries.Count-1) current_page++;
		if (entries.Count > 0) {
			journaltext.text = entries[current_page];
		}
		if (current_page < prev_images.Count) {
			for (int i = 0; i < imageboxes.Count; i++) {
				imageboxes[i].texture = prev_images[current_page][i];
			}
		}

		pagenumber.text = (current_page + 1) + "/" + entries.Count;
		EventManager.TriggerEvent(EventManager.EventType.INTERFACE_PAGE_RIGHT,
			                      current_page.ToString());
	}

	public void add_entry(string data) {
		entries.Add(data);
		current_page = entries.Count-1;
		pagenumber.text = (current_page + 1) + "/" + entries.Count;
		journaltext.text = entries[current_page];
	}

	public void load_image(List<string> urls) {
		//set the current image to display
		//read from cache or load from url

		//TODO: add cache functionality

		prev_images.Add(Enumerable.Repeat(default_image, 10).ToList());
		StartCoroutine(_load_images(urls,prev_images.Count-1));


	}

	IEnumerator _load_images(List<string> urls, int index) {
		//try to get several valid images from the list, otherwise use default

		int numfound = 0;
		foreach (string url in urls) {
			if(numfound >= numtoget) {
				break;
			}
			using (WWW www = new WWW(url)) {
				yield return www;
				if (string.IsNullOrEmpty(www.error)) {
					Texture2D texture = www.texture;
					yield return null;
					if (texture != null && texture.width > 8 && texture.height > 8) {
						if(prev_images.Count-1 == index) {//prevent delayed loading mismatch
							imageboxes[numfound].texture = texture;
						}
						prev_images[index].Add(texture);
						numfound++;
					}
				}
			}
		}
		if (numfound < numtoget && prev_images.Count - 1 == index) {//prevent delayed loading mismatch
			while(numfound < numtoget) {
				imageboxes[numfound].texture = default_image;
				numfound++;
			}
		}
	}
	
}
