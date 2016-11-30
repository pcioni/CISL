using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;


//[RequireComponent(typeof(Text))]
public class TimeLineBar : MonoBehaviour {

	//private Text txt;
	private UnityAction<string> listener;

	public static long minDays = 0;
	public static long maxDays = 1095000;
    public static long zoomDivisor = 1;
    private static long minDaysTarget = 0;
    private static long maxDaysTarget = 0;
    private static long zoomDivisorTarget = 1;

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
        setData();
//        setData(Camera.main.orthographicSize);
	}

    private IEnumerator animateTimeLineBarCoroutine = null;

    public void setData(long _minDays = 0, long _maxDays = 0, long _zoomDivisor = 0, float seconds = 0)
    {
        if (_minDays == 0) _minDays = minDays;
        if (_maxDays == 0) _maxDays = maxDays;
        if (_zoomDivisor == 0) _zoomDivisor = zoomDivisor;

        minDaysTarget = _minDays;
        maxDaysTarget = _maxDays;
        zoomDivisorTarget = _zoomDivisor;

        if (animateTimeLineBarCoroutine != null) StopCoroutine(animateTimeLineBarCoroutine);

        if (seconds > 0)
        {
            animateTimeLineBarCoroutine = _animateTimeLineBar(seconds, _minDays, _maxDays, _zoomDivisor);
            StartCoroutine(animateTimeLineBarCoroutine);
        } else
        {
            minDays = minDaysTarget;
            maxDays = maxDaysTarget;
            zoomDivisor = zoomDivisorTarget;

            setData(Camera.main.orthographicSize);
        }
    }

    public AnimationCurve easeInOut;
    private Vector3 dataFrom;
    private Vector3 dataTo;
    private Vector3 currentData;

    public IEnumerator _animateTimeLineBar (float _secs, long _minDaysTo, long _maxDaysTo, long _zoomDivisorTo)
    {
        float t = 0f;
        dataFrom = new Vector3(minDays, maxDays, zoomDivisor);
        dataTo = new Vector3(_minDaysTo, _maxDaysTo, _zoomDivisorTo);
        //currentData = new Vector3(minDays, maxDays, zoomDivisor);
        while (t < 1)
        {
            t += Time.deltaTime / _secs;

            currentData = Vector3.Lerp(dataFrom, dataTo, easeInOut.Evaluate(t));
             
            minDays = (long) currentData.x;
            maxDays = (long) currentData.y;
            zoomDivisor = (long) currentData.z;

            setData(Camera.main.orthographicSize);

            yield return null;
        }

        yield return null;
    }

    public void setData (float zoomlevel) {
        //TODO: Hack for variable zoom level during demo, remove later!
        zoomlevel = zoomlevel / zoomDivisor;

        // Clear previous tick marks
		foreach(GameObject g in sections) {
			Destroy(g);
		}
		sections.Clear();

        // create new tick marks based on granularity
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

			section.transform.GetChild(0).GetComponent<Text>().text = (ystr >= 0) ? ystr.ToString() + "\nCE" : (-ystr).ToString() + "\nBCE";

		}

	}
}
