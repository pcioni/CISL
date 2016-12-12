using UnityEngine;
using System.Collections;

public class DebugMaps : MonoBehaviour {
    [SerializeField]
    private GameObject m_camera;
	// Use this for initialization
	void Start () {
	
	}

    // Update is called once per frame
    void Update() {
        if (DebugMode.MS_ACTIVE)
        {
            RecursivelySetLayer(GameObject.Find("[Map]"), 14);
            m_camera.active = false;
        }
        else
        {
            RecursivelySetLayer(GameObject.Find("[Map]"), 0);
            m_camera.active = true;
        }
	}

    void RecursivelySetLayer(GameObject go, int layer)
    {
        go.layer = layer;
        foreach(Transform t in go.transform)
        {
            RecursivelySetLayer(t.gameObject, layer);
        }
    }
}
