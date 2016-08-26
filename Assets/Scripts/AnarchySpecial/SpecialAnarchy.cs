using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XInputDotNetPure;

public class SpecialAnarchy : SpecialBase
{
    //todo:
    //GameObject missPrefab;
    //GameObject hitPrefab;

    private GameObject UI;
    private GameObject APrefab;
    private GameObject BPrefab;
    private GameObject XPrefab;
    private GameObject YPrefab;

    private List<GameObject> arrows;
    private GameObject attacker; //person who is using the special
    private GameObject defender; //person who is taking the special

    [SerializeField] private float arrowSpeed = 4;
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
    [SerializeField] private int numberOfAttacks = 30;
    private int attacksLeft = 30;
    private int endPhaseLength = 1;

    [SerializeField]private float cooldownLength = 0.16f;
    private float currentCooldown = 0;
    [SerializeField]private float damage;

    private Vector3 spawnPosA;   //the position where the arrows should spawn
    private Vector3 spawnPosB;
    private Vector3 spawnPosX;
    private Vector3 spawnPosY;
    private Vector3 offset;     //the position of the defending player as stepmania phase starts

    public enum TriggerButtons
    {
        A = 0,
        B = 1,
        X = 2,
        Y = 3
    }
    private GameObject[] triggers;
    [SerializeField]private GameObject triggerPrefab;
    private float boxColliderHeight;
    private int direction = 0;

    private bool aActive;
    private bool bActive;
    private bool xActive;
    private bool yActive;

    float distanceApart = 5;

