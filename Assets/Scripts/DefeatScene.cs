using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefeatScene : MonoBehaviour
{
    private Animator unitAnim;
    private bool isDead = false;
    // Start is called before the first frame update
    void Start()
    {
        unitAnim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDead)
        {
            isDead = true;
            unitAnim.SetBool("isDeathAnim", true);
        }
    }
}
