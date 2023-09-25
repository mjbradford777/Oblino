using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private GameObject[] UIButtons = new GameObject[16];

    public List<GameObject> AIUnits = new List<GameObject>();

    public float timeRemaining = 300;
    public bool timerIsRunning = false;
    public Text timeText;
    public Text resourceText;

    public float playerScore = 0;
    public float computerScore = 0;

    public float playerResources = 500;
    public float computerResources = 500;
    // Start is called before the first frame update
    void Start()
    {
        timeText = GameObject.Find("Timer").GetComponent<Text>();
        resourceText = GameObject.Find("Resources").GetComponent<Text>();
        UIButtons = GameObject.FindGameObjectsWithTag("UIButton");
        timerIsRunning = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                timeRemaining = 0;
                timerIsRunning = false;
                Debug.Log(playerScore);
                if (playerScore >= computerScore)
                {
                    StartCoroutine(MoveToNewScene("Victory"));
                }
                else
                {
                    StartCoroutine(MoveToNewScene("Defeat"));
                }
            }
        }
    }

    private void ToggleOffUIButtons()
    {
        foreach (GameObject button in UIButtons)
        {
            button.SetActive(false);
        }
    }

    private void ToggleOnUIButtons()
    {
        foreach(GameObject button in UIButtons)
        {
            button.SetActive(true);
        }
    }

    private void DisplayTime(float displayTime)
    {
        displayTime += 1;
        float minutes = Mathf.FloorToInt(displayTime / 60);
        float seconds = Mathf.FloorToInt(timeRemaining % 60);
        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void UpdateResourceCount(string player, float amount)
    {
        if (player == "Player")
        {
            playerResources -= amount;
            resourceText.text = "Resources: " + playerResources;
        }
        else if (player == "Enemy")
        {
            computerResources -= amount;
        }
    }

    public void IncrementScore(string player)
    {
        if (timeRemaining > 0)
        {
            if (player == "Player")
            {
                playerScore += 0.01f;
            }
            else if (player == "Enemy")
            {
                computerScore += 0.01f;
            }
        }
    }

    public void GameEnd(string victor)
    {
        if (victor == "Player")
        {
            StartCoroutine(MoveToNewScene("Victory"));
        }
        else if (victor == "Enemy")
        {
            StartCoroutine(MoveToNewScene("Defeat"));
        }
    }

    IEnumerator MoveToNewScene(string sceneName)
    {
        yield return new WaitForSeconds(2.0f);
        SceneManager.LoadScene(sceneName: sceneName);
        yield return null;
    }
}
