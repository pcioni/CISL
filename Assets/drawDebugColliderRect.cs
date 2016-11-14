using UnityEngine;
using System.Collections;

public class drawDebugColliderRect : MonoBehaviour {
	[SerializeField]
	GameObject panel;
	private RectTransform m_panelRTrans;
	private BoxCollider2D m_collider;

	void Awake () {

		m_panelRTrans = panel.GetComponent<RectTransform>();

		try {
			m_collider = this.GetComponent<BoxCollider2D> ();
		} catch {
		}

        refresh();
    }

    void refresh()
    {
        // hide/show panel
        panel.SetActive(DebugMode.MS_ACTIVE);
        // resize panel to match BoxCollider2D
        if (m_collider != null && (m_panelRTrans.sizeDelta.x != m_collider.size.x || m_panelRTrans.sizeDelta.y != m_collider.size.y || !m_panelRTrans.position.Equals(m_collider.offset)))
        {
            m_panelRTrans.sizeDelta = new Vector2(m_collider.size.x, m_collider.size.y);
            m_panelRTrans.localPosition = m_collider.offset;


        }
    }

    // Use this for initialization
    void Start () {
	
	}

	// Update is called once per frame
	void Update () {
        refresh();
	}
}
