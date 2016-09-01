﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using JsonConstructs;


public class NarrationJournal : MonoBehaviour {

	public Texture2D default_image;
	public Text journaltext;
	public Text pagenumber;
	public RawImage img;
	private UnityAction<string> listener;

	private List<Texture2D> prev_images;

	public int current_page = 0;

	public List<string> entries;

	void Awake() {
		entries = new List<string>();
		prev_images = new List<Texture2D>();
		listener = delegate (string data) {
			DataConstruct1 dc1 = JsonUtility.FromJson<DataConstruct1>(data);
			add_entry(dc1.text);
			load_image(dc1.imgurl);
		};
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
			img.texture = prev_images[current_page];
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
			img.texture = prev_images[current_page];
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

	public void load_image(string url) {
		//set the current image to display
		//read from cache or load from url

		//TODO: add cache functionality

		StartCoroutine(_load_image(url));


	}

	IEnumerator _load_image(string url) {
		using (WWW www = new WWW(url)) {
			yield return www;
			if (string.IsNullOrEmpty(www.error)) {
				Texture2D texture = www.texture;
				yield return null;
				if (texture != null && texture.width > 8 && texture.height > 8) {
					img.texture = texture;
					prev_images.Add(texture);
				}else {
					prev_images.Add(default_image);
				}
			}else {
				prev_images.Add(default_image);
			}
		}
	}
	
}
