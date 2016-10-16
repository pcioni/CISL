using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DebugDrawCollider : MonoBehaviour {
    [SerializeField]
    private BoxCollider2D m_groupCollisionBox;
    [SerializeField]
    private Transform m_groupCollisionImage;

    [SerializeField]
    private float m_scale = 140.0f;
    // Use this for initialization
    void Start () {
    }

    void Awake()
    {

        if (!DebugMode.MS_ACTIVE)
        {
            Destroy(m_groupCollisionImage.gameObject);
            Destroy(this.transform.parent.gameObject);
        }
    }

    // Update is called once per frame
    void Update() {
    }
}
