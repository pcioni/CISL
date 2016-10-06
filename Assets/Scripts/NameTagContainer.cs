﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NameTagContainer : MonoBehaviour
{
    public Transform follow;
    [SerializeField]
    private SpringJoint2D m_springJoint;

    [SerializeField]
    public NameTag m_originalNameTag;
    [SerializeField]
    public List<NameTag> m_nameTags;
    [SerializeField]
    private GameObject m_slot;

    private Transform m_nextSlotPosition;

    // Update is called once per frame
    void Update()
    {
        Vector3 tmp = transform.position;
        tmp.z = 0;
        transform.position = tmp;

        tmp = follow.position;
        tmp.z = 0;
        tmp.y += .5f;
        m_springJoint.connectedAnchor = tmp;

        float zw = Camera.main.orthographicSize / 100f;
        int i = 0;

        RectTransform t = this.GetComponent<RectTransform>();

        foreach (NameTag nt in m_nameTags)
        {
            nt.SetNewTarget(new Vector3(t.rect.xMax, t.rect.yMax - 20.0f * ++i, 0));
        }
    }


    public void SetNextAvailableSlot(Transform slot)
    {
        this.m_nextSlotPosition = slot;
    }

    public GameObject GetNextAvailableSlot()
    {
        return m_nextSlotPosition.gameObject;
    }

    public void SetTarget(Transform target, string name)
    {
        follow = target;

        for (int i = 0; i < m_nameTags.Count; i++)
        {
            m_nameTags[i].setTarget((GameObject.Instantiate(m_slot, target.position, target.rotation, target) as GameObject).GetComponent<NameTagSlot>(), name, follow);
        }
    }

    public void ReCenter()
    {
        transform.position = follow.position;

        foreach (NameTag nameTag in m_nameTags)
        {
            nameTag.reCenter();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        NameTagContainer container = other.gameObject.GetComponent<NameTagContainer>();

        if (container == null)
        {
            return;
        }

        m_nameTags.RemoveAll(item => item == null);
        container.m_nameTags.RemoveAll(item => item == null);

        m_nameTags.Remove(container.m_originalNameTag);

        if (container.m_nameTags.Contains(container.m_originalNameTag) == false)
        {
            container.m_nameTags.Add(container.m_originalNameTag);
        }

    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.transform.position.y < transform.position.y)
        {
            NameTagContainer container = other.gameObject.GetComponent<NameTagContainer>();

            if (container == null || m_nameTags == null)
            {
                return;
            }

            m_nameTags.RemoveAll(item => item == null);
            container.m_nameTags.RemoveAll(item => item == null);

            for (int i = 0; i < container.m_nameTags.Count; i++)
            {
                // adopt the other nametag

                if (this.m_nameTags.Contains(container.m_nameTags[i]) == false)
                {
                    this.m_nameTags.Add(container.m_nameTags[i]);
                }
            }

            foreach (NameTag nt in m_nameTags)
            {
                container.m_nameTags.Remove(nt);
            }
        }
    }
}
