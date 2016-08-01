using UnityEngine;
using System.Collections;
using XInputDotNetPure;
using System;

public class PlayerControls : MonoBehaviour
{

    #region VARS 
    public PlayerIndex playerIndex;

    public GameObject arrow;
    public GameObject Enemy;
    public SpecialBase specialAttackScript;

    public GameObject arrowSpawner;
    public GameObject trailRenderer;

    private GameObject dustCloudEmitter;

    public short health = 100;
    public short special = 0;
    public short maxSpecial = 100;
    public short arrowSpeed = 15; // 15 seems reasonable

    public float timeToCounter = 0.15f;
    public float counterTimer;

    public float airControl = 8;
    public float playerSize = 1;
    public float gravPower = 1;
    public float speedLimit = 15; // player left right walk speed
    public float jumpForce = 400;
    public float friction = 12;

    public float arrowHitTime = 0.1f; // IN SECONDS

    //Animation Lengths
    public float tauntAnimLength = 0.5f;
    public float jumpAnimLength = 0.5f;

    public bool paused;

    float colourTimer;
    float stepTimer;
    float justJumped = 0;

    Vector2 savedThumbState;
    Vector2 savedTriggerState;

    bool savedHeavyAttack;

    ArrowDirState counterDir;

    AudioScript audioSource;

    GamePadState controllerState;
    GamePadState prevControllerState;
    Animator animator;

    public enum animationState
    {
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
        STATE_ATTACK_SIDE = 11
    }

    public float attackTimer;
    public bool startAttack = false;
    public bool isAttacking = false; //is player attacking\
    bool hasSpawnedArrow = false;
    bool isGrounded = false; // is player on the ground
    bool isTaunting = false; // is player Taunting
    bool isWalking = false; // is player Walking or Running

    public animationState currentAnimationState = animationState.STATE_IDLE;
    public float currentAnimationTime = 0;


    //magic number to stop you snapping back up into a platform
    //todo: make this platform dependant, reset on actions rather than time
    private float tapFallTimer = 0;
    private float maxTapFallTime = 0.25f;

    public bool turning = false;

    #endregion

    // Use this for initialization
    void Start()
    {
        //define the animator attached to the player
        animator = this.GetComponent<Animator>();

        //find audioScript
        audioSource = FindObjectOfType<AudioScript>();

        //set up the dustCloudEmitter
        dustCloudEmitter = GameObject.Find("player" + ((int)playerIndex + 1) + "DustCloudEmitter");
        dustCloudEmitter.GetComponent<ParticleSystem>().emissionRate = 0;
    }

