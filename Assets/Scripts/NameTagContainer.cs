﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NameTagContainer : MonoBehaviour
{
	public Transform follow;

	[SerializeField]
	public NameTag m_originalNameTag;
	[SerializeField]
	public List<NameTag> m_nameTags;
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

    }

    private bool initialized = false;
    void initialize()
    {
        // stretch / shrink width of collider to size of text panel
        Vector3 scale = m_labelPanel.transform.lossyScale;
        float w = m_labelPanel.GetComponent<RectTransform>().rect.width * scale.x; // TODO: why isn't this working as 
        float h = m_nodeCollisionBox.size.y;
        m_nodeCollisionBox.size = new Vector2(w, h);

        updateLabelPositions();

        initialized = true;
    }

    void captureOriginalPositions () {
		m_nodeOriginalPosition = m_nodeCollider.transform.position;
		m_groupOiginalPosition = m_groupCollider.transform.position;
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
        
        Bounds b = m_groupCollisionBox.bounds;
        

        b.SetMinMax(
            new Vector3(0f, 0f, 0f),
            new Vector3(2.5f, 2f, 0f)
            );

        m_nodeCollisionBox.bounds.Encapsulate(b);
        
        m_groupCollisionBox.offset = b.center;
        m_groupCollisionBox.size = b.size;

        //remove null items
        m_nameTags.RemoveAll(item => item == null);
		container.m_nameTags.RemoveAll(item => item == null);

		m_nameTags.Remove(container.m_originalNameTag);

		if (container.m_nameTags.Contains(container.m_originalNameTag) == false)
		{
			container.m_nameTags.Add(container.m_originalNameTag);
		}

        updateLabelPositions();
    }

    public void CollisionStart(NameTagContainer container)
	{
		if (container == null || m_nameTags == null)
		{
			return;
		}


        // remove null items in m_nametags for both containers
        m_nameTags.RemoveAll(item => item == null);
		container.m_nameTags.RemoveAll(item => item == null);

		for (int i = 0; i < container.m_nameTags.Count; i++)
		{
			// adopt the other nametags
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




        

        if (m_nameTags.Count > 0) {

            Vector2 max = new Vector2(0, 0);
            Vector2 min = new Vector2(Mathf.Infinity, Mathf.Infinity);

            foreach (NameTag nt in m_nameTags)
            {
                float xMax = nt.m_curContainer.m_groupCollisionBox.transform.position.x + nt.m_curContainer.m_groupCollisionBox.offset.x + nt.m_curContainer.m_groupCollisionBox.size.x / 2;
                if ( xMax> max.x)
                {
                    max.x = xMax;
                }


                float yMax = nt.m_curContainer.m_groupCollisionBox.transform.position.y + nt.m_curContainer.m_groupCollisionBox.offset.y + nt.m_curContainer.m_groupCollisionBox.size.y / 2;
                if (yMax > max.y)
                {
                    max.y = yMax;
                }

                float xMin = nt.m_curContainer.m_groupCollisionBox.transform.position.x + nt.m_curContainer.m_groupCollisionBox.offset.x - nt.m_curContainer.m_groupCollisionBox.size.x / 2;
                if (xMin < min.x)
                {
                    min.x = xMin;
                }


                float yMin = nt.m_curContainer.m_groupCollisionBox.transform.position.y + nt.m_curContainer.m_groupCollisionBox.offset.y - nt.m_curContainer.m_groupCollisionBox.size.y / 2;
                if (yMin < min.y)
                {
                    min.y = yMin;
                }
            }

            Vector2 size = max - min;

            Vector2 center = min;

            m_groupCollisionBox.offset = new Vector2(m_groupCollisionBox.transform.position.x, m_groupCollisionBox.transform.position.y) - center;
            m_groupCollisionBox.size = size;
        }
        else
        {
            Vector2 size = new Vector2(0,0);

            Vector2 center = new Vector2(0, 0);

            m_groupCollisionBox.offset = new Vector2(m_groupCollisionBox.transform.position.x, m_groupCollisionBox.transform.position.y) - center;
            m_groupCollisionBox.size = size;

        }
        updateLabelPositions();
    }

    public void updateLabelPositions()
    {
        Vector2 scale = m_labelPanel.GetComponent<Transform>().lossyScale;
        Vector3 p = m_groupCollider.transform.position;
        p.x += (m_groupCollisionBox.size.x + m_labelXOffset) * scale.x;
        p.z = 0f;

        //sort the nametags by y value
        foreach (NameTag nt in m_nameTags.OrderBy(go => -go.getMarkerPosition().y))
        {
            nt.SetNewTarget(p);
            p.y -= (m_labelPanel.GetComponent<RectTransform>().rect.height + m_labelVPadding) * scale.y;
        }
    }
}
