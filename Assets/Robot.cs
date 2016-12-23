using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot : MonoBehaviour {

    int asleepTimer = 60 * 5;
    int asleepTimerReset;

    Animator animator;
	// Use this for initialization
	void Start () {
        asleepTimerReset = asleepTimer;
        animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void FixedUpdate()
    {
        //if we arent asleep
        if (!animator.GetBool("Asleep"))
        {
            asleepTimer--;

            if (asleepTimer == 0)
            {
                asleepTimer = asleepTimerReset;
                animator.SetBool("Asleep", true);
            }
        }
    }

    public void WakeUp()
    {
        animator.SetBool("Asleep", false);
    }
}
