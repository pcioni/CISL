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
		//txt = GetComponent<Text>();
		listener = delegate (string data) {
			//print(data);
			//setText(float.Parse(data));
			setData(float.Parse(data));
		};
	}

	//draw increments on the bar

	//increments of 10 depending on zoom level

	// Use this for initialization
	void Start () {
		EventManager.StartListening(EventManager.EventType.INTERFACE_ZOOM_OUT, listener);
		EventManager.StartListening(EventManager.EventType.INTERFACE_ZOOM_IN, listener);
		EventManager.StartListening(EventManager.EventType.INTERFACE_PAN, listener);
		setData(Camera.main.orthographicSize);
	}

	void setData(float zoomlevel) {
		//calculate granularity
		//place leftmost section

		foreach(GameObject g in sections) {
			Destroy(g);
		}
		sections.Clear();

		int granularity = (int)(zoomlevel % 2 == 0 ? zoomlevel : (zoomlevel + 1));
		granularity = granularity == 0 ? 1 : granularity;
		print("g: " + granularity % 9);//(int)(granularity / 12.5f));
		/*int marksize = 75;//how wide is each section
		string innertext = "";
		switch (granularity%9) {//((int)(granularity / 12.5f)) {
			case 0:
				marksize = 75;
				innertext = "<size=30>|</size>            ";
				break;
			case 1:
				marksize = 80;
				innertext = "<size=30>|</size>             ";
				break;
			case 2:
				marksize = 85;
				innertext = "<size=30>|</size>              ";
				break;
			case 3:
				marksize = 90;
				innertext = "<size=30>|</size>               ";
				break;
			case 4:
				marksize = 95;
				innertext = "<size=30>|</size>                ";
				break;
			case 5:
				marksize = 100;
				innertext = "<size=30>|</size>                 ";
				break;
			case 6:
				marksize = 105;
				innertext = "<size=30>|</size>                  ";
				break;
			case 7:
				marksize = 118;
				innertext = "<size=30>|</size>         <size=16>|</size>         ";
				break;
			case 8:
				marksize = 128;
				innertext = "<size=30>|</size>          <size=16>|</size>           ";
				break;
		}*/


		//<size=30>|</size>      <size=30>|</size> == 60 <-- each space is 10
		/*innertext = "<size=30>|</size>      ";
		marksize = 60;
		for(int i=0; i<granularity%12; i++) {
			if (i % 6 == 0) {
				innertext += "|";
				marksize += 5;
			}else {
				innertext += " ";
				marksize += 10;
			}
			
		}
			
		innertext += "<size=30>|</size>";*/

		//marksize = 2*116;

		float screenWidth = rt.rect.width;
		int leftyear = Mathf.CeilToInt(positionToDate(Camera.main.ScreenToWorldPoint(rt.anchoredPosition).x) / 365);

		int rightyear = Mathf.CeilToInt(positionToDate(Camera.main.ScreenToWorldPoint(new Vector2(rt.anchoredPosition.x + rt.rect.width,0)).x) / 365);




		print(leftyear + " " + rightyear);

		//float leftlimit = rt.anchoredPosition.x;
		//float rightlimit = rt.anchoredPosition.x + rt.rect.width;

		//float curwidth = 0;
		//int curx = 0;

		//int remainder = leftyear % marksize;
		//int startyear = leftyear + ((remainder < marksize / 2) ? -remainder : (marksize - remainder));


		//int yearWidth = Mathf.Abs(Mathf.CeilToInt(positionToDate(Camera.main.ScreenToWorldPoint(new Vector2(marksize,0)).x) / 365));

		int roundvalue = 100;
		print("granuklarity: " + granularity);
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

		//print("start: " + startyear);

		//offset is the distance to the nearest on-tick leftmost marker
		//float offset = Camera.main.WorldToScreenPoint(new Vector3(dateToPosition(startyear*365), 0, 0)).x;

		//print("ofs: " + offset);

		//while (curwidth < screenWidth) { 
		//while(curx < 50) { 
		for(int xyr = (startyear/roundvalue - 1); xyr < (rightyear/roundvalue + 1); xyr++) {
			GameObject section = Instantiate(sectionFab) as GameObject;
			sections.Add(section);		
			//curwidth += marksize;
			section.transform.SetParent(transform, false);
			RectTransform srt = section.transform as RectTransform;
			//float xpos = offset + curx * marksize;
			float xpos = Camera.main.WorldToScreenPoint(new Vector3(dateToPosition(xyr * roundvalue * 365), 0, 0)).x;
			srt.anchoredPosition = new Vector2(xpos, 0);
			int ystr = Mathf.CeilToInt(positionToDate(Camera.main.ScreenToWorldPoint(srt.anchoredPosition).x) / 365);
			//section.GetComponent<Text>().text = innertext;
			section.transform.GetChild(0).GetComponent<Text>().text = (ystr >= 0) ? ystr.ToString() : (-ystr).ToString() + " B.C.";
			//curx++;
			//if(xpos > rightlimit) {
			//	break;
			//}
		}

	}

	/*void setText(float zoomlevel) {
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

			//int year = i*(int)map(granularity,0,maxTimelineWidth,minDays,maxDays)/365;

			int year = i * (int)map(granularity, 0, 100, 0, 2000) / 75;

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
			/*
			//put a half tick if even
			else if (i % (granularity / 2) == 0) {
				s1 += "|";
				s2 += " ";
			}
			//put a quarter tick if even
			else if (i % (granularity/4) == 0) {
				s1 += "<size=" + hsize + ">|</size>";
				s2 += "<size=" + hsize + "> </size>";
			}*/

			/*//put a fifth tick if odd
			else if (!even && i % (granularity / 5) == 0) {
				s1 += "<size=" + hsize + ">|</size>";
				s2 += "<size=" + hsize + "> </size>";
			}*//*

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
	}*/
}
