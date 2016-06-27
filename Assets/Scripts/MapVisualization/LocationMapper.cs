using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class LocationMapper : MonoBehaviour {

	//maps the coordinate between a location and the map image

	//requires 2 points of reference on the map and their lat/lon in order to map the pixels

	public string referenceLoc1;
	public Vector2 referencePix1;
	public Vector2 referenceCoord1;
	public string referenceLoc2;
	public Vector2 referencePix2;
	public Vector2 referenceCoord2;

	private float w, h;



	// Use this for initialization
	void Awake () {
		RectTransform rt = transform as RectTransform;
		w = rt.rect.width;
		h = rt.rect.height;
	}
	
	public Vector2 pix2coord(Vector2 pix) {
		return new Vector2(pix.x * 360 / w, pix.y * 180 / h);
	}

	public Vector2 coord2pix(Vector2 coord) {
		return new Vector2(coord.y * w / 360, coord.x * h / 180);
	}
}