    //Update plz
    void Update()
    {
        currentAnimationTime = (1 - GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime);
        tapFallTimer -= Time.deltaTime;

        //reset to idle if not moving fast
        if (IsCurrentAnimationStateCancellable() &&
            Math.Abs(GetComponent<Rigidbody>().velocity.x) < 0.25f)
        {
            changeState(animationState.STATE_IDLE);
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
                    changeState(animationState.STATE_IDLE);
                else
                    changeState(animationState.STATE_FALL);
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

        if (isAttacking || startAttack)
        {
            Attack();
        }

        GetCounterState();
        CheckTrail();
        CheckFriction();
        CheckFootsteps();
        ExtraGravity();
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
            if (Mathf.Abs(prevControllerState.ThumbSticks.Right.X) >= 0.3f
                || Mathf.Abs(prevControllerState.ThumbSticks.Right.Y) >= 0.3f)
            {
                return;
            }
            counterTimer -= Time.deltaTime;
            if (controllerState.ThumbSticks.Right.X > 0.3f)
            {
                if (controllerState.ThumbSticks.Right.Y > 0.3f)
                {
                    counterDir = ArrowDirState.RightUp;
                    counterTimer = timeToCounter;
                }
                else if (controllerState.ThumbSticks.Right.Y < -0.3f)
                {
                    counterDir = ArrowDirState.RightDown;
                    counterTimer = timeToCounter;
                }
                else
                {
                    counterDir = ArrowDirState.Right;
                    counterTimer = timeToCounter;
                }

            }
            else if (controllerState.ThumbSticks.Right.X < -0.3f)
            {
                if (controllerState.ThumbSticks.Right.Y > 0.3f)
                {
                    counterDir = ArrowDirState.LeftUp;
                    counterTimer = timeToCounter;
                }
                else if (controllerState.ThumbSticks.Right.Y < -0.3f)
                {
                    counterDir = ArrowDirState.LeftDown;
                    counterTimer = timeToCounter;
                }
                else
                {
                    counterDir = ArrowDirState.Left;
                    counterTimer = timeToCounter;
                }
            }

            else if (controllerState.ThumbSticks.Right.Y > 0.3f)
            {
                counterDir = ArrowDirState.Up;
                counterTimer = timeToCounter;
            }
            else if (controllerState.ThumbSticks.Right.Y < -0.3f)
            {
                counterDir = ArrowDirState.Down;
                counterTimer = timeToCounter;
            }
        }

        if (counterTimer < 0)
        {
            counterTimer = 0;
            counterDir = ArrowDirState.NOCOUNTER;
        }
        else
            counterTimer -= Time.deltaTime;
    }
    public void DidCounter(bool counterSuccess, bool isHeavy)
    {
        if (counterSuccess)
        {
            special += 5;
        }
        else
        {
            health -= 5;
            audioSource.playSound(playerAudio.CLIP_HIT, playerIndex);
            if (isHeavy)
                health -= 10;
            isAttacking = false;
            isWalking = false;
        }
        colourTimer = 0.5f;
    }
    public float CheckCounter(simpleMove incomingArrow)
    {
        bool heavyCounter = (controllerState.Buttons.RightShoulder == ButtonState.Pressed);

        //CHECK IF MATCHING
        if (counterDir == incomingArrow.direction)
        {
            if (incomingArrow.SpecialScript == null)
            {
                if (heavyCounter == incomingArrow.heavy)
                    return counterTimer;
                else
                {
                    return 0;
                }
            }
            else
            {
                incomingArrow.SpecialScript.RunAttack(this);
                return counterTimer;
            }
        }
        return 0;
    }

