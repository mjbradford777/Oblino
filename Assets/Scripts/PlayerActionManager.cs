using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerActionManager : MonoBehaviour
{
    public Camera camera;
    public enum Action
    {
        Select,
        Drag,
        Move,
        Attack,
        Cast,
        Camera
    }

    public Action currentAction;
    public EventSystem eventSystem;

    private List<GameObject> currentlySelectedUnits = new List<GameObject>();
    private List<GameObject> controlGroup1 = new List<GameObject>();
    private List<GameObject> controlGroup2 = new List<GameObject>();
    private List<GameObject> controlGroup3 = new List<GameObject>();

    private Vector3 dragStart;

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
        /*eventSystem = GetComponent<EventSystem>();*/
        /*currentAction = Action.Select;*/
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                Debug.Log(touch);
            }
        }

        if (currentAction == Action.Drag)
        {
            if (Input.GetMouseButtonUp(0))
            {
                Vector3 mousePosition = Input.mousePosition;
                Ray ray = camera.ScreenPointToRay(mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    currentlySelectedUnits.Clear();
                    Vector3 centerPoint = new Vector3((dragStart.x + hit.point.x) / 2, hit.point.y, (dragStart.z +  hit.point.z) / 2);
                    Vector3 half;
                    if (hit.point.y < 1)
                    {
                        half = new Vector3(Mathf.Abs(dragStart.x - hit.point.x) / 2, 1, Mathf.Abs(dragStart.z - hit.point.z) / 2);
                    }
                    else
                    {
                        half = new Vector3(Mathf.Abs(dragStart.x - hit.point.x) / 2, hit.point.y / 2, Mathf.Abs(dragStart.z - hit.point.z) / 2);
                    }
                    Collider[] encompassedColliders = Physics.OverlapBox(centerPoint, half);
                    foreach (Collider collider in encompassedColliders)
                    {
                        if (collider.gameObject.tag == "PlayerUnit")
                        {
                            currentlySelectedUnits.Add(collider.gameObject);
                        }
                    }
                    Debug.Log(currentlySelectedUnits.Count);
                }
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (!eventSystem.IsPointerOverGameObject())
            {
                Vector3 mousePosition = Input.mousePosition;
                Ray ray = camera.ScreenPointToRay(mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (currentAction == Action.Select && hit.collider.gameObject.tag == "PlayerUnit")
                    {
                        Debug.Log("Unit selected");
                        currentlySelectedUnits.Add(hit.collider.gameObject);
                    }
                    else if (currentAction == Action.Drag)
                    {
                        dragStart = hit.point;
                    }
                    else if (currentAction == Action.Move && currentlySelectedUnits.Count > 0)
                    {
                        foreach (GameObject go in currentlySelectedUnits)
                        {
                            Unit temp = go.GetComponent<Unit>();
                            temp.targetPosition = hit.point;
                            temp.StartPathfinding();
                        }
                    }
                }
            }
        }
    }

    public void SetSelectAction()
    {
        currentAction = Action.Select;
        Debug.Log(Equals(currentAction, Action.Select));
    }

    public void SetDragAction()
    {
        currentAction = Action.Drag;
        Debug.Log(Equals(currentAction, Action.Drag));
    }

    public void SetMoveAction()
    {
        currentAction = Action.Move;
        Debug.Log(Equals(currentAction, Action.Move));
    }

    public void SetAttackAction()
    {
        currentAction = Action.Attack;
        Debug.Log(Equals(currentAction, Action.Attack));
    }
    public void SetCastAction()
    {
        currentAction = Action.Cast;
        Debug.Log(Equals(currentAction, Action.Cast));
    }

    public void ClearSelection()
    {
        currentlySelectedUnits.Clear();
        Debug.Log(currentlySelectedUnits.Count);
    }

    public void CallHalt()
    {
        // Halt moving units if still selected
        foreach (GameObject go in currentlySelectedUnits)
        {
            Unit temp = go.GetComponent<Unit>();
            temp.Halt();
        }
        Debug.Log("Halt");
    }
}
