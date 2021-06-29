using UnityEngine;
using System.Collections.Generic;
using System;
using XInputDotNetPure;

public class DickSpecial : SpecialBase
{
    [SerializeField]
    private GameObject arrowPrefab;
    [SerializeField]
    private GameObject shadowPrefab;

    private GameObject enemy;
    private List<GameObject> arrows;

    [SerializeField]
    private float arrowFallSpeed;
    [SerializeField]
    private short arrowDamage;

    [HideInInspector]
    public bool enabled;

    private int numHoles = 5;

    [HideInInspector]
    public bool hit = false;

    // Use this for initialization

    void Start()
    {
        arrows = new List<GameObject>();
        enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!enabled)
        {
            Cleanup();
        }
    }

    public override void RunAttack(PlayerIndex playerIndex)
    {
        enabled = true;
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
        float amountAboveFloor = 20;

        float wallDiff = Math.Abs(walls[0].transform.position.x - walls[1].transform.position.x);
        float arrowWidth = arrowPrefab.GetComponent<BoxCollider>().size.x;
        float numArrows = wallDiff / arrowWidth;

        if (numArrows < 8)
            numHoles = 3;

        List<int> whereAreHoles = new List<int>(numHoles);
        for (int i = 0; i < numHoles; ++i)
            whereAreHoles.Add((int)UnityEngine.Random.Range(2, numArrows - 2));

        //xOffset: the offset to spawn the left-most arrow at
        float xOffset = leftWall.transform.position.x;

        //y: should be certain distance above the floor
        float shadowY = GameObject.FindGameObjectWithTag("Terrain").gameObject.transform.position.y;
        float y = shadowY + amountAboveFloor;
        shadowY += .3f; //magic numbers af
        //z: should be from the player
        float z = enemy.transform.position.z;

        //todo: put the timer in this script, make the timer based on the time it takes to touch the ground
        //todo: more gaps

        //x: should be based on distance between both walls and width of arrows
        for (int i = 0; i < numArrows; ++i)
        {
            if (whereAreHoles.Contains(i))
                continue;
            GameObject newObj = (GameObject)Instantiate(arrowPrefab, new Vector3((i * arrowWidth) + xOffset, y, z), Quaternion.Euler(0, 0, 270));
            GameObject shadowObj = (GameObject)Instantiate(shadowPrefab, new Vector3((i * arrowWidth + xOffset), shadowY, z), Quaternion.Euler(70, 0, 0));

            newObj.GetComponent<DickSpecialArrow>().SetVars(arrowFallSpeed, enemy, arrowDamage, this);
            arrows.Add(newObj);
            arrows.Add(shadowObj);
        }
    }

    public void Cleanup()
    {
        for (int i = 0; i < arrows.Count; ++i)
        {
            Destroy(arrows[i]);
        }
        hit = false;
        arrows.Clear();
        GameObject dummy = FindObjectOfType<DickDummyArrowMovementDummy>().gameObject;
        dummy.GetComponent<DickDummyArrowMovementDummy>().Cleanup();
        Destroy(dummy);
    }
}