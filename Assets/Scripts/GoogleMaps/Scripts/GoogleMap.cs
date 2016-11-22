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
	private string filePath = "";
	public string saveFilePath = "maps/google_staticmap_lastSaved";
	public string terrainFilePath =       "maps/google_staticmap_6400x3472_terrain";
	public string satelliteFilePath =     "maps/google_staticmap_6400x3472_satellite";
	public string satelliteDarkFilePath = "maps/google_staticmap_6400x3472_satellite_drk";

	public enum MapType
	{
		RoadMap,
		Satellite,
		Terrain,
		Hybrid
	}

	public bool refreshOnStart = true;
	private bool refreshed = true; // set to true so it doesn't refresh twice on start
	private bool loaded = true;
	private bool saved = true;

	public GoogleMapLocation centerLocation;
	public int zoom = 4;
	public int scale = 2;
	public MapType mapType;

	// tiling settings
	public int tileCount = 1;
	public int tileSpacing = 0; // this might depend on zoom level
	public int tileWidth = 200;
	public int tileHeight = 200;

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

		filePath = satelliteFilePath;

		// create texture with appropriate dimensions and apply it to m_image object
		Texture2D texture = new Texture2D ((int)m_locationMapper.GetWidth (), (int)m_locationMapper.GetHeight ());
		m_image.texture = texture;

		calcExtents (zoom);

		if (refreshOnStart) {
			Refresh ();
		} else {
			loadMap (filePath);
		}
	}

	void Update() {
		//check for shift keypress
		bool shiftDown = (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift));

		//check for refresh map kepress (SHIFT + M + R)
		bool refresh = (shiftDown && Input.GetKey (KeyCode.M) && Input.GetKey (KeyCode.R));

		//check for load map kepress (SHIFT + M + L)
		bool load = (shiftDown && Input.GetKey (KeyCode.M) && Input.GetKey (KeyCode.L));
		// check for map to load keypresses
		bool terrain = (load && Input.GetKey (KeyCode.N));
		bool satellite = (load && Input.GetKey (KeyCode.E));
		bool satelliteDrk = (load && Input.GetKey (KeyCode.K));

		//check for save map kepress (SHIFT + M + S)
		bool save = (shiftDown && Input.GetKey (KeyCode.M) && Input.GetKey (KeyCode.S));

		if (save && !saved) {
			// Encode texture into PNG
			byte[] bytes = (m_image.texture as Texture2D).EncodeToPNG ();

			// write to a file in the project folder
			File.WriteAllBytes (Application.dataPath + "/Resources/" + saveFilePath + ".png", bytes);

			Debug.Log ("GoogleMap.update() :: saved image filePath = " + saveFilePath);

			saved = true;
		} else {
			saved = false;
		}

		if (refresh && !refreshed) {
			Refresh ();
			refreshed = true;
			Debug.Log ("GoogleMap.update() :: refreshed image.");
		} else {
			refreshed = false;
		}

		if (load && !loaded) {
			if (terrain) {
				filePath = terrainFilePath;
			} 
			if (satellite) {
				filePath = satelliteFilePath;
			} 
			if (satelliteDrk) {
				filePath = satelliteDarkFilePath;
			}
			loadMap (filePath);
			loaded = true;
		} else {
			loaded = false;
		}
	}

	public void calcExtents (int _zoom) {
		if (_zoom == 4) {
			// TODO: make these calculations respond to different zoom levels
			m_minLatitude = centerLocation.latitude - (lattidudeRange/2) * width/height; 
			m_maxLatitude = centerLocation.latitude + (lattidudeRange/2) * width/height; 
			m_maxLongitude = centerLocation.longitude + (longitudeRange/2);
			m_minLongitude = centerLocation.longitude - (longitudeRange/2);
		}
	}

	public void loadMap (string _filePath) {
		//TODO: load the texture from last saved map by default
		Texture2D texture = LoadPNG(_filePath);
		//if a texture was loaded, apply it to in-game object
		if (texture != null) {
			m_image.texture = texture;

			Debug.Log("GoogleMap.loadMap() :: loaded map");
		} else {
			//else, leave the default texture in-place and throw a warning
			Debug.Log("GoogleMap.loadMap() :: map not loaded");
		}
		Debug.Log("GoogleMap.loadMap() :: filePath = " + _filePath);
	}

	public static Texture2D LoadPNG(string _filePath) {

		Texture2D tex = Resources.Load (_filePath) as Texture2D;

		if (tex != null) {
			tex.filterMode = FilterMode.Point;
		} else {
			Debug.Log ("GoogleMaps.LoadPNG() :: texture file does not exist.");
			Debug.Log ("GoogleMaps.LoadPNG() :: _filePath = " + _filePath);
		}

		return tex;
	}

	public void Refresh() {

		Debug.Log ("Refreshing");

		var url = "http://maps.googleapis.com/maps/api/staticmap";
		var qs = "";


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

		calcExtents (zoom);

		// TODO: create requests to refresh image data one tile at a time
		float shiftRatio =  (tileCount -1)/4.0f; //how far the center of the top left tile is from center of map
		float lat_left = centerLocation.latitude - (shiftRatio * lattidudeRange);
		float lng_top = centerLocation.longitude - (shiftRatio * longitudeRange);
		Debug.Log (string.Format("GoogleMap.Refresh() ::             shiftRatio = {0}", shiftRatio));
		Debug.Log (string.Format("GoogleMap.Refresh() ::    (lat_left, lng_top) = ({0},{1})", lat_left, lng_top));

		// resize texture based on tile count
		var texture = new Texture2D (
			(int)m_locationMapper.GetWidth ()  * tileCount, 
			(int)m_locationMapper.GetHeight () * tileCount
		);
		texture.filterMode = FilterMode.Point;
		m_image.texture = texture;


		// TODO: loop through requests, shifting the center periodically for each tile
		for (int y = 0; y < tileCount; y++) {
			for (int x = 0; x < tileCount; x++) {

				float lat = lat_left + (x * lattidudeRange / tileCount);
				float lng = lng_top + (y * longitudeRange / tileCount);

				Debug.Log (string.Format("GoogleMap.Refresh() :: requesting tile (x, y) = ({0},{1})", x, y));
				Debug.Log (string.Format("GoogleMap.Refresh() ::             (lat, lng) = ({0},{1})", lat, lng));

				string center = "center=" + HTTP.URL.Encode (string.Format ("{0},{1}", lat, lng));
				qs = center + qs;

				HTTP.Request req = new HTTP.Request ("GET",url + "?" + qs, true);
				Debug.Log (req.uri);
				req.Send ();
				// TODO: pass settings in here for target to copy pixels into, include center, width, height, etc.
				// TODO: grab a slightly larger image than needed to avoid watermark //TODO: research google premium for academic institutions (RPI might already have this)
				StartCoroutine( ProcessRequest(
					req,
					0, 0, 
					0, 0, (int)m_locationMapper.GetWidth (), (int)m_locationMapper.GetHeight (), 
					m_image.texture, 
					0, 0, 
					0, 0
				));
			}
		}
	}
		

	private IEnumerator ProcessRequest(
		HTTP.Request req, 
		int srcElement, int srcMip, 
		int srcX, int srcY, int srcWidth, int srcHeight, 
		Texture dstTx, 
		int dstElement, int dstMip, 
		int dstX, int dstY){

		while (req == null || !req.isDone) {
			yield return new WaitForEndOfFrame ();
		}

		if (req.exception == null) {
			var texture = new Texture2D (2,2);
			texture.LoadImage (req.response.Bytes); //..this will auto-resize the texture dimensions.

			m_image.texture = texture;
			/*
			// TODO: update this to copy pixels instead of replacing entire texture

			Graphics.CopyTexture(
				texture, 
				srcElement, srcMip, 
				srcX, srcY, tileWidth, tileHeight, 
				m_image.texture, 
				dstElement, dstMip, 
				dstX, dstY);
//*/
//            m_image.Apply(); //TODO: is something like this necessary?

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