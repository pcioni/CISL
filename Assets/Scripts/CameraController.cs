using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    private static CameraController ms_instance;
    private Vector3 last_mouse_position;
    public float minZoom = 0f;
    public float maxZoom = 100f;
    public float orthographicMomentum = 0.0f;

    private Camera targetCam;

    public Camera[] cameras = new Camera[2];

    private float m_scrollTime = 0.0f;

    [SerializeField]
    private float m_maxCameraAcceleration = 4.0f;
    [SerializeField]
    private float m_maxCameraVelocity = 10.0f;
    [SerializeField]
    private AnimationCurve m_scrollCurve;
    [SerializeField]
    private AnimationCurve m_distanceCurve;

    void Awake()
    {
        ms_instance = this;
    }

    void Update()
    { 
        foreach (Camera cam in cameras) {
            if (cam.pixelRect.Contains(Input.mousePosition)) {
                targetCam = cam;
                break;
            } else
            {
                targetCam = null;
            }
        }
        orthographicMomentum = orthographicMomentum / 1.1f;
        m_scrollTime = m_scrollTime / 1.01f;

        orthographicMomentum = Mathf.Abs(orthographicMomentum) < 0.05f ? 0.0f : orthographicMomentum;

        //Pan camera with mouse
        if (Input.GetMouseButtonDown(1)) //Store the click position wheneve the mouse is clicked
            last_mouse_position = Input.mousePosition;
        if (Input.GetMouseButton(1) && targetCam != null) //While the mouse is down translate the position of the camera
        {
            Vector3 delta = targetCam.ScreenToWorldPoint(last_mouse_position) - targetCam.ScreenToWorldPoint(Input.mousePosition);
            targetCam.transform.Translate(delta.x, delta.y, 0); //TODO: check the math on this, it could be the root of strange parallax
            last_mouse_position = Input.mousePosition;
            EventManager.TriggerEvent(EventManager.EventType.INTERFACE_PAN, targetCam.orthographicSize.ToString());
        }

        //scroll
        ZoomOrthoCamera(Camera.main.ScreenToWorldPoint(Input.mousePosition), Input.GetAxis("Mouse ScrollWheel"));

    }

    // ortographic camera zoom towards a point in world coordinates. 
    // negative amount zooms in, positive zooms out
    // TODO: stop camera movement when at zoom limit
    void ZoomOrthoCamera(Vector3 zoomTowards, float amount)
    {
        if (targetCam == null) return;

        // Create a deadzone to avoid scroll wheel noise
        //
        if (Mathf.Abs(amount) < .1f)
        {
            return;
        }

        // If we're moving in the opposite direction of the momentum kill all momentum immediately
        //
        if (Mathf.Sign(amount) != Mathf.Sign(orthographicMomentum))
        {
            orthographicMomentum = 0.0f;
            m_scrollTime = 0.0f;
        }

        //Debug.Log(m_distanceCurve.Evaluate(Mathf.InverseLerp(6.0f, 100.0f, GetComponent<Camera>().orthographicSize)));

        m_scrollTime += Time.deltaTime;
        amount += Mathf.Sign(amount) * m_scrollCurve.Evaluate(m_scrollTime) * 10.0f + Mathf.Sign(amount) * m_distanceCurve.Evaluate(Mathf.InverseLerp(6.0f, 100.0f, targetCam.orthographicSize)) * 10.0f;
        orthographicMomentum += amount;

        // Calculate how much we will have to move towards the zoomTowards position
        float multiplier = (1.0f / targetCam.orthographicSize * amount);

        // Zoom camera
        targetCam.orthographicSize -= amount;

        // Limit zoom
        targetCam.orthographicSize = Mathf.Clamp(targetCam.orthographicSize, minZoom, maxZoom);
        EventManager.TriggerEvent(amount < 0 ? EventManager.EventType.INTERFACE_ZOOM_OUT : EventManager.EventType.INTERFACE_ZOOM_IN, Camera.main.orthographicSize.ToString());
    }

    private NameTagContainer[] containers;
    public static void CollisionDetection()
    {
        if (ms_instance.containers != null)
        {
            foreach (NameTagContainer ntc in ms_instance.containers)
            {
                foreach (NameTagContainer ntc2 in ms_instance.containers)
                {
                    if (ntc == ntc2)
                    {
                        continue;
                    }
                    ntc.CollisionEnd(ntc2);
                }
            }
        }

        //Refresh our lists of containers, group colliders and node colliders
        ms_instance.containers = (from u in GameObject.FindGameObjectsWithTag("NameTagContainer") select u.GetComponent<NameTagContainer>()).ToArray();

        //Check for NameTagCollsions
        //while (ms_instance.NodeOnNodeCheck(ms_instance.containers) == true) ;
        while (ms_instance.NodeOnNodeCheck(ms_instance.containers) == true)
        {
            //Resize the group containers each time after node on node check is performed
        }
    }


    public bool NoContainment(NameTagContainer containerA, NameTagContainer containerB)
    {
        return !containerA.ContainsContainer(containerB) && !containerB.ContainsContainer(containerA);
    }

    //Return true if there are new collisions
    bool NodeOnNodeCheck(NameTagContainer[] containers)
    {
        bool collisionDetected = false;
        for (int i = 0; i < containers.Length; i++)
        {
            for (int j = i + 1; j < containers.Length; j++)
            {

                NameTagContainer containerA = containers[i];
                NameTagContainer containerB = containers[j];

                BoxCollider2D aCollider;
                BoxCollider2D bCollider;

                switch (containerA.m_nameTags.Count)
                {
                    case 0:
                        continue;
                        //break;  // unreachable code
                    case 1:
                        aCollider = containerA.m_nodeCollisionBox;
                        break;
                    default:
                        aCollider = containerA.m_groupCollisionBox;
                        break;
                }

                switch (containerB.m_nameTags.Count)
                {
                    case 0:
                        continue;
                        //break; // unreachable code
                    case 1:
                        bCollider = containerB.m_nodeCollisionBox;
                        break;
                    default:
                        bCollider = containerB.m_groupCollisionBox;
                        break;
                }

                //TODO: zero out z of center? might be missing collision detection if objects are not at same depth
                //Vector3 centerTMP = bCollider.bounds.center;
                //centerTMP.z = 0;
                //bCollider.bounds.center = centerTMP;

                if (aCollider.bounds.Intersects(bCollider.bounds) && NoContainment(containerA, containerB))
                {
                    collisionDetected = true;
                    //merge the two colliders
                    if (containerA.m_nameTags.Count > containerB.m_nameTags.Count)
                    {
                        containerA.CollisionStart(containerB);
                    }
                    else
                    {
                        containerB.CollisionStart(containerA);
                    }
                }
            }
        }

        return collisionDetected;
    }

}
