using UnityEngine;
using System.Collections.Generic;
using XInputDotNetPure;

public class AdamSpecial : SpecialBase {

    [SerializeField]
    private GameObject arrowPrefab;

    private PlayerControls user;
    private PlayerControls enemy;

    [SerializeField]
    private float speed = 15;
    [SerializeField]
    private float deathTimer = 0.2f;
    [SerializeField]
    private int damage = 2;

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

        Vector3 startPos = user.arrowSpawner.transform.position;
        Vector2 forward = user.transform.forward;
        GameObject arrow = (GameObject)Instantiate(arrowPrefab);
        arrow.GetComponent<ArrowMovement>().SetVars(forward, speed, deathTimer, enemy.gameObject, damage);
        arrow.transform.position = startPos;
        arrow.GetComponent<ArrowMovement>().numReflections = 1000;
        arrow.GetComponent<ArrowMovement>().growRate = arrow.GetComponent<ArrowMovement>().growRate * 0.75f;

        arrow = (GameObject)Instantiate(arrowPrefab);
        arrow.transform.position = startPos;
        forward = Quaternion.Euler(0, 0, 25) * forward;
        arrow.GetComponent<ArrowMovement>().SetVars(forward, speed, deathTimer, enemy.gameObject, damage);
        arrow.GetComponent<ArrowMovement>().numReflections = 1000;
        arrow.GetComponent<ArrowMovement>().growRate = arrow.GetComponent<ArrowMovement>().growRate * 0.75f;

        arrow = (GameObject)Instantiate(arrowPrefab);
        arrow.transform.position = startPos;
        forward = Quaternion.Euler(0, 0, 270) * forward;
        arrow.GetComponent<ArrowMovement>().SetVars(forward, speed, deathTimer, enemy.gameObject, damage);
        arrow.GetComponent<ArrowMovement>().numReflections = 1000;
        arrow.GetComponent<ArrowMovement>().growRate = arrow.GetComponent<ArrowMovement>().growRate * 0.75f;
    }
}
