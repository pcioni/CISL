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
    public static long minDaysTarget = 0;
    public static long maxDaysTarget = 0;
    public static long zoomDivisorTarget = 1;

    public static float maxTimelineWidth = 100;

	private RectTransform rt;

	public GameObject sectionFab;
	private List<GameObject> sections = new List<GameObject>();
    
	public static float dateToPosition(long totaldays, bool animated = false) {
        //given a date in total days, give the x position
        if (animated) return (totaldays - minDays) * (maxTimelineWidth + maxTimelineWidth) / (maxDays - minDays) - maxTimelineWidth;
        return (totaldays - minDaysTarget) * (maxTimelineWidth + maxTimelineWidth) / (maxDaysTarget - minDaysTarget) - maxTimelineWidth;
	}

	public static long positionToDate(float xpos, bool animated = false) {
        //given the x position, give a date in total days
        if (animated) return (long)((xpos + maxTimelineWidth) * (maxDays - minDays) / (maxTimelineWidth + maxTimelineWidth) + minDays);
        return (long)((xpos + maxTimelineWidth) * (maxDaysTarget - minDaysTarget) / (maxTimelineWidth + maxTimelineWidth) + minDaysTarget);
    }

    static float map(float x, float in_min, float in_max, float out_min, float out_max) {
		return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
	}

	void Awake() {
		sections = new List<GameObject>();
		rt = transform as RectTransform;
		listener = delegate (string data) {
			updateTimeLineBar(float.Parse(data));
		};

    }

	// Use this for initialization
	void Start () {
		EventManager.StartListening(EventManager.EventType.INTERFACE_ZOOM_OUT, listener);
		EventManager.StartListening(EventManager.EventType.INTERFACE_ZOOM_IN, listener);
		EventManager.StartListening(EventManager.EventType.INTERFACE_PAN, listener);
        setData();
	}

    private IEnumerator animateTimeLineBarCoroutine = null;

    // min days of zero will error out
    public void setData(float _minDays = Mathf.NegativeInfinity, float _maxDays = Mathf.Infinity, float _zoomDivisor = 0, float seconds = 0)
    {
        if (_minDays == Mathf.NegativeInfinity) _minDays = minDays;
        if (_maxDays == Mathf.Infinity) _maxDays = maxDays;
        if (_zoomDivisor == 0) _zoomDivisor = zoomDivisor;

        minDaysTarget = (long) _minDays;
        maxDaysTarget = (long) _maxDays;
        zoomDivisorTarget = (long) _zoomDivisor;

        if (animateTimeLineBarCoroutine != null) StopCoroutine(animateTimeLineBarCoroutine);

        if (seconds > 0)
        {
            animateTimeLineBarCoroutine = _animateTimeLineBar(seconds, (long) _minDays, (long)_maxDays, (long)_zoomDivisor);
            StartCoroutine(animateTimeLineBarCoroutine);
        } else
        {
            minDays = minDaysTarget;
            maxDays = maxDaysTarget;
            zoomDivisor = zoomDivisorTarget;

            updateTimeLineBar(Camera.main.orthographicSize);
        }
    }

    //TODO: are there Unity versions of these curves? If not, we should save some static curves that can be accessed from anywhere instead of declaring them everywhere that we need them.
    public AnimationCurve easeInOut;
    public AnimationCurve easeIn;
    public AnimationCurve easeOut;

    public IEnumerator _animateTimeLineBar (float _secs, long _minDaysTo, long _maxDaysTo, long _zoomDivisorTo)
    {
        float t = 0f;
        float _minDaysFrom = minDays;
        float _maxDaysFrom = maxDays;
        float _zoomDivisorFrom = zoomDivisor;
        //currentData = new Vector3(minDays, maxDays, zoomDivisor);
        while (t < 1)
        {
            t += Time.deltaTime / _secs;
            
            minDays =     (long) Mathf.Lerp(_minDaysFrom, _minDaysTo, easeInOut.Evaluate(t));
            maxDays =     (long) Mathf.Lerp(_maxDaysFrom, _maxDaysTo, easeInOut.Evaluate(t));
            zoomDivisor = (long) Mathf.Lerp(_zoomDivisorFrom, _zoomDivisorTo, easeOut.Evaluate(t));

            updateTimeLineBar(Camera.main.orthographicSize);

            yield return null;
        }

        yield return null;
    }

    public void updateTimeLineBar (float zoomlevel) {
        //TODO: Hack for variable zoom level during demo, remove later!
        zoomlevel = zoomlevel / zoomDivisor;

        // calculate granularity based on zoomlevel
        int granularity = (int)(zoomlevel % 2 == 0 ? zoomlevel : (zoomlevel + 1));
		granularity = granularity == 0 ? 1 : granularity;

        // calculate range of years to create tick marks for
        int leftyear = Mathf.CeilToInt(positionToDate(Camera.main.ScreenToWorldPoint(rt.anchoredPosition).x, true) / 365);
		int rightyear = Mathf.CeilToInt(positionToDate(Camera.main.ScreenToWorldPoint(new Vector2(rt.anchoredPosition.x + rt.rect.width,0)).x, true) / 365);

        // calculate span between tick marks in years
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

        // determine which rounded year to start on
        int startyear = roundvalue * Mathf.CeilToInt(leftyear / roundvalue);
        // keep track of how many tick marks were needed
        int i = 0;

        // handle tick marks for years to be displayed
        for (int xyr = (startyear/roundvalue - 1); xyr < (rightyear/roundvalue + 1); xyr++) {
            GameObject section;
            if (sections.Count < i + 1)
            {
                section = Instantiate(sectionFab) as GameObject;
                sections.Add(section);
                section.transform.SetParent(transform, false);
            }
                else
            {
                section = sections[i];
                section.SetActive(true);
            }

            RectTransform srt = section.transform as RectTransform;

			float xpos = Camera.main.WorldToScreenPoint(new Vector3(dateToPosition(xyr * roundvalue * 365, true), 0, 0)).x;
			srt.anchoredPosition = new Vector2(xpos, 0);
			int ystr = Mathf.RoundToInt(positionToDate(Camera.main.ScreenToWorldPoint(srt.anchoredPosition).x + .05f, true) / 365); //have to add tiny bit to prevent rounding jitter

			section.transform.GetChild(0).GetComponent<Text>().text = (ystr >= 0) ? ystr.ToString() + "\nCE" : (-ystr).ToString() + "\nBCE";
            i++;
        }

        // disable unused tick marks
        foreach (GameObject g in sections.GetRange(i, sections.Count-i))
        {
            g.SetActive(false);
        }
    }
}
