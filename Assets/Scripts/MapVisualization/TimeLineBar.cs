using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;

//[RequireComponent(typeof(Text))]
public class TimeLineBar : MonoBehaviour {

	//private Text txt;
	private UnityAction<string> listener;

	public static long minDays = 0;
	public static long maxDays = 1095000;

	public static float maxTimelineWidth = 100;

	private RectTransform rt;

	public GameObject sectionFab;
	private List<GameObject> sections;

	public static float dateToPosition(long totaldays) {
		//given a date in total days, give the x position

		//return map(totaldays, minDays, maxDays, -maxTimelineWidth, maxTimelineWidth);
		return (totaldays - minDays) * (maxTimelineWidth + maxTimelineWidth) / (maxDays - minDays) - maxTimelineWidth;

	}

	public static long positionToDate(float xpos) {
		//given the x position, give a date in total days

		//return (long)map(xpos, -maxTimelineWidth, maxTimelineWidth, minDays, maxDays);
		return (long)((xpos + maxTimelineWidth) * (maxDays - minDays) / (maxTimelineWidth + maxTimelineWidth) + minDays);

	}

	static float map(float x, float in_min, float in_max, float out_min, float out_max) {
		return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
	}

	void Awake() {
		sections = new List<GameObject>();
		rt = transform as RectTransform;
		listener = delegate (string data) {
			setData(float.Parse(data));
		};
	}

	// Use this for initialization
	void Start () {
		EventManager.StartListening(EventManager.EventType.INTERFACE_ZOOM_OUT, listener);
		EventManager.StartListening(EventManager.EventType.INTERFACE_ZOOM_IN, listener);
		EventManager.StartListening(EventManager.EventType.INTERFACE_PAN, listener);
		setData(Camera.main.orthographicSize);
	}

	void setData(float zoomlevel) {

		foreach(GameObject g in sections) {
			Destroy(g);
		}
		sections.Clear();

		int granularity = (int)(zoomlevel % 2 == 0 ? zoomlevel : (zoomlevel + 1));
		granularity = granularity == 0 ? 1 : granularity;


		//float screenWidth = rt.rect.width;
		int leftyear = Mathf.CeilToInt(positionToDate(Camera.main.ScreenToWorldPoint(rt.anchoredPosition).x) / 365);

		int rightyear = Mathf.CeilToInt(positionToDate(Camera.main.ScreenToWorldPoint(new Vector2(rt.anchoredPosition.x + rt.rect.width,0)).x) / 365);

		int roundvalue = 100;
		if (granularity < 5) {
			roundvalue = 10;
		}
		else if (granularity < 10) {
			roundvalue = 20;
		}
		else if (granularity < 20) {
			roundvalue = 50;
		}
		else if (granularity < 40) {
			roundvalue = 100;
		}
		else if (granularity < 80) {
			roundvalue = 200;
		}else {
			roundvalue = 500;
		}

		int startyear = roundvalue * Mathf.CeilToInt(leftyear / roundvalue);


		for(int xyr = (startyear/roundvalue - 1); xyr < (rightyear/roundvalue + 1); xyr++) {
			GameObject section = Instantiate(sectionFab) as GameObject;
			sections.Add(section);		

			section.transform.SetParent(transform, false);
			RectTransform srt = section.transform as RectTransform;

			float xpos = Camera.main.WorldToScreenPoint(new Vector3(dateToPosition(xyr * roundvalue * 365), 0, 0)).x;
			srt.anchoredPosition = new Vector2(xpos, 0);
			int ystr = Mathf.RoundToInt(positionToDate(Camera.main.ScreenToWorldPoint(srt.anchoredPosition).x + .05f) / 365); //have to add tiny bit to prevent rounding jitter

			section.transform.GetChild(0).GetComponent<Text>().text = (ystr >= 0) ? ystr.ToString() : (-ystr).ToString() + " B.C.";

		}

	}
}
