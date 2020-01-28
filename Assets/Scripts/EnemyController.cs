using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public GameObject target;
    public Transform healthBarImage;
    Animator animator;
    public float attackFrequence = 7;

    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        StartCoroutine(AttackLoop());
    }


    IEnumerator AttackLoop()
    {
        while (!animator.GetBool("dead"))
        {
            yield return new WaitForSeconds(2);
            animator.SetTrigger("attacking");
            yield return new WaitForSeconds(1.5f);
            target.GetComponent<HealthManager>().TakeDamage(2);
            yield return new WaitForSeconds(attackFrequence);
        }
    }
}
