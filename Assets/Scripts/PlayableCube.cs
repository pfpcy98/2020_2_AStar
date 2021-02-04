using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayableCube : MonoBehaviour
{
    public float moveSpeed = 1.0f;
    [SerializeField]
    private float deceleration = 2.0f;
    [SerializeField]
    private float maxSpeed = 0.01f;

    private const string obstacleTag = "Obstacle";

    private Stack<Node> movePath = null;
    private bool is_NeedToMove = false;
    private Vector3 dest;
    public Vector3 velocity = Vector3.zero;

    //// OffsetPursuit 관련 변수
    [SerializeField]
    private Transform _pursuitTarget = null;
    [SerializeField]
    private Vector3 _offset = Vector3.zero;

    //// WallAvoidance 관련 변수
    // 탐지 거리
    [SerializeField]
    private float _detectionDistance = 5.0f;
    // 좌우 움직임 가중치
    [SerializeField]
    private float _breakingWeight = 0.2f;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(1) &&
            Graph.GetInstance().is_Movemode)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo))
            {
                if (!hitInfo.transform.CompareTag("Obstacle"))
                {
                    if (Graph.GetInstance().is_Freemode || _pursuitTarget == null)
                    {
                        dest = hitInfo.point;

                        Node closestToPlayer = null;
                        Node closestToDest = null;

                        float distToPlayer = float.MaxValue;
                        float distToDest = float.MaxValue;

                        for (int i = 0; i < Graph.GetInstance().node_List.Count; i++)
                        {
                            float newDistToPlayer = Vector3.Distance(transform.position, Graph.GetInstance().node_List[i].transform.position);
                            float newDistToDest = Vector3.Distance(dest, Graph.GetInstance().node_List[i].transform.position);

                            if (newDistToPlayer < distToPlayer)
                            {
                                closestToPlayer = Graph.GetInstance().node_List[i];
                                distToPlayer = newDistToPlayer;
                            }

                            if (newDistToDest < distToDest)
                            {
                                closestToDest = Graph.GetInstance().node_List[i];
                                distToDest = newDistToDest;
                            }
                        }

                        Graph.GetInstance().SearchClear();
                        KeyValuePair<Stack<Node>, float> AStarResult = Graph.GetInstance().AStar(closestToPlayer, closestToDest);

                        movePath = AStarResult.Key;
                    }

                    is_NeedToMove = true;
                }
            }
        }

        if(is_NeedToMove)
        {
            if (Graph.GetInstance().is_Freemode ||
                _pursuitTarget == null)
            {
                if (movePath != null)
                {
                    if (movePath.Count > 0)
                    {
                        velocity = velocity + Arrive(movePath.Peek().transform.position) + (WallAvoidance() * Time.deltaTime);
                        if (Vector3.Distance(transform.position, movePath.Peek().transform.position) < 0.01f)
                        {
                            movePath.Pop();
                        }
                    }
                    else if (Vector3.Distance(transform.position, dest) > 0.01f)
                    {
                        velocity = velocity + Arrive(dest) + (WallAvoidance() * Time.deltaTime);
                    }
                    else
                    {
                        velocity = Vector3.zero;
                        is_NeedToMove = false;
                    }
                }
                else
                {
                    velocity = Vector3.zero;
                    is_NeedToMove = false;
                }
            }
            else
            {
                if(_pursuitTarget != null)
                {
                    velocity = velocity + (OffsetPursuit() + WallAvoidance()) * Time.deltaTime;
                    if(Vector3.Distance(transform.position, _pursuitTarget.position + _offset) < 0.05f)
                    {
                        velocity = Vector3.zero;
                        is_NeedToMove = false;
                    }
                }
            }
        }

        if(velocity.magnitude > 0.005f)
        {
            transform.forward = velocity.normalized;
        }

        transform.position = transform.position + velocity;
    }

    private Vector3 Arrive(Vector3 target)
    {
        Vector3 targetPos = target;
        targetPos.y = transform.position.y;

        Vector3 distance = targetPos - transform.position;

        if (distance.magnitude > 0.0f)
        {
            float speed = distance.magnitude / deceleration;

            speed = Mathf.Min(speed, maxSpeed);

            Vector3 desired_Velocity = distance / distance.magnitude * speed;

            return (desired_Velocity - velocity);
        }

        return Vector3.zero;
    }

    private Vector3 OffsetPursuit()
    {
        if (_pursuitTarget != null)
        {
            // 오프셋을 적용한 좌표 계산
            Vector3 targetPos = _pursuitTarget.TransformPoint(_offset);
            // 오프셋 - Agent 간 거리 계산
            Vector3 distance = targetPos - transform.position;

            float lookAheadTime = distance.magnitude / (_pursuitTarget.GetComponent<PlayableCube>().moveSpeed + moveSpeed);

            return Arrive(targetPos + (_pursuitTarget.GetComponent<PlayableCube>().velocity * lookAheadTime));
        }

        // 추적할 대상이 없을 경우 조종힘을 반환하지 않음.
        return Vector3.zero;
    }

    private Vector3 WallAvoidance()
    {
        // 더듬이로 장애물 측정
        Ray[] rays = new Ray[3];
        rays[0] = new Ray(transform.position, transform.forward);
        rays[1] = new Ray(transform.position, (transform.forward + -transform.right).normalized);
        rays[2] = new Ray(transform.position, (transform.forward + transform.right).normalized);

        RaycastHit[][] hitInfos = new RaycastHit[3][];
        for (int i = 0; i < hitInfos.Length; i++)
        {
            hitInfos[i] = Physics.RaycastAll(rays[i], _detectionDistance);
        }

        Transform closestIP = null;
        float distToClosestIP = float.MaxValue;
        Vector3 intersectionPoint = Vector3.zero;
        int detectedFeelerIndex = -1;

        for (int i = 0; i < hitInfos.Length; i++)
        {
            if (hitInfos[i] != null)
            {
                for (int j = 0; j < hitInfos[i].Length; j++)
                {
                    // 더듬이에 닿은 장애물이 벽일 경우에만 로직 수행
                    if (hitInfos[i][j].transform.tag == obstacleTag)
                    {
                        // 해당 충돌체와의 거리를 비교
                        if (hitInfos[i][j].distance < distToClosestIP)
                        {
                            // 더 짧은 거리의 충돌체일 경우 해당 충돌체를 기록
                            distToClosestIP = hitInfos[i][j].distance;

                            closestIP = hitInfos[i][j].transform;

                            intersectionPoint = hitInfos[i][j].point;

                            detectedFeelerIndex = i;
                        }
                    }
                }
            }
        }

        // 더듬이로 피해야 하는 충돌체를 감지한 경우
        if (closestIP != null)
        {
            Vector3 target;

            // 과충돌한 거리를 계산 (더듬이의 끝 위치 - 충돌 지점)
            Vector3 overShoot = (transform.position + rays[detectedFeelerIndex].direction * _detectionDistance) - intersectionPoint;

            // 해당 벽의 로컬좌표계로부터 Agent의 상대 위치를 측정,
            // z좌표가 음수일 경우 법선 벡터의 반대방향 벡터로 계산(벽의 뒷면)
            // 조종힘 계산 (벽의 법선벡터 * 과충돌 벡터의 크기)
            if (closestIP.InverseTransformPoint(transform.position).z < 0.0f)
            {
                target = -closestIP.forward * overShoot.magnitude;
            }
            else
            {
                target = closestIP.forward * overShoot.magnitude;
            }

            return target;
        }

        // 아닐 경우 값이 0인 벡터를 반환
        return Vector3.zero;
    }
}
