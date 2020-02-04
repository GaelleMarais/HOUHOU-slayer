using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpeedAnim : MonoBehaviour
{
    float rand;
    public float decalSound;
    AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        rand = Random.Range(1f, 1.5f);
        audioSource = GetComponent<AudioSource>();
        GetComponent<Animator>().SetFloat("speedAnim", rand);
        Debug.Log("helloitsme");
        Invoke("PlaySound", rand);
    }
    
    void PlaySound()
    {
        if (audioSource != null)
            audioSource.Play();
    }
}
