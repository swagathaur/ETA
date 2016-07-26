using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameController : MonoBehaviour {

    GameObject[] players;
    Text text;

    bool started = false;
    private float countdown = 3;

	// Use this for initialization
	void Start () {
        text = GameObject.Find("Countdown").GetComponent<Text>();
        players = GameObject.FindGameObjectsWithTag("Player");
	    foreach(GameObject player in players)
        {
            player.SetActive(false);
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (countdown > 0)
        {
            countdown -= Time.deltaTime;
            if (countdown > 2)
            {
                //todo: say 3
                text.text = "3";
            }
            else if (countdown > 1)
            {
                //todo: say 2
                text.text = "2";
            }
            else if (countdown > 0)
            {
                //say 1
                text.text = "1";
            }
        }
        else
        {
            text.text = "";
            foreach (GameObject player in players)
            {
                player.SetActive(true);
            }
            started = true;
        }

	}
}
