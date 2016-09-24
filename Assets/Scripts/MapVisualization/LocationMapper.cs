using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LocationMapper : MonoBehaviour {

	//maps the coordinate between a location and the map image

	//requires 2 points of reference on the map and their lat/lon in order to map the pixels
	private static LocationMapper ms_instance;

	public string referenceLoc1;
	public Vector2 referencePix1;
	public Vector2 referenceCoord1;
	public string referenceLoc2;
	public Vector2 referencePix2;
	public Vector2 referenceCoord2;

	private float w, h;
	private Image img;

	[SerializeField]private float m_minWidth;
	[SerializeField]private float m_maxWidth;
	[SerializeField]private float m_minHeight;
	[SerializeField]private float m_maxHeight;

	[SerializeField]private float m_minLatitude;
	[SerializeField]private float m_maxLatitude;
	[SerializeField]private float m_minLongitude;
	[SerializeField]private float m_maxLongitude;

	public float GetWidth(){
		return m_maxWidth - m_minWidth;
	}

	public float GetHeight(){
		return m_maxHeight - m_minHeight;
	}

	// Use this for initialization
	void Awake () {
		ms_instance = this;
		img = GetComponent<Image>();
		RectTransform rt = transform as RectTransform;
		w = rt.rect.width;
		h = rt.rect.height;

		Debug.Log ("Width:" + w + " Height: " + h);
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

	public Vector2 coord2local(Vector2 coord) {
		Vector2 tmp = new Vector2 (Mathf.Lerp (m_minWidth, m_maxWidth, Mathf.InverseLerp (m_minLongitude, m_maxLongitude,coord.y)),
			Mathf.Lerp (m_minHeight, m_maxHeight, Mathf.InverseLerp (m_minLatitude, m_maxLatitude,coord.x)));
	
		return tmp;
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
		float fadetime = 1.5f;
		float fadespeed = 1.0f / fadetime;

		while (img.color.a < 1) {
			Color tmp = img.color;
			tmp.a += Time.deltaTime * fadespeed;
			img.color = tmp;
			yield return null;
		}
	}

	private IEnumerator _fadeout() {
		float fadetime = 1.5f;
		float fadespeed = 1.0f / fadetime;
		while (img.color.a > 0) {
			Color tmp = img.color;
			tmp.a -= Time.deltaTime * fadespeed;
			img.color = tmp;
			yield return null;
		}
	}

}
