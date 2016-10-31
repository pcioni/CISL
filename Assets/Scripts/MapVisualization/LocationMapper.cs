using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LocationMapper : MonoBehaviour {

	//maps the coordinate between a location and the map image

	//requires 2 points of reference on the map and their lat/lon in order to map the pixels
	public static LocationMapper ms_instance;


	private float w, h;
	private Image img;

	[SerializeField]private float m_minWidth;
	[SerializeField]private float m_maxWidth;
	[SerializeField]private float m_minHeight;
	[SerializeField]private float m_maxHeight;

	[SerializeField]private RectTransform m_transform;

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
    
	public Vector2 coord2local(Vector2 coord) {
		Vector2 tmp = new Vector2 (
            Mathf.Lerp (m_minWidth, m_maxWidth, Mathf.InverseLerp (GoogleMap.m_minLongitude, GoogleMap.m_maxLongitude, coord.y)),
			Mathf.Lerp (m_minHeight, m_maxHeight, Mathf.InverseLerp (GoogleMap.m_minLatitude, GoogleMap.m_maxLatitude, coord.x))
            );
	
		return tmp;
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
