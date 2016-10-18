using UnityEngine;
using System.Collections;
using XInputDotNetPure;

public class WinCounter : MonoBehaviour {

    [HideInInspector]
    public int player1Wins = 0;
    [HideInInspector]
    public int player2Wins = 0;

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	    
	}

    //pi is losing player
    public void Increment(PlayerIndex pi)
    {
        if (pi == PlayerIndex.One)
            player2Wins++;
        else
            player1Wins++;
    }

    public void Clear()
    {
        player1Wins = 0;
        player2Wins = 0;
    }
}
