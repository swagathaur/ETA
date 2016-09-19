using UnityEngine;
using System.Collections;
using XInputDotNetPure;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [SerializeField]
    private GameObject pressStart;
    private bool add = true;

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
        if (!(P1prefab == null || P2prefab == null) )
        {
            if (add)
            {
                pressStart = GameObject.Find("PressStart");
                Color temp = pressStart.GetComponent<Image>().color;
                temp.a += Time.deltaTime * 2;
                if (temp.a >= 1)
                {
                    temp.a = 1;
                    add = false;
                }
                pressStart.GetComponent<Image>().color = temp;
            }
            else
            {
                pressStart = GameObject.Find("PressStart");
                Color temp = pressStart.GetComponent<Image>().color;
                temp.a -= Time.deltaTime * 2;
                if (temp.a <= 0)
                {
                    temp.a = 0;
                    add = true;
                }
                pressStart.GetComponent<Image>().color = temp;
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed || GamePad.GetState(PlayerIndex.Two).Buttons.Start == ButtonState.Pressed)
            {
                P1prefab.GetComponent<PlayerControls>().glowColor = P1Color;
                P2prefab.GetComponent<PlayerControls>().glowColor = P2Color;
                loaded = true;
                loadLevel(Level.Anarchy);
            }
            if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed || GamePad.GetState(PlayerIndex.Two).Buttons.Start == ButtonState.Pressed)
            {
                P1prefab.GetComponent<PlayerControls>().glowColor = P1Color;
                P2prefab.GetComponent<PlayerControls>().glowColor = P2Color;
                loaded = true;
                loadLevel(Level.Anarchy);
            }
        }
        else if (!loaded)
        {
            Color temp = pressStart.GetComponent<Image>().color;
            temp.a = 0;
            pressStart.GetComponent<Image>().color = temp;
        }
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            P1prefab = Resources.Load("Prefabs/Anarchy/Anarchy 1") as GameObject;
            P2prefab = Resources.Load("Prefabs/Anarchy/Anarchy 2") as GameObject;
            P1prefab.GetComponent<PlayerControls>().glowColor = Color.red;
            P2prefab.GetComponent<PlayerControls>().glowColor = Color.blue;
            loaded = true;
            loadLevel(Level.Anarchy);
        }
    }

    void Awake()
    {
        if (FindObjectsOfType(GetType()).Length > 1)
        {
            Destroy(gameObject);
        }
    }
}