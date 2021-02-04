using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SearchHelper : MonoBehaviour
{
    [SerializeField]
    private GameObject menuObject = null;
    [SerializeField]
    private Text movemodeText = null;
    [SerializeField]
    private Text clickmodeText = null;

    private bool is_MenuEnabled = false;

    private const string nodeTag = "Node";
    private const string obstacleTag = "Obstacle";

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            is_MenuEnabled = !is_MenuEnabled;

            if(menuObject != null)
            {
                menuObject.SetActive(is_MenuEnabled);
                if(is_MenuEnabled)
                {
                    if (movemodeText != null)
                    {
                        if (Graph.GetInstance().is_Freemode)
                        {
                            movemodeText.text = "Free";
                        }
                        else
                        {
                            movemodeText.text = "Formation";
                        }
                    }

                    if (clickmodeText != null)
                    {
                        if (Graph.GetInstance().is_Movemode)
                        {
                            clickmodeText.text = "Movement";
                        }
                        else
                        {
                            clickmodeText.text = "Obstacle";
                        }
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(0) &&
            !Graph.GetInstance().is_Movemode)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo))
            {
                if(hitInfo.transform.CompareTag(nodeTag))
                {
                    Node node = hitInfo.transform.GetComponent<Node>();
                    if(node != null)
                    {
                        node.is_Obstacle = true;
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(1) &&
            !Graph.GetInstance().is_Movemode)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo))
            {
                if (hitInfo.transform.CompareTag(obstacleTag))
                {
                    Node node = hitInfo.transform.GetComponentInParent<Node>();
                    if (node != null)
                    {
                        node.is_Obstacle = false;
                    }
                }
            }
        }
    }

    public void SetMoveMode()
    {
        Graph.GetInstance().is_Freemode = !Graph.GetInstance().is_Freemode;
        if(movemodeText != null)
        {
            if(Graph.GetInstance().is_Freemode)
            {
                movemodeText.text = "Free";
            }
            else
            {
                movemodeText.text = "Formation";
            }
        }
    }

    public void SetClickMode()
    {
        Graph.GetInstance().is_Movemode = !Graph.GetInstance().is_Movemode;
        if(clickmodeText != null)
        {
            if(Graph.GetInstance().is_Movemode)
            {
                clickmodeText.text = "Movement";
            }
            else
            {
                clickmodeText.text = "Obstacle";
            }
        }
    }
}
