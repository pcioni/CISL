using UnityEngine;
using System.Collections.Generic;

using System.Xml.Serialization;

[XmlRoot("AIMind")]
public class AIMind 
{
	[XmlArray("Features")]
	[XmlArrayItem("Feature")]
	public List<Feature> features = new List<Feature>();
	//[XmlArray("Monsters")]
	//[XmlArrayItem("Monster")]
	//public List<Monster> Monsters = new List<Monster>();
	[XmlElement("Root")]
	public Root root;
}
