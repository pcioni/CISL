using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NameTag : MonoBehaviour {

	public NameTagSlot follow;
	private Text txt;
	private SpringJoint2D sj;
	private BoxCollider2D bc;
	private LineRenderer lr;
    private Transform m_marker;
    [SerializeField]private RectTransform m_rectTransform;
    [SerializeField]
    private NameTagContainer m_originalContainer;

    [SerializeField]
    private RectTransform m_childTransform;

	void Awake () {
		sj = GetComponent<SpringJoint2D>();
		txt = GetComponentInChildren<Text>();
		bc = GetComponent<BoxCollider2D>();
		lr = GetComponent<LineRenderer>();
		lr.SetVertexCount(2);
	}

    public void SetNewTarget(Vector3 targetPosition)
    {
        follow.transform.position = targetPosition;
    }

    public NameTagSlot GetNextSlot()
    {
        return follow;
    }

    void Start() {
		StartCoroutine(_resize());
	}

	IEnumerator _resize() {
		//adjust box collider to fit size of text in text box
		//has to execute after first udpate due to content size fitter
		yield return new WaitForEndOfFrame();
		bc.size = new Vector2(m_childTransform.rect.width + 10, m_childTransform.rect.height + 10);
		bc.offset = new Vector2(m_childTransform.rect.width / 2+5, -(m_childTransform.rect.height+10)/2);
	}

	public void setTarget(NameTagSlot target, string s,Transform marker) {
        this.m_marker = marker;
        follow = target;
		txt.text = s;
	}

    public void reCenter() {
		transform.position = follow.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		//i dont know how on earth the z position screws up when you zoom
		//but it needs to be constantly set to 0 or else it goes out of camera cull
		Vector3 tmp = transform.position;
		tmp.z = 0;
		transform.position = tmp;

		tmp = follow.transform.position;
		tmp.z = 0;
		tmp.y += .5f;
		sj.connectedAnchor = tmp;

		Vector3 tmp2 = m_marker.position;
		tmp2.z = 0;


		lr.SetPosition(0, tmp2);
		lr.SetPosition(1, new Vector3(m_rectTransform.position.x + m_rectTransform.lossyScale.x * m_rectTransform.rect.width/2, m_rectTransform.position.y - m_rectTransform.lossyScale.y *10.0f, m_rectTransform.position.z));
		float zw = Camera.main.orthographicSize/100f;
		lr.SetWidth(zw,zw);
    }
}
