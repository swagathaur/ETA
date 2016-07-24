using UnityEngine;
using System.Collections;
using UnityEngine.UI;

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
    }
}
