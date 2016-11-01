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
        panel.SetActive(DebugMode.MS_ACTIVE);
        if (m_collider != null && (m_panelRTrans.sizeDelta.x != m_collider.size.x || m_panelRTrans.sizeDelta.y != m_collider.size.y))
        {
            m_panelRTrans.sizeDelta = new Vector2 (m_collider.size.x, m_collider.size.y);
            //panelRect.width = m_collider.size.x;
            //panelRect.height = m_collider.size.y;
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
