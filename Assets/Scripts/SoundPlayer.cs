using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundPlayer : MonoBehaviour
{

    public IEnumerator TriggerSound(AudioClip sound, float wait)
    {
        yield return new WaitForSeconds(wait);
        if(sound != null)
        {
            GetComponent<AudioSource>().PlayOneShot(sound);
        }
    }
}	
