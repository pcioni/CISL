using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NameTag : MonoBehaviour {

	public Transform follow;
	private Text txt;
	private SpringJoint2D sj;
	private BoxCollider2D bc;
	private LineRenderer lr;

	void Awake () {
		sj = GetComponent<SpringJoint2D>();
		txt = GetComponentInChildren<Text>();
		bc = GetComponent<BoxCollider2D>();
		lr = GetComponent<LineRenderer>();
		lr.SetVertexCount(2);
	}

	void Start() {
		StartCoroutine(_resize());
	}

	IEnumerator _resize() {
		//adjust box collider to fit size of text in text box
		//has to execute after first udpate due to content size fitter
		yield return new WaitForEndOfFrame();
		RectTransform rt = transform as RectTransform;
		bc.size = new Vector2(rt.rect.width + 10, rt.rect.height + 10);
		bc.offset = new Vector2(0, rt.rect.height / 2);
	}

	public void setTarget(Transform target, string s) {
		follow = target;
		txt.text = s;
	}

	public void reCenter() {
		transform.position = follow.position;
	}
	
	// Update is called once per frame
	void Update () {
		//i dont know how on earth the z position screws up when you zoom
		//but it needs to be constantly set to 0 or else it goes out of camera cull
		Vector3 tmp = transform.position;
		tmp.z = 0;
		transform.position = tmp;

		tmp = follow.position;
		tmp.z = 0;
		tmp.y += .5f;
		sj.connectedAnchor = tmp;

		Vector3 tmp2 = transform.position;
		tmp2.z = 0;


		lr.SetPosition(0, tmp2);
		lr.SetPosition(1, tmp);
		float zw = Camera.main.orthographicSize/100f;
		lr.SetWidth(zw,zw);
	}
}
