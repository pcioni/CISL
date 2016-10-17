using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DebugDrawCollider : MonoBehaviour {
    [SerializeField]
    private BoxCollider2D m_groupCollisionBox;
    [SerializeField]
    private Transform m_groupCollisionImage;

    [SerializeField]
    private LineRenderer m_lineRenderer;

    [SerializeField]
    private float m_scale = 140.0f;
    // Use this for initialization
    void Start () {
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
    void Update() {
        //add bottom left to linerenderer
        m_lineRenderer.SetPosition(0, new Vector3(m_groupCollisionBox.bounds.min.x, m_groupCollisionBox.bounds.min.y, 0));
        //add bottom right 
        m_lineRenderer.SetPosition(1, new Vector3(m_groupCollisionBox.bounds.max.x, m_groupCollisionBox.bounds.min.y, 0));

        //add top right
        m_lineRenderer.SetPosition(2, new Vector3(m_groupCollisionBox.bounds.max.x, m_groupCollisionBox.bounds.max.y, 0));

        //add top left
        m_lineRenderer.SetPosition(3, new Vector3(m_groupCollisionBox.bounds.min.x, m_groupCollisionBox.bounds.max.y, 0));

        //Add bottom left again
        m_lineRenderer.SetPosition(4, new Vector3(m_groupCollisionBox.bounds.min.x, m_groupCollisionBox.bounds.min.y, 0));


    }
}
