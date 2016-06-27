using UnityEngine;
using System.Collections;

using System.Xml;
using System.Xml.Serialization;

public class Timeobj 
{
	[XmlAttribute("relationship")]
	public string relationship;

	[XmlAttribute("value")]
	public string value;
}
