using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpeedAnim : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Animator>().SetFloat("speedAnim", Random.Range(1f, 1.5f));
        Debug.Log("helloitsme");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
