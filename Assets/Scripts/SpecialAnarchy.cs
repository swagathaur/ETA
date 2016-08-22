using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpecialAnarchy : SpecialBase
{
    GameObject UI;
    private GameObject APrefab;
    private GameObject BPrefab;
    private GameObject XPrefab;
    private GameObject YPrefab;
    //GameObject missPrefab;
    //GameObject hitPrefab;

    List<GameObject> arrows;
    private GameObject user; //person who is using the special
    private GameObject taker; //person who is taking the special

    public float yClamp;

    public float arrowSpeed = 4;
    private float attackTimer = 10;

    private bool running;

    enum AnarchySpecialPhase
    {
        startPhase,
        stepmaniaPhase,
        endPhase
    }
    private AnarchySpecialPhase currentPhase;
    private float currentPhaseTime;
    private int startPhaseLength = 1;
    int numberOfAttacks = 30;
    private int endPhaseLength = 1;

    public float cooldownLength = 0.16f;
    private float currentCooldown = 0;
    public float damage;

    public void Start()
    {
        running = false;
        APrefab = Resources.Load("../Prefabs/AnarchySpecialArrows/A.prefab") as GameObject;
        BPrefab = Resources.Load("../Prefabs/AnarchySpecialArrows/B.prefab") as GameObject;
        XPrefab = Resources.Load("../Prefabs/AnarchySpecialArrows/X.prefab") as GameObject;
        YPrefab = Resources.Load("../Prefabs/AnarchySpecialArrows/Y.prefab") as GameObject;
    }

    public void Update()
    {
        if (running)
        {
            //manage timers
            currentPhaseTime -= Time.deltaTime;
            currentCooldown -= Time.deltaTime;

            //running code
            switch (currentPhase)
            {
                //starting the special
                case AnarchySpecialPhase.startPhase:
                    //todo: play start animations
                    //todo: move characters to a nice place
                    //todo: change camera
                    //todo: art stuff
                    break;
                //running the special
                case AnarchySpecialPhase.stepmaniaPhase:
                    //no more attacks? End it
                    if (numberOfAttacks <= 0 && arrows.Count == 0)
                    {
                        currentPhase = AnarchySpecialPhase.endPhase;
                        currentPhaseTime = endPhaseLength;
                    }

                    if (currentCooldown <= 0)
                    {
                        PlayerControls pc = user.GetComponent<PlayerControls>();
                        if (pc.ButtonPressed(PlayerControls.GamepadButtons.A))
                        {
                            //create a new object from prefab, at the right coords (y especially)
                            
                            //add it to the list
                        }
                        else if (pc.ButtonPressed(PlayerControls.GamepadButtons.B))
                        {

                        }
                        else if (pc.ButtonPressed(PlayerControls.GamepadButtons.X))
                        {

                        }
                        else if (pc.ButtonPressed(PlayerControls.GamepadButtons.Y))
                        {

                        }
                    }

                    //update arrows
                    foreach(GameObject a in arrows)
                    { 
                        
                    }

                    break;
                //returning to normal gameplay
                case AnarchySpecialPhase.endPhase:
                    break;
            }

            //transitioning code
            if (currentPhaseTime <= 0)
            {
                switch (currentPhase)
                {
                    case AnarchySpecialPhase.startPhase:
                        currentPhase = AnarchySpecialPhase.stepmaniaPhase;
                        currentPhaseTime = float.MaxValue;
                        break;
                    case AnarchySpecialPhase.endPhase:
                        taker.GetComponent<PlayerControls>().isSuspended = false;
                        user.GetComponent<PlayerControls>().isSuspended = false;
                        break;
                }
            }

        }
    }

    public override void RunAttack(PlayerControls otherPlayer)
    {
        //todo: do some checks (are we already in a special?)
        running = true;
        currentPhase = AnarchySpecialPhase.startPhase;
        currentPhaseTime = startPhaseLength;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            //assign taker and user to the right variables
            if (otherPlayer == p)
                taker = p;
            else
                user = p;
            //also suspend the players, dont go anywhere
            p.GetComponent<PlayerControls>().isSuspended = true;
        }
    }
}
