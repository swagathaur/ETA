using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HealthBarScript : MonoBehaviour
{

    public PlayerControls player;
    private float maxHealth;

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
            SceneManager.LoadScene("main");
        }
    }
}
