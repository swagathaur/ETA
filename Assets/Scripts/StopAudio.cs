using UnityEngine;
using System.Collections;
using UnityEditor.SceneManagement;

public class StopAudio : MonoBehaviour
{
    private string currentScene;

    void Start()
    {
        currentScene = EditorSceneManager.GetActiveScene().name;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentScene != EditorSceneManager.GetActiveScene().name)
        {
            FindObjectOfType<AudioScript>().StopSound();
        }

    }
}
