using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.VR;

public class Graph : MonoBehaviour
{
    private static Graph m_instance = null;

    // Graph 정보
    public List<Node> node_List = new List<Node>();
    private int nodeIndex = 0; // 생성되는 노드에 매겨줄 번호 값 변수

    // 기타 변수
    private int searchIndex = 0; // 탐색 실행 중 번호 지정을 위한 값 변수

    public bool is_Freemode = false;
    public bool is_Movemode = true;

    public static Graph GetInstance()
    {
        if(m_instance == null)
        {
            return null;
        }

        return m_instance;
    }

    void Awake()
    {
        if (m_instance == null)
        {
            m_instance = this;
        }

        if(node_List == null)
        {
            Debug.LogError(name + " : Warning! Node List doesn't exist!");
        }
    }

    public KeyValuePair<Stack<Node>, float> AStar(Node startNode, Node goalNode)
    {
        // 예외 처리
        if (startNode == null ||
            goalNode == null)
        {
            return new KeyValuePair<Stack<Node>, float>(null, -1);
        }

        KeyValuePair<float, Edge>[] distances = new KeyValuePair<float, Edge>[node_List.Count];
        for (int i = 0; i < node_List.Count; i++)
        {
            distances[i] = new KeyValuePair<float, Edge>(float.MaxValue, null);
        }
        distances[startNode.id] = new KeyValuePair<float, Edge>(0, null);

        Priority_Queue.SimplePriorityQueue<Node> pq = new Priority_Queue.SimplePriorityQueue<Node>();
        pq.Enqueue(startNode, 0);

        int visitCount = 0; // 노드 방문횟수 측정을 위한 변수

        // A* (Dijkstra) 알고리즘
        while (pq.Count > 0)
        {
            Node current = pq.First;
            float dist = pq.GetPriority(current);
            current.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 1);

            pq.Dequeue();

            visitCount++;
            if (current == goalNode) // 목표까지 탐색이 완료되면 탈출
            {
                break;
            }

            if (dist < distances[current.id].Key)
            {
                continue;
            }

            for (int i = 0; i < current.edges.Count; i++)
            {
                Node next = null;
                if (current == current.edges[i].left_Node)
                {
                    next = current.edges[i].right_Node;
                }
                else if (current == current.edges[i].right_Node)
                {
                    next = current.edges[i].left_Node;
                }
                else
                {
                    Debug.LogError("Dijkstra critical error occured!");
                    return new KeyValuePair<Stack<Node>, float>(null, -1);
                }

                // 이어지는 노드가 장애물로 되어있을 경우 처리하지 않음(값을 무한대로 유지)
                if(next.is_Obstacle)
                {
                    continue;
                }

                float nextDist = dist + current.edges[i].weight + Vector3.Distance(next.transform.position, goalNode.transform.position);
                if (nextDist < distances[next.id].Key)
                {
                    distances[next.id] = new KeyValuePair<float, Edge>(nextDist, current.edges[i]);
                    pq.Enqueue(next, nextDist);
                }
            }
        }

        // 최단 경로 산출(distance 배열 edge 역행)
        Stack<Node> pathStack = new Stack<Node>();

        Node reversingNode = goalNode;
        int reversingIndex = goalNode.id;
        while (reversingNode != startNode)
        {
            // 경로 표시
            pathStack.Push(reversingNode);
            VisitNode(reversingNode);
            distances[reversingIndex].Value.GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0);

            if (reversingNode == distances[reversingIndex].Value.left_Node)
            {
                reversingNode = distances[reversingIndex].Value.right_Node;
            }
            else if (reversingNode == distances[reversingIndex].Value.right_Node)
            {
                reversingNode = distances[reversingIndex].Value.left_Node;
            }
            else
            {
                Debug.LogError("Dijkstra : Critical error occured!");
                break;
            }

            reversingIndex = reversingNode.id;
        }
        pathStack.Push(reversingNode);
        VisitNode(reversingNode);

        Debug.Log("A* visits node " + visitCount + " time(s)!");

        return new KeyValuePair<Stack<Node>, float>(pathStack, distances[goalNode.id].Key);
    }

    public int GetNodeID()
    {
        nodeIndex++;
        return nodeIndex - 1;
    }

    public Node GetNodeFromID(int id)
    {
        for(int i = 0; i < node_List.Count; i++)
        {
            if(node_List[i].id == id)
            {
                return node_List[i];
            }
        }

        return null;
    }

    private void VisitNode(Node node)
    {
        node.is_Visited = true;
        node.gameObject.GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0);
        node.gameObject.GetComponentInChildren<TextMesh>().text = string.Format("{0}", searchIndex + 1);
        searchIndex++;
    }

    public void SearchClear()
    {
        foreach(Node node in node_List)
        {
            node.is_Visited = false;
            node.gameObject.GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1);
            node.gameObject.GetComponentInChildren<TextMesh>().text = string.Empty;

            for(int i = 0; i < node.edges.Count; i++)
            {
                node.edges[i].GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0);
            }
        }

        searchIndex = 0;
    }

    public void AllClear()
    {
        foreach(Node node in node_List)
        {
            node.is_Obstacle = false;
        }
        SearchClear();
    }

    public void SetFreemode(bool value)
    {
        is_Freemode = value;
    }
}
