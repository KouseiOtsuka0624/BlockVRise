using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectScript : MonoBehaviour
{   
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip setMinoSoundEffect;
    [SerializeField] private AudioClip deleteMinoSoundEffect;
    [SerializeField] private AudioClip errorMinoSpinSoundEffect;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlaySetMinoSoundEffect()
    {
        audioSource.PlayOneShot(setMinoSoundEffect);
    }

    public void PlayDeleteMinoSoundEffect()
    {
        audioSource.PlayOneShot(deleteMinoSoundEffect);
    }

    public void PlayErrorMinoSpinSoundEffect()
    {
        audioSource.PlayOneShot(errorMinoSpinSoundEffect);
    }
    
}
