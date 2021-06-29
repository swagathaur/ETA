using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonSounds : MonoBehaviour
{
    [SerializeField]
    private AudioClip buttonSound;
    [SerializeField]
    private AudioClip pressSound;

    public void Selected()
    {
        GetComponent<AudioSource>().PlayOneShot(buttonSound);
    }

    public void Pressed()
    {
        GetComponent<AudioSource>().PlayOneShot(pressSound);
    }
}