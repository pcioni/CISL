using UnityEngine;
using System.Collections;

public class DebugMode : MonoBehaviour {
    public static bool MS_ACTIVE = false;
    [SerializeField]
    private bool m_active;
	// Use this for initialization
	void Awake () {
        MS_ACTIVE = m_active;

    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
