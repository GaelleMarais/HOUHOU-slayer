using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterController : MonoBehaviour
{
    Animator animator;
    public GameObject target;
    public Transform healthBarImage;
    public float attackFrequence = 7;
    public RawImage screen;

    private SoundPlayer soundPlayer;
    private CharacterController targetController;
    public bool canAttack = true;
    public bool canBlock = true;
    private float health = 10f;
    private float healthBar = 10f;

    public AudioClip hitSound;
    public AudioClip attackSound;
    public AudioClip blockSound;
    public AudioClip deathSound;

    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        targetController = target.GetComponent<CharacterController>();
        soundPlayer = GetComponent<SoundPlayer>();

        if (gameObject.tag == "enemy")
        {
            StartCoroutine(EnemyAttack());
        } else if (gameObject.tag == "player")
        {
            StartCoroutine(PlayerAttack());
        }
    }

    public void TakeDamage(float damage)
    {
        if (targetController.canAttack)
        {
            canAttack = false;
            targetController.canAttack = false;
            canBlock = false;
            health -= damage;
            if(gameObject.tag == "player")
            {
                StartCoroutine(BleedEffect());
            }

            StartCoroutine(UpdateBar());

            if (health <= 0)
            {
                gameObject.GetComponent<Animator>().SetBool("dead", true);
                target.GetComponent<Animator>().SetBool("victory", true);
                StartCoroutine(soundPlayer.TriggerSound(deathSound, 0.1f));
            }
            else
            {
                gameObject.GetComponent<Animator>().SetTrigger("hit");
                StartCoroutine(soundPlayer.TriggerSound(hitSound, 0.1f));
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
                StartCoroutine(soundPlayer.TriggerSound(attackSound, 0.1f));
                yield return new WaitForSeconds(0.6f);
                targetController.TakeDamage(2);
                yield return new WaitForSeconds(1);
                canAttack = true;
                targetController.canAttack = true;
            }
            else if (Input.GetAxis("Fire1") == 1 && canBlock)
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
                StartCoroutine(soundPlayer.TriggerSound(attackSound, 0.1f));
                targetController.canAttack = false;
                yield return new WaitForSeconds(1.2f);
                targetController.TakeDamage(4.5f);
                yield return new WaitForSeconds(1);
                canAttack = true;
                targetController.canAttack = true;
                targetController.canBlock = true;
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
    }

    public IEnumerator BleedEffect()
    {
        float bleeding = 0;
        yield return new WaitForSeconds(0.15f);
        while (bleeding < 0.3f)
        {
            bleeding += 0.05f;
            screen.color = new Color(1, 0, 0, bleeding);
            yield return new WaitForSeconds(0.01f);
        }
        while (bleeding > 0.05f)
        {
            bleeding -= 0.05f;
            screen.color = new Color(1, 0, 0, bleeding);
            yield return new WaitForSeconds(0.01f);
        }
    }
}
