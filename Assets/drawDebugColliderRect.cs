using UnityEngine;
using System.Collections;

public class drawDebugColliderRect : MonoBehaviour {
	[SerializeField]
	GameObject panel;
	private Rect panelRect;
	private BoxCollider2D collider;

	void Awake () {

		panelRect = panel.GetComponent<RectTransform> ().rect;

		try {
			collider = this.GetComponent<BoxCollider2D> ();
		} catch {
		}
	}

	// Use this for initialization
	void Start () {
	
	}

	// Update is called once per frame
	void Update () {
		panel.SetActive (DebugMode.MS_ACTIVE);
		if (collider != null) {
			panelRect.width = collider.size.x;
			panelRect.height = collider.size.y;
		}
	}
}