    public void Start()
    {
        running = false;

        APrefab = (GameObject)Resources.Load("Prefabs/AnarchySpecialArrows/A", typeof(GameObject));
        BPrefab = (GameObject)Resources.Load("Prefabs/AnarchySpecialArrows/B", typeof(GameObject));
        XPrefab = (GameObject)Resources.Load("Prefabs/AnarchySpecialArrows/X", typeof(GameObject));
        YPrefab = (GameObject)Resources.Load("Prefabs/AnarchySpecialArrows/Y", typeof(GameObject));

        boxColliderHeight = triggerPrefab.GetComponent<BoxCollider>().size.y;
        arrows = new List<GameObject>();
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
                //main phase of the special
                case AnarchySpecialPhase.stepmaniaPhase:
                    //no more attacks? End it
                    if (attacksLeft <= 0 && arrows.Count == 0)
                    {
                        currentPhase = AnarchySpecialPhase.endPhase;
                        currentPhaseTime = endPhaseLength;
                    }

                    #region attacking player
                    if (currentCooldown <= 0 && attacksLeft >= 0)
                    {
                        PlayerControls pca = attacker.GetComponent<PlayerControls>();
                        if (pca.ButtonPressed(PlayerControls.GamepadButtons.A))
                        {
                            //create a new object from prefab, at the right coords (y especially)
                            //add it to the list
                            arrows.Add((GameObject)Instantiate(APrefab, spawnPosA, Quaternion.identity));
                            --attacksLeft;
                            currentCooldown = cooldownLength;
                        }
                        else if (pca.ButtonPressed(PlayerControls.GamepadButtons.B))
                        {
                            arrows.Add((GameObject)Instantiate(BPrefab, spawnPosB, Quaternion.identity));
                            --attacksLeft;
                            currentCooldown = cooldownLength;
                        }
                        else if (pca.ButtonPressed(PlayerControls.GamepadButtons.X))
                        {
                            arrows.Add((GameObject)Instantiate(XPrefab, spawnPosX, Quaternion.identity));
                            --attacksLeft;
                            currentCooldown = cooldownLength;
                        }
                        else if (pca.ButtonPressed(PlayerControls.GamepadButtons.Y))
                        {
                            arrows.Add((GameObject)Instantiate(YPrefab, spawnPosY, Quaternion.identity));
                            --attacksLeft;
                            currentCooldown = cooldownLength;
                        }
                    }
                    #endregion

                    #region defending player
                    ResetTriggerSprites();
                    PlayerControls pcd = defender.GetComponent<PlayerControls>();
                    if (pcd.ButtonPressed(PlayerControls.GamepadButtons.A))
                    {
                        GameObject trigger = triggers[(int)TriggerButtons.A];
                        trigger.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 1);
                        aActive = true;
                    }
                    if (pcd.ButtonPressed(PlayerControls.GamepadButtons.B))
                    {
                        GameObject trigger = triggers[(int)TriggerButtons.B];
                        trigger.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 1);
                        bActive = true;
                    }
                    if (pcd.ButtonPressed(PlayerControls.GamepadButtons.X))
                    {
                        GameObject trigger = triggers[(int)TriggerButtons.X];
                        trigger.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 1);
                        xActive = true;
                    }
                    if (pcd.ButtonPressed(PlayerControls.GamepadButtons.Y))
                    {
                        GameObject trigger = triggers[(int)TriggerButtons.Y];
                        trigger.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 1);
                        yActive = true;
                    }
                    #endregion

                    #region update arrows
                    //reverse iteration to avoid iterator invalidation
                    for (int i = arrows.Count - 1;  i >= 0; --i)
                    {
                        GameObject arrow = arrows[i];
                        arrow.transform.position = new Vector3(arrow.transform.position.x + arrowSpeed * direction * Time.deltaTime, 
                            arrow.transform.position.y, arrow.transform.position.z);
                        
                        //arrow is inside the box
                        if (arrow.GetComponent<AnarchySpecialArrow>().collided)
                        {
                            switch(arrow.GetComponent<AnarchySpecialArrow>().button)
                            {
                                case TriggerButtons.A:
                                    if (aActive)
                                    {
                                        //todo: play some animation to show you blocked it
                                        arrows.RemoveAt(i);
                                        DestroyImmediate(arrow);
                                    }
                                    break;
                                case TriggerButtons.B:
                                    if (bActive)
                                    {
                                        //todo: play some animation to show you blocked it
                                        arrows.RemoveAt(i);
                                        DestroyImmediate(arrow);
                                    }
                                    break;
                                case TriggerButtons.X:
                                    if (xActive)
                                    {
                                        //todo: play some animation to show you blocked it
                                        arrows.RemoveAt(i);
                                        DestroyImmediate(arrow);
                                    }
                                    break;
                                case TriggerButtons.Y:
                                    if (yActive)
                                    {
                                        //todo: play some animation to show you blocked it
                                        arrows.RemoveAt(i);
                                        DestroyImmediate(arrow);
                                    }
                                    break;
                            }
                        }
                        //Arrow has left the box!
                        if (arrow.GetComponent<AnarchySpecialArrow>().missed)
                        {
                            defender.GetComponent<PlayerControls>().health -= (short)damage;
                            arrows.RemoveAt(i);
                            DestroyImmediate(arrow);
                            continue;
                        }
                    }
                    #endregion
                    break;
                //returning to normal gameplay
                case AnarchySpecialPhase.endPhase:
                    //todo: some artsy shit here
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
                        //set direction
                        Vector3 dir = defender.transform.position - attacker.transform.position;
                        dir.Normalize();
                        direction = dir.x < 0 ? -1 : 1;
                        //save offset
                        offset = new Vector3(defender.transform.position.x, 
                            defender.transform.position.y + (defender.GetComponent<BoxCollider>().size.y * 0.5f),
                            defender.transform.position.z);

                        //set spawn positions
                        spawnPosA = new Vector3(offset.x - direction * distanceApart, offset.y, offset.z);
                        spawnPosB = new Vector3(offset.x - direction * distanceApart, offset.y + boxColliderHeight, offset.z);
                        spawnPosX = new Vector3(offset.x - direction * distanceApart, offset.y + boxColliderHeight * 2, offset.z);
                        spawnPosY = new Vector3(offset.x - direction * distanceApart, offset.y + boxColliderHeight * 3, offset.z);

                        //set up trigger hitboxes
                        triggers = new GameObject[] {
                            (GameObject)Instantiate(triggerPrefab, offset, Quaternion.identity),                                                                            //a
                            (GameObject)Instantiate(triggerPrefab, new Vector3(offset.x, offset.y + boxColliderHeight, offset.z), Quaternion.identity) as GameObject,       //b
                            (GameObject)Instantiate(triggerPrefab, new Vector3(offset.x, offset.y + boxColliderHeight * 2, offset.z), Quaternion.identity) as GameObject,   //x
                            (GameObject)Instantiate(triggerPrefab, new Vector3(offset.x, offset.y + boxColliderHeight * 3, offset.z), Quaternion.identity) as GameObject    //y
                        }; 
                        break;
                    //ended
                    case AnarchySpecialPhase.endPhase:
                        //allow the players to move again
                        defender.GetComponent<PlayerControls>().isSuspended = false;
                        attacker.GetComponent<PlayerControls>().isSuspended = false;

                        //clean up the trigger boxes
                        for (int i=0; i < triggers.Length; ++i)
                        {
                            Destroy(triggers[i]);
                        }
                        break;
                }
            }
            
            //reset the active hitboxes
            aActive = false;
            bActive = false;
            xActive = false;
            yActive = false;
        }
    }

    void ResetTriggerSprites()
    {
        for (int i = 0; i < 4; ++i)
        {
            triggers[i].GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
        }
    }

    public override void RunAttack(PlayerIndex playerIndex)
    {
        //todo: do some checks (are we already in a special?)
        running = true;
        currentPhase = AnarchySpecialPhase.startPhase;
        currentPhaseTime = startPhaseLength;

        attacksLeft = numberOfAttacks;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            //assign taker and user to the right variables
            if (playerIndex != p.GetComponent<PlayerControls>().playerIndex)
            {
                attacker = p;
            }
            else
            {
                defender = p;
            }
            //also suspend the players, dont go anywhere
            p.GetComponent<PlayerControls>().isSuspended = true;
            p.GetComponent<PlayerControls>().Freeze();
        }
    }
}