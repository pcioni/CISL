using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

public class MainController : MonoBehaviour {

    //Prefabs
    public GameObject network_node_prefab;

    //The scene's main camera
    private GameObject main_camera;

    //Mouse Scrolling Variables
    //Where the mouse was when it was clicked down
    Vector3 last_mouse_position;
    //How much do we pan when the mouse moves
    public float pan_sensitivity;

    public string xml_file_name;
    //A list containing every node in the network
    private List<GameObject> network_nodes;
    //The id of the root node
    private int root_id;

    //Placement variables
    public float repulsion_strength;

	// Use this for initialization
	void Start ()
    {
        main_camera = Camera.main.gameObject;
        network_nodes = new List<GameObject>();

        //Load the xml specified by xml_file_name
        //All XML files will be in Assets\xml. Append to file name.
        string file_path = "Assets\\xml\\" + xml_file_name + ".xml";
        LoadXML(file_path);

        //Place the nodes in the network.
        PlaceNodes();
	}//end method Start

    //Loads the AIMind XML at the given file path into the scene
    private void LoadXML(string file_path)
    {
        print("Loading xml at " + file_path);

        XmlSerializer serializer = new XmlSerializer(typeof(AIMind));
        FileStream stream = new FileStream(file_path, FileMode.Open);
        AIMind container = (AIMind)serializer.Deserialize(stream);
        stream.Close();

        GameObject new_node = null;

        print("Root: " + container.root.id);
        //Get the id of the root node
        root_id = container.root.id;
        foreach (Feature temp_feature in container.features)
        {
            //Make a new node with this feature's id and information.
            new_node = Instantiate(network_node_prefab);
            new_node.GetComponent<NetworkNodeController>().Initialize(temp_feature);
            //new_node.SetActive(false);
            network_nodes.Add(new_node);
        }//end foreach

        //Compile the adjacency lists for each node and create the node text for each node.
        foreach (GameObject temp_node in network_nodes)
        {
            Feature temp_feature = temp_node.GetComponent<NetworkNodeController>().GetFeature();
            List<int> neighbor_ids = new List<int>();
            //Go through each neighbor in temp_node
            foreach (Neighbor temp_neighbor in temp_feature.neighbors)
            {
                //Go through all the other nodes until you find the node
                //whose id matches this neighbor's id
                foreach (GameObject potential_neighbor in network_nodes)
                {
                    //Add this node's id to the neighbor id list
                    neighbor_ids.Add(temp_neighbor.dest);
                    if (potential_neighbor.GetComponent<NetworkNodeController>().GetFeature().id == temp_neighbor.dest)
                    {
                        //Add this potential neighbor to the node's adjacency list.
                        temp_node.GetComponent<NetworkNodeController>().AddAdjacentNode(potential_neighbor, temp_neighbor);
                    }//end if
                }//end foreach
            }//end foreach
        }//end foreach
    }//end method LoadXML

    private void InitialPlacement()
    {

    }//end method InitialPlacement

    //Place all the nodes in the network in the scene
    private void PlaceNodes()
    {
        //A local copy of the full node list to keep track of which nodes haven't been traversed
        //by the bfs. This ensures that unconnected graphs can still be handled.
        List<GameObject> local_node_list = new List<GameObject>();
        List<GameObject> nodes_in_scene = new List<GameObject>();
        //Starting with the root node, perform BFS and place nodes in the scene as they appear in the search.
        Queue<GameObject> bfs_queue = new Queue<GameObject>();
        //Get the root node
        GameObject root_node = null;
        foreach (GameObject temp_node in network_nodes)
        {
            //Populate the local nodes list
            local_node_list.Add(temp_node);
            //Check if this node's id matches the root node id
            if (temp_node.GetComponent<NetworkNodeController>().GetFeature().id.Equals(root_id))
            {
                root_node = temp_node;
            }//end if
        }//end foreach
        //If no root node was found, assign the first node in the list as the root node.
        if (root_node.Equals(null))
        {
            root_node = local_node_list[0];
        }//end if
        //Start BFS with the root node
        bfs_queue.Enqueue(root_node);

        GameObject current_node = null;
        GameObject current_parent = null;
        Vector3 repulsion_center = new Vector3(0, 0, 0);
        Vector3 placement_position = new Vector3(0, 0, 0);
        Vector3 current_delta = new Vector3(0, 0, 0);
        //Place until all nodes are in the scene
        while (local_node_list.Count > 0)
        {
            //Take the first item out of the queue
            current_node = bfs_queue.Dequeue();
            //To find its place, repel it relative to each other node in the scene.
            //To decide on the center to repel from, look at this node's bfs parent.
            //If it has none, it is a root node. Repel it from 0, 0, 0.
            current_parent = current_node.GetComponent<NetworkNodeController>().bfs_parent;
            if (current_parent == null)
            {
                repulsion_center = new Vector3(0, 0, current_node.transform.position.z);
            }//end if
            else
            {
                repulsion_center = new Vector3(current_parent.transform.position.x
                    , current_parent.transform.position.y + 1
                    , current_parent.transform.position.z);
            }//end if

            print("node " + current_node.GetComponent<NetworkNodeController>().GetFeature().id + " center " + repulsion_center.ToString());

            //Calculate this node's position
            placement_position = repulsion_center;
            //Repel it from each other node
            foreach (GameObject repulsion_object in nodes_in_scene)
            {
                //Find delta between objects
                current_delta = repulsion_center - repulsion_object.transform.position;
                current_delta = new Vector3(repulsion_center.x - repulsion_object.transform.position.x
                    , repulsion_center.y - repulsion_object.transform.position.y
                    , 0);
                //Calculate contribution of this node
                //current_delta = current_delta.normalized * (1 / current_delta.sqrMagnitude) * repulsion_strength;
                current_delta = current_delta * repulsion_strength;
                placement_position += current_delta;
            }//end foreach
            print("node " + current_node.GetComponent<NetworkNodeController>().GetFeature().id + " placement " + placement_position.ToString());
            //Give the node its placement position
            current_node.transform.position = new Vector3(placement_position.x, placement_position.y, placement_position.z);
            //Remove the node from the local node list
            local_node_list.Remove(current_node);
            //Add it to the list of nodes in the scene
            nodes_in_scene.Add(current_node);
            
            //Add all of this node's neighbors to the queue if they aren't already in the scene or queue
            foreach (GameObject neighbor_node in current_node.GetComponent<NetworkNodeController>().GetNeighbors())
            {
                if (!nodes_in_scene.Contains(neighbor_node) && !bfs_queue.Contains(neighbor_node))
                {
                    print("adding node " + neighbor_node.GetComponent<NetworkNodeController>().GetFeature().id + " to queue");
                    //Add the neighbor to the queue
                    bfs_queue.Enqueue(neighbor_node);
                    //Set its bfs parent to the current node
                    neighbor_node.GetComponent<NetworkNodeController>().bfs_parent = current_node;
                }//end if
            }//end foreach
        }//end while
    }//end method PlaceNodes

    // Update is called once per frame
    void Update ()
    {
        //Pan camera with mouse
        if (Input.GetMouseButtonDown(1))
        {
            //When the mouse button is pressed down, note its location
            last_mouse_position = Input.mousePosition;
        }//end if
        if (Input.GetMouseButton(1))
        {
            //While the mouse button is pressed down, move the camera some amount relative
            //to how much the mouse moves.
            Vector3 delta = last_mouse_position - Input.mousePosition;
            main_camera.transform.Translate(delta.x * pan_sensitivity, delta.y * pan_sensitivity, 0);
            last_mouse_position = Input.mousePosition;
        }//end if
    }//end method Update
}
