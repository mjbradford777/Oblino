using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerActionManager : MonoBehaviour
{
    public Camera camera;

    public Transform InvalidTargetWarning;
    private Transform UIOverlay;

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
    private Action priorAction;
    public EventSystem eventSystem;

    private List<GameObject> currentlySelectedUnits = new List<GameObject>();
    private List<GameObject> controlGroup1 = new List<GameObject>();
    private List<GameObject> controlGroup2 = new List<GameObject>();
    private List<GameObject> controlGroup3 = new List<GameObject>();

    private Vector3 dragStart;

    private LineRenderer lineRend;

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
        UIOverlay = GameObject.Find("UI Overlay").transform;
        lineRend = GetComponent<LineRenderer>();
        lineRend.positionCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                Debug.Log(touch);
            }
        }*/
        if (currentAction == Action.Camera)
        {
            if (Input.mousePosition.x >= Screen.width - Screen.width * 0.01)
            {
                camera.transform.Translate(Vector3.right * Time.deltaTime * 20);
            }
            if (Input.mousePosition.x <= Screen.width * 0.01)
            {
                camera.transform.Translate(Vector3.left * Time.deltaTime * 20);
            }
            if (Input.mousePosition.y >= Screen.height - Screen.width * 0.01)
            {
                camera.transform.Translate(Vector3.forward * Time.deltaTime * 20, Space.World);
            }
            if (Input.mousePosition.y <= Screen.width * 0.01)
            {
                camera.transform.Translate(Vector3.back * Time.deltaTime * 20, Space.World);
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
                        currentlySelectedUnits.Add(hit.collider.gameObject);
                        Unit temp = hit.collider.gameObject.GetComponent<Unit>();
                        temp.AddHighlighting();
                    }
                    else if (currentAction == Action.Drag)
                    {
                        dragStart = hit.point;
                        lineRend.positionCount = 4;
                        lineRend.SetPosition(0, new Vector3(dragStart.x, 0.1f, dragStart.z));
                        lineRend.SetPosition(1, new Vector3(dragStart.x, 0.1f, dragStart.z));
                        lineRend.SetPosition(2, new Vector3(dragStart.x, 0.1f, dragStart.z));
                        lineRend.SetPosition(3, new Vector3(dragStart.x, 0.1f, dragStart.z));
                    }
                    else if (currentAction == Action.Move && currentlySelectedUnits.Count > 0)
                    {
                        if (hit.collider.gameObject.layer == 6)
                        {
                            Transform tempWarning = Instantiate(InvalidTargetWarning, UIOverlay);
                            Destroy(tempWarning.gameObject, 1.5f);
                        } 
                        else
                        {
                            foreach (GameObject go in currentlySelectedUnits)
                            {
                                Unit temp = go.GetComponent<Unit>();
                                temp.StartPathfinding(hit.point, "move", false);
                            }
                        }
                    }
                    else if (currentAction == Action.Attack)
                    {
                        if (hit.collider.gameObject.tag == "EnemyUnit")
                        {
                            foreach (GameObject go in currentlySelectedUnits)
                            {
                                Unit temp = go.GetComponent<Unit>();
                                temp.attackTarget = hit.collider.gameObject;
                                temp.attackTargetPos = hit.collider.gameObject.transform.position;
                                temp.StartPathfinding(hit.point, "attack target", false);
                            }
                        }
                        else if (hit.collider.gameObject.layer == 6)
                        {
                            Transform tempWarning = Instantiate(InvalidTargetWarning, UIOverlay);
                            Destroy(tempWarning.gameObject, 1.5f);
                        }
                        else
                        {
                            foreach (GameObject go in currentlySelectedUnits)
                            {
                                Unit temp = go.GetComponent<Unit>();
                                temp.StartPathfinding(hit.point, "attack", false);
                            }
                        }
                    }
                }
            }
        }

        if (currentAction == Action.Drag)
        {
            if (Input.GetMouseButtonUp(0))
            {
                lineRend.positionCount = 0;
                if (!eventSystem.IsPointerOverGameObject())
                {
                    Vector3 mousePosition = Input.mousePosition;
                    Ray ray = camera.ScreenPointToRay(mousePosition);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        Vector3 centerPoint = new Vector3((dragStart.x + hit.point.x) / 2, hit.point.y, (dragStart.z + hit.point.z) / 2);
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
                        bool hasPlayerUnit = false;
                        foreach (Collider collider in encompassedColliders)
                        {
                            if (!hasPlayerUnit && collider.gameObject.tag == "PlayerUnit")
                            {
                                hasPlayerUnit = true;
                                foreach (GameObject unit in currentlySelectedUnits)
                                {
                                    Unit temp = unit.GetComponent<Unit>();
                                    temp.RemoveHighlighting();
                                }
                                currentlySelectedUnits.Clear();
                                currentlySelectedUnits.Add(collider.gameObject);
                                Unit temp2 = collider.gameObject.GetComponent<Unit>();
                                temp2.AddHighlighting();
                            }
                            else if (collider.gameObject.tag == "PlayerUnit")
                            {
                                currentlySelectedUnits.Add(collider.gameObject);
                                Unit temp = collider.gameObject.GetComponent<Unit>();
                                temp.AddHighlighting();
                            }
                        }
                    }
                }
            }

            if (Input.GetMouseButton(0) && !eventSystem.IsPointerOverGameObject())
            {
                Vector3 mousePosition = Input.mousePosition;
                Ray ray = camera.ScreenPointToRay(mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    lineRend.SetPosition(0, new Vector3(dragStart.x, 0.1f, dragStart.z));
                    lineRend.SetPosition(1, new Vector3(dragStart.x, 0.1f,  hit.point.z));
                    lineRend.SetPosition(2, new Vector3(hit.point.x, 0.1f,  hit.point.z));
                    lineRend.SetPosition(3, new Vector3(hit.point.x, 0.1f,  dragStart.z));
                }
            }
        }
    }

    public void SetAction(string action)
    {
        if (action == "select")
        {
            currentAction = Action.Select;
        }
        else if (action == "drag")
        {
            currentAction = Action.Drag;
        }
        else if (action == "move")
        {
            currentAction = Action.Move;
        }
        else if (action == "attack")
        {
            currentAction = Action.Attack;
        }
        else if (action == "cast")
        {
            currentAction = Action.Cast;
        }
        else if (action == "camera")
        {
            priorAction = currentAction;
            currentAction = Action.Camera;
        }
    }

    public void EndCameraAction()
    {
        currentAction = priorAction;
    }

    public void ClearSelection()
    {
        foreach (GameObject unit in currentlySelectedUnits)
        {
            Unit temp = unit.GetComponent<Unit>();
            temp.RemoveHighlighting();
        }
        currentlySelectedUnits.Clear();
    }

    public void CallHalt()
    {
        // Halt moving units if still selected
        foreach (GameObject go in currentlySelectedUnits)
        {
            Unit temp = go.GetComponent<Unit>();
            temp.Halt();
        }
    }

    public void SetGroup(int groupNum)
    {
        if (groupNum == 1)
        {
            controlGroup1.Clear();
            foreach (GameObject unit in currentlySelectedUnits)
            {
                controlGroup1.Add(unit);
            }
        }
        else if (groupNum == 2)
        {
            controlGroup2.Clear();
            foreach (GameObject unit in currentlySelectedUnits)
            {
                controlGroup2.Add(unit);
            }
        }
        else if (groupNum == 3)
        {
            controlGroup3.Clear();
            foreach (GameObject unit in currentlySelectedUnits)
            {
                controlGroup3.Add(unit);
            }
        }
    }

    public void SelectGroup(int groupNum)
    {
        if (groupNum == 1)
        {
            foreach (GameObject unit in currentlySelectedUnits)
            {
                Unit temp = unit.GetComponent<Unit>();
                temp.RemoveHighlighting();
            }
            currentlySelectedUnits.Clear();
            if (controlGroup1.Count > 0)
            {
                foreach (GameObject unit in controlGroup1)
                {
                    currentlySelectedUnits.Add(unit);
                    Unit temp = unit.GetComponent<Unit>();
                    temp.AddHighlighting();
                }
            }
        }
        else if (groupNum == 2)
        {
            foreach (GameObject unit in currentlySelectedUnits)
            {
                Unit temp = unit.GetComponent<Unit>();
                temp.RemoveHighlighting();
            }
            currentlySelectedUnits.Clear();
            if (controlGroup2.Count > 0)
            {
                foreach (GameObject unit in controlGroup2)
                {
                    currentlySelectedUnits.Add(unit);
                    Unit temp = unit.GetComponent<Unit>();
                    temp.AddHighlighting();
                }
            }
        }
        else if (groupNum == 3)
        {
            foreach (GameObject unit in currentlySelectedUnits)
            {
                Unit temp = unit.GetComponent<Unit>();
                temp.RemoveHighlighting();
            }
            currentlySelectedUnits.Clear();
            if (controlGroup3.Count > 0)
            {
                foreach (GameObject unit in controlGroup3)
                {
                    currentlySelectedUnits.Add(unit);
                    Unit temp = unit.GetComponent<Unit>();
                    temp.AddHighlighting();
                }
            }
        }
    }

    public void CenterCamera(string target)
    {
        if (target == "unit")
        {
            if (currentlySelectedUnits.Count > 0)
            {
                camera.transform.position = new Vector3(currentlySelectedUnits[0].transform.position.x, camera.transform.position.y, currentlySelectedUnits[0].transform.position.z - 10);
            }
        }
        else if (target == "home")
        {
            // Implement this when base exists
        }
    }
}
