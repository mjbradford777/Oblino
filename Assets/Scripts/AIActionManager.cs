using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIActionManager : MonoBehaviour
{
    private GameManager gameManager;
    private GameObject HomeBase;
    private GameObject EnemyBase;
    private GameObject Oblino1;
    private GameObject Oblino2;
    private GameObject Oblino3;

    private List<GameObject> defenseUnits = new List<GameObject>();
    private List<GameObject> currentlySelectedUnits = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        HomeBase = GameObject.Find("MushroomHouse");
        EnemyBase = GameObject.Find("KnightHouse");
        Oblino1 = GameObject.Find("Oblino 1");
        Oblino2 = GameObject.Find("Oblino 2");
        Oblino3 = GameObject.Find("Oblino 3");
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.computerResources >= 30)
        {
            if (gameManager.AIUnits.Count < 4)
            {
                if (gameManager.computerResources >= 50)
                {
                    int decider = Random.Range(0, 2);
                    if (decider == 0)
                    {
                        HomeBase.GetComponent<Building>().BuildUnit("Grunt");
                    }
                    else
                    {
                        HomeBase.GetComponent<Building>().BuildUnit("Golem");
                    }
                }
                else
                {
                    HomeBase.GetComponent<Building>().BuildUnit("Grunt");
                }
            }

            if (gameManager.timeRemaining > 240)
            {
                if (gameManager.AIUnits.Count < 6)
                {
                    if (gameManager.computerResources >= 50)
                    {
                        int decider = Random.Range(0, 2);
                        if (decider == 0)
                        {
                            HomeBase.GetComponent<Building>().BuildUnit("Grunt");
                        }
                        else
                        {
                            HomeBase.GetComponent<Building>().BuildUnit("Golem");
                        }
                    }
                    else
                    {
                        HomeBase.GetComponent<Building>().BuildUnit("Grunt");
                    }
                }
            }
        }

        if (defenseUnits.Count < 2 && gameManager.AIUnits.Count >= 2)
        {
            defenseUnits.Add(gameManager.AIUnits[0]);
            defenseUnits.Add(gameManager.AIUnits[1]);
            foreach (GameObject go in defenseUnits)
            {
                Unit temp = go.GetComponent<Unit>();
                temp.StartPathfinding(new Vector3(HomeBase.transform.position.x - 10.0f, 0, HomeBase.transform.position.z - 10.0f), "attack", false);
            }
        }

        if (gameManager.timeRemaining < 240 && gameManager.timeRemaining > 60)
        {
            if (gameManager.AIUnits.Count > 4)
            {
                Unit temp1 = gameManager.AIUnits[2].GetComponent<Unit>();
                temp1.StartPathfinding(new Vector3(Oblino3.transform.position.x - 20.0f, 0, Oblino3.transform.position.z - 20.0f), "attack", false);
                Unit temp2 = gameManager.AIUnits[3].GetComponent<Unit>();
                temp2.StartPathfinding(new Vector3(Oblino3.transform.position.x - 20.0f, 0, Oblino3.transform.position.z - 20.0f), "attack", false);

                for (int i = 4; i < gameManager.AIUnits.Count; i++)
                {
                    Unit temp = gameManager.AIUnits[i].GetComponent<Unit>();
                    temp.StartPathfinding(new Vector3(Oblino2.transform.position.x - 20.0f, 0, Oblino2.transform.position.z - 20.0f), "attack", false);
                }
            }
            else if (gameManager.AIUnits.Count > 2)
            {
                for (int i = 2; i < gameManager.AIUnits.Count; i++)
                {
                    Unit temp = gameManager.AIUnits[i].GetComponent<Unit>();
                    temp.StartPathfinding(new Vector3(Oblino3.transform.position.x - 20.0f, 0, Oblino3.transform.position.z - 20.0f), "attack", false);
                }
            }
        }

        if (gameManager.timeRemaining < 60)
        {
            if (gameManager.computerScore < gameManager.playerScore)
            {
                for (int i = 0; i < gameManager.AIUnits.Count; i++)
                {
                    Unit temp = gameManager.AIUnits[i].GetComponent<Unit>();
                    temp.StartPathfinding(new Vector3(EnemyBase.transform.position.x - 15.0f, 0, EnemyBase.transform.position.z - 15.0f), "attack", false);
                }
            }
        }
    }
}
