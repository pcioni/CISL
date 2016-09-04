using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NameTag : MonoBehaviour {

	public Transform follow;
	private Text txt;
	private SpringJoint2D sj;
	private BoxCollider2D bc;
	private LineRenderer lr;
	//private Rigidbody2D rb;

	// Use this for initialization
	void Awake () {
		sj = GetComponent<SpringJoint2D>();
		txt = GetComponentInChildren<Text>();
		bc = GetComponent<BoxCollider2D>();
		lr = GetComponent<LineRenderer>();
		//rb = GetComponent<Rigidbody2D>();
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
	
	// Update is called once per frame
	void Update () {
		Vector3 tmp = follow.position;
		tmp.y += .5f;
		sj.connectedAnchor = tmp;

		Vector3 tmp2 = transform.position;
		tmp2.z = 0;


		lr.SetPosition(0, tmp2);
		lr.SetPosition(1, tmp);
		float zw = Camera.main.orthographicSize/100f;
		lr.SetWidth(zw,zw);

		/*if(rb.velocity.sqrMagnitude > 0) {
			bc.isTrigger = false;
		}else {
			bc.isTrigger = true;
		}*/
	}

	/*void OnTriggerEnter2D(Collider2D col) {
		var rel = transform.position - col.transform.position;
		if (rel.y > 0.5f) // if we are over the other
			rb.AddForce(-rel * 10, ForceMode2D.Impulse);
		
	}*/
}
