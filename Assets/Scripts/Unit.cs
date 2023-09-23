using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    const float minPathUpdateTime = 0.2f;
    const float pathUpdateMoveThreshold = 0.5f;
    // Creates variables for the target of movement, speed of movement, the path, and the index of the target within the path
    public Vector3 targetPosition;
    public float speed = 10.0f;
    public float turnSpeed = 3.0f;
    public float turnDistance = 5.0f;
    public float stoppingDistance = 10.0f;

    private bool movementTimeoutRunning = false;
    private float duration = 3.0f;

    private bool isMoving = false;

    private Animator unitAnim;
    private GameObject highlighter;

    AStarPath path;

    private void Start()
    {
        unitAnim = GetComponent<Animator>();
        highlighter = this.transform.GetChild(2).gameObject;
    }

    public void Update()
    {
        if (movementTimeoutRunning)
        {
            duration -= Time.deltaTime;
        }
    }

    public void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            // Callback for after path found. Loads path, cancels any current movement, and begins movement to the target per the provided pathfinding
            path = new AStarPath(waypoints, transform.position, turnDistance, stoppingDistance);
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    IEnumerator UpdatePath()
    {
        if (Time.timeSinceLevelLoad < 0.3f)
        {
            yield return new WaitForSeconds(0.3f);
        }
        PathRequestManager.RequestPath(new PathRequest(transform.position, targetPosition, OnPathFound));

        float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPosOld = targetPosition;
        while (true)
        {
            yield return new WaitForSeconds(minPathUpdateTime);
            if ((targetPosition - targetPosOld).sqrMagnitude > sqrMoveThreshold)
            {
                PathRequestManager.RequestPath(new PathRequest(transform.position, targetPosition, OnPathFound));
                targetPosOld = targetPosition;
            }
        }
    }

    IEnumerator FollowPath()
    {
        isMoving = true;
        bool followingPath = true;
        int pathIndex = 0;
        transform.LookAt(path.lookPoints[0]);

        float speedPercent = 1;

        unitAnim.SetBool("isRunningAnim", true);

        while (followingPath)
        {
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
            while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
            {
                if (pathIndex == path.finishLineIndex)
                {
                    followingPath = false;
                    unitAnim.SetBool("isRunningAnim", false);
                    unitAnim.SetBool("isWalkingAnim", false);
                    isMoving = false;
                    break;
                }
                else
                {
                    pathIndex++;
                }
            }

            if (followingPath)
            {
                if (pathIndex >= path.slowDownIndex && stoppingDistance > 0)
                {
                    speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D) / stoppingDistance);
                    if (speedPercent < 0.25f && unitAnim.GetBool("isRunningAnim") == true)
                    {
                        unitAnim.SetBool("isRunningAnim", false);
                        unitAnim.SetBool("isWalkingAnim", true);
                        if (!movementTimeoutRunning)
                        {
                            movementTimeoutRunning = true;
                        } else if (movementTimeoutRunning && duration <= 0f)
                        {
                            followingPath = false;
                            isMoving = false;
                            unitAnim.SetBool("isRunningAnim", false);
                            unitAnim.SetBool("isWalkingAnim", false);
                            movementTimeoutRunning = false;
                            duration = 3.0f;
                        }
                    }
                    if (speedPercent < 0.01f)
                    {
                        followingPath = false;
                        isMoving = false;
                        unitAnim.SetBool("isWalkingAnim", false);
                    }
                }

                Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position, Vector3.up); 
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
                transform.eulerAngles = new Vector3(transform.rotation.eulerAngles.x * 0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z * 0);
                transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
            }
            yield return null;
        }
    }

    public void StartPathfinding()
    {
        StartCoroutine(UpdatePath());
    }

    public void Halt()
    {
        StopCoroutine("FollowPath");
        isMoving = false;
        unitAnim.SetBool("isRunningAnim", false);
        unitAnim.SetBool("isWalkingAnim", false);
        movementTimeoutRunning = false;
        duration = 3.0f;
    }

    public void AddHighlighting()
    {
        highlighter.SetActive(true);
    }

    public void RemoveHighlighting()
    {
        highlighter.SetActive(false);
    }
}
