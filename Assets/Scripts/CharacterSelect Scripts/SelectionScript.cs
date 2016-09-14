using UnityEngine;
using System.Collections;
using XInputDotNetPure;
using UnityEngine.SceneManagement;

public class SelectionScript : MonoBehaviour
{
    [HideInInspector]
    public GameObject P1prefab;
    [HideInInspector]
    public GameObject P2prefab;
    [HideInInspector]
    public Color P1Color;
    [HideInInspector]
    public Color P2Color;

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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed || GamePad.GetState(PlayerIndex.Two).Buttons.Start == ButtonState.Pressed)
            {
            loaded = true;
            loadLevel(Level.Anarchy);
            }
        }
    }
}