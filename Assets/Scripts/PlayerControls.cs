using UnityEngine;
using System.Collections;
using XInputDotNetPure;
using System;

public class PlayerControls : MonoBehaviour
{

    #region VARS 
    public PlayerIndex playerIndex;

    public GameObject heavyArrow;
    public GameObject arrow;

    [HideInInspector]
    public SpecialBase specialAttackScript;
    [HideInInspector]
    public GameObject enemy;
    public GameObject arrowSpawner;

    [HideInInspector]
    public GameObject trailRenderer;

    private GameObject dustCloudEmitter;

    public short health = 100;
    public short special = 0;
    public short maxSpecial = 100;
    public short arrowSpeed = 15; // 15 seems reasonable

    private int arrowNumber = 0; //Used to track whether the same arrowgroup can collide
    private int lastArrowHitBy = -1; //tracks which arrow you were last hit by (can't be hit by two arrows with the same ID)

    public float timeToCounter = 0.15f;
    public float counterTimer;

    public float airControl = 8;
    public float playerSize = 1;
    public float gravPower = 1;
    public float speedLimit = 15; // player left right walk speed
    public float jumpForce = 400;
    public float friction = 12;
    public float pushStrength = 5;
    public float perfectCounterTimer = 0.34f;

    public float arrowHitTime = 0.1f; // IN SECONDS

    public int arrowDamage = 5;
    public int heavyArrowDamage = 10;

    //Animation Lengths
    public float tauntAnimLength = 0.5f;
    public float jumpAnimLength = 0.5f;

    float colourTimer;
    float stepTimer;
    float justJumped = 0;

    Vector2 savedThumbState;
    Vector2 savedTriggerState;

    bool savedHeavyAttack;

    Vector2 counterDir;

    AudioScript audioSource;

    GamePadState controllerState;
    GamePadState prevControllerState;
    Animator animator;

    public enum animationState
    {
        STATE_START = 0,
        STATE_IDLE = 1,
        STATE_WALK = 2,
        STATE_RUN = 3,
        STATE_TAUNT = 4,
        STATE_JUMP = 5,
        STATE_FALL = 6,
        STATE_HIT = 7,
        STATE_DEATH = 8,
        STATE_ATTACK_DOWN = 9,
        STATE_ATTACK_UP = 10,
        STATE_ATTACK_SIDE = 11,
        STATE_COUNTER = 12,
        STATE_WIN = 13,
    }

    public float attackTimer;
    private bool startAttack = false;
    private bool nextAttackIsSpecial = false;
    private bool isAttacking = false; //is player attacking
    bool hasSpawnedArrow = false;
    bool isGrounded = false; // is player on the ground
    bool isTaunting = false; // is player Taunting
    bool isWalking = false; // is player Walking or Running

    public animationState currentAnimationState = animationState.STATE_START;
    public float currentAnimationTime = 0;


    //magic number to stop you snapping back up into a platform
    //todo: make this platform dependant, reset on actions rather than time
    private float tapFallTimer = 0;
    private float maxTapFallTime = 0.25f;

    public bool turning = false;
    //[HideInInspector]
    public bool isSuspended; //a suspended player still most things except input. Also, start suspended
    #endregion

    // Use this for initialization
    void Start()
    {
        isSuspended = true;

        //define the animator attached to the player
        animator = this.GetComponent<Animator>();

        //find audioScript
        audioSource = FindObjectOfType<AudioScript>();

        //set up the dustCloudEmitter
        dustCloudEmitter = GameObject.Find("player" + ((int)playerIndex + 1) + "DustCloudEmitter");
        dustCloudEmitter.GetComponent<ParticleSystem>().emissionRate = 0;

        //find the enemy
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players[0] == gameObject)
            enemy = players[1];
        else
            enemy = players[0];

        //special attack
        specialAttackScript = GetComponent<SpecialBase>();

        //trail renderer
        trailRenderer = gameObject.GetComponentInChildren<TrailRenderer>().gameObject;

        //player 2 should start flipped
        if (playerIndex == PlayerIndex.Two)
        {
            ChangeDirection(1);
        }
    }

    //Update plz
    void Update()
    {
        //if we're suspended (beginning/end of match, during supers?)
        if (isSuspended)
        {
            //apply g
            ExtraGravity();
            if (health <= 0)
            {
                //todo: play death animation
                return;
            }
            else if (currentAnimationState == animationState.STATE_START)
            {
                //todo: play start animation
                //set animation time, so the next else if doesn't run on frame 2
            }
            //wait till animation is over then play idle
            else if (currentAnimationState != animationState.STATE_IDLE)
            {
                if (currentAnimationTime <= 0)
                    ChangeState(animationState.STATE_IDLE);
            }
        }
        //normal update logic
        else
        {
            currentAnimationTime = (1 - GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime);
            tapFallTimer -= Time.deltaTime;

            //reset to idle if not moving fast
            if (IsCurrentAnimationStateCancellable() &&
                Math.Abs(GetComponent<Rigidbody>().velocity.x) < 0.25f)
            {
                ChangeState(animationState.STATE_IDLE);
            }
            //reset to idle or fall
            if (currentAnimationTime <= 0
                && !((currentAnimationState == animationState.STATE_IDLE)
                  || (currentAnimationState == animationState.STATE_FALL)
                  || (currentAnimationState == animationState.STATE_WALK)
                  || (currentAnimationState == animationState.STATE_RUN)))
            {
                if (!isAttacking)
                {
                    if (isGrounded)
                        ChangeState(animationState.STATE_IDLE);
                    else
                        ChangeState(animationState.STATE_FALL);
                }
            }

            //UPDATE GAMEPAD
            prevControllerState = controllerState;
            controllerState = GamePad.GetState(playerIndex);

            if (IsCurrentAnimationStateCancellable())
            {
                CheckInput();
                ChangeDirection();
            }

            GetCounterState();
            CheckTrail();
            CheckFriction();
            CheckFootsteps();
            ExtraGravity();

            if (isAttacking || startAttack)
            {
                Attack();
            }
        }
    }

    private void CheckFootsteps()
    {
        if (currentAnimationState == animationState.STATE_WALK || currentAnimationState == animationState.STATE_RUN)
        {
            if (stepTimer > GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length * 0.5f)
            {
                stepTimer = 0;
                audioSource.playSound(baseAudio.CLIP_FOOTSTEP);
            }

            stepTimer += Time.deltaTime;
        }
        else
            stepTimer = 0;
    }

    private void CheckFriction()
    {
        Vector3 newVelocity = GetComponent<Rigidbody>().velocity;
        const float runningVelocityDecrease = 3;
        const float turnSpeedBurst = 10;

        if (isGrounded)
        {
            if (!isWalking)
            {
                if (isAttacking)
                {
                    newVelocity.x -= newVelocity.x * friction * 0.3f * Time.deltaTime;
                }
                else newVelocity.x -= newVelocity.x * friction * Time.deltaTime;
            }
            else if (isWalking)
            {
                //current velocity so far
                //todo: running speed
                if ((newVelocity.x > 0) && (controllerState.ThumbSticks.Left.X < -0.4f))
                {
                    newVelocity.x -= newVelocity.x * (friction / runningVelocityDecrease) * Time.deltaTime;
                    turning = true;
                }
                else if ((newVelocity.x < 0) && (controllerState.ThumbSticks.Left.X > 0.4f))
                {
                    newVelocity.x -= newVelocity.x * (friction / runningVelocityDecrease) * Time.deltaTime;
                    turning = true;
                }
            }
            //we're turning here
            if (turning && Mathf.Abs(newVelocity.x) < 0.2f)
            {
                turning = false;
                DustCloud(Quaternion.Inverse(transform.rotation));

                if (controllerState.ThumbSticks.Left.X < -0.4f)
                {
                    newVelocity.x -= turnSpeedBurst;
                }
                else if (controllerState.ThumbSticks.Left.X > 0.4f)
                {
                    newVelocity.x += turnSpeedBurst;
                }
            }

            //if (newVelocity.x < 2)
            //    newVelocity.x = 0;

        }
        newVelocity.x = Mathf.Clamp(newVelocity.x, -speedLimit, speedLimit);
        GetComponent<Rigidbody>().velocity = newVelocity;
    }

    //creates a dust cloud rotated at the right rotation
    private void DustCloud(Quaternion rotation)
    {
        audioSource.playSound(baseAudio.CLIP_DASH);

        dustCloudEmitter.transform.position = transform.position;
        dustCloudEmitter.transform.position.Set(transform.position.x, transform.position.y + 50, transform.position.z);
        dustCloudEmitter.transform.rotation = rotation;
        dustCloudEmitter.GetComponent<ParticleSystem>().Emit(10);
    }
    private void ExtraGravity()
    {
        if (!isGrounded)
            GetComponent<Rigidbody>().AddForce(0, -1 * gravPower * Time.deltaTime, 0);
    }

    public void GetCounterState()
    {
        if (counterTimer == 0)
        {
            //Make sure a coutner is started this frame
            if (Mathf.Abs(controllerState.ThumbSticks.Right.X) < 0.3f
                && Mathf.Abs(controllerState.ThumbSticks.Right.Y) < 0.3f)
            {
                return;
            }
            if (Mathf.Abs(prevControllerState.ThumbSticks.Right.X) >= 0.3f
                || Mathf.Abs(prevControllerState.ThumbSticks.Right.Y) >= 0.3f)
            {
                return;
            }

            //if it is start the animation and set a counter direction based on the joysticks position
            ChangeState(animationState.STATE_COUNTER);
            
            counterDir.x = controllerState.ThumbSticks.Right.X;
            counterDir.y = controllerState.ThumbSticks.Right.Y;
            counterDir.Normalize();
            Debug.Log(counterDir);

            counterTimer = timeToCounter;
        }
        else
        {
            counterTimer -= Time.deltaTime;

            if (counterTimer < 0)
                counterTimer = 0;
        }
    }

    public void DidCounter(bool counterSuccess, bool isHeavy, int arrowID, int damage)
    {
        if (arrowID == lastArrowHitBy)
            return;
        if (counterSuccess)
        {
            special += 5;
        }
        else
        {
            health -= (short)damage;
            audioSource.playSound(playerAudio.CLIP_HIT, playerIndex);
            //wot
            //isAttacking = false;
            //isWalking = false;
        }
        lastArrowHitBy = arrowID;
        colourTimer = 0.5f;
    }
    public float CheckCounter(simpleMove incomingArrow)
    {
        if (incomingArrow.SpecialScript != null)
        {
            incomingArrow.SpecialScript.RunAttack(playerIndex);
            return currentAnimationState == animationState.STATE_COUNTER ? 1 : 0.1f;
        }

        bool heavyCounter = (controllerState.Buttons.RightShoulder == ButtonState.Pressed);
        //return a fail if not countering
        if (counterTimer == 0)
            return 0;

        //CHECK IF MATCHING
        if (Vector2.Dot(counterDir, -incomingArrow.transform.right) > 0.0f) //NEEDS FIXING
        {
            if (incomingArrow.SpecialScript == null)
            {
                if (heavyCounter == incomingArrow.heavy)
                    return currentAnimationState == animationState.STATE_COUNTER ? 1 : 0.1f;
                else
                {
                    return 0;
                }
            }
            //moved to the start of the function
            /*else
            {
                incomingArrow.SpecialScript.RunAttack(this);
                return currentAnimationState == animationState.STATE_COUNTER ? 1 : 0.1f;
            }*/
        }
        return 0;
    }

    void ChangeState(animationState newState)
    {
        if (currentAnimationState == newState)
            return;
        if (attackTimer > 0)
            return;

        if (newState == animationState.STATE_IDLE)
        {
            GetComponent<Animator>().ResetTrigger("RUN");
            GetComponent<Animator>().ResetTrigger("WALK");
        }

        currentAnimationState = newState;
        PlayAnimationState();
        currentAnimationTime = (1 - GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime);
    }
    void PlayAnimationState()
    {
        switch (currentAnimationState)
        {
            case animationState.STATE_IDLE:
                animator.SetTrigger("IDLE");
                break;
            case animationState.STATE_WALK:
                animator.SetTrigger("WALK");
                break;
            case animationState.STATE_RUN:
                animator.SetTrigger("RUN");
                break;
            case animationState.STATE_TAUNT:
                animator.SetTrigger("TAUNT");
                break;
            case animationState.STATE_JUMP:
                animator.SetTrigger("JUMP");
                break;
            case animationState.STATE_FALL:
                animator.SetTrigger("FALL");
                break;
            case animationState.STATE_HIT:
                animator.SetTrigger("HIT");
                break;
            case animationState.STATE_DEATH:
                animator.SetTrigger("DEATH");
                break;
            case animationState.STATE_ATTACK_DOWN:
                animator.SetTrigger("ATTACK_DOWN");
                break;
            case animationState.STATE_ATTACK_UP:
                animator.SetTrigger("ATTACK_UP");
                break;
            case animationState.STATE_ATTACK_SIDE:
                animator.SetTrigger("ATTACK_SIDE");
                break;
            case animationState.STATE_COUNTER:
                animator.SetTrigger("COUNTER");
                break;
            case animationState.STATE_WIN:
                animator.SetTrigger("WIN");
                break;
        }
    }
    private bool IsCurrentAnimationStateCancellable()
    {
        return (currentAnimationState == animationState.STATE_IDLE)
            || (currentAnimationState == animationState.STATE_RUN)
            || (currentAnimationState == animationState.STATE_WALK)
            || (currentAnimationState == animationState.STATE_JUMP)
            || (currentAnimationState == animationState.STATE_FALL)
            || (currentAnimationState == animationState.STATE_START);
    }

    void OnTriggerExit(Collider coll) 
    {
        isGrounded = false;
    } 
    void OnTriggerStay(Collider coll)
    {
        if (coll.tag == "Platform")
        {
            //standing on a platform
            if (controllerState.ThumbSticks.Left.Y > -0.95f
                 && GetComponent<Rigidbody>().velocity.y < 1
                 && (transform.position.y + GetComponent<BoxCollider>().center.y - (GetComponent<BoxCollider>().size.y * 0.5f))
                 > coll.transform.position.y + coll.GetComponent<BoxCollider>().center.y + (coll.GetComponent<BoxCollider>().size.y * 0.43f))
            {
                if (!isGrounded
                    && tapFallTimer <= 0)
                {
                    GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 0, 0);
                    //Snap to top of platform
                    Vector3 temp = transform.position;
                    temp.y = (GetComponent<BoxCollider>().size.y * 0.7f) - GetComponent<BoxCollider>().center.y + coll.transform.position.y + coll.GetComponent<BoxCollider>().center.y + (coll.GetComponent<BoxCollider>().size.y * 0.45f) - (GetComponent<BoxCollider>().center.y - coll.GetComponent<BoxCollider>().size.y * 0.5f);
                    transform.position = temp;

                    isGrounded = true;
                    GetComponent<Animator>().ResetTrigger("JUMP");
                    ChangeState(animationState.STATE_IDLE);
                }
            }
            //tapping through
            else
            {
                if (isGrounded && (controllerState.ThumbSticks.Left.Y < -0.95f))
                {
                    isGrounded = false;
                    GetComponent<Rigidbody>().AddForce(Vector3.down * speedLimit * 0.25f);

                    //start a timer to not snap back up
                    tapFallTimer = maxTapFallTime;
                }
            }
        }
        if (coll.tag == "Terrain")
        {
            if (!isGrounded)
            {
                GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 0, 0);
                //Snap to top of platform
                Vector3 temp = transform.position;
                temp.y = (GetComponent<BoxCollider>().size.y * 0.7f) - GetComponent<BoxCollider>().center.y + coll.transform.position.y + coll.GetComponent<BoxCollider>().center.y + (coll.GetComponent<BoxCollider>().size.y * 0.45f) - (GetComponent<BoxCollider>().center.y - coll.GetComponent<BoxCollider>().size.y * 0.5f);
                transform.position = temp;

                isGrounded = true;
                GetComponent<Animator>().ResetTrigger("JUMP");
                ChangeState(animationState.STATE_IDLE);
                DustCloud(Quaternion.Euler(-transform.up));
            }
        }
    }
    void OnTriggerEnter(Collider coll)
    {
        if (coll.tag == "Platform")
        {
            if (GetComponent<Rigidbody>().velocity.y >= 0)
                return;

            RaycastHit hit;
            Debug.DrawRay(transform.position + Vector3.up * 50, Vector3.down);

            if (Physics.Raycast(transform.position + Vector3.up * 50, Vector3.down, out hit, 200, LayerMask.NameToLayer("Platform")))
            {
                if ((transform.position - hit.point).magnitude < 2)
                {
                    if (!isGrounded && controllerState.ThumbSticks.Left.Y > -0.95f)
                    {
                        //zero velocity
                        GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 0, 0);

                        //Snap to top of platform
                        Vector3 temp = transform.position;
                        temp.y = coll.transform.position.y + coll.GetComponent<BoxCollider>().center.y + (coll.GetComponent<BoxCollider>().size.y * 0.45f);
                        transform.position = temp;

                        //set all variables to grounded state
                        isGrounded = true;
                        GetComponent<Animator>().ResetTrigger("JUMP");
                        ChangeState(animationState.STATE_IDLE);
                        DustCloud(Quaternion.Euler(-transform.up));
                    }
                }
            }

            //if you have landed from above


            /*
            if (GetComponent<Rigidbody>().velocity.y < 0
                && (transform.position.y + GetComponent<BoxCollider>().center.y - (GetComponent<BoxCollider>().size.y * 0.5f)) 
                 > coll.transform.position.y + coll.GetComponent<BoxCollider>().center.y)
            {
                if (!isGrounded && controllerState.ThumbSticks.Left.Y > -0.95f)
                {
                    GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 0, 0);
                    //Snap to top of platform
                    Vector3 temp = transform.position;
                    temp.y = (GetComponent<BoxCollider>().size.y * 0.7f) - GetComponent<BoxCollider>().center.y + coll.transform.position.y + coll.GetComponent<BoxCollider>().center.y + (coll.GetComponent<BoxCollider>().size.y * 0.45f) - (GetComponent<BoxCollider>().center.y - coll.GetComponent<BoxCollider>().size.y * 0.5f);
                    transform.position = temp;

                    isGrounded = true;
                    GetComponent<Animator>().ResetTrigger("JUMP");
                    changeState(animationState.STATE_IDLE);
                    DustCloud(Quaternion.Euler(-transform.up));
                }
            }
            */
        }
        if (coll.tag == "Terrain")
        {
            if (!isGrounded)
            {
                GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 0, 0);
                //Snap to top of platform
                Vector3 temp = transform.position;
                temp.y = coll.transform.position.y + coll.GetComponent<BoxCollider>().center.y + (coll.GetComponent<BoxCollider>().size.y * 0.45f);
                transform.position = temp;

                isGrounded = true;
                GetComponent<Animator>().ResetTrigger("JUMP");
                ChangeState(animationState.STATE_IDLE);
                DustCloud(Quaternion.Euler(-transform.up));
            }
        }
    }

    /*if directionoverride = 0, changes based on input ***Default***
      if directionoverride = 1, forces you left
      if directionoverride = 2, forces you right*/
    void ChangeDirection(int directionOverride = 0)
    {
        if (isAttacking)
            return;

        if (controllerState.ThumbSticks.Left.X > 0.375f || directionOverride == 2)
        {
            transform.localEulerAngles = new Vector3(0, 0, 0);
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
        }
        if (controllerState.ThumbSticks.Left.X < -0.375f || directionOverride == 1)
        {
            transform.localEulerAngles = new Vector3(0, 180, 0);
            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
        }

        if ((enemy.transform.position - transform.position).magnitude < 1)
        {
            GetComponent<Rigidbody>().AddForce((transform.position - enemy.transform.position).normalized.x * speedLimit * Time.deltaTime * pushStrength, 0, 0);
        }
    }

    void CheckInput()
    {
        if (controllerState.IsConnected == false)
            return;

        //check all animation
        isWalking = false;

        if (justJumped > 0 && controllerState.Buttons.A == ButtonState.Pressed)
        {
            justJumped -= Time.deltaTime;
            GetComponent<Rigidbody>().AddForce(new Vector2(0, jumpForce * 2) * Time.deltaTime);
        }

        //Attack
        if ((prevControllerState.Buttons.X == ButtonState.Released && controllerState.Buttons.X == ButtonState.Pressed)
            || (prevControllerState.Buttons.B == ButtonState.Released && controllerState.Buttons.B == ButtonState.Pressed))
        {
            if (!isAttacking && attackTimer <= 0)
            {
                startAttack = true;
            }
        }
        if (prevControllerState.Buttons.Y == ButtonState.Released && controllerState.Buttons.Y == ButtonState.Pressed
            && special >= 100)
        {
            if (!isAttacking && attackTimer <= 0)
            {
                startAttack = true;
                nextAttackIsSpecial = true;
                special = 0;
            }
        }

        //Jump
        else if ((prevControllerState.Buttons.A == ButtonState.Released && controllerState.Buttons.A == ButtonState.Pressed)
            && IsCurrentAnimationStateCancellable())
        {
            if (isGrounded)
            {
                // move from ground and jump
                isGrounded = false;
                justJumped = 0.3f;
                audioSource.playSound(playerAudio.CLIP_JUMP, playerIndex);
                GetComponent<Rigidbody>().AddForce(new Vector2(0, jumpForce * 0.6f));
                //animate
                ChangeState(animationState.STATE_JUMP);
            }
        }

        #region check Walking or Running
        //Run Right
        else if (controllerState.ThumbSticks.Left.X > 0.8f)
        {
            if (!isAttacking)
            {
                if (isGrounded)
                {
                    GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x + (speedLimit * Time.deltaTime), 0, 0);
                    ChangeState(animationState.STATE_RUN);
                    isWalking = true;
                }
                else GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x + (speedLimit * airControl * Time.deltaTime), GetComponent<Rigidbody>().velocity.y, 0);
            }
        }
        //Run Left
        else if (controllerState.ThumbSticks.Left.X < -0.8f)
        {
            if (!isAttacking)
            {
                if (isGrounded)
                {
                    GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x - (speedLimit * Time.deltaTime), 0, 0);
                    ChangeState(animationState.STATE_RUN);
                    isWalking = true;
                }
                else
                {
                    GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x - (speedLimit * airControl * Time.deltaTime), GetComponent<Rigidbody>().velocity.y, 0);
                }
            }
        }
        //Walk Right
        else if (controllerState.ThumbSticks.Left.X > 0.3f)
        {
            if (!isAttacking)
            {
                if (isGrounded)
                {
                    GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x + (speedLimit * Time.deltaTime), 0, 0);
                    ChangeState(animationState.STATE_RUN);
                    isWalking = true;
                }
                else
                {
                    GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x + (speedLimit * airControl * Time.deltaTime), GetComponent<Rigidbody>().velocity.y, 0);
                }
            }
        }
        //Walk Left
        else if (controllerState.ThumbSticks.Left.X < -0.3f)
        {
            if (!isAttacking)
            {
                if (isGrounded)
                {
                    GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x - (speedLimit * Time.deltaTime), 0, 0);
                    ChangeState(animationState.STATE_RUN);
                    isWalking = true;
                }
                else
                {
                    GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x - (speedLimit * airControl * Time.deltaTime), GetComponent<Rigidbody>().velocity.y, 0);
                }
            }
        }
        #endregion
    }

    public enum GamepadButtons
    {
        A, B, X, Y
    }
    //returns true if button was pressed this frame
    public bool WasButtonPressed(GamepadButtons b)
    {
        ButtonState currButtonState = 0;
        ButtonState prevButtonState = 0;
        switch (b)
        {
            case GamepadButtons.A:
                currButtonState = controllerState.Buttons.A;
                prevButtonState = prevControllerState.Buttons.A;
                break;
            case GamepadButtons.B:
                currButtonState = controllerState.Buttons.B;
                prevButtonState = prevControllerState.Buttons.B;
                break;
            case GamepadButtons.X:
                currButtonState = controllerState.Buttons.X;
                prevButtonState = prevControllerState.Buttons.X;
                break;
            case GamepadButtons.Y:
                currButtonState = controllerState.Buttons.Y;
                prevButtonState = prevControllerState.Buttons.Y;
                break;
        }
        return (currButtonState == ButtonState.Pressed) && (currButtonState == ButtonState.Released);
    }
    //returns true if button is held down
    public bool ButtonPressed(GamepadButtons b)
    {
        switch (b)
        {
            case GamepadButtons.A:
                return controllerState.Buttons.A == ButtonState.Pressed;
            case GamepadButtons.B:
                return controllerState.Buttons.B == ButtonState.Pressed;
            case GamepadButtons.X:
                return controllerState.Buttons.X == ButtonState.Pressed;
            case GamepadButtons.Y:
                return controllerState.Buttons.Y == ButtonState.Pressed;
        }
        return false;
    }

    void Attack()
    {
        //first frame of attacking, set stuff up
        if (startAttack)
        {
            //unity fuck off
            GetComponent<Animator>().ResetTrigger("IDLE");
            GetComponent<Animator>().ResetTrigger("WALK");
            GetComponent<Animator>().ResetTrigger("RUN");
            GetComponent<Animator>().ResetTrigger("JUMP");
            GetComponent<Animator>().ResetTrigger("FALL");

            //set attack direction
            if (controllerState.ThumbSticks.Left.Y > 0.3)
            {
                ChangeState(animationState.STATE_ATTACK_UP);
            }
            else if (controllerState.ThumbSticks.Left.Y < -0.3)
            {
                ChangeState(animationState.STATE_ATTACK_DOWN);
            }
            else
            {
                ChangeState(animationState.STATE_ATTACK_SIDE);
            }

            //play sound
            audioSource.playSound(playerAudio.CLIP_ATTACK, playerIndex);

            //set up stuff
            savedHeavyAttack = (controllerState.Buttons.B == ButtonState.Pressed);
            savedThumbState.x = controllerState.ThumbSticks.Left.X;
            savedThumbState.y = controllerState.ThumbSticks.Left.Y;
            savedTriggerState.x = controllerState.Triggers.Left;
            savedTriggerState.y = controllerState.Triggers.Right;

            //more setup
            attackTimer = 1;
            startAttack = false;
            hasSpawnedArrow = false;
            isAttacking = true;
        }

        if (attackTimer <= 0 && isAttacking)
        {
            isAttacking = false;
            attackTimer = 0;
            return;
        }
        else
        {
            GetComponent<Animator>().ResetTrigger("IDLE");
            GetComponent<Animator>().ResetTrigger("WALK");
            GetComponent<Animator>().ResetTrigger("RUN");
            GetComponent<Animator>().ResetTrigger("JUMP");
            GetComponent<Animator>().ResetTrigger("FALL");
        }

        //make sure the attack timer is fine
        if (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.SideHit")
            || GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.UpHit")
            || GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.DownHit"))
        {
            attackTimer = currentAnimationTime = (1 - GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime);
            #region Create Arrow
            //spawn the arrow
            if (!hasSpawnedArrow && (attackTimer < 0.5f)
                && currentAnimationTime > 0)
            {
                hasSpawnedArrow = true;

                if (savedThumbState.x > 0.3f)
                {
                    //right up
                    if (savedThumbState.y > 0.3f)
                        SpawnArrow(ArrowDirState.Up, nextAttackIsSpecial);
                    //right down
                    else if (savedThumbState.y < -0.3f)
                        SpawnArrow(ArrowDirState.Down, nextAttackIsSpecial);
                    //right
                    else
                        SpawnArrow(ArrowDirState.Right, nextAttackIsSpecial);

                }
                else if (savedThumbState.x < -0.3f)
                {
                    //left up
                    if (savedThumbState.y > 0.3f)
                        SpawnArrow(ArrowDirState.Up, nextAttackIsSpecial);
                    //left down
                    else if (savedThumbState.y < -0.3f)
                        SpawnArrow(ArrowDirState.Down, nextAttackIsSpecial);
                    //hard left
                    else
                        SpawnArrow(ArrowDirState.Left, nextAttackIsSpecial);
                }

                //hard up
                else if (savedThumbState.y > 0.3f)
                    SpawnArrow(ArrowDirState.Up, nextAttackIsSpecial);
                //hard down
                else if (savedThumbState.y < -0.3f)
                    SpawnArrow(ArrowDirState.Down, nextAttackIsSpecial);

                //hard right
                else if (transform.localEulerAngles.y < 90)
                    SpawnArrow(ArrowDirState.Right, nextAttackIsSpecial);
                else
                    SpawnArrow(ArrowDirState.Left, nextAttackIsSpecial);

            }
            #endregion
        }
        //this partially stops animation locking
        else if (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Idle")
                 || GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Walk")
                 || GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Run"))
        {
            attackTimer = 1;
            isAttacking = true;
        }

    }

    //spawns an arrow with the stats based on playercontrols fields, 
    void SpawnArrow(ArrowDirState arrowDir, bool special)
    {
        int newArrowDamage = savedHeavyAttack ? heavyArrowDamage : arrowDamage;
        switch (arrowDir)
        {
            case ArrowDirState.Up:
                {
                    GameObject[] newArrows;
                    if (savedHeavyAttack)
                        newArrows = new GameObject[] { Instantiate(heavyArrow), Instantiate(heavyArrow), Instantiate(heavyArrow) };
                    else
                        newArrows = new GameObject[] { Instantiate(arrow), Instantiate(arrow), Instantiate(arrow) };

                    for (int i = 0; i < 3; ++i)
                    {
                        newArrows[i].transform.position = arrowSpawner.transform.position;
                        newArrows[i].GetComponent<ArrowMovement>().SetVars(arrowDir, arrowSpeed, arrowHitTime, enemy, arrowNumber, newArrowDamage);
                        newArrows[i].GetComponent<ArrowMovement>().heavy = savedHeavyAttack;
                        ++arrowDir;
                        if (special)
                            newArrows[i].GetComponent<ArrowMovement>().SpecialScript = specialAttackScript;
                    }
                    ++arrowNumber;
                    break;
                }
            case ArrowDirState.Down:
                {
                    GameObject[] newArrows;
                    if (savedHeavyAttack)
                        newArrows = new GameObject[] { Instantiate(heavyArrow), Instantiate(heavyArrow), Instantiate(heavyArrow) };
                    else
                        newArrows = new GameObject[] { Instantiate(arrow), Instantiate(arrow), Instantiate(arrow) };
                    for (int i = 0; i < 3; ++i)
                    {
                        newArrows[i].transform.position = arrowSpawner.transform.position;
                        newArrows[i].GetComponent<ArrowMovement>().SetVars(arrowDir, arrowSpeed, arrowHitTime, enemy, arrowNumber, newArrowDamage);
                        newArrows[i].GetComponent<ArrowMovement>().heavy = savedHeavyAttack;
                        ++arrowDir;
                        if (special)
                            newArrows[i].GetComponent<ArrowMovement>().SpecialScript = specialAttackScript;
                    }
                    ++arrowNumber;
                    break;
                }
            case ArrowDirState.Left:
                {
                    GameObject newArrow = savedHeavyAttack ? Instantiate(heavyArrow) : Instantiate(arrow);
                    newArrow.transform.position = arrowSpawner.transform.position;
                    newArrow.GetComponent<ArrowMovement>().SetVars(arrowDir, arrowSpeed, arrowHitTime, enemy, arrowNumber++, newArrowDamage);
                    newArrow.GetComponent<ArrowMovement>().heavy = savedHeavyAttack;
                    if (special)
                        newArrow.GetComponent<ArrowMovement>().SpecialScript = specialAttackScript;
                    break;
                }
            case ArrowDirState.Right:
                {
                    GameObject newArrow = savedHeavyAttack ? Instantiate(heavyArrow) : Instantiate(arrow);
                    newArrow.transform.position = arrowSpawner.transform.position;
                    newArrow.GetComponent<ArrowMovement>().SetVars(arrowDir, arrowSpeed, arrowHitTime, enemy, arrowNumber++, newArrowDamage);
                    newArrow.GetComponent<ArrowMovement>().heavy = savedHeavyAttack;
                    if (special)
                        newArrow.GetComponent<ArrowMovement>().SpecialScript = specialAttackScript;
                    break;
                }
        }
        //reset special
        if (nextAttackIsSpecial)
            nextAttackIsSpecial = false;
    }

    public void Freeze()
    {
        //todo: this tomorrow
    }

    void CheckTrail()
    {
        if (currentAnimationState == animationState.STATE_ATTACK_DOWN
            || currentAnimationState == animationState.STATE_ATTACK_UP
            || currentAnimationState == animationState.STATE_ATTACK_SIDE)
        {
            trailRenderer.GetComponent<TrailRenderer>().enabled = true;
        }
        else trailRenderer.GetComponent<TrailRenderer>().enabled = false;
    }
}