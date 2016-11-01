using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NameTag : MonoBehaviour {

	public NameTagSlot follow;
	private Text txt;
	private SpringJoint2D sj;
	private LineRenderer lr;
	private Transform m_marker;
	[SerializeField]private RectTransform m_rectTransform;
	[SerializeField]
	private NameTagContainer m_originalContainer;
	public NameTagContainer m_curContainer;
	[SerializeField] private Color tmpWhite_pnl;
	[SerializeField] private Color tmpRed_pnl;
	[SerializeField] private Color tmpWhite_lr;
	[SerializeField] private Color tmpRed_lr;


    [SerializeField]
	private RectTransform m_childTransform;

	void Awake () {
		sj = GetComponent<SpringJoint2D>();
		txt = GetComponentInChildren<Text>();
		lr = GetComponent<LineRenderer>();
		lr.SetVertexCount(2);
		lr.material = new Material(Shader.Find("Particles/Additive (Soft)"));
    }

    public void SetNewTarget(Vector3 targetPosition)
	{
		follow.transform.position = targetPosition;
	}

    public Vector3 getMarkerPosition()
    {
        return m_marker.position;
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
	}

	public void setTarget(NameTagSlot target, string s, Transform marker) {
		this.m_marker = marker;
		follow = target;
		txt.text = s;
	}


	void OnCollisionStay2D(Collision2D coll)
	{

	}

	public void reCenter() {
		transform.position = new Vector3(follow.transform.position.x, follow.transform.position.y, 0);
	}
	
	// Update is called once per frame
	void Update () {

		//i dont know how on earth the z position screws up when you zoom
		//but it needs to be constantly set to 0 or else it goes out of camera cull
		Vector3 tmp = transform.position;
		tmp.z = 0;
		transform.position = tmp;

//		tmp = m_marker.position;
		tmp = follow.transform.position;
		tmp.z = 0;
//		tmp.y += .5f;
		sj.connectedAnchor = tmp;

		Vector3 tmp2 = m_marker.position;
		tmp2.z = 0;


		lr.SetPosition(0, tmp2);
		lr.SetPosition(1, new Vector3(
				m_rectTransform.position.x + m_rectTransform.lossyScale.x * m_rectTransform.rect.width/2, 
				m_rectTransform.position.y - m_rectTransform.lossyScale.y * 10.0f, 
				m_rectTransform.position.z
			)
		);

		float zw = Camera.main.orthographicSize/100f;
		lr.SetWidth(zw,zw);

        if (m_marker.GetComponent<timelineNode>().state == timelineNode.focusState.IN)
        {
            this.GetComponentInChildren<Image>().color = tmpRed_pnl;
			lr.SetColors (tmpRed_lr, tmpRed_lr);
        }
        else
        {
            this.GetComponentInChildren<Image>().color = tmpWhite_pnl;
			lr.SetColors (tmpWhite_lr, tmpWhite_lr);
        }

    }
}
