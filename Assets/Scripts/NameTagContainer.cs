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

		tmp = follow.position;
		tmp.z = 0;
		tmp.y += .5f;

		float zw = Camera.main.orthographicSize / 100f;

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


 //   public void SetNextAvailableSlot(Transform slot)
	//{
	//	this.m_nextSlotPosition = slot;
	//}

	//public GameObject GetNextAvailableSlot()
	//{
	//	return m_nextSlotPosition.gameObject;
	//}

	public void SetTarget(Transform target, string name)
	{
		follow = target;

        //TODO: set the parent Collision objects to be parented to the follow
        //m_colliderObjects.transform.SetParent(target);
		captureOriginalPositions ();

        // TODO: this seems to duplicate functionality of 
        for (int i = 0; i < m_nameTags.Count; i++)
		{
			m_nameTags[i].setTarget((GameObject.Instantiate(
				m_slot, 
				target.position, 
				target.rotation, 
				target
			) as GameObject).GetComponent<NameTagSlot>(), name, follow);
		}

        updateLabelPositions();

        CameraController.CollisionDetection();
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

        container.m_groupCollisionBox.bounds.SetMinMax(
            new Vector3(0f, 0f, 0f), 
            new Vector3(2.5f, 2f, 0f)
            );

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

    public void CollsionStart(NameTagContainer container)
	{
		if (container == null || m_nameTags == null)
		{
			return;
		}

        container.m_groupCollisionBox.bounds.Encapsulate(m_nodeCollisionBox.bounds.min);

        container.m_groupCollisionBox.bounds.Encapsulate(m_nodeCollisionBox.bounds.max);


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
