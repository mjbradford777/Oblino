using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryScene : MonoBehaviour
{
    private Animator unitAnim;
    private bool isDancing = false;
    // Start is called before the first frame update
    void Start()
    {
        unitAnim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDancing)
        {
            isDancing = true;
            unitAnim.SetBool("isVictoryAnim", true);
        }
    }
}
