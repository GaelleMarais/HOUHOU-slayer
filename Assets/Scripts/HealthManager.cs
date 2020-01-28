using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public Transform healthBarImage;
    public GameObject target;
    public float health = 13.3f;
    private float healthBar = 13.3f;

    public void TakeDamage(int damage)
    {
        health -= damage;
        StartCoroutine(UpdateBar());

        if(health <= 0){
            gameObject.GetComponent<Animator>().SetBool("dead", true);
            target.GetComponent<Animator>().SetBool("victory", true);
        }
    }

    IEnumerator UpdateBar()
    {
        while (healthBar > health)
        {
            healthBar -= 0.1f;
            if(healthBar >= 0)
            {
                healthBarImage.localScale = new Vector3(healthBar, 0.2f, 0.2f);
            }
            yield return new WaitForSeconds(0.01f);
        }
        yield return null;
    }

}
