using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class StopAudio : MonoBehaviour
{
    private string currentScene;

    void Start()
    {
        currentScene = SceneManager.GetActiveScene().name;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentScene != SceneManager.GetActiveScene().name)
        {
            FindObjectOfType<AudioScript>().StopSound();
        }

    }
}
