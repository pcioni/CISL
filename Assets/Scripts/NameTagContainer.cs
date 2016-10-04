using UnityEngine;
using System.Collections;

public class NameTagContainer : MonoBehaviour
{
    public Transform follow;
    [SerializeField]
    private SpringJoint2D m_springJoint;

    [SerializeField]
    private NameTag[] m_nameTags;
    [SerializeField]
    private NameTagSlot[] m_nameTagSlots;

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
    }

    public void SetTarget(Transform target, string name)
    {
        follow = target;
        m_nameTags[0].setTarget(m_nameTagSlots[0].transform, name,follow);
    }

    public void ReCenter()
    {
        transform.position = follow.position;

        foreach (NameTag nameTag in m_nameTags)
        {
            nameTag.reCenter();
        }
    }
}
