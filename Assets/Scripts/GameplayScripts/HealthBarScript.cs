using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using XInputDotNetPure;

public class HealthBarScript : MonoBehaviour
{
    public PlayerIndex playerIndex;
    private GameObject[] players;
    private PlayerControls player;
    public GameObject winPrefab;

    private float maxHealth = 0;
    private float exitTimer = 3;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        GetPlayer();
        if (maxHealth  == 0)
        {
            maxHealth = player.health;
        }
        GetComponent<Image>().fillAmount = player.health / maxHealth;
        if (player.health <= 0)
        {
            if (exitTimer < 0)
            {
                SceneManager.LoadScene("MainMenu");
            }
            else if (exitTimer == 3)
            {
                //suspend both the players
                foreach (GameObject p in players)
                {
                    p.GetComponent<PlayerControls>().isSuspended = true;
                    p.GetComponent<PlayerControls>().Freeze();
                }
                //todo: load this some other way
                //grab the winprefab
                winPrefab.GetComponent<Image>().enabled = true;
            }
            exitTimer -= Time.fixedDeltaTime;
        }
    }

    void GetPlayer()
    {
        if (players == null)
        {

            players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject p in players)
            {
                PlayerControls pc = p.GetComponent<PlayerControls>();
                if (pc.playerIndex == playerIndex)
                    player = pc;
            }
        }
    }
}
