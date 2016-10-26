using UnityEngine;
using System.Collections;
using XInputDotNetPure;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectionScript : MonoBehaviour
{
//    [HideInInspector]
    public GameObject P1prefab;
    //[HideInInspector]
    public GameObject P2prefab;
    [HideInInspector]
    public Color P1Color;
    [HideInInspector]
    public Color P2Color;
    [HideInInspector]
    public Sprite P1Icon;
    [HideInInspector]
    public Sprite P2Icon;
    [HideInInspector]
    public bool loaded = false;

    [SerializeField]
    private GameObject pressStart;
    private bool add = true;

    public void loadLevel()
    {
        DontDestroyOnLoad(this.gameObject);
        SceneManager.LoadScene("Level Select");
    }

    void Update()
    {
        if (pressStart == null)
            pressStart = GameObject.Find("PressStart");

        if (!loaded)
        {
            if (!(P1prefab == null || P2prefab == null))
            {
                if (add)
                {
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
                    loadLevel();
                }
                if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed || GamePad.GetState(PlayerIndex.Two).Buttons.Start == ButtonState.Pressed)
                {
                    P1prefab.GetComponent<PlayerControls>().glowColor = P1Color;
                    P2prefab.GetComponent<PlayerControls>().glowColor = P2Color;
                    loaded = true;
                    loadLevel();
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
                if (P1prefab == null)
                    P1prefab = Resources.Load("Prefabs/Dick/Richard1") as GameObject;
                if (P2prefab == null)
                    P2prefab = Resources.Load("Prefabs/Anarchy/Anarchy 1") as GameObject;
                P1prefab.GetComponent<PlayerControls>().glowColor = Color.red;
                P2prefab.GetComponent<PlayerControls>().glowColor = Color.blue;

                P1Icon = GameObject.Find("HardwoodButton").GetComponent<Image>().sprite;
                P2Icon = GameObject.Find("AnarchyButton").GetComponent<Image>().sprite;

                loaded = true;
                add = false;
                loadLevel();
            }
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