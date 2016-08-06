﻿using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
	private Vector3 last_mouse_position;
	public float minZoom = 0f;
	public float maxZoom = 100f;


	void Update()
	{
		//Pan camera with mouse
		if (Input.GetMouseButtonDown(1))
			last_mouse_position = Input.mousePosition;
		if (Input.GetMouseButton(1)) {
			Vector3 delta = last_mouse_position - Input.mousePosition;
			Camera.main.transform.Translate(delta.x * .1f, delta.y * .1f, 0);
			last_mouse_position = Input.mousePosition;
			EventManager.TriggerEvent(EventManager.EventType.INTERFACE_PAN, Camera.main.orthographicSize.ToString());
		}

		//scroll in
		if (Input.GetAxis("Mouse ScrollWheel") > 0) {
			ZoomOrthoCamera(Camera.main.ScreenToWorldPoint(Input.mousePosition), 1);
			EventManager.TriggerEvent(EventManager.EventType.INTERFACE_ZOOM_OUT, Camera.main.orthographicSize.ToString());
		}
		//scroll out
		if (Input.GetAxis("Mouse ScrollWheel") < 0) {
			ZoomOrthoCamera(Camera.main.ScreenToWorldPoint(Input.mousePosition), -1);
			EventManager.TriggerEvent(EventManager.EventType.INTERFACE_ZOOM_IN, Camera.main.orthographicSize.ToString());
		}
	}

	// ortographic camera zoom towards a point in world coordinates. 
	// negative amount zooms in, positive zooms out
	// TODO: stop camera movement when at zoom limit
	void ZoomOrthoCamera(Vector3 zoomTowards, float amount) {
		// Calculate how much we will have to move towards the zoomTowards position
		float multiplier = (1.0f / GetComponent<Camera>().orthographicSize * amount);

		// Move camera
		transform.position += (zoomTowards - transform.position) * multiplier;

		// Zoom camera
		GetComponent<Camera>().orthographicSize -= amount;

		// Limit zoom
		GetComponent<Camera>().orthographicSize = Mathf.Clamp(GetComponent<Camera>().orthographicSize, minZoom, maxZoom);
	}

}
