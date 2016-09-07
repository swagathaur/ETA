using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using XInputDotNetPure;

public class SpecialBarScript : MonoBehaviour {

    public PlayerIndex playerIndex;
    public PlayerControls player;
    private GameObject[] players;

    // Use this for initialization
    void Start ()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            PlayerControls pc = p.GetComponent<PlayerControls>();
            if (pc.playerIndex == playerIndex)
                player = pc;
        }
    }
	
	// Update is called once per frame
	void Update () {
        GetComponent<Image>().fillAmount = player.special / (float)player.maxSpecial;
	}
}
