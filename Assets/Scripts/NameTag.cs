using UnityEngine;
using UnityEngine.UI;

public class NameTag : MonoBehaviour {

	public Transform follow;
	//private RectTransform rt;
	private Text txt;

	// Use this for initialization
	void Awake () {
		txt = GetComponentInChildren<Text>();
		//rt = transform as RectTransform;
	}

	public void setTarget(Transform target, string s) {
		follow = target;
		txt.text = s;
	}
	
	// Update is called once per frame
	void Update () {
		//rt.position = Camera.main.WorldToScreenPoint(follow.position);
		Vector3 tmp = follow.position;
		tmp.y += .5f;
		transform.position = tmp;
	}
}
