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
    private List<GameObject> controlGroup4 = new List<GameObject>();

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

    public void ClearSelection()
    {
        currentlySelectedUnits.Clear();
    }

    public void SetMoveAction()
    {
        currentAction = Action.Move;
        Debug.Log(Equals(currentAction, Action.Move));
    }
}
