using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    Animator animator;
    public GameObject target;
    public Transform healthBarImage;
    public float attackFrequence = 7;

    private CharacterController targetController;
    public bool canAttack = true; //TODO private
    private float health = 13.3f;
    private float healthBar = 13.3f;

    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        targetController = target.GetComponent<CharacterController>();

        if (gameObject.tag == "enemy")
        {
            StartCoroutine(EnemyAttack());
        } else if (gameObject.tag == "player")
        {
            StartCoroutine(PlayerAttack());
        }
    }

    public void TakeDamage(int damage)
    {
        if (targetController.canAttack)
        {
            canAttack = false;
            targetController.canAttack = false;
            health -= damage;

            StartCoroutine(UpdateBar());

            if (health <= 0)
            {
                gameObject.GetComponent<Animator>().SetBool("dead", true);
                target.GetComponent<Animator>().SetBool("victory", true);
            }
            else
            {
                gameObject.GetComponent<Animator>().SetTrigger("hit");
            }
        }       
    }

    IEnumerator PlayerAttack()
    {
        while (!animator.GetBool("dead"))
        {
            if (Input.GetAxis("Fire2") == 1 && canAttack)
            {
                targetController.canAttack = false;
                animator.SetTrigger("attacking");
                yield return new WaitForSeconds(0.6f);
                targetController.TakeDamage(2);
            }
            else if (Input.GetAxis("Fire1") == 1)
            {
                canAttack = false;
                targetController.canAttack = false;
                animator.SetTrigger("blocking");
                yield return new WaitForSeconds(2);
                targetController.canAttack = true;
                canAttack = true;
            }
            else
            {
                yield return new WaitForSeconds(0.01f);
            }
        }
    }

    IEnumerator EnemyAttack()
    {
        while (!animator.GetBool("dead"))
        {
            if (canAttack)
            {
                animator.SetTrigger("attacking");
                targetController.canAttack = false;
                yield return new WaitForSeconds(1.2f);
                targetController.TakeDamage(6);
                
            }
            yield return new WaitForSeconds(attackFrequence);
        }
    }

    IEnumerator UpdateBar()
    {
        while (healthBar > health)
        {
            healthBar -= 0.1f;
            if (healthBar >= 0)
            {
                healthBarImage.localScale = new Vector3(healthBar, 0.2f, 0.2f);
            }
            yield return new WaitForSeconds(0.01f);
        }
        yield return new WaitForSeconds(1);
        canAttack = true;
        targetController.canAttack = true;
    }
}
