using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private GameObject[] UIButtons = new GameObject[16];
    // Start is called before the first frame update
    void Start()
    {
        UIButtons = GameObject.FindGameObjectsWithTag("UIButton");
    }

    // Update is called once per frame
    void Update()
    {
        
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
}
