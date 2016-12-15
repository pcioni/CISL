using UnityEngine;
using System.Collections;

public class DebugMaps : MonoBehaviour {
    [SerializeField]
    private GameObject m_camera;
    [SerializeField]
    private GameObject m_slippyCamera;

	// Use this for initialization
	void Start () {
	
	}

    // Update is called once per frame
    void Update()
    {
        if (DebugMode.MS_ACTIVE)
        {
            m_slippyCamera.SetActive(true);
            m_camera.SetActive(false);
        }
        else
        {
            m_slippyCamera.SetActive(false);
            m_camera.SetActive(true);

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
