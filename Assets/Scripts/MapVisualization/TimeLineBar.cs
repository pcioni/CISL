using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Text))]
public class TimeLineBar : MonoBehaviour {

	private Text txt;
	private UnityAction<string> listener;

	public static long minDays = 0;
	public static long maxDays = 1095000;

	public static float maxTimelineWidth = 100;

	private RectTransform rt;

	public static float dateToPosition(long totaldays) {
		//given a date in total days, give the x position

		return map(totaldays, minDays, maxDays, -maxTimelineWidth, maxTimelineWidth);
	}

	public static long positionToDate(float xpos) {
		//given the x position, give a date in total days

		return (long)map(xpos, -maxTimelineWidth, maxTimelineWidth, minDays, maxDays);
	}

	static float map(float x, float in_min, float in_max, float out_min, float out_max) {
		return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
	}


	void Awake() {
		rt = transform as RectTransform;
		txt = GetComponent<Text>();
		listener = delegate (string data) {
			//print(data);
			setText(float.Parse(data));
		};
	}

	//draw increments on the bar

	//increments of 10 depending on zoom level

	// Use this for initialization
	void Start () {
		EventManager.StartListening(EventManager.EventType.INTERFACE_ZOOM_OUT, listener);
		EventManager.StartListening(EventManager.EventType.INTERFACE_ZOOM_IN, listener);
		setText(Camera.main.orthographicSize);
	}
	
	void setText(float zoomlevel) {
		//Vector3 pos = RectTransformUtility.WorldToScreenPoint(Camera.main, transform.position);
		//print(pos.x + " " + positionToDate(pos.x) + " " + dateToPosition(positionToDate(pos.x)));



		//STEP 1: ALIGN ZERO

		//STEP 2: SCALE INTERVALS

		//as zoom out, intervals become smaller until they merge

		//years between tall marks
		int granularity = (int)zoomlevel;
		granularity = granularity == 0 ? 1 : granularity;


		bool even = (granularity & 1) == 0;



		//as zoom in, add more increments between smaller units
		//small units turn to large ones

		int fsize = (int)(txt.fontSize * 1.5f);
		int hsize = (int)(txt.fontSize / 1.5f);
		float rsize = rt.rect.width;
		print(granularity);
		string s1 = "";
		string s2 = "";
		int skip = 0;
		for (int i=0; i<10*granularity; i++) {

			int year = i * granularity;
			
			if (even && i % granularity == 0) {
				s1 += "<size=" + fsize + ">|</size>";
				s2 += year;
				skip += year.ToString().Length-1;
			}
			else if (!even && i % granularity == 1) {
				s1 += "<size=" + fsize + ">|</size>";
				s2 += year;
				skip += year.ToString().Length-1;
			}
			//put a half tick if even
			else if (even && i % (granularity / 2) == 0) {
				s1 += "|";
				s2 += " ";
			}
			//put a quarter tick if even
			else if (even && i % (granularity/4) == 0) {
				s1 += "<size=" + hsize + ">|</size>";
				s2 += "<size=" + hsize + "> </size>";
			}

			//put a fifth tick if odd
			else if (!even && i % (granularity / 5) == 0) {
				s1 += "<size=" + hsize + ">|</size>";
				s2 += "<size=" + hsize + "> </size>";
			}
			//put

			else {
				s1 += " ";
				if (skip > 0 ) {
					skip--;
				}else {
					s2 += " ";
				}
				
			}
		}

		txt.text = s1 + "\n" + s2;

		
	}
}
