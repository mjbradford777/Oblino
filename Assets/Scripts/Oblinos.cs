using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oblinos : MonoBehaviour
{
    private GameManager gameManager;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        List<GameObject> unitList = unitsInRange();
        int numPlayer = 0;
        int numEnemy = 0;
        foreach (GameObject unit in unitList)
        {
            if (unit.tag == "PlayerUnit")
            {
                numPlayer++;
            }
            else if (unit.tag == "EnemyUnit")
            {
                numEnemy++;
            }
        }
        if (numPlayer > 0 && numEnemy == 0)
        {
            for (int i = 0; i < numPlayer; i++)
            {
                gameManager.IncrementScore("Player");
            }
        }
        else if (numPlayer == 0 && numEnemy > 0)
        {
            for (int i = 0; i < numEnemy; i++)
            {
                gameManager.IncrementScore("Enemy");
            }
        }
    }

    private List<GameObject> unitsInRange()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 20.0f);
        List<GameObject> units = new List<GameObject>();
        foreach (Collider collider in colliders) {
            if (collider.gameObject.tag == "PlayerUnit" || collider.gameObject.tag == "EnemyUnit")
            {
                units.Add(collider.gameObject);
            }
        }
        return units;
    }
}
