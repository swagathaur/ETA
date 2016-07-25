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
    public float attackAnimLength = 0.5f;
    public float tauntAnimLength = 0.5f;
    public float jumpAnimLength = 0.5f;

    public bool paused;

    float colourTimer;
    float attackTimer;

    Vector2 savedThumbState;
    Vector2 savedTriggerState;

    bool savedHeavyAttack;

    ArrowDirState counterDir;

    GamePadState state;
    GamePadState prevState;
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

    bool isAttacking = false; //is player attacking
    bool isGrounded = false; // is player on the ground
    bool isTaunting = false; // is player Taunting
    bool isWalking = false; // is player Walking or Running

    public animationState currentAnimationState = animationState.STATE_IDLE;
    public float currentAnimationTime = 0;

    #endregion

    // Use this for initialization
    void Start()
    {
        //define the animator attached to the player
        animator = this.GetComponent<Animator>();
    }

    //Update plz
    void Update()
    {
        currentAnimationTime = (1 - GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime);
        //currentAnimationTime -= Time.deltaTime;
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
        prevState = state;
        state = GamePad.GetState(playerIndex);

        if (IsCurrentAnimationStateCancellable())
        {
            CheckInput();
            ChangeDirection();
        }

        if (isAttacking)
        {
            Attack();
        }

        GetCounterState();
        CheckTrail();
        CheckFriction();
        ExtraGravity();
    }

    private void CheckFriction()
    {
        Vector3 newVelocity = GetComponent<Rigidbody>().velocity;

        if (isGrounded)
        {
            if (!isWalking)
            {
                newVelocity.x -= newVelocity.x * friction * Time.deltaTime;
            }
            //if (newVelocity.x < 2)
            //    newVelocity.x = 0;

        }
        newVelocity.x = Mathf.Clamp(newVelocity.x, -speedLimit, speedLimit);
        GetComponent<Rigidbody>().velocity = newVelocity;
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
            if (Mathf.Abs(prevState.ThumbSticks.Right.X) >= 0.3f
                || Mathf.Abs(prevState.ThumbSticks.Right.Y) >= 0.3f)
            {
                return;
            }
            counterTimer -= Time.deltaTime;
            if (state.ThumbSticks.Right.X > 0.3f)
            {
                if (state.ThumbSticks.Right.Y > 0.3f)
                {
                    counterDir = ArrowDirState.RightUp;
                    counterTimer = timeToCounter;
                }
                else if (state.ThumbSticks.Right.Y < -0.3f)
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
            else if (state.ThumbSticks.Right.X < -0.3f)
            {
                if (state.ThumbSticks.Right.Y > 0.3f)
                {
                    counterDir = ArrowDirState.LeftUp;
                    counterTimer = timeToCounter;
                }
                else if (state.ThumbSticks.Right.Y < -0.3f)
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

            else if (state.ThumbSticks.Right.Y > 0.3f)
            {
                counterDir = ArrowDirState.Up;
                counterTimer = timeToCounter;
            }
            else if (state.ThumbSticks.Right.Y < -0.3f)
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
            if (isHeavy)
                health -= 10;
            isAttacking = false;
            isWalking = false;
        }
        colourTimer = 0.5f;
    }
    public float CheckCounter(simpleMove incomingArrow)
    {
        bool heavyCounter = (state.Buttons.RightShoulder == ButtonState.Pressed);

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
            isAttacking = false;
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
            if (!(state.ThumbSticks.Left.Y < -0.4f)
                 && GetComponent<Rigidbody>().velocity.y < 0
                 && transform.position.y > coll.transform.position.y - coll.transform.localScale.y * 0.5f)
            {
                if (!isGrounded)
                {
                    GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 0, 0);
                    transform.position = new Vector3(transform.position.x, coll.transform.position.y + coll.transform.localScale.y * 0.5f, 0);
                    isGrounded = true;
                    changeState(animationState.STATE_IDLE);
                }
            }
            else
            {
                if (isGrounded && (state.ThumbSticks.Left.Y < -0.4f))
                {
                    isGrounded = false;
                    GetComponent<Rigidbody>().AddForce(Vector3.down * speedLimit * 0.25f);
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
            if (GetComponent<Rigidbody>().velocity.y < 0)
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

    void ChangeDirection()
    {
        if (isAttacking)
            return;

        if (state.ThumbSticks.Left.X > 0.3f)
            transform.localEulerAngles = new Vector3(0, 0, 0);
        if (state.ThumbSticks.Left.X < -0.3f)
            transform.localEulerAngles = new Vector3(0, 180, 0);

        if ((Enemy.transform.position - transform.position).magnitude < 1)
        {
            GetComponent<Rigidbody>().AddForce((transform.position - Enemy.transform.position).normalized.x * speedLimit * 0.5f, 0, 0);
        }
    }

    void CheckInput()
    {
        if (state.IsConnected == false)
            return;

        //check all animation
        isWalking = false;


        //Attack
        if ((prevState.Buttons.X == ButtonState.Released && state.Buttons.X == ButtonState.Pressed)
            || (prevState.Buttons.B == ButtonState.Released && state.Buttons.B == ButtonState.Pressed))
        {
            isAttacking = true;
        }

        //Jump
        else if ((prevState.Buttons.A == ButtonState.Released && state.Buttons.A == ButtonState.Pressed)
            && IsCurrentAnimationStateCancellable())
        {
            if (isGrounded)
            {
                // move from ground and jump
                isGrounded = false;
                GetComponent<Rigidbody>().AddForce(new Vector2(0, jumpForce));
                //animate
                changeState(animationState.STATE_JUMP);
            }
        }

        #region check Walking or Running
        //Run Right
        else if (state.ThumbSticks.Left.X > 0.8f)
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
        else if (state.ThumbSticks.Left.X < -0.8f)
        {
            if (!isAttacking)
            {
                if (isGrounded)
                {
                    GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x - (speedLimit * Time.deltaTime), 0, 0);
                    changeState(animationState.STATE_RUN);
                    isWalking = true;
                }
                else GetComponent<Rigidbody>().AddForce(new Vector3(-airControl * speedLimit, 0, 0));
            }
        }
        //Walk Right
        else if (state.ThumbSticks.Left.X > 0.2f)
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
        //Walk Left
        else if (state.ThumbSticks.Left.X < -0.2f)
        {
            if (!isAttacking)
            {
                if (isGrounded)
                {
                    GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x - (speedLimit * Time.deltaTime), 0, 0);
                    changeState(animationState.STATE_RUN);
                    isWalking = true;
                }
                else GetComponent<Rigidbody>().AddForce(new Vector3(-airControl * speedLimit, 0, 0));
            }
        }
        #endregion

    }

    void Attack()
    {
        if (attackTimer == 0)
        {
            if (state.ThumbSticks.Left.Y > 0.3)
            {
                changeState(animationState.STATE_ATTACK_UP);
            }
            else if (state.ThumbSticks.Left.Y < -0.3)
            {
                changeState(animationState.STATE_ATTACK_DOWN);
            }
            else changeState(animationState.STATE_ATTACK_SIDE);


            attackAnimLength = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length;
            savedHeavyAttack = (state.Buttons.B == ButtonState.Pressed);
            savedThumbState.x = state.ThumbSticks.Left.X;
            savedThumbState.y = state.ThumbSticks.Left.Y;
            savedTriggerState.x = state.Triggers.Left;
            savedTriggerState.y = state.Triggers.Right;
        }

        attackTimer += Time.deltaTime;

        if (isAttacking && (attackTimer > attackAnimLength * 0.5f))
        {
            isAttacking = false;
            attackTimer = 0;

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