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
		EventManager.StartListening(EventManager.EventType.INTERFACE_PAN, listener);
		setText(Camera.main.orthographicSize);
	}

	void setText(float zoomlevel) {
		//Vector3 pos = RectTransformUtility.WorldToScreenPoint(Camera.main, transform.position);
		//print(pos.x + " " + positionToDate(pos.x) + " " + dateToPosition(positionToDate(pos.x)));
		//print(pos);

		//print(transform.position.x + " " + positionToDate(transform.position.x)/365 );

		//print(rt.rect);

		//STEP 1: ALIGN ZERO

		//STEP 2: SCALE INTERVALS

		//as zoom out, intervals become smaller until they merge


		





		//years between tall marks
		//round to nearest even number
		int granularity = (int)(zoomlevel % 2 == 0 ? zoomlevel : (zoomlevel + 1));
		granularity = granularity == 0 ? 1 : granularity;


		//bool even = (granularity & 1) == 0;



		//as zoom in, add more increments between smaller units
		//small units turn to large ones


		int fsize = (int)(txt.fontSize * 1.5f);
		int hsize = (int)(txt.fontSize / 1.5f);
		float rsize = rt.rect.width;
		print(granularity);
		string s1 = "";
		string s2 = "";
		int skip = 0;

		int ticks = Mathf.CeilToInt(maxDays/ (maxTimelineWidth/1.1f) / granularity);

		for (int i=0; i< ticks; i++) {

			int year = i*(int)map(granularity,0,maxTimelineWidth,minDays,maxDays)/365;
			
			if (i % granularity == 0) {
				s1 += "<size=" + fsize + ">|</size>";
				s2 += year;
				skip += year.ToString().Length-4;
			}
			/*else if (!even && i % granularity == 1) {
				s1 += "<size=" + fsize + ">|</size>";
				s2 += year;
				skip += year.ToString().Length-4;
			}*/
			//put a half tick if even
			else if (i % (granularity / 2) == 0) {
				s1 += "|";
				s2 += " ";
			}
			//put a quarter tick if even
			else if (i % (granularity/4) == 0) {
				s1 += "<size=" + hsize + ">|</size>";
				s2 += "<size=" + hsize + "> </size>";
			}

			/*//put a fifth tick if odd
			else if (!even && i % (granularity / 5) == 0) {
				s1 += "<size=" + hsize + ">|</size>";
				s2 += "<size=" + hsize + "> </size>";
			}*/

			else {
				s1 += " ";
				if (skip > 0 ) {
					skip--;
				}
				else if (skip < 0) {
					skip++;
					s2 += " ";
				}
				else {
					s2 += " ";
				}
				
			}
		}

		txt.text = s1 + "\n" + s2;

		Vector3 tmp = transform.position;
		tmp.x = dateToPosition(0);
		transform.position = tmp;
	}
}
