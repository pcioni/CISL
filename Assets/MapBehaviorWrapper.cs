using UnityEngine;
using System.Collections;

public class MapBehaviorWrapper : MonoBehaviour {

    UnitySlippyMap.Map.MapBehaviour mapBehavior = null;

    [SerializeField]
    private float m_latitude;
    [SerializeField]
    private float m_longitude;
    [SerializeField]
    private float m_zoom;
    // Use this for initialization
    void Start () {
        StartCoroutine(WaitForMapBehaviorAppearance());
	}
	
    private IEnumerator WaitForMapBehaviorAppearance()
    {
        while (mapBehavior == null)
        {
            mapBehavior = GameObject.Find("[Map]").GetComponent<UnitySlippyMap.Map.MapBehaviour>();
            yield return new WaitForEndOfFrame();
        }

        InitLocation();
    }

    private void InitLocation()
    {
        mapBehavior.CenterWGS84 = new double[2] { m_longitude, m_latitude };
        Debug.Log(mapBehavior.CurrentZoom);
        Debug.Log(mapBehavior.MinZoom);
        Debug.Log(mapBehavior.MaxZoom);
        Debug.Log(mapBehavior.ZoomStepLowerThreshold);
        Debug.Log(mapBehavior.ZoomStepUpperThreshold);
        mapBehavior.CurrentZoom = mapBehavior.MinZoom;
        mapBehavior.Zoom(0.0f);
    }
}
