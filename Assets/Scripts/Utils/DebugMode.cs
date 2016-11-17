using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebugMode : MonoBehaviour {
    public static bool MS_ACTIVE = false;
    [SerializeField]
    private bool m_active;
	// Use this for initialization
	void Awake () {
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.D))
        {
            m_active = !m_active;
        }

        MS_ACTIVE = m_active;

    }

    private static Dictionary<string, float> timers = new Dictionary<string, float>();
    public static void startTimer(string name)
    {
        timers[name] = Time.realtimeSinceStartup;
    }

    public static void stopTimer(string name)
    {
        if (timers.ContainsKey(name))
        {
            float totalTime = Time.realtimeSinceStartup - timers[name];
            print(string.Format("{0} -- totalTime = {1} seconds", name, totalTime));

            timers.Remove(name);
        }
    }
}
