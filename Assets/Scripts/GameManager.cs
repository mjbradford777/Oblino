using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private GameObject[] UIButtons = new GameObject[16];

    private float timeRemaining = 300;
    public bool timerIsRunning = false;
    public Text timeText;

    private float playerScore = 0;
    private float computerScore = 0;
    // Start is called before the first frame update
    void Start()
    {
        timeText = GameObject.Find("Timer").GetComponent<Text>();
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
                // End game here
                if (playerScore >= computerScore)
                {
                    // win
                }
                else
                {
                    // lose
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

    private void OpenMenu()
    {
        // Implement open menu
    }

    private void CloseMenu()
    {
        // Implement close menu
    }

    private void DisplayTime(float displayTime)
    {
        displayTime += 1;
        float minutes = Mathf.FloorToInt(displayTime / 60);
        float seconds = Mathf.FloorToInt(timeRemaining % 60);
        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
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
}
