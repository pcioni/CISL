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

		float cxt = Mathf.InverseLerp(referencePix1.x, referencePix2.x, pix.x);
		float cyt = Mathf.InverseLerp(referencePix1.y, referencePix2.y, pix.y);

		return new Vector2(Mathf.Lerp(referenceCoord1.x,referenceCoord2.x, cxt), Mathf.Lerp(referenceCoord1.y, referenceCoord2.y, cyt));

	}

	public Vector2 coord2pix(Vector2 coord) {

		float pxt = Mathf.InverseLerp(referenceCoord1.x, referenceCoord2.x, coord.x);
		float pyt = Mathf.InverseLerp(referenceCoord1.y, referenceCoord2.y, coord.y);

		return new Vector2(Mathf.Lerp(referencePix1.x, referencePix2.x, pxt), Mathf.Lerp(referencePix1.y, referencePix2.y, pyt));
	}
}
