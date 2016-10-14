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
    public BoxCollider2D m_metaBoxColliders;

    [SerializeField]
    public BoxCollider2D m_baseBoxCollider;

	// Update is called once per frame
	void Update()
	{
		Vector3 tmp = transform.position;
		tmp.z = 0;
		transform.position = tmp;

		tmp = follow.position; //TODO: consider using Camera.main.GetScreenSpaceFromWorldCoordinate
		tmp.z = 0;
		tmp.y += .5f;

		float zw = Camera.main.orthographicSize / 100f;
		int i = 0;
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
        container.m_metaBoxColliders.bounds.SetMinMax(container.m_baseBoxCollider.bounds.min, container.m_baseBoxCollider.bounds.max);

        m_nameTags.RemoveAll(item => item == null);
		container.m_nameTags.RemoveAll(item => item == null);

		m_nameTags.Remove(container.m_originalNameTag);

		if (container.m_nameTags.Contains(container.m_originalNameTag) == false)
		{
			container.m_nameTags.Add(container.m_originalNameTag);
		}

	}

	public void CollsionStart(NameTagContainer container)
	{
		if (container == null || m_nameTags == null)
		{
			return;
		}

        container.m_metaBoxColliders.bounds.Encapsulate(m_baseBoxCollider.bounds.min);

        container.m_metaBoxColliders.bounds.Encapsulate(m_baseBoxCollider.bounds.max);


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

		RectTransform t = this.GetComponent<RectTransform>();
		int j = 0;

		//sort the nametags by y value
		

		foreach (NameTag nt in m_nameTags.OrderBy(go => go.transform.position.y))
		{
			nt.SetNewTarget(new Vector3(t.position.x + t.rect.width*5.0f, t.position.y + t.rect.height / 2 - nt.GetComponent<RectTransform>().lossyScale.y * 4 * ++j, 0));
		}
	}
}