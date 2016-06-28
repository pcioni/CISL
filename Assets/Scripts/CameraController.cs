using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    private Vector3 last_mouse_position;

	void Start () {
	
	}

    void Update()
    {
        //Pan camera with mouse
        if (Input.GetMouseButtonDown(1))
            last_mouse_position = Input.mousePosition;
        if (Input.GetMouseButton(1))
        {
            Vector3 delta = last_mouse_position - Input.mousePosition;
            Camera.main.transform.Translate(delta.x * .1f, delta.y * .1f, 0);
            last_mouse_position = Input.mousePosition;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Camera.main.orthographicSize += scroll * 10;
    }
}
