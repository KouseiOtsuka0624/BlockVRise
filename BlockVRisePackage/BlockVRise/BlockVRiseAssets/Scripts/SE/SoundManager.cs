using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public GameObject speaker;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnPlaySetMinoSE(Vector3 soundPosition)
    {
        GameObject speakerInstance = Instantiate(speaker);
        speakerInstance.transform.position = soundPosition;
        FindObjectOfType<SoundEffectScript>().PlaySetMinoSoundEffect();
        StartCoroutine(DestroySpeaker(speakerInstance));
    }

    public void SpawnPlayDeleteMinoSE(int yPosition)
    {
        GameObject speakerInstance = Instantiate(speaker);
        speakerInstance.transform.position = new Vector3(4.5f, (float)yPosition, 0);
        FindObjectOfType<SoundEffectScript>().PlayDeleteMinoSoundEffect();
        StartCoroutine(DestroySpeaker(speakerInstance));
    }

    public void SpawnPlayErrorMinoSpinSE(Vector3 soundPosition)
    {
        GameObject speakerInstance = Instantiate(speaker);
        speakerInstance.transform.position = soundPosition;
        FindObjectOfType<SoundEffectScript>().PlayErrorMinoSpinSoundEffect();
        StartCoroutine(DestroySpeaker(speakerInstance));
    }
    private IEnumerator DestroySpeaker(GameObject speakerInstance)
    {
        yield return new WaitForSeconds(1f);
        Destroy(speakerInstance);
    }
}
