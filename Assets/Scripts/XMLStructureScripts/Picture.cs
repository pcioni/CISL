using UnityEngine;
using System.Collections;

using System.Xml;
using System.Xml.Serialization;

public class Picture {
	[XmlAttribute("url")]
	public string url;

	[XmlAttribute("label")]
	public string label;
}