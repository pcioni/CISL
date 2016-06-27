using UnityEngine;
using System.Collections;

using System.Xml;
using System.Xml.Serialization;

public class Coordinates 
{
	[XmlAttribute("lat")]
	public float lat;

	[XmlAttribute("lon")]
	public float lon;
}
