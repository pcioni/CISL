using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using System.IO;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
[CustomEditor(typeof(GoogleMap))]
#endif
[System.Serializable]
public class GoogleMap : MonoBehaviour
{
	[SerializeField]private RawImage m_image;
	[SerializeField]private LocationMapper m_locationMapper;

	//TODO: create a Refresh() button
	//TODO: allow file selection
	public string filePath = "/Images/google_staticmap_1280x868.png";

	public enum MapType
	{
		RoadMap,
		Satellite,
		Terrain,
		Hybrid
	}

	public bool refreshOnStart = true;
	private bool refreshed = true; // set to true so it doesn't refresh twice on start
	private bool saved = true;

	public GoogleMapLocation centerLocation;
	public int zoom = 4;
	public int scale = 2;
	public MapType mapType;

	// tiling settings

	// settings for relative lattitude and longitude placement on map
	public float lattidudeRange = 40.0f;
	public float longitudeRange = 53.2f;

	public static float m_minLatitude;
	public static float m_maxLatitude;
	public static float m_minLongitude;
	public static float m_maxLongitude;

	[SerializeField]public GoogleMapFeature[] features; //https://developers.google.com/maps/documentation/static-maps/styling

	private const float height = 700.0f;
	private const float width = 434.0f;

	void Awake(){

		m_locationMapper = GameObject.Find ("MapImage").GetComponent<LocationMapper>();

		// create texture with appropriate dimensions and apply it to m_image object
		Texture2D texture = new Texture2D ((int)m_locationMapper.GetWidth (), (int)m_locationMapper.GetHeight ());
		m_image.texture = texture;

		if (refreshOnStart) {
			Refresh ();
		} else {
			//TODO: load the texture from last saved map
			texture = LoadPNG(filePath);
			//if a texture was loaded, apply it to in-game object
			if (texture != null) {
				m_image.texture = texture;
			} else {
				//else, leave the default texture in-place and throw a warning
				Debug.Log("GoogleMap.Awake() :: image not loaded");
				Debug.Log("GoogleMap.Awake() :: filePath = " + filePath);
			}
		}
	}

	void Update() {
		//check for shift keypress
		bool shiftDown = (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift));
		//check for refresh map kepress (SHIFT + M + R)
		bool refreshMap = (shiftDown && Input.GetKey (KeyCode.M) && Input.GetKey (KeyCode.R));
		//check for save map kepress (SHIFT + M + S)
		bool saveMap = (shiftDown && Input.GetKey (KeyCode.M) && Input.GetKey (KeyCode.S));

		if (saveMap && !saved) {
			// Encode texture into PNG
			byte[] bytes = (m_image.texture as Texture2D).EncodeToPNG ();

			// write to a file in the project folder
			File.WriteAllBytes (Application.dataPath + filePath, bytes);

			// debug
			Debug.Log ("GoogleMap :: saved image filePath = " + filePath);

			saved = true;
		} else {
			saved = false;
		}

		if (refreshMap && !refreshed) {
			Refresh ();
			refreshed = true;
		} else {
			refreshed = false;
		}
	}

	public static Texture2D LoadPNG(string _filePath) {
		// by IMD from http://answers.unity3d.com/answers/802424/view.html

		Texture2D tex = null;
		byte[] fileData;

		if (File.Exists (Application.dataPath + _filePath)) {
			fileData = File.ReadAllBytes (Application.dataPath + _filePath);
			tex = new Texture2D (2, 2);
			tex.filterMode = FilterMode.Point;
			tex.LoadImage (fileData); //..this will auto-resize the texture dimensions.
		} else {
			Debug.Log ("GoogleMaps.LoadPNG() :: File does not exist.");
			Debug.Log ("GoogleMaps.LoadPNG() :: _filePath = " + _filePath);
		}

		return tex;
	}

	public void Refresh() {
		m_locationMapper = GameObject.Find ("MapImage").GetComponent<LocationMapper>();

		Debug.Log ("Refreshing");

		var url = "http://maps.googleapis.com/maps/api/staticmap";
		var qs = "";

		qs += "center=" + HTTP.URL.Encode (string.Format ("{0},{1}", centerLocation.latitude, centerLocation.longitude));

		qs += "&zoom=" + zoom.ToString ();
		qs += "&size=" + HTTP.URL.Encode (string.Format ("{0}x{1}", height,width));
		qs += "&scale=" + scale.ToString ();
		qs += "&maptype=" + mapType.ToString ().ToLower ();

		//Break style into for loop
		foreach (GoogleMapFeature feature in features) {
			qs += "&style=feature:" + feature.m_featureType + "%7Celement:" + feature.m_element;
			foreach (string rule in feature.m_myRules) {
				qs += "%7C" + rule;
			}
		}

		var usingSensor = false;

		#if UNITY_IPHONE
		usingSensor = Input.location.isEnabledByUser && Input.location.status == LocationServiceStatus.Running;
		#endif

		qs += "&sensor=" + (usingSensor ? "true" : "false");

		if (zoom == 4) {
			// TODO: make these calculations respond to different zoom levels
			m_minLatitude = centerLocation.latitude - (lattidudeRange/2) * width/height; 
			m_maxLatitude = centerLocation.latitude + (lattidudeRange/2) * width/height; 
			m_maxLongitude = centerLocation.longitude + (longitudeRange/2);
			m_minLongitude = centerLocation.longitude - (longitudeRange/2);
		}

		// TODO: create requests to refresh image data one tile at a time
		// TODO: loop through requests, shifting the center periodically for each tile
		HTTP.Request req = new HTTP.Request ("GET",url + "?" + qs, true);
		Debug.Log (req.uri);
		req.Send ();
		StartCoroutine(ProcessRequest (req));
		// TODO: add settings in here for target to copy pixels into, include center, width, height, etc.
	}

	private IEnumerator ProcessRequest(HTTP.Request req){
		while (req == null || !req.isDone) {
			yield return new WaitForEndOfFrame ();
		}

		if (req.exception == null) {
			var texture = new Texture2D ((int)m_locationMapper.GetWidth (), (int)m_locationMapper.GetHeight ());
            texture.filterMode = FilterMode.Point;
			texture.LoadImage (req.response.Bytes);
			m_image.texture = texture;
			// TODO: update this to copy pixels instead of replacing entire texture
			/*

			Graphics.CopyTexture(
				texture, 
				int srcElement, int srcMip, 
				0, 0, tileWidth, tileHeight, 
				m_image.texture, 
				int dstElement, int dstMip, 
				int dstX, int dstY);

            m_image.Apply();	

			*/
		}
	}

}

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
public class GoogleMapLocation
{
	public string address;
	public float latitude;
	public float longitude;
}

[System.Serializable]
public class GoogleMapFeature
{
	public string m_featureType;
	public string m_element;
	public string [] m_myRules;
}

[System.Serializable]
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
public class GoogleMapPath
{
	public int weight = 5;
	public GoogleMapColor color;
	public bool fill = false;
	public GoogleMapColor fillColor;
	public GoogleMapLocation[] locations;	
}