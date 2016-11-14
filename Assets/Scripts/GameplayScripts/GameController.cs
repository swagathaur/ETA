using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

using XInputDotNetPure;

public class GameController : MonoBehaviour
{

    GameObject[] players;
    Image image;

    [HideInInspector]
    public Sprite sprite0;
    [HideInInspector]
    public Sprite sprite1;
    [HideInInspector]
    public Sprite sprite2;
    [HideInInspector]
    public Sprite sprite3;

    AudioScript audioSource;

    [HideInInspector]
    public AudioClip clip3;
    [HideInInspector]
    public AudioClip clip2;
    [HideInInspector]
    public AudioClip clip1;
    [HideInInspector]
    public AudioClip clipFight;

    public bool CountdownOverride = false;
    private float countdown = 4;

    [HideInInspector]
    public bool started = false; //stops PlayerControl.IsSuspended being reset every frame
    [HideInInspector]
    public bool ended = false;

    //end game shizz
    private float exitTimer = 3;
    private bool hasInstantiatedExitMenu = false;
    GameObject endMenuParent;

    // Use this for initialization
    void Start()
    {
        SpawnPlayers();
        audioSource = FindObjectOfType<AudioScript>();
        image = GameObject.Find("Countdown").GetComponent<Image>();
        endMenuParent = GameObject.Find("EndGameMenu");

        GameObject.Find("P1WinCounter").GetComponent<Text>().text = GameObject.Find("SELECTIONS").GetComponent<WinCounter>().player1Wins.ToString();
        GameObject.Find("P2WinCounter").GetComponent<Text>().text = GameObject.Find("SELECTIONS").GetComponent<WinCounter>().player2Wins.ToString();

        if (CountdownOverride)
            countdown = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (countdown >= 0)
        {
            countdown -= Time.deltaTime;
            image.enabled = true;
            if (countdown < 3.9f)
            {
                if (countdown > 3)
                {
                    if (image.sprite != sprite3)
                    {
                        audioSource.playSound(clip3);
                        image.sprite = sprite3;
                    }
                }
                else if (countdown > 2)
                {
                    if (image.sprite != sprite2)
                    {
                        audioSource.playSound(clip2);
                        image.sprite = sprite2;
                    }
                }
                else if (countdown > 1)
                {
                    if (image.sprite != sprite1)
                    {
                        audioSource.playSound(clip1);
                        image.sprite = sprite1;
                    }

                }
                else if (countdown > 0)
                {
                    if (image.sprite != sprite0)
                    {
                        audioSource.playSound(clipFight);
                        image.sprite = sprite0;
                    }
                }
                else
                {
                    audioSource.PlayBGM();
                }
            }
        }
        else if (!started)
        {
            image.enabled = false;
            //start the game!
            foreach (GameObject player in players)
                player.GetComponent<PlayerControls>().isSuspended = false;

            started = true;
        }
    }

    private void SpawnPlayers()
    {
        if (players == null)
        {
            foreach (GameObject spawnPoint in GameObject.FindGameObjectsWithTag("SpawnPoints"))
            {
                if (spawnPoint.GetComponent<spawnPointHolder>().playerIndex == XInputDotNetPure.PlayerIndex.One)
                {
                    GameObject temp = Instantiate(FindObjectOfType<SelectionScript>().P1prefab, spawnPoint.transform.position,
                        new Quaternion(), GameObject.FindGameObjectWithTag("FighterRotations").transform) as GameObject;
                    temp.GetComponent<PlayerControls>().playerIndex = XInputDotNetPure.PlayerIndex.One;

                    GameObject.Find("Player1Icon").GetComponent<UnityEngine.UI.Image>().sprite = FindObjectOfType<SelectionScript>().P1Icon;
                }
                else
                {
                    GameObject temp = Instantiate(FindObjectOfType<SelectionScript>().P2prefab, spawnPoint.transform.position,
                        new Quaternion(), GameObject.FindGameObjectWithTag("FighterRotations").transform) as GameObject;
                    temp.GetComponent<PlayerControls>().playerIndex = XInputDotNetPure.PlayerIndex.Two;

                    GameObject.Find("Player2Icon").GetComponent<UnityEngine.UI.Image>().sprite = FindObjectOfType<SelectionScript>().P2Icon;
                }
            }

            players = GameObject.FindGameObjectsWithTag("Player");
        }
    }

    //passes in the losing player
    public void EndGame(PlayerIndex player)
    {
        ended = true;
        if (exitTimer < 0)
        {
            if (!hasInstantiatedExitMenu)
            {
                //disable ability to pause
                transform.GetComponent<PauseScript>().Disable();
                //find gameover menu
                endMenuParent.GetComponent<EndMenuScript>().Enable();
                //store wins
                GameObject.Find("SELECTIONS").GetComponent<WinCounter>().Increment(player);
                hasInstantiatedExitMenu = true;
            }
        }
        else if (exitTimer == 3)
        {
            //suspend both the players
            foreach (GameObject p in players)
            {
                if (p.GetComponent<PlayerControls>().playerIndex == player)
                {
                    p.GetComponent<PlayerControls>().KillPlayer();
                }
                else
                {
                    p.GetComponent<PlayerControls>().WinPlayer();
                }
            }
            //todo: load this some other way
            //grab the winprefab
            GameObject winPrefab;
            if (player == PlayerIndex.One)
                winPrefab = GameObject.Find("P2 Wins");
            else
                winPrefab = GameObject.Find("P1 Wins");

            winPrefab.GetComponent<Image>().enabled = true;
        }
        exitTimer -= Time.fixedDeltaTime;
    }

}
