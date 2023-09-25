using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public enum Action
    {
        Nothing,
        Moving,
        Attacking,
        AttackingTarget
    }

    public enum UnitAffiliation
    {
        Player,
        Enemy
    }

    public UnitAffiliation affiliation;

    public Action currentAction;
    const float minPathUpdateTime = 0.2f;
    const float pathUpdateMoveThreshold = 0.5f;
    // Creates variables for the target of movement, speed of movement, the path, and the index of the target within the path
    private float speed;
    private float turnSpeed;
    private float turnDistance;
    private float stoppingDistance;
    public bool isDead;

    private float sight;
    private float attackRange;
    private float maxHP;
    private float currentHP;
    private float attackPower;
    private float armor;
    private float attackSpeed;

    public Vector3 attackTargetPos;
    public GameObject attackTarget;

    private bool movementTimeoutRunning;
    private float duration;

    private bool isMovementSuspended;
    private bool needsNewPath;
    private Vector3 suspendedTarget;
    private bool inAttackRange;
    private bool isFighting;

    private Animator unitAnim;
    private GameObject highlighter;

    AStarPath path;

    public Unit(UnitAffiliation affiliation, Action currentAction, float speed, float turnSpeed, float turnDistance, float stoppingDistance, bool isDead, float sight, float attackRange, float maxHP, float currentHP, float attackPower, float armor, float attackSpeed, bool movementTimeoutRunning, float duration, bool isMovementSuspended, bool needsNewPath, bool inAttackRange, bool isFighting)
    {
        this.affiliation = affiliation;
        this.currentAction = currentAction;
        this.speed = speed;
        this.turnSpeed = turnSpeed;
        this.turnDistance = turnDistance;
        this.stoppingDistance = stoppingDistance;
        this.isDead = isDead;
        this.sight = sight;
        this.attackRange = attackRange;
        this.maxHP = maxHP;
        this.currentHP = currentHP;
        this.attackPower = attackPower;
        this.armor = armor;
        this.attackSpeed = attackSpeed;
        this.movementTimeoutRunning = movementTimeoutRunning;
        this.duration = duration;
        this.isMovementSuspended = isMovementSuspended;
        this.needsNewPath = needsNewPath;
        this.inAttackRange = inAttackRange;
        this.isFighting = isFighting;
    }

    private void Start()
    {
        unitAnim = GetComponent<Animator>();
        highlighter = this.transform.GetChild(2).gameObject;
        if (affiliation == UnitAffiliation.Player)
        {
            tag = "PlayerUnit";
        }
        else
        {
            tag = "EnemyUnit";
        }
    }

    public void Update()
    {
        if (!isDead)
        {
            if (currentHP <= 0)
            {
                StopCoroutine("UpdatePath");
                StopCoroutine("FollowPath");
                StopCoroutine("Fighting");
                isDead = true;
            } 
            else
            {
                if (movementTimeoutRunning)
                {
                    duration -= Time.deltaTime;
                }

                if (currentAction != Action.Moving && currentAction != Action.AttackingTarget)
                {
                    List<GameObject> targets = EnemiesInSight();
                    if (targets.Count > 0)
                    {
                        int index = 0;
                        float distance = 100.0f;
                        for (int i = 0; i < targets.Count; i++)
                        {
                            float tempDist = Mathf.Abs(targets[i].transform.position.x - transform.position.x) + Mathf.Abs(targets[i].transform.position.z - transform.position.z);
                            if (tempDist < distance)
                            {
                                index = i;
                                distance = tempDist;
                            }
                        }
                        attackTarget = targets[index];
                        attackTargetPos = attackTarget.transform.position;
                        if (currentAction == Action.Attacking)
                        {
                            isMovementSuspended = true;
                            if (!needsNewPath)
                            {
                                suspendedTarget = path.lookPoints[path.lookPoints.Length - 1];
                            }
                            Debug.Log(suspendedTarget);
                        }
                        currentAction = Action.AttackingTarget;
                        if (IsTargetInAttackRange(attackTargetPos))
                        {
                            inAttackRange = true;
                        }
                        else
                        {
                            StartPathfinding(attackTargetPos, "attack target", false);
                        }
                    }
                }

                if (currentAction == Action.AttackingTarget && !inAttackRange)
                {
                    if (attackTargetPos != attackTarget.transform.position)
                    {
                        attackTargetPos = attackTarget.transform.position;
                        StopCoroutine("UpdatePath");
                        StopCoroutine("FollowPath");
                        StartPathfinding(attackTargetPos, "attack target", false);
                    }
                }

                if (inAttackRange && !isFighting)
                {
                    StopCoroutine("FollowPath");
                    isFighting = true;
                    StartCoroutine("Fighting");
                }
            }
        }


        if (isDead)
        {
            unitAnim.SetBool("isDeathAnim", true);
            Destroy(gameObject, 1.5f);
        }
    }

    public void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            // Callback for after path found. Loads path, cancels any current movement, and begins movement to the target per the provided pathfinding
            path = new AStarPath(waypoints, transform.position, turnDistance, stoppingDistance);
            StopCoroutine("FollowPath");
            if ((currentAction == Action.Nothing || currentAction == Action.Moving) || (!inAttackRange && !isFighting))
            {
                StartCoroutine("FollowPath");
            }
        }
    }

    IEnumerator UpdatePath(Vector3 targetPosition)
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
        needsNewPath = false;
        bool followingPath = true;
        int pathIndex = 0;
        transform.LookAt(path.lookPoints[0]);

        float speedPercent = 1;

        unitAnim.SetBool("isRunningAnim", true);

        while (followingPath)
        {
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
            if (currentAction == Action.AttackingTarget)
            {
                if (IsTargetInAttackRange(attackTarget.transform.position))
                {
                    inAttackRange = true;
                }
            }
            while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
            {
                if (pathIndex == path.finishLineIndex)
                {
                    followingPath = false;
                    StartCoroutine("MoveAwayFromAlly");
                    unitAnim.SetBool("isRunningAnim", false);
                    unitAnim.SetBool("isWalkingAnim", false);
                    currentAction = Action.Nothing;
                    isMovementSuspended = false;
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
                            StartCoroutine("MoveAwayFromAlly");
                            unitAnim.SetBool("isRunningAnim", false);
                            unitAnim.SetBool("isWalkingAnim", false);
                            movementTimeoutRunning = false;
                            duration = 3.0f;
                            currentAction = Action.Nothing;
                            isMovementSuspended = false;
                        }
                    }
                    if (speedPercent < 0.01f)
                    {
                        followingPath = false;
                        StartCoroutine("MoveAwayFromAlly");
                        unitAnim.SetBool("isWalkingAnim", false);
                        currentAction = Action.Nothing;
                        isMovementSuspended = false;
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

    public void StartPathfinding(Vector3 targetPosition, string type, bool finishedFighting)
    {
        if (finishedFighting)
        {
            StopCoroutine("Fighting");
            unitAnim.SetBool("isAttackingAnim", false);
        }
        if (type == "move")
        {
            StopCoroutine("Fighting");
            unitAnim.SetBool("isAttackingAnim", false);
            currentAction = Action.Moving;
            isFighting = false;
            inAttackRange = false;
        }
        else if (type == "attack")
        {
            currentAction = Action.Attacking;
        }
        else if (type == "attack target")
        {
            currentAction = Action.AttackingTarget;
        }
        StartCoroutine(UpdatePath(targetPosition));
    }

    public void Halt()
    {
        StopCoroutine("FollowPath");
        StartCoroutine("MoveAwayFromAlly");
        unitAnim.SetBool("isRunningAnim", false);
        unitAnim.SetBool("isWalkingAnim", false);
        movementTimeoutRunning = false;
        duration = 3.0f;
        currentAction = Action.Nothing;
        isMovementSuspended = false;
    }

    public void AddHighlighting()
    {
        highlighter.SetActive(true);
    }

    public void RemoveHighlighting()
    {
        highlighter.SetActive(false);
    }

    public List<GameObject> EnemiesInSight()
    {
        Collider[] temp = Physics.OverlapSphere(transform.position, sight);
        List<GameObject> results = new List<GameObject>();

        // THIS WILL NEED TO BE ADDRESSED LATER AS THIS WILL CAUSE AI PROBLEMS
        if (tag == "PlayerUnit")
        {
            foreach (Collider collider in temp)
            {
                if ((collider.gameObject.tag == "EnemyUnit" && !collider.gameObject.GetComponent<Unit>().isDead) || (collider.gameObject.tag == "EnemyBuilding" && !collider.gameObject.GetComponent<Building>().isDestroyed))
                {
                    results.Add(collider.gameObject);
                }
            }
        }
        else if (tag == "EnemyUnit")
        {
            foreach (Collider collider in temp)
            {
                if ((collider.gameObject.tag == "PlayerUnit" && !collider.gameObject.GetComponent<Unit>().isDead) || (collider.gameObject.tag == "PlayerBuilding" && !collider.gameObject.GetComponent<Building>().isDestroyed))
                {
                    results.Add(collider.gameObject);
                }
            }
        }
        return results;
    }

    public bool IsTargetInAttackRange(Vector3 target)
    {
        if (Mathf.Abs(transform.position.x - target.x) <= attackRange && Mathf.Abs(transform.position.z - target.z) <= attackRange)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public IEnumerator Fighting()
    {
        unitAnim.SetBool("isWalkingAnim", false);
        unitAnim.SetBool("isRunningAnim", false);
        unitAnim.SetBool("isAttackingAnim", true);
       
        List<GameObject> target = new List<GameObject>()
        {
            attackTarget
        };
        if (attackTarget.tag == "EnemyUnit" || attackTarget.tag == "PlayerUnit")
        {
            while (!isDead && !attackTarget.GetComponent<Unit>().isDead)
            {
                if (!IsTargetInAttackRange(attackTarget.transform.position))
                {
                    isFighting = false;
                    inAttackRange = false;
                    attackTargetPos = attackTarget.transform.position;
                    StartPathfinding(attackTargetPos, "attack target", true);
                }
                else
                {
                    transform.LookAt(attackTarget.transform.position);
                    InflictDamage(target);
                }
                yield return new WaitForSeconds(attackSpeed);
            }
        }
        else if (attackTarget.tag == "EnemyBuilding" || attackTarget.tag == "PlayerBuilding")
        {
            while (!isDead && !attackTarget.GetComponent<Building>().isDestroyed)
            {
                if (!IsTargetInAttackRange(attackTarget.transform.position))
                {
                    isFighting = false;
                    inAttackRange = false;
                    attackTargetPos = attackTarget.transform.position;
                    StartPathfinding(attackTargetPos, "attack target", true);
                }
                else
                {
                    transform.LookAt(attackTarget.transform.position);
                    InflictDamage(target);
                }
                yield return new WaitForSeconds(attackSpeed);
            }
        }
        isFighting = false;
        inAttackRange = false;
        unitAnim.SetBool("isAttackingAnim", false);
        if (isMovementSuspended)
        {
            isMovementSuspended = false;
            needsNewPath = true;
            unitAnim.SetBool("isRunningAnim", true);
            StartPathfinding(suspendedTarget, "attack", true);
        }
        else
        {
            currentAction = Action.Nothing;
            StartCoroutine("MoveAwayFromAlly");
        }
        yield return null;
    }

    private void InflictDamage(List<GameObject> targets)
    {
        foreach (GameObject target in targets)
        {
            if (target != null)
            {
                if (target.tag == "EnemyUnit" || target.tag == "PlayerUnit")
                {
                    target.GetComponent<Unit>().TakeDamage(attackPower);
                }
                else if (target.tag == "EnemyBuilding" || target.tag == "PlayerBuilding")
                {
                    target.GetComponent<Building>().TakeDamage(attackPower);
                }
            }
        }
    }

    public void TakeDamage(float damageAmount)
    {
        currentHP -= damageAmount - armor;
        Debug.Log(name + ": " + currentHP);
    }

    IEnumerator MoveAwayFromAlly()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1.0f);
        List<Collider> validColliders = new List<Collider>();
        foreach(Collider collider in colliders)
        {
            if (collider.gameObject.tag == tag && collider.gameObject.name != name)
            {
                validColliders.Add(collider);
            }
        }
        float xToMove = 0;
        float zToMove = 0;
        while (validColliders.Count > 0)
        {
            foreach (Collider collider in validColliders)
            {
                if (collider.gameObject.transform.position.x > transform.position.x)
                {
                    xToMove = -0.5f;
                }
                else if (collider.gameObject.transform.position.x < transform.position.x)
                {
                    xToMove = 0.5f;
                }

                if (collider.gameObject.transform.position.z > transform.position.z)
                {
                    zToMove = -0.5f;
                }
                else if (collider.gameObject.transform.position.z < transform.position.z)
                {
                    zToMove = 0.5f;
                }

            }
            transform.Translate(new Vector3(xToMove, 0, zToMove), Space.World);
            yield return new WaitForSeconds(0.5f);
            colliders = Physics.OverlapSphere(transform.position, 0.5f);
            validColliders.Clear();
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.tag == tag && collider.gameObject.name != name)
                {
                    validColliders.Add(collider);
                }
            }
        }
        yield return null;
    }
}
