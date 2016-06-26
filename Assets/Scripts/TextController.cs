using UnityEngine;
using System.Collections;

public class TextController : MonoBehaviour {
	public GameObject piece;
	
	private float text_offset;
	
	// Use this for initialization
	void Start () 
	{

	}

	public void Initialize()
	{

	}//end method initialize

	public void setText(string text_to_set)
	{
		gameObject.GetComponent<TextMesh> ().text = text_to_set;
	}//end method setText

	// Update is called once per frame
	void Update () 
	{
		
	}
	
	void FixedUpdate()
	{
	}//end method FixedUpdate
}
