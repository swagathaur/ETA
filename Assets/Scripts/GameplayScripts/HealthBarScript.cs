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
    private float maxHealth = 0;

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
            GameObject.Find("GameController").GetComponent<GameController>().EndGame(playerIndex);
        }
    }

    void GetPlayer()
    {
        if (players == null || players.Length == 0)
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
