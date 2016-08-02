using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HealthBarScript : MonoBehaviour
{

    public PlayerControls player;
    public GameObject winPrefab;

    private float maxHealth;
    private float exitTimer = 3;

    // Use this for initialization
    void Start()
    {
        maxHealth = player.health;
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Image>().fillAmount = player.health / maxHealth;

        if (player.health <= 0)
        {
            if (exitTimer < 0)
            {
                SceneManager.LoadScene("MainMenu");
            }
            else if (exitTimer == 3)
            {
                Time.timeScale = 0;

                winPrefab.GetComponent<Image>().enabled = true;
            }
            exitTimer -= Time.fixedDeltaTime;
        }
    }
}
