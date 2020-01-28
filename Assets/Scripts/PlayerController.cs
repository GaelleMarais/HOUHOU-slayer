using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Animator animator;
    public GameObject target;


    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        StartCoroutine(Attack());
    }

    IEnumerator Attack()
    {
        while (true)
        {
            if (Input.GetAxis("Fire2") == 1)
            {
                animator.SetTrigger("attacking");
                yield return new WaitForSeconds(0.7f);
                target.GetComponent<HealthManager>().TakeDamage(2);
            } else if (Input.GetAxis("Fire1") == 1)
            {
                animator.SetTrigger("blocking");
                yield return new WaitForSeconds(1);
            }
            else
            {
                yield return new WaitForSeconds(0.01f);
            }            
        }        
    }

    private void Update()
    {
        
    }
}
