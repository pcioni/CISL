using UnityEngine;
using System.Collections.Generic;

public class NetworkNodeController : MonoBehaviour {

    //Prefab for a node's text
    public GameObject network_node_text_prefab;

    //Each network node has a feature
    private Feature node_feature;
    //This node's text object
    private GameObject network_node_text;
    //A list of nodes adjacent to this node
    private List<GameObject> adjacent_nodes;
    //A dictionary of neighbor relationships mapped to feature id's
    private Dictionary<int, string> adjacent_relationships;

    //When placing nodes, bfs is performed. This is this node's parent
    //from that bfs.
    public GameObject bfs_parent;

	// Use this for initialization
	void Start ()
    {
	
	}

    //Initializes this node with a feature
    //Not called automatically
    public void Initialize(Feature initial_feature)
    {
        bfs_parent = null;
        adjacent_nodes = new List<GameObject>();
        adjacent_relationships = new Dictionary<int, string>();

        //Set this node's feature to the feature given
        node_feature = initial_feature;
        //Name this node according to its feature id
        gameObject.name = "node_" + node_feature.id;

        //Have this node make its own node text object
        network_node_text = (GameObject)Instantiate(network_node_text_prefab, this.transform.position, Quaternion.identity);
        //Name it according to this node's feature id
        network_node_text.name = "node_text_" + node_feature.id;

        //Set the node text's parent to this object
        network_node_text.transform.SetParent(gameObject.transform);
        //Set the text object's text to this node's feature's name
        network_node_text.GetComponent<TextMesh>().text = node_feature.data;
    }//end method Initialize
	
	// Update is called once per frame
	void Update ()
    {
        if (network_node_text != null)
        {
            //Make sure the node text is offset in Z from the node object by at least -1
            network_node_text.transform.localPosition = new Vector3(0, 0, -1);
        }//end if
    } //end method Update

    //Accessors/Mutators
    public Feature GetFeature()
    {
        return node_feature;
    }//end method GetFeature
    public void SetFeature(Feature to_set)
    {
        node_feature = to_set;
    }//end method SetFeature

    public List<GameObject> GetNeighbors()
    {
        return adjacent_nodes;
    }//end method GetNeighbors
    //Accepts the adjaceny node itself and the neighbor object that this adjacency
    //is based off of.
    public void AddAdjacentNode(GameObject adjacent_node, Neighbor base_neighbor)
    {
        //Make sure there isn't already a node with this node's id in the adjacency list.
        if (!adjacent_relationships.ContainsKey(base_neighbor.dest))
        {
            //Add the node to the list of adjacent nodes
            adjacent_nodes.Add(adjacent_node);
            //Add the relationship to the dictionary
            adjacent_relationships.Add(base_neighbor.dest, base_neighbor.relationship);
        }//end if
        else
        {
            print("Node " + node_feature.id + ": duplicate adjacent node " + base_neighbor.dest);
        }//end if
    }//end method AddAdjacentNode
}
