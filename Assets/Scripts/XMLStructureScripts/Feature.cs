using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

//A class containing information on each feature, as defined in
//XML data input

public class Feature {

	[XmlAttribute("id")]
	public int id;

	[XmlAttribute("data")]
	public string data;

    /*[XmlElement("speak")]
	public Speak speak;*/
    public string speak;

	[XmlArray("neighbors")]
	[XmlArrayItem("neighbor")]
	public List<Neighbor> neighbors = new List<Neighbor>();
	
	//spatio-temporal stuff
	
	[XmlArray("timedata")]
	[XmlArrayItem("timeobj")]
	public List<Timeobj> timedata = new List<Timeobj>();
	
	[XmlArray("geodata")]
	[XmlArrayItem("coordinates")]
	public List<Coordinates> geodata = new List<Coordinates>();
	
	//end

	[XmlArray("parents")]
	[XmlArrayItem("parent")]
	public List<Parent> parents = new List<Parent> ();
	
	private string speak_value;

	//Initialization function (not called automatically)
	public void Initialize()
	{
		id = 0;
		data = "";
		speak_value = "";
	}//end method Initialize

	//Accessors and mutators
	public void setId(int new_id)
	{
		id = new_id;
	}//end method setId
	public int getId()
	{
		return id;
	}//end method getId

	public void setData(string new_data)
	{
		data = new_data;
	}//end method setData
	public string getData()
	{
		return data;
	}//end method getData

	public void setSpeakValue(string new_speak_value)
	{
		speak_value = new_speak_value;
	}//end method setSpeakValue
	public string getSpeakValue()
	{
		return speak_value;
	}//end method getSpeakValue

	// Update is called once per frame
	void Update () 
	{
	
	}
}
