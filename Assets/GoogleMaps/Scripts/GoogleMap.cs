using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

	public enum MapType
	{
		RoadMap,
		Satellite,
		Terrain,
		Hybrid
	}

	public bool refreshOnStart = true;

	public GoogleMapLocation centerLocation;
	public int zoom = 4;
	public MapType mapType;

	// tiling settings

	// settings for relative lattitude and longitude placement on map

	public static float m_minLatitude;
	public static float m_maxLatitude;
	public static float m_minLongitude;
	public static float m_maxLongitude;

	[SerializeField]public GoogleMapFeature[] features; //https://developers.google.com/maps/documentation/static-maps/styling

	private const float height = 700.0f;
	private const float width = 434.0f;

	void Awake(){

		// TODO: create, set its attributes, and apply it to the in-game object
		//		var texture = new Texture2D ((int)m_locationMapper.GetWidth (), (int)m_locationMapper.GetHeight ());
		//		texture.filterMode = FilterMode.Point;
		//		texture.LoadImage (req.response.Bytes);

		if (refreshOnStart) {
			Refresh ();
		} else {
			//TODO: load the texture from last saved map
		}
	}

	void Update() {

		/*
		// TODO: create a refresh button / kepress (SHIFT + M + R?)

		// TODO: create a save texture as PNG button / keypress (SHIFT + M + S?)

			// Encode texture into PNG
			byte[] bytes = tex.EncodeToPNG();
			Object.Destroy(tex);

			// For testing purposes, also write to a file in the project folder
			// File.WriteAllBytes(Application.dataPath + "/../SavedScreen.png", bytes);

		*/
	}

	public void Refresh() {
		m_locationMapper = GameObject.Find ("MapImage").GetComponent<LocationMapper>();

		Debug.Log ("Refreshing");

		var url = "http://maps.googleapis.com/maps/api/staticmap";
		var qs = "";

		qs += "center=" + HTTP.URL.Encode (string.Format ("{0},{1}", centerLocation.latitude, centerLocation.longitude));

		qs += "&zoom=" + zoom.ToString ();

		qs += "&size=" + HTTP.URL.Encode (string.Format ("{0}x{1}", height,width));
		qs += "&scale=2"; //TODO: put this somewhere serializeable
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
			m_minLatitude = centerLocation.latitude - 20.0f * width/height; 
			m_maxLatitude = centerLocation.latitude + 20.0f * width/height; 
			m_maxLongitude = centerLocation.longitude + 26.6f;
			m_minLongitude = centerLocation.longitude - 26.6f;
			// TODO: make these calculations respond to different zoom levels
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