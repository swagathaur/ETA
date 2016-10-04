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

    [SerializeField] private float arrowFallSpeed;
    [SerializeField] private short arrowDamage;

    // Use this for initialization

    void Start()
    {
        arrows = new List<GameObject>();
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

        GameObject leftWall = walls[0];
        GameObject rightWall = walls[1];

        if (leftWall.transform.position.x > rightWall.transform.position.x)
        {
            GameObject temp = rightWall;
            rightWall = leftWall;
            leftWall = temp;
        }

        //magic numbers
        float amountAboveFloor = 15;

        float wallDiff = Math.Abs(walls[0].transform.position.x - walls[1].transform.position.y);
        float arrowWidth = arrowPrefab.GetComponent<BoxCollider>().size.x;
        float numArrows = wallDiff / arrowWidth;

        int deletedOne = (int)UnityEngine.Random.Range(2, numArrows - 2);

        //xOffset: the offset to spawn the left-most arrow at
        float xOffset = leftWall.transform.position.x;

        //y: should be certain distance above the floor
        float y = GameObject.FindGameObjectWithTag("Terrain").gameObject.transform.position.y + amountAboveFloor;
        //z: should be from the player
        float z = enemy.transform.position.z;

        //x: should be based on distance between both walls and width of arrows
        for (int i = 0; i < numArrows; ++i)
        {
            if (i == deletedOne)
                continue;
            GameObject newObj = (GameObject)Instantiate(arrowPrefab, new Vector3((i * arrowWidth) + xOffset, y, z), Quaternion.Euler(0, 0, 270));
            newObj.GetComponent<DickSpecialArrow>().SetVars(arrowFallSpeed, enemy, arrowDamage);
            arrows.Add(newObj);
                        
        }

        //todo: create shadows
    }

    public void Cleanup()
    {
        for (int i = 0; i < arrows.Count; ++i)
        {
            Destroy(arrows[i]);
        }
        //Destroy();
    }
}
