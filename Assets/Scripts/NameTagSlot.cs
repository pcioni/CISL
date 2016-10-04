﻿using UnityEngine;
using System.Collections;

public class NameTagSlot : MonoBehaviour {
    private NameTag m_assignedNameTag;

    public void AssignNameTag(NameTag nametag)
    {
        this.m_assignedNameTag = nametag;
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, 1.0f);
    }
}
