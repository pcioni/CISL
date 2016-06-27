using UnityEngine;
using System.Collections;

public class alignTest : MonoBehaviour {

	public LocationMapper lm;
	public Vector2 coordinates;

	// Use this for initialization
	void Start () {
		//align map to location
		transform.position = lm.coord2world(coordinates);
	}
	
}