    void changeState(animationState newState)
    {
        if (currentAnimationState == newState)
            return;

        if ((newState != animationState.STATE_ATTACK_DOWN)
            && (newState != animationState.STATE_ATTACK_SIDE)
            && (newState != animationState.STATE_ATTACK_UP))
        {
            attackTimer = 0;
            //isAttacking = false;
            startAttack = false;
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
        }
    }
    private bool IsCurrentAnimationStateCancellable()
    {
        return (currentAnimationState == animationState.STATE_IDLE)
            || (currentAnimationState == animationState.STATE_RUN)
            || (currentAnimationState == animationState.STATE_WALK)
            || (currentAnimationState == animationState.STATE_JUMP)
            || (currentAnimationState == animationState.STATE_FALL);
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
                 && transform.position.y + (GetComponent<BoxCollider>().size.y * 0.01f) > coll.transform.position.y - (coll.transform.localScale.y * 0.5f))
            {
                if (!isGrounded
                    && tapFallTimer <= 0)
                {
                    GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 0, 0);
                    transform.position = new Vector3(transform.position.x, coll.transform.position.y + GetComponent<BoxCollider>().size.y * 0.5f, 0);
                    isGrounded = true;
                    changeState(animationState.STATE_IDLE);
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
                transform.position = new Vector3(transform.position.x, coll.transform.position.y + coll.transform.localScale.y * 0.5f, 0);
                isGrounded = true;
                changeState(animationState.STATE_IDLE);
            }
        }
    }
    void OnTriggerEnter(Collider coll)
    {
        if (coll.tag == "Platform")
        {
            if (GetComponent<Rigidbody>().velocity.y < 0
                && transform.position.y + (GetComponent<BoxCollider>().size.y * 0.5f) > coll.transform.position.y)
            {
                if (!isGrounded && controllerState.ThumbSticks.Left.Y > -0.95f)
                {
                    GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 0, 0);
                    transform.position = new Vector3(transform.position.x, coll.transform.position.y + coll.transform.localScale.y * 0.5f, 0);
                    isGrounded = true;
                    changeState(animationState.STATE_IDLE);
                    DustCloud(Quaternion.Euler(-transform.up));
                }
            }
        }
        if (coll.tag == "Terrain")
        {
            if (!isGrounded && controllerState.ThumbSticks.Left.Y > -0.95f
                && transform.position.y + (GetComponent<BoxCollider>().size.y * 0.5f) > coll.transform.position.y)
            {
                GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 0, 0);
                transform.position = new Vector3(transform.position.x, coll.transform.position.y + coll.transform.localScale.y * 0.5f, 0);
                isGrounded = true;
                changeState(animationState.STATE_IDLE);
                DustCloud(Quaternion.Euler(-transform.up));
            }
        }
    }

    void ChangeDirection()
    {
        if (isAttacking)
            return;

        if (controllerState.ThumbSticks.Left.X > 0.375f)
            transform.localEulerAngles = new Vector3(0, 0, 0);
        if (controllerState.ThumbSticks.Left.X < -0.375f)
            transform.localEulerAngles = new Vector3(0, 180, 0);

        if ((Enemy.transform.position - transform.position).magnitude < 1)
        {
            GetComponent<Rigidbody>().AddForce((transform.position - Enemy.transform.position).normalized.x * speedLimit * 0.3f, 0, 0);
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
            if (!isAttacking)
            {
                startAttack = true;
                isAttacking = true;
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
                changeState(animationState.STATE_JUMP);
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
                    changeState(animationState.STATE_RUN);
                    isWalking = true;
                }
                else GetComponent<Rigidbody>().AddForce(new Vector3(airControl * speedLimit, 0, 0));
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
                    changeState(animationState.STATE_RUN);
                    isWalking = true;
                }
                else
                {
                    GetComponent<Rigidbody>().AddForce(new Vector3(-airControl * speedLimit, 0, 0));
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
                    changeState(animationState.STATE_RUN);
                    isWalking = true;
                }
                else
                {
                    GetComponent<Rigidbody>().AddForce(new Vector3(airControl * speedLimit, 0, 0));
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
                    changeState(animationState.STATE_RUN);
                    isWalking = true;
                }
                else
                {
                    GetComponent<Rigidbody>().AddForce(new Vector3(-airControl * speedLimit, 0, 0));
                }
            }
        }
        #endregion

    }

    void Attack()
    {
        GetComponent<Animator>().ResetTrigger("IDLE");
        GetComponent<Animator>().ResetTrigger("WALK");
        GetComponent<Animator>().ResetTrigger("RUN");
        GetComponent<Animator>().ResetTrigger("JUMP");
        GetComponent<Animator>().ResetTrigger("FALL");

        if (startAttack)
        {
            if (controllerState.ThumbSticks.Left.Y > 0.3)
            {
                changeState(animationState.STATE_ATTACK_UP);
            }
            else if (controllerState.ThumbSticks.Left.Y < -0.3)
            {
                changeState(animationState.STATE_ATTACK_DOWN);
            }
            else changeState(animationState.STATE_ATTACK_SIDE);

            audioSource.playSound(playerAudio.CLIP_ATTACK, playerIndex);

            savedHeavyAttack = (controllerState.Buttons.B == ButtonState.Pressed);
            savedThumbState.x = controllerState.ThumbSticks.Left.X;
            savedThumbState.y = controllerState.ThumbSticks.Left.Y;
            savedTriggerState.x = controllerState.Triggers.Left;
            savedTriggerState.y = controllerState.Triggers.Right;

            attackTimer = 1;
            startAttack = false;
            return;
        }

        //make sure the attack timer is fine
        if (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.SideHit")
            || GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.UpHit")
            || GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.DownHit"))
        {
            attackTimer = currentAnimationTime = (1 - GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime);
            if (attackTimer < 0 && hasSpawnedArrow == false)
                return;
        }
        //this stops animation locking
        else if ((GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Idle")
            || GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Walk")
            || GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Run"))
            && attackTimer < 0.8)
        {
            attackTimer = 0;
            isAttacking = false;
        }

        //spawn the arrow
        if (!hasSpawnedArrow && (attackTimer < 0.5f))
        {
            hasSpawnedArrow = true;

            #region Create Arrow
            GameObject newArrow = Instantiate(arrow);
            newArrow.transform.position = arrowSpawner.transform.position;

            if (savedThumbState.x > 0.3f)
            {
                if (savedThumbState.y > 0.3f)
                {
                    newArrow.GetComponent<ArrowMovement>().SetVars(ArrowDirState.RightUp, arrowSpeed, arrowHitTime, Enemy);
                    newArrow.transform.position += Vector3.right + Vector3.up;
                }
                else if (savedThumbState.y < -0.3f)
                {
                    newArrow.GetComponent<ArrowMovement>().SetVars(ArrowDirState.RightDown, arrowSpeed, arrowHitTime, Enemy);
                    newArrow.transform.position += Vector3.right + Vector3.down;
                }
                else
                {
                    newArrow.GetComponent<ArrowMovement>().SetVars(ArrowDirState.Right, arrowSpeed, arrowHitTime, Enemy);
                    newArrow.transform.position += Vector3.right;
                }

            }
            else if (savedThumbState.x < -0.3f)
            {
                if (savedThumbState.y > 0.3f)
                {
                    newArrow.GetComponent<ArrowMovement>().SetVars(ArrowDirState.LeftUp, arrowSpeed, arrowHitTime, Enemy);
                    newArrow.transform.position += Vector3.left + Vector3.up;
                }
                else if (savedThumbState.y < -0.3f)
                {
                    newArrow.GetComponent<ArrowMovement>().SetVars(ArrowDirState.LeftDown, arrowSpeed, arrowHitTime, Enemy);
                    newArrow.transform.position += Vector3.left + Vector3.up;
                }
                else
                {
                    newArrow.GetComponent<ArrowMovement>().SetVars(ArrowDirState.Left, arrowSpeed, arrowHitTime, Enemy);
                    newArrow.transform.position += Vector3.left;
                }
            }


            else if (savedThumbState.y > 0.3f)
            {
                newArrow.GetComponent<ArrowMovement>().SetVars(ArrowDirState.Up, arrowSpeed, arrowHitTime, Enemy);

                newArrow.transform.position += (transform.up * 2);
            }
            else if (savedThumbState.y < -0.3f)
            {
                newArrow.GetComponent<ArrowMovement>().SetVars(ArrowDirState.Down, arrowSpeed, arrowHitTime, Enemy);

                newArrow.transform.position += (-transform.up * 2);
            }

            else if (transform.localEulerAngles.y < 90)
            {
                newArrow.GetComponent<ArrowMovement>().SetVars(ArrowDirState.Right, arrowSpeed, arrowHitTime, Enemy);

                newArrow.transform.position += Vector3.right;
            }
            else
            {
                newArrow.GetComponent<ArrowMovement>().SetVars(ArrowDirState.Left, arrowSpeed, arrowHitTime, Enemy);

                newArrow.transform.position += Vector3.left;
            }
            if (savedHeavyAttack)
                newArrow.GetComponent<ArrowMovement>().heavy = true;
            if (savedTriggerState.x > 0.5f && savedTriggerState.y > 0.5f && special == 100)
                newArrow.GetComponent<ArrowMovement>().SpecialScript = specialAttackScript;
            #endregion
        }

        if (attackTimer <= 0)
        {
            isAttacking = false;
            hasSpawnedArrow = false;
        }
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