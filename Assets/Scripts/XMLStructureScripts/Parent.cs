using UnityEngine;
using System.Collections;

using System.Xml;
using System.Xml.Serialization;

public class Parent 
{
	[XmlAttribute("dest")]
	public int dest;
	
	[XmlAttribute("weight")]
	public float weight;
	
	[XmlAttribute("relationship")]
	public string relationship;
}