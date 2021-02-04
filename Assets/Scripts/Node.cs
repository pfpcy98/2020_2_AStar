using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    [SerializeField]
    private bool is_Visible = true;
    [SerializeField]
    private MeshRenderer orderText = null;
    [SerializeField]
    private GameObject innerObstacle = null;
    [SerializeField]
    private GameObject edgePrefab = null;

    public bool is_Obstacle = false;

    public int id = 0;
    public bool is_Visited = false;

    public List<Edge> edges = new List<Edge>();

    private const string compareTag = "Node";

    // Start is called before the first frame update
    void Start()
    {
        id = Graph.GetInstance().GetNodeID();
        Graph.GetInstance().node_List.Add(this);

        if(!is_Visible)
        {
            GetComponent<MeshRenderer>().enabled = false;
            if(orderText != null)
            {
                orderText.enabled = false;
            }
        }

        RaycastHit[] output = Physics.SphereCastAll(transform.position, 3.0f, Vector3.up, 0.0f);
        if(output != null)
        {
            foreach(RaycastHit result in output)
            {
                if (transform != result.transform)
                {
                    if (result.transform.CompareTag(compareTag))
                    {
                        Node node = result.transform.GetComponent<Node>();
                        if (node != null)
                        {
                            bool is_Entried = false;

                            foreach (Edge edge in node.edges)
                            {
                                if (node == edge.left_Node)
                                {
                                    if (this == edge.right_Node)
                                    {
                                        is_Entried = true;
                                    }
                                }
                                else if (node == edge.right_Node)
                                {
                                    if (this == edge.left_Node)
                                    {
                                        is_Entried = true;
                                    }
                                }
                            }

                            if (!is_Entried)
                            {
                                if (edgePrefab != null)
                                {
                                    GameObject edge_Object = Instantiate(edgePrefab);
                                    Edge edge = edge_Object.GetComponent<Edge>();

                                    if (edge != null)
                                    {
                                        edge.SetConnection(this, node, true);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    void Update()
    {
        if(innerObstacle != null)
        {
            innerObstacle.SetActive(is_Obstacle);
        }
    }
}
