using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(GoogleMap))]
[System.Serializable]
public class GoogleMap : Editor
{
	[SerializeField]private RawImage m_image;
	[SerializeField]private LocationMapper m_locationMapper;
	[SerializeField]private MapRetriever m_mapRetriever;
	private static GoogleMap m_instance;

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		if(GUILayout.Button("Build Map"))
		{
			m_instance = this;
			Refresh();	
		}
	}

	public enum MapType
	{
		RoadMap,
		Satellite,
		Terrain,
		Hybrid
	}
	public bool loadOnStart = true;
	public bool autoLocateCenter = true;
	public GoogleMapLocation centerLocation;
	public int zoom = 4;
	public MapType mapType;
	public bool doubleResolution = false;
	[SerializeField]public GoogleMapMarker [] markers;
	[SerializeField]public GoogleMapPath[] paths;
	HTTP.Request req = null;

	void Awake(){
		Refresh ();
	}
	void Update() {

	}

	public void Refresh() {
		m_locationMapper = GameObject.Find ("MapImage").GetComponent<LocationMapper>();
		m_mapRetriever = GameObject.Find ("MapImage").GetComponent<MapRetriever> ();

		Debug.Log ("Refreshing");
		if (markers == null) {
			return;
		}

		if(autoLocateCenter && (markers.Length == 0 && paths.Length == 0)) {
			Debug.LogError("Auto Center will only work if paths or markers are used.");	
		}var url = "http://maps.googleapis.com/maps/api/staticmap";
		var qs = "";
		if (!autoLocateCenter) {
			if (centerLocation.address != "")
				qs += "center=" + HTTP.URL.Encode (centerLocation.address);
			else {
				qs += "center=" + HTTP.URL.Encode (string.Format ("{0},{1}", centerLocation.latitude, centerLocation.longitude));
			}

			qs += "&zoom=" + zoom.ToString ();
		}
		qs += "&size=" + HTTP.URL.Encode (string.Format ("{0}x{1}", m_locationMapper.GetWidth(),m_locationMapper.GetHeight()));
		qs += "&scale=" + (doubleResolution ? "2" : "1");
		qs += "&maptype=" + mapType.ToString ().ToLower ();
		var usingSensor = false;
		#if UNITY_IPHONE
		usingSensor = Input.location.isEnabledByUser && Input.location.status == LocationServiceStatus.Running;
		#endif
		qs += "&sensor=" + (usingSensor ? "true" : "false");

		foreach (var i in markers) {
			qs += "&markers=" + string.Format ("size:{0}|color:{1}|label:{2}", i.size.ToString ().ToLower (), i.color, i.label);
			foreach (var loc in i.locations) {
				qs += "|" + HTTP.URL.Encode (string.Format ("{0},{1}", loc.latitude, loc.longitude));
			}
		}

		foreach (var i in paths) {
			qs += "&path=" + string.Format ("weight:{0}|color:{1}", i.weight, i.color);
			if(i.fill) qs += "|fillcolor:" + i.fillColor;
			foreach (var loc in i.locations) {
				if (loc.address != "")
					qs += "|" + HTTP.URL.Encode (loc.address);
				else
					qs += "|" + HTTP.URL.Encode (string.Format ("{0},{1}", loc.latitude, loc.longitude));
			}
		}

		Debug.Log (qs);

		req = new HTTP.Request ("GET",url + "?" + qs, true);
		Debug.Log (req.uri);
		req.Send ();
		m_mapRetriever.StartCoroutine(m_mapRetriever.ProcessRequest (req));
	}

}

[ExecuteInEditMode]
public enum GoogleMapColor
{
	black,
	brown,
	green,
	purple,
	yellow,
	blue,
	gray,
	orange,
	red,
	white
}

[System.Serializable]
[ExecuteInEditMode]
public class GoogleMapLocation
{
	public string address;
	public float latitude;
	public float longitude;
}

[System.Serializable]
[ExecuteInEditMode]
public class GoogleMapMarker
{
	public enum GoogleMapMarkerSize
	{
		Tiny,
		Small,
		Mid
	}
	public GoogleMapMarkerSize size;
	public GoogleMapColor color;
	public string label;
	public GoogleMapLocation[] locations;
	
}

[System.Serializable]
[ExecuteInEditMode]
public class GoogleMapPath
{
	public int weight = 5;
	public GoogleMapColor color;
	public bool fill = false;
	public GoogleMapColor fillColor;
	public GoogleMapLocation[] locations;	
}