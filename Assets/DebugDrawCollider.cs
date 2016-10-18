using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DebugDrawCollider : MonoBehaviour
{
    [SerializeField]
    private BoxCollider2D m_groupCollisionBox;
    [SerializeField]
    private Transform m_groupCollisionImage;

    [SerializeField]
    private LineRenderer m_lineRenderer;

    [SerializeField]
    private float m_scale = 140.0f;
    // Use this for initialization
    void Start()
    {
    }

    void Awake()
    {

        if (!DebugMode.MS_ACTIVE)
        {
            Destroy(m_lineRenderer);
            Destroy(this.transform.parent.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //add top left to linerenderer
        m_lineRenderer.SetPosition(0, new Vector3(Mathf.Lerp(m_groupCollisionBox.bounds.min.x, m_groupCollisionBox.bounds.max.x, .99f), m_groupCollisionBox.bounds.min.y, 0)); //bottom left: min x min y
        m_lineRenderer.SetPosition(1, new Vector3(m_groupCollisionBox.bounds.max.x, Mathf.Lerp(m_groupCollisionBox.bounds.min.y, m_groupCollisionBox.bounds.max.y,.01f), 0)); //bottom left: min x min y

        //add bottom right 
        m_lineRenderer.SetPosition(2, new Vector3(m_groupCollisionBox.bounds.max.x, Mathf.Lerp(m_groupCollisionBox.bounds.min.y, m_groupCollisionBox.bounds.max.y,.99f), 0)); //bottom right max x min y
        m_lineRenderer.SetPosition(3, new Vector3(Mathf.Lerp(m_groupCollisionBox.bounds.min.x, m_groupCollisionBox.bounds.max.x, .99f), m_groupCollisionBox.bounds.max.y, 0)); //bottom right max x max y

        //add top right
        m_lineRenderer.SetPosition(4, new Vector3(Mathf.Lerp(m_groupCollisionBox.bounds.max.x, m_groupCollisionBox.bounds.min.x,.99f), m_groupCollisionBox.bounds.max.y, 0)); //top right max x max y
        m_lineRenderer.SetPosition(5, new Vector3(m_groupCollisionBox.bounds.min.x, Mathf.Lerp(m_groupCollisionBox.bounds.min.y, m_groupCollisionBox.bounds.max.y, .99f), 0)); //bottom right max x max y

        //add top left
        m_lineRenderer.SetPosition(6, new Vector3(m_groupCollisionBox.bounds.min.x, Mathf.Lerp(m_groupCollisionBox.bounds.min.y, m_groupCollisionBox.bounds.max.y, .01f), 0)); // max x max y
        m_lineRenderer.SetPosition(7, new Vector3(Mathf.Lerp(m_groupCollisionBox.bounds.max.x, m_groupCollisionBox.bounds.min.x, .99f), m_groupCollisionBox.bounds.min.y, 0));//**** min x min y

        //Add bottom left again
        m_lineRenderer.SetPosition(8, new Vector3(Mathf.Lerp(m_groupCollisionBox.bounds.min.x, m_groupCollisionBox.bounds.max.x, .99f), m_groupCollisionBox.bounds.min.y, 0)); //min x min y
        m_lineRenderer.SetPosition(9, new Vector3(m_groupCollisionBox.bounds.max.x, Mathf.Lerp(m_groupCollisionBox.bounds.min.y, m_groupCollisionBox.bounds.max.y, .01f), 0)); //min x max y


    }
}
