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
        if (Input.GetMouseButtonDown(1)) //Store the click position wheneve the mouse is clicked
            last_mouse_position = Input.mousePosition;
        if (Input.GetMouseButton(1)) //While the mouse is down translate the position of the camera
        {
            Vector3 delta = Camera.main.ScreenToWorldPoint(last_mouse_position) - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Camera.main.transform.Translate(delta.x, delta.y, 0); //TODO: check the math on this, it could be the root of strange parallax
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

        //Refresh our lists of containers, group colliders and node colliders

        foreach(NameTagContainer ntc in containers)
        {
            foreach (NameTagContainer ntc2 in containers)
            {
                ntc.CollisionEnd(ntc2);
            }
        }


        //1. Check for node on node collisions. For each node on node collision merge their groups into a new group. Goto 2.
        if(NodeOnNodeCheck(containers) == true)
        {
            return;
        }

        do
        {
            //2. Check for group on group collisions. For each group on group collision merge their groups into a new group. If there are collisions goto 2. Else goto 3.
            while (GroupOnGroupCheck(containers) == false) ;

            //3. Check for group on node collisions. For each node that collides merge them into the colliding group. If there are collisions goto 2. Else Finish
        } while (GroupOnNodeCheck(containers) == false);
    }

    public bool NoContainment(NameTagContainer containerA, NameTagContainer containerB)
    {
        return !containerA.ContainsContainer(containerB) && !containerB.ContainsContainer(containerA);
    }

    //Return true if there are no new collisions
    bool GroupOnNodeCheck(NameTagContainer[] containers)
    {
        bool noCollisions = true;
        for (int i = 0; i < containers.Length; i++)
        {
            for (int j = 0; j < containers.Length; j++)
            {
                if (i == j)
                {
                    continue;
                }

                //check for 
                NameTagContainer containerA = containers[i];
                NameTagContainer containerB = containers[j];

                if (containerA.m_nameTags.Count == 0)
                {
                    continue;
                }

                if (containerB.m_nameTags.Count == 0)
                {
                    continue;
                }

                if (containerA.m_groupCollisionBox.bounds.Intersects(containerB.m_nodeCollisionBox.bounds) && NoContainment(containerA,containerB))
                {
                    noCollisions = false;
                    //merge the two colliders
                    if (containerA.m_groupCollisionBox.size.magnitude > containerB.m_groupCollisionBox.size.magnitude)
                    {
                        containerA.CollsionStart(containerB);
                    }
                    else
                    {
                        containerB.CollsionStart(containerA);
                    }
                }

            }
        }

        return noCollisions;
    }

    //Return true if there are no new collisions
    bool GroupOnGroupCheck(NameTagContainer[] containers)
    {
        bool noCollisions = true;
        for (int i = 0; i < containers.Length; i++)
        {
            if (containers[i].DoesNotHaveNameTags())
            {
                continue;
            }
            for (int j = 0; j < containers.Length; j++)
            {
                if (containers[j].DoesNotHaveNameTags())
                {
                    continue;
                }
                if (i == j)
                {
                    continue;
                }

                //check for 
                NameTagContainer containerA = containers[i];
                NameTagContainer containerB = containers[j];

                if(containerA.m_nameTags.Count == 0)
                {
                    continue;
                }

                if(containerB.m_nameTags.Count == 0)
                {
                    continue;
                }

                if (containerA.m_groupCollisionBox.bounds.Intersects(containerB.m_groupCollisionBox.bounds) && NoContainment(containerA, containerB))
                {
                    noCollisions = false;
                    //merge the two colliders
                    if (containerA.m_groupCollisionBox.size.magnitude > containerB.m_groupCollisionBox.size.magnitude)
                    {
                        containerA.CollsionStart(containerB);
                    }
                    else
                    {
                        containerB.CollsionStart(containerA);
                    }
                }

            }
        }

        return noCollisions;
    }



    //Return true if there are no new collisions
    bool NodeOnNodeCheck(NameTagContainer[] containers)
    {
        bool noCollisions = true;
        for (int i = 0; i < containers.Length; i++)
        {
            for (int j = 0; j < containers.Length; j++)
            {
                if (i == j)
                {
                    continue;
                }

                //check for 
                NameTagContainer containerA = containers[i];
                NameTagContainer containerB = containers[j];

                if (containerA.m_nameTags.Count == 0)
                {
                    continue;
                }

                if (containerB.m_nameTags.Count == 0)
                {
                    continue;
                }

                if (containerA.m_nodeCollisionBox.bounds.Intersects(containerB.m_nodeCollisionBox.bounds) && NoContainment(containerA, containerB))
                {
                    noCollisions = false;
                    //merge the two colliders
                    if (containerA.m_groupCollisionBox.size.magnitude > containerB.m_groupCollisionBox.size.magnitude)
                    {
                        containerA.CollsionStart(containerB);
                    }
                    else
                    {
                        containerB.CollsionStart(containerA);

                    }
                }

            }
        }

        return noCollisions;
    }

}
