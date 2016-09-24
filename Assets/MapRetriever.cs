using UnityEngine;
using UnityEngine.UI;

using System.Collections;

[ExecuteInEditMode]
public class MapRetriever : MonoBehaviour {
	[SerializeField]private LocationMapper m_locationMapper;
	[SerializeField]private RawImage m_image;

	// Update is called once per frame
	public IEnumerator ProcessRequest(HTTP.Request req ) {

		while (req == null || !req.isDone) {
			yield return new WaitForEndOfFrame();
		}
		if (req.exception == null) {
			var tex = new Texture2D ((int)m_locationMapper.GetWidth (), (int)m_locationMapper.GetHeight ());
			tex.LoadImage (req.response.Bytes);
			m_image.texture = tex;
		} else {
			Debug.LogError (req.exception.Message);
		}
	}
}
