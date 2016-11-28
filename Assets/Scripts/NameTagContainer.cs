using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NameTagContainer : MonoBehaviour
{
	public Transform follow;

	[SerializeField]
	public NameTag m_originalNameTag;
	[SerializeField]
	public List<NameTag> m_nameTags = new List<NameTag>();
	[SerializeField]
	private GameObject m_slot;

	private Transform m_nextSlotPosition;

    [SerializeField]
    public BoxCollider2D m_groupCollisionBox;

    [SerializeField]
    public BoxCollider2D m_nodeCollisionBox;

    [SerializeField]
    private GameObject m_nodeCollider;
	[SerializeField]
	private GameObject m_groupCollider;


    public GameObject m_labelPanel;
    [SerializeField]
    private float m_labelXOffset;
    [SerializeField]
    private float m_labelVPadding;

    private Vector3 m_nodeOriginalPosition;
	private Vector3 m_groupOiginalPosition;
    
    void Start()
    {
		captureOriginalPositions ();
        if (!m_nameTags.Contains(m_originalNameTag))
        {
            m_nameTags.Add(m_originalNameTag);
            // TODO: debug this
            // adding this without "if" check fixes "diocletian" 
            // but results in null nodes being added as well
            // why is the currently focused node different then all the rest?
        }

    }

    private bool initialized = false;
    void initialize()
    {
        // stretch / shrink width of collider to size of text panel
        Vector3 scale = m_labelPanel.transform.lossyScale;
        float w = m_labelPanel.GetComponent<RectTransform>().rect.width + m_labelXOffset;
        float h = m_nodeCollisionBox.size.y;
        m_nodeCollisionBox.size = new Vector2(w, h);
        m_nodeCollisionBox.offset = new Vector2(-w / 2, 0f);

        updateLabelPositions();

        initialized = true;
    }

    void captureOriginalPositions () {
        //m_nodeOriginalPosition = m_nodeCollider.transform.position;
        //m_groupOiginalPosition = m_groupCollider.transform.position;
        Vector3? markerPos = m_originalNameTag.getMarkerPosition();//Camera.main.WorldToScreenPoint(m_originalNameTag.getMarkerPosition());


        if(markerPos == null)
        {
            //do nothing
            return;
        }
        m_nodeOriginalPosition = (Vector3)markerPos;
        m_nodeOriginalPosition.z = 0f;
        m_groupOiginalPosition = m_groupCollider.transform.position;
        m_groupOiginalPosition.z = 0f;
    }

    void FixedUpdate()
    {
        m_nodeCollider.transform.position = m_nodeOriginalPosition;
        m_groupCollider.transform.position = m_groupOiginalPosition;

    }

    void LateUpdate()
    {
        m_nodeCollider.transform.position = m_nodeOriginalPosition;
        m_groupCollider.transform.position = m_groupOiginalPosition;

    }

    // Update is called once per frame
    void Update()
	{
        m_nodeCollider.transform.position = m_nodeOriginalPosition;
        m_groupCollider.transform.position = m_groupOiginalPosition;

        Vector3 tmp = transform.position;
		tmp.z = 0;
		transform.position = tmp;

        if (!initialized) initialize();
    }

    public bool DoesNotHaveNameTags()
    {
        return m_nameTags.Count > 0;
    }

    public bool ContainsContainer(NameTagContainer other)
    {
        return m_nameTags.Contains(other.m_originalNameTag);
    }

	public void SetTarget(Transform target, string name)
	{
		follow = target;
		captureOriginalPositions ();

        // TODO: this seems to duplicate functionality of updateLabelPositions();
        for (int i = 0; i < m_nameTags.Count; i++)
		{			
			m_nameTags[i].setTarget((GameObject.Instantiate(
				m_slot, 
				target.position, 
				target.rotation, 
				target
			) as GameObject).GetComponent<NameTagSlot>(), name, follow);
		}

        updateLabelPositions(); // TODO: check placement of these calls
        EventManager.TriggerEvent(EventManager.EventType.LABEL_COLLISION_CHECK, "");
    }

	//TODO: Part of demo hack for rescalable timeline, remove later!
	public void UpdateTarget(Transform target)
	{
		follow = target;
		captureOriginalPositions ();

        updateLabelPositions(); // TODO: check placement of these calls
        EventManager.TriggerEvent(EventManager.EventType.LABEL_COLLISION_CHECK, "");
	}//end method UpdateTarget
	
    public void ReCenter()
	{
		transform.position = new Vector3(follow.position.x, follow.position.y, 0);

		foreach (NameTag nameTag in m_nameTags)
		{
			nameTag.reCenter();
		}
	}

	public void CollisionEnd(NameTagContainer container)
	{
		if (container == null)
		{
			return;
		}

		m_nameTags.Remove(container.m_originalNameTag);

		if (container.m_nameTags.Contains(container.m_originalNameTag) == false)
		{
			container.m_nameTags.Add(container.m_originalNameTag);
		}

        ResizeGroupCollisionBox();
        container.ResizeGroupCollisionBox();
    }

    public void CollisionStart(NameTagContainer container)
	{
		if (container == null || m_nameTags == null)
		{
			return;
		}

        // adopt the other nametags
        for (int i = 0; i < container.m_nameTags.Count; i++)
		{
			if (this.m_nameTags.Contains(container.m_nameTags[i]) == false)
			{
				this.m_nameTags.Add(container.m_nameTags[i]);
			}
		}

        // remove nametags from other container
		foreach (NameTag nt in m_nameTags)
		{
			container.m_nameTags.Remove(nt);
		}

        ResizeGroupCollisionBox();
    }

    public void ResizeGroupCollisionBox()
    {
        Vector2 m_center = new Vector2(0f, 0f);
        Vector2 m_size = new Vector2(1f,1f);

        //Now we calculate the maximum and minimum based on our contained nametags
        if (m_nameTags.Count > 1)
        {

            Vector2 max = new Vector2(-Mathf.Infinity, -Mathf.Infinity);
            Vector2 min = new Vector2(Mathf.Infinity, Mathf.Infinity);
            Vector2 scale = m_labelPanel.GetComponent<Transform>().lossyScale;
            float height = m_nameTags.Count * m_labelPanel.GetComponent<RectTransform>().rect.height + (m_nameTags.Count - 1) * m_labelVPadding / scale.y;

            foreach (NameTag nt in m_nameTags)
            {
                Transform ntTRNS = nt.m_curContainer.m_nodeCollisionBox.transform;
                BoxCollider2D ntCLDR = nt.m_curContainer.m_nodeCollisionBox;
                float xMax = ntTRNS.position.x / scale.x + ntCLDR.offset.x + ntCLDR.size.x / 2;
                if (xMax > max.x)
                {
                    max.x = xMax;
                }


                float yMax = ntTRNS.position.y / scale.y + ntCLDR.offset.y + ntCLDR.size.y / 2;
                if (yMax > max.y)
                {
                    max.y = yMax;
                }

                float xMin = ntTRNS.position.x / scale.x + ntCLDR.offset.x - ntCLDR.size.x / 2;
                if (xMin < min.x)
                {
                    min.x = xMin;
                }


                float yMin = ntTRNS.position.y / scale.y + ntCLDR.offset.y - ntCLDR.size.y / 2;
                if (yMin < min.y)
                {
                    min.y = yMin;
                }
            }


            m_size = max -  min;

            Vector2 tempCenter = new Vector2(m_size.x, m_size.y);
            tempCenter /= 2;
            tempCenter += min;
            m_center = tempCenter - new Vector2(m_nodeOriginalPosition.x / scale.x, m_nodeOriginalPosition.y / scale.y);

            m_size.y = height;
        }
        else
        {
            m_size = new Vector2(30f, 30f);
            m_center = new Vector2(0, 0);
        }

        m_groupCollisionBox.size = m_size;
        m_groupCollisionBox.offset = m_center;

        updateLabelPositions();
    }

    public void updateLabelPositions()
    {
        Vector2 scale = new Vector2(1f,1f);
        Vector2 size = new Vector2(0f,0f);
        Vector3 p = new Vector3(m_nodeOriginalPosition.x, m_nodeOriginalPosition.y, 0f);
        Vector3 pScaled;

        if (m_nameTags.Count > 1)
        {
            scale = m_groupCollider.GetComponent<Transform>().lossyScale;
            size = new Vector2(m_groupCollisionBox.size.x, m_groupCollisionBox.size.y);
            p += new Vector3(m_groupCollisionBox.offset.x * scale.x, m_groupCollisionBox.offset.y * scale.y, 0f);
        }
        else
        {
            scale = m_nodeCollider.GetComponent<Transform>().lossyScale;
            size = new Vector2(0f, m_nodeCollisionBox.size.y);
        }

        pScaled = new Vector3(p.x, p.y, 0f);
        pScaled.x += size.x * scale.x / 2 + m_labelXOffset;
        pScaled.y += size.y * scale.y / 2;

        //sort the nametags by y value
        foreach (NameTag nt in m_nameTags.OrderBy(go => -((Vector3)go.getMarkerPosition()).y))
        {
            nt.SetNewTarget(pScaled);
            pScaled.y -= (m_labelPanel.GetComponent<RectTransform>().rect.height) * scale.y + m_labelVPadding;
        }
    }
}
