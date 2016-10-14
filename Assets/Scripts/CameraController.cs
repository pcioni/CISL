using UnityEngine;
using System.Collections;
using System.Linq;

public class CameraController : MonoBehaviour
{
    private Vector3 last_mouse_position;
    public float minZoom = 0f;
    public float maxZoom = 100f;
    public float orthographicMomentum = 0.0f;

    private float m_scrollTime = 0.0f;

    [SerializeField]
    private float m_maxCameraAcceleration = 4.0f;
    [SerializeField]
    private float m_maxCameraVelocity = 10.0f;
    [SerializeField]
    private AnimationCurve m_scrollCurve;
    [SerializeField]
    private AnimationCurve m_distanceCurve;


    void Update()
    {
        orthographicMomentum = orthographicMomentum / 1.1f;
        m_scrollTime = m_scrollTime / 1.01f;

        orthographicMomentum = Mathf.Abs(orthographicMomentum) < 0.05f ? 0.0f : orthographicMomentum;

        //Pan camera with mouse
        if (Input.GetMouseButtonDown(1))
            last_mouse_position = Input.mousePosition;
        if (Input.GetMouseButton(1))
        {
            Vector3 delta = last_mouse_position - Input.mousePosition;
            Camera.main.transform.Translate(delta.x * .1f, delta.y * .1f, 0);
            last_mouse_position = Input.mousePosition;
            EventManager.TriggerEvent(EventManager.EventType.INTERFACE_PAN, Camera.main.orthographicSize.ToString());
        }

        //scroll
        ZoomOrthoCamera(Camera.main.ScreenToWorldPoint(Input.mousePosition), Input.GetAxis("Mouse ScrollWheel"));

    }

    // ortographic camera zoom towards a point in world coordinates. 
    // negative amount zooms in, positive zooms out
    // TODO: stop camera movement when at zoom limit
    void ZoomOrthoCamera(Vector3 zoomTowards, float amount)
    {


        if (Mathf.Abs(amount) < .1f)
        {
            return;
        }

        if (Mathf.Sign(amount) != Mathf.Sign(orthographicMomentum))
        {
            orthographicMomentum = 0.0f;
            m_scrollTime = 0.0f;
        }

        //Debug.Log(m_distanceCurve.Evaluate(Mathf.InverseLerp(6.0f, 100.0f, GetComponent<Camera>().orthographicSize)));

        m_scrollTime += Time.deltaTime;
        amount += Mathf.Sign(amount) * m_scrollCurve.Evaluate(m_scrollTime) * 10.0f + Mathf.Sign(amount) * m_distanceCurve.Evaluate(Mathf.InverseLerp(6.0f, 100.0f, GetComponent<Camera>().orthographicSize)) * 10.0f;
        orthographicMomentum += amount;

        // Calculate how much we will have to move towards the zoomTowards position
        float multiplier = (1.0f / GetComponent<Camera>().orthographicSize * amount);

        // Zoom camera
        GetComponent<Camera>().orthographicSize -= amount;

        // Limit zoom
        GetComponent<Camera>().orthographicSize = Mathf.Clamp(GetComponent<Camera>().orthographicSize, minZoom, maxZoom);
        EventManager.TriggerEvent(amount < 0 ? EventManager.EventType.INTERFACE_ZOOM_OUT : EventManager.EventType.INTERFACE_ZOOM_IN, Camera.main.orthographicSize.ToString());

        //Check for NameTagCollsions
        NameTagContainer[] containers = (from u in GameObject.FindGameObjectsWithTag("NameTagContainer") select u.GetComponent<NameTagContainer>()).ToArray();
        for(int i = 0; i< containers.Length; i++)
        {
            for(int j = i; j < containers.Length; j++)
            {
                if (i != j && containers[i].m_metaBoxColliders.bounds.Intersects(containers[j].m_metaBoxColliders.bounds))
                {
                    containers[i].CollsionStart(containers[j]);
                }
                else if(i != j)
                {
                    containers[i].CollisionEnd(containers[j]);

                }
            }
        }

    }

}
