using UnityEngine;
using System.Collections;
using System;
using XInputDotNetPure;

public class DickSpecial : SpecialBase
{
    [SerializeField]
    private GameObject arrowPrefab;
    private GameObject enemy;
    private GameObject[] arrows;
    // Use this for initialization

    void Start()
    {        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void RunAttack(PlayerIndex playerIndex)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (var i = 0; i < players.Length; ++i)
        {
            if (players[i].GetComponent<PlayerControls>().playerIndex == playerIndex)
            {
                enemy = players[i];
                break;
            }
        }

        //sort walls
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");


        //magic numbers
        float amountAboveFloor = 50;

        float wallDiff = Math.Abs(walls[0].transform.position.x - walls[1].transform.position.y);
        float arrowWidth = arrowPrefab.GetComponent<BoxCollider>().size.x;
        float numArrows = wallDiff / arrowWidth;

        //x: should be bsed on distance between both walls and width of arrows
        
        //y: should be certain distance above the floor
        float y = GameObject.FindGameObjectWithTag("Terrain").gameObject.transform.position.y + amountAboveFloor;
        //z: should be from the player
        float z = enemy.transform.position.z;
    }
}
