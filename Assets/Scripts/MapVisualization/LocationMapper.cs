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
	private Image img;


	// Use this for initialization
	void Awake () {
		img = GetComponent<Image>();
		RectTransform rt = transform as RectTransform;
		w = rt.rect.width;
		h = rt.rect.height;
	}

	public Vector2 pix2coord(Vector2 pix) {
		//convert local pixels on map to coordinate point
		float cxt = Mathf.InverseLerp(referencePix1.x, referencePix2.x, pix.x);
		float cyt = Mathf.InverseLerp(referencePix1.y, referencePix2.y, pix.y);

		return new Vector2(Mathf.Lerp(referenceCoord1.x,referenceCoord2.x, cxt), Mathf.Lerp(referenceCoord1.y, referenceCoord2.y, cyt));

	}

	public Vector2 coord2pix(Vector2 coord) {
		//convert coordinates to local pixels on map
		float pxt = Mathf.InverseLerp(referenceCoord1.x, referenceCoord2.x, coord.x);
		float pyt = Mathf.InverseLerp(referenceCoord1.y, referenceCoord2.y, coord.y);

		return new Vector2(Mathf.Lerp(referencePix1.x, referencePix2.x, pxt), Mathf.Lerp(referencePix1.y, referencePix2.y, pyt));
	}

	public Vector2 coord2world(Vector2 coord) {
		//convert coordinate point to world position

		Vector2 tmp = coord2pix(coord);
		tmp.x -= w/2;
		tmp.y -= h/2;
		tmp.y *= -1; // something went wrong somewhere?

		return transform.TransformPoint(tmp);

	}

	public void alignCoordinatesToWorldPosition(Vector2 worldpoint, Vector2 coordinates) {
		//align a point on the map to a point in the world

		Vector2 offset = worldpoint - coord2world(coordinates);
		transform.position += (Vector3)offset;
	}

	public void fadeIn() {
		StartCoroutine(_fadein());
	}

	public void fadeOut() {
		StartCoroutine(_fadeout());
	}

	private IEnumerator _fadein() {
		while(img.color.a < 255) {
			Color tmp = img.color;
			tmp.a += 0.01f;
			img.color = tmp;
			yield return null;
		}
	}

	private IEnumerator _fadeout() {
		while (img.color.a > 0) {
			Color tmp = img.color;
			tmp.a -= 0.01f;
			img.color = tmp;
			yield return null;
		}
	}

}
