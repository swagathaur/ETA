using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SelectionScript : MonoBehaviour
{
    [HideInInspector]
    public GameObject P1prefab;
    public GameObject P2prefab;

    private bool loaded = false;

    public enum Level
    {
        Anarchy,
        Hardwood
    };

    public Level chosenLevel;

    public void loadLevel(Level levToLoad)
    {
        DontDestroyOnLoad(this.gameObject);

        switch (levToLoad)
        {
            case Level.Anarchy:
                SceneManager.LoadScene("Anarchy Level");
                break;
            case Level.Hardwood:
                SceneManager.LoadScene("Hardwood Level");
                break;
        }

    }

    void Update()
    {
        if (!(P1prefab == null || P2prefab == null) && !loaded)
        {
            loaded = true;
            loadLevel(Level.Anarchy);
        }
    }
}