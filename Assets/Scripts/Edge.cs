using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge : MonoBehaviour
{
    public Node left_Node;
    public Node right_Node;

    public float weight = 1;

    public void SetConnection(Node left, Node right, bool is_Propotional = false)
    {
        if (left != null &&
            right != null)
        {
            left_Node = left;
            right_Node = right;

            // 가중치를 거리 비례로 계산한다면 가중치를 변경
            if (is_Propotional)
            {
                weight = Vector3.Distance(left_Node.transform.position, right_Node.transform.position);
            }

            transform.position = Vector3.Lerp(left_Node.transform.position, right_Node.transform.position, 0.5f);
            transform.forward = Vector3.Normalize(left_Node.transform.position - transform.position);
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, Vector3.Distance(left_Node.transform.position, right_Node.transform.position));

            left_Node.edges.Add(this);
            right_Node.edges.Add(this);
        }
    }
}
