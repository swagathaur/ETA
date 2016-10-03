using UnityEngine;
using System.Collections.Generic;
using System;
using XInputDotNetPure;

public class DickSpecial : SpecialBase
{
    [SerializeField]
    private GameObject arrowPrefab;
    private GameObject enemy;
    private List<GameObject> arrows;
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
        float amountAboveFloor = 100;

        float wallDiff = Math.Abs(walls[0].transform.position.x - walls[1].transform.position.y);
        float arrowWidth = arrowPrefab.GetComponent<BoxCollider>().size.x;
        float numArrows = wallDiff / arrowWidth;

        int deletedOne = (int)UnityEngine.Random.Range(2, numArrows - 2);

        //y: should be certain distance above the floor
        float y = GameObject.FindGameObjectWithTag("Terrain").gameObject.transform.position.y + amountAboveFloor;
        //z: should be from the player
        float z = enemy.transform.position.z;

        //x: should be bsed on distance between both walls and width of arrows
        for (int i = 0; i < numArrows; ++i)
        {
            if (i == deletedOne)
                continue;
            arrows.Add((GameObject)Instantiate(arrowPrefab, new Vector3(i * arrowWidth, y, z), Quaternion.identity));
        }

        UnityEditor.EditorApplication.isPaused = true;
        //todo: create shadows
    }
}
