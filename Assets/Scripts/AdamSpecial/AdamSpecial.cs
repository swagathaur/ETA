using UnityEngine;
using System.Collections.Generic;
using XInputDotNetPure;

public class AdamSpecial : SpecialBase {

    [SerializeField]
    private GameObject arrowPrefab;

    private PlayerControls user;
    private PlayerControls enemy;

    private float speed = 15;
    private float deathTimer = 0.2f;
    private int damage = 3;

	// Use this for initialization
	void Start ()
    {
	    
	}
	
	// Update is called once per frame
	void Update ()
    {
	    
	}

    public override void RunAttack(PlayerIndex otherPlayer)
    {
        PlayerControls[] players = GameObject.FindObjectsOfType<PlayerControls>();
        if (players[0].playerIndex == otherPlayer)
        {
            user = players[1];
            enemy = players[0];
        }
        else
        {
            user = players[0];
            enemy = players[1];
        }

        //spawn 3 arrows

        Vector2 forward = enemy.transform.forward;
        GameObject arrow = (GameObject)Instantiate(arrowPrefab);
        arrow.GetComponent<ArrowMovement>().SetVars(forward, speed, deathTimer, enemy.gameObject, damage);
        arrow.transform.position = user.transform.position;

        arrow = (GameObject)Instantiate(arrowPrefab);
        arrow.transform.position = user.transform.position;
        forward = Quaternion.Euler(0, 0, 25) * forward;
        arrow.GetComponent<ArrowMovement>().SetVars(forward, speed, deathTimer, enemy.gameObject, damage);

        arrow = (GameObject)Instantiate(arrowPrefab);
        arrow.transform.position = user.transform.position;
        forward = Quaternion.Euler(0, 0, 25) * forward;
        arrow.GetComponent<ArrowMovement>().SetVars(forward, speed, deathTimer, enemy.gameObject, damage);

        arrow = (GameObject)Instantiate(arrowPrefab);
        arrow.transform.position = user.transform.position;
        forward = Quaternion.Euler(0, 0, 245) * forward;
        arrow.GetComponent<ArrowMovement>().SetVars(forward, speed, deathTimer, enemy.gameObject, damage);

        arrow = (GameObject)Instantiate(arrowPrefab);
        arrow.transform.position = user.transform.position;
        forward = Quaternion.Euler(0, 0, 25) * forward;
        arrow.GetComponent<ArrowMovement>().SetVars(forward, speed, deathTimer, enemy.gameObject, damage);
    }
}
