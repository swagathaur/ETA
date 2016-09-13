using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class GameController : MonoBehaviour
{

    GameObject[] players;
    Image image;

    public Sprite sprite0;
    public Sprite sprite1;
    public Sprite sprite2;
    public Sprite sprite3;

    AudioScript audioSource;

    public AudioClip clip3;
    public AudioClip clip2;
    public AudioClip clip1;
    public AudioClip clipFight;

    public bool CountdownOverride = false;
    private float countdown = 4;
    private bool started = false; //stops PlayerControl.IsSuspended being reset every frame

    // Use this for initialization
    void Start()
    {
        SpawnPlayers();
        audioSource = FindObjectOfType<AudioScript>();
        image = GameObject.Find("Countdown").GetComponent<Image>();

        if (CountdownOverride)
            countdown = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (countdown > 0)
        {
            image.enabled = true;
            countdown -= Time.deltaTime;
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
                    audioSource.PlayBGM();
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
                    GameObject temp = Instantiate(FindObjectOfType<SelectionScript>().P1prefab, spawnPoint.transform.position, new Quaternion(), GameObject.FindGameObjectWithTag("FighterRotations").transform) as GameObject;
                    temp.GetComponent<PlayerControls>().playerIndex = XInputDotNetPure.PlayerIndex.One;
                }
                else
                {
                    GameObject temp = Instantiate(FindObjectOfType<SelectionScript>().P2prefab, spawnPoint.transform.position, new Quaternion(), GameObject.FindGameObjectWithTag("FighterRotations").transform) as GameObject;
                    temp.GetComponent<PlayerControls>().playerIndex = XInputDotNetPure.PlayerIndex.Two;
                }
            }

            players = GameObject.FindGameObjectsWithTag("Player");
        }
    }
}
