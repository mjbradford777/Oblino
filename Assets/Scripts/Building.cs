using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;
using UnityEditor.PackageManager;
using UnityEngine;

public class Building : MonoBehaviour
{
    private GameManager gameManager;

    private string side;
    public Transform FootmanPBR;
    public Transform DogPBR;
    public Transform GruntPBR;
    public Transform PBR_Golem;

    private string unitOption;

    private Transform unitsContainer;

    private float maxHP;
    private float currentHP;
    private float armor;

    public bool isDestroyed = false;
    private bool isBuilding = false;

    public Building(string side, float maxHP, float currentHP, float armor)
    {
        this.side = side;
        this.maxHP = maxHP;
        this.currentHP = currentHP;
        this.armor = armor;
    }

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (side == "Player")
        {
            unitsContainer = GameObject.Find("PlayerUnits").transform;
        }   
        else if (side == "Enemy")
        {
            unitsContainer = GameObject.Find("EnemyUnits").transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDestroyed)
        {
            if (currentHP <= 0.0f)
            {
                Debug.Log(currentHP);
                Debug.Log("Being Destroyed");
                isDestroyed = true;
            }
        }
        if (isDestroyed)
        {
            StopCoroutine("CreateUnit");
            if (side == "Enemy")
            {
                gameManager.GameEnd("Player");
            }
            else if (side == "Player")
            {
                gameManager.GameEnd("Enemy");
            }
            Destroy(gameObject, 1.5f);
        }
    }

    public void TakeDamage(float damageAmount)
    {
        currentHP -= damageAmount - armor;
        Debug.Log(name + ": " + currentHP);
    }

    public void BuildUnit(string unitType)
    {
        unitOption = unitType;
        if (!isBuilding)
        {
            StartCoroutine("CreateUnit");
        }
    }

    IEnumerator CreateUnit()
    {
        isBuilding = true;
        if (unitOption == "Footman")
        {
            if (gameManager.playerResources >= 30)
            {
                yield return new WaitForSeconds(6.0f);
                Transform createdObject = Instantiate(FootmanPBR, new Vector3(transform.position.x - 5.0f, 0, transform.position.z - 5.0f), transform.rotation, unitsContainer);
                GameObject unit = createdObject.gameObject;
                unit.tag = "PlayerUnit";
                gameManager.UpdateResourceCount("Player", 30);
            }
        }
        else if (unitOption == "Grunt")
        {
            yield return new WaitForSeconds(3.0f);
            Transform createdObject = Instantiate(GruntPBR, new Vector3(transform.position.x - 5.0f, 0, transform.position.z - 5.0f), transform.rotation, unitsContainer);
            GameObject unit = createdObject.gameObject;
            unit.tag = "EnemyUnit";
            gameManager.AIUnits.Add(unit);
            gameManager.UpdateResourceCount("Enemy", 30);
        }
        else if (unitOption == "Knight")
        {
            if (gameManager.playerResources >= 50)
            {
                yield return new WaitForSeconds(6.0f);
                Transform createdObject = Instantiate(DogPBR, new Vector3(transform.position.x - 5.0f, 0, transform.position.z - 5.0f), transform.rotation, unitsContainer);
                GameObject unit = createdObject.gameObject;
                unit.tag = "PlayerUnit";
                gameManager.UpdateResourceCount("Player", 50);
            }
        }
        else if (unitOption == "Golem")
        {
            yield return new WaitForSeconds(6.0f);
            Transform createdObject = Instantiate(PBR_Golem, new Vector3(transform.position.x - 5.0f, 0, transform.position.z - 5.0f), transform.rotation, unitsContainer);
            GameObject unit = createdObject.gameObject;
            unit.tag = "EnemyUnit";
            gameManager.AIUnits.Add(unit);
            gameManager.UpdateResourceCount("Enemy", 50);
        }
        isBuilding = false;
        yield return null;
    }
}
