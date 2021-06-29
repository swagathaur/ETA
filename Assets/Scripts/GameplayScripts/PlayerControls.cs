using UnityEngine;
using System.Collections;
using XInputDotNetPure;
using System;

public class PlayerControls : MonoBehaviour
{

    #region VARS 
    public PlayerIndex playerIndex;

    [SerializeField]
    private GameObject heavyArrow;
    [SerializeField]
    private GameObject arrow;
    [SerializeField]
    private GameObject specialArrow;

    [SerializeField]
    private AudioClip[] attackSounds;
    [SerializeField]
    private AudioClip[] hitSounds;
    [SerializeField]
    private AudioClip[] tauntSounds;
    [SerializeField]
    private AudioClip specialAttackSound;

    [HideInInspector]
    public SpecialBase specialAttackScript;
    [HideInInspector]
    public GameObject enemy;
    [HideInInspector]
    public Color glowColor;
    [SerializeField]
    public GameObject arrowSpawner;
    private GameObject trailRenderer;
    private GameObject dustCloudEmitter;

    [SerializeField]
    public short health = 100;
    [SerializeField]
    public short special = 0;
    public short maxSpecial = 100;

    public short amountOfSpecialConsumed = 100; //amount of special consumed per use of special
    [SerializeField]
    private short arrowSpeed = 15; // 15 seems reasonable

    [HideInInspector]
    public float currentSpecialCooldown;
    [SerializeField]
    private float specialCooldown = 1.5f;
    
    private float airControl = 2;
    private float playerSize = 0;
    private float gravPower = 700;
    private float speedLimit = 8; // player left right walk speed
    private float jumpForce = 300;
    private float friction = 12;
    private float pushStrength = 5;
    private bool hasBunted = false;

    [SerializeField]
    private float arrowHitTime = 0.1f; // IN SECONDS

    [SerializeField]
    private int arrowDamage = 5;
    [SerializeField]
    private int heavyArrowDamage = 10;

    //Animation Lengths
    private float tauntAnimLength = 0.5f;
    private float jumpAnimLength = 0.5f;

    [SerializeField]
    public float freezeTimeWhenCountered = 0.3f;
    private float freezeTimer = 0;

    private float colourTimer;
    private float stepTimer;
    private float justJumped = 0;

    private Vector2 savedThumbState;
    private Vector2 savedTriggerState;

    private bool savedHeavyAttack;

    [HideInInspector]
    public Vector2 counterDir;
    private short specialGainOnCounter = 15;

    private AudioScript audioSource;

    private GamePadState controllerState;
    private GamePadState prevControllerState;
    private Animator animator;

    private bool stoppedHighJumping;

    public bool useGravity;

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
        STATE_ATTACK_LIGHT = 9,
        STATE_ATTACK_HEAVY = 10,
        STATE_ATTACK_SPECIAL = 11,
        STATE_COUNTER = 12,
        STATE_WIN = 13,
    }

    private float attackTimer;
    private bool startAttack = false;
    private bool nextAttackIsSpecial = false;
    private bool isAttacking = false; //is player attacking
    private bool hasSpawnedArrow = false;
    private bool isGrounded = false; // is player on the ground
    private bool isTaunting = false; // is player Taunting
    private bool isWalking = false; // is player Walking or Running

    [SerializeField]
    public animationState currentAnimationState = animationState.STATE_START;
    [SerializeField]
    public float currentAnimationTime = 0;

    //magic number to stop you snapping back up into a platform
    //todo: make this platform dependant, reset on actions rather than time
    private float tapFallTimer = 0;
    private float maxTapFallTime = 0.25f;

    private bool turning = false;
    
    
    //    [HideInInspector]
    public bool isSuspended; //a suspended player still most things except input. still does update input states though


    //when dying
    private bool isDead = false;
    private float deathSpin = 0;
    private float deathZoom = 1;
    private const float deathDuration = 3;
    private Vector3 deathSpinPos;

    //when winning
    private bool isWinning = false;

    #endregion

    AnimatorStateInfo asi;

    [HideInInspector]
    private GameObject specialGlowPrefab;

    // Use this for initialization
    void Start()
    {
        useGravity = true;
        isSuspended = true;

        currentSpecialCooldown = -1;

        //define the animator attached to the player
        animator = GetComponent<Animator>();

        //find audioScript
        audioSource = FindObjectOfType<AudioScript>();

        //set up the dustCloudEmitter
        dustCloudEmitter = transform.Find("dustCloudEmitter").gameObject;
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

        specialGlowPrefab = Resources.Load<GameObject>("Prefabs/SpecialGlow");
    }

    //Update plz
    void Update()
    {
        if (isDead)
        {
            DoDeathZoomRotate();
            return;
        }
        else if (isWinning)
        {
            DoWin();
            return;
        }

        try
        {
            animator.enabled = true;
        }
        catch
        {
            animator = GetComponentInChildren<Animator>();
        }

        //UPDATE GAMEPAD
        prevControllerState = controllerState;
        controllerState = GamePad.GetState(playerIndex);

        currentSpecialCooldown -= Time.deltaTime;

        //if we're suspended (beginning/end of match, during supers?)
        if (isSuspended)
        {
            GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 0, GetComponent<Rigidbody>().velocity.z);
            currentAnimationTime = (1 - animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
            currentSpecialCooldown = -1; //fixes anarchy/hardwood special interaction
            //apply g
            //ExtraGravity();
            if (health <= 0)
            {
                //todo: play death animation
                GamePad.SetVibration(playerIndex, 0, 0); //turn off vibration
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
            currentAnimationTime = (1 - (animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1.1f));
            tapFallTimer -= Time.deltaTime;

            //reset to idle if not moving fast
            if (IsCurrentAnimationStateCancellable() &&
                Math.Abs(GetComponent<Rigidbody>().velocity.x) < 0.25f)
            {
                if (isGrounded)
                    ChangeState(animationState.STATE_IDLE);
                else if (GetComponent<Rigidbody>().velocity.y > 0)
                    ChangeState(animationState.STATE_JUMP);
                else
                    ChangeState(animationState.STATE_FALL);
            }
            //reset to idle or fall if not looping and animation has ended
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

            GetCounterState();

            //if the animation isn't cancellable, ignore most input
            if (IsCurrentAnimationStateCancellable() || currentAnimationState == animationState.STATE_COUNTER)
            {
                CheckInput();
                if (currentAnimationState != animationState.STATE_COUNTER)
                    ChangeDirection();
            }
            if (CheckCounterFreeze())
                return;

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
            if (stepTimer > animator.GetCurrentAnimatorStateInfo(0).length * 0.5f)
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
        const float turnSpeedBurst = 5f;

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
                    newVelocity.x = -turnSpeedBurst;
                }
                else if (controllerState.ThumbSticks.Left.X > 0.4f)
                {
                    newVelocity.x = turnSpeedBurst;
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
        if (!isGrounded && useGravity)
            GetComponent<Rigidbody>().AddForce(0, -1 * gravPower * Time.deltaTime, 0);
    }

    //this is where countering starts
    public void GetCounterState()
    {
        if (currentAnimationState != animationState.STATE_COUNTER)
        {
            counterDir = new Vector2(0, 0);
            //Make sure a counter is started this frame
            if (Mathf.Abs(controllerState.ThumbSticks.Right.X) < 0.3f
                && Mathf.Abs(controllerState.ThumbSticks.Right.Y) < 0.3f)
            {
                return;
            }
            else if (Mathf.Abs(prevControllerState.ThumbSticks.Right.X) >= 0.3f
                || Mathf.Abs(prevControllerState.ThumbSticks.Right.Y) >= 0.3f)
            {
                return;
            }
            else if (!IsCurrentAnimationStateCancellable())
            {
                return;
            }

            animator.ResetTrigger("IDLE");
            //if it is start the animation and set a counter direction based on the joysticks position
            ChangeState(animationState.STATE_COUNTER);

            counterDir.x = controllerState.ThumbSticks.Right.X;
            counterDir.y = controllerState.ThumbSticks.Right.Y;
            counterDir.Normalize();

            if (Vector3.Dot(transform.forward, counterDir) < -0.1)
            {
                if (counterDir.x > 0.375f)
                {
                    ChangeDirection(2);
                }
                if (counterDir.x < -0.375f)
                {
                    ChangeDirection(1);
                }
            }
            hasBunted = false;
        }
        else if (!hasBunted && currentAnimationTime < 0.5f)
        {
            hasBunted = true;

            //do knockback
            Vector3 vecBetween = transform.position - enemy.transform.position;
            if (Vector3.Dot(transform.forward, vecBetween) < 0
                && Math.Abs(vecBetween.y) < 1)
            {
                if (Math.Abs(vecBetween.x) < 1.5)
                {
                    int dir = vecBetween.x > 0 ? -1 : 1;
                    enemy.GetComponent<Rigidbody>().AddForce(new Vector3(100 * dir, 100, 0));
                }
            }
        }
    }

    public void DoCounter(bool counterSuccess, bool isHeavy, int damage, bool canGainSpecial = true)
    {
        if (counterSuccess)
        {
            if (canGainSpecial)
            {
                special += specialGainOnCounter;
                if (special > maxSpecial)
                    special = maxSpecial;
            }
        }
        else
        {
            Camera.main.GetComponent<CameraPos>().ShakeTheCamera(0.05f, 0.1f);
            if (!isSuspended)
            {
                StartCoroutine(Rumble(1, 0.25f));
                health -= (short)damage;
            }
            audioSource.playSound(hitSounds[UnityEngine.Random.Range(0, hitSounds.Length)]);
        }
        colourTimer = 0.5f;
    }
    //utility function for rumbling, should be called with StartCoroutine
    IEnumerator Rumble(int rumbleAmount, float time)
    {
        GamePad.SetVibration(playerIndex, rumbleAmount, rumbleAmount);
        yield return new WaitForSeconds(time);
        GamePad.SetVibration(playerIndex, 0, 0);
    }
    public bool CheckCounterSuccess(ArrowMovement incomingArrow)
    {
        if (incomingArrow.SpecialScript != null
            && incomingArrow.target != enemy)
        {
            incomingArrow.SpecialScript.RunAttack(playerIndex);
            incomingArrow.GetComponent<BoxCollider>().enabled = false;
            return false;
        }

        bool heavyCounter = (controllerState.Buttons.RightShoulder == ButtonState.Pressed);
        //return a fail if not countering
        if (currentAnimationState == animationState.STATE_COUNTER)
            return true;

        //CHECK IF MATCHING
        if (Vector2.Dot(counterDir, -incomingArrow.transform.right) > 0.0f) //NEEDS FIXING
        {
            if (incomingArrow.SpecialScript == null)
            {
                if (heavyCounter == incomingArrow.heavy)
                    return currentAnimationState == animationState.STATE_COUNTER ? true : false;
                else
                {
                    return false;
                }
            }
        }
        return false;
    }

    //get animation clip by name
    public AnimationClip GetAnimationClip(string name)
    {
        if (!animator)
            return null;
        foreach(AnimationClip ac in animator.runtimeAnimatorController.animationClips)
        {
            if (ac.name == name)
            {
                return ac;
            }
        }

        return null;
    }
    public void ChangeState(animationState newState)
    {
        //todo: reset all triggers
        if (currentAnimationState == newState)
            return;
        if (attackTimer > 0)
            return;

        //correct for allowing run/jump during counter
        if ((newState == animationState.STATE_RUN ||
            newState == animationState.STATE_JUMP)
            && currentAnimationState == animationState.STATE_COUNTER)
        {
            return;
        }

        if (newState == animationState.STATE_IDLE)
        {
            animator.ResetTrigger("RUN");
            animator.ResetTrigger("WALK");
        }

        currentAnimationState = newState;
        PlayAnimationState();
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
            case animationState.STATE_ATTACK_LIGHT:
                animator.SetTrigger("ATTACK_LIGHT");
                break;
            case animationState.STATE_ATTACK_HEAVY:
                animator.SetTrigger("ATTACK_HEAVY");
                break;
            case animationState.STATE_ATTACK_SPECIAL:
                animator.SetTrigger("ATTACK_SPECIAL");
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
        if (coll.tag == "Terrain" || coll.tag == "Platform")
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
                    animator.ResetTrigger("JUMP");
                    //ChangeState(animationState.STATE_IDLE);
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
                animator.ResetTrigger("JUMP");
                //ChangeState(animationState.STATE_IDLE);
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
            Debug.DrawRay(transform.position + Vector3.up * 2, Vector3.down);

            if (Physics.Raycast(transform.position + Vector3.up * 2, Vector3.down, out hit, 200, LayerMask.NameToLayer("Platform")))
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
                        animator.ResetTrigger("JUMP");
                        ChangeState(animationState.STATE_IDLE);
                        DustCloud(Quaternion.Euler(-transform.up));
                    }
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
                temp.y = coll.transform.position.y + coll.GetComponent<BoxCollider>().center.y + (coll.GetComponent<BoxCollider>().size.y * 0.45f);
                transform.position = temp;

                isGrounded = true;
                if (animator != null)
                    animator.ResetTrigger("JUMP");
                //ChangeState(animationState.STATE_IDLE);
                DustCloud(Quaternion.Euler(-transform.up));
            }
        }
    }

    /*if directionoverride = 0, changes based on input ***Default***
      if directionoverride = 1, forces you left
      if directionoverride = 2, forces you right*/
    void ChangeDirection(int directionOverride = 0)
    {
        //if we're countering don't let movement override
        if (currentAnimationState == animationState.STATE_COUNTER
            && directionOverride == 0)
            return;

        if (isAttacking)
            return;

        //left
        if (directionOverride == 2)
        {
            transform.localEulerAngles = new Vector3(0, 0, 0);
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
        }
        //right
        else if (directionOverride == 1)
        {
            transform.localEulerAngles = new Vector3(0, 180, 0);
            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
        }
        else if (controllerState.ThumbSticks.Left.X > 0.375f)
        {
            transform.localEulerAngles = new Vector3(0, 0, 0);
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
        }
        else if (controllerState.ThumbSticks.Left.X < -0.375f)
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

        if (justJumped > 0 && controllerState.Buttons.A == ButtonState.Released)
        {
            stoppedHighJumping = true;
        }

        //differentiate shorthop from long hops
        if (justJumped > 0 && controllerState.Buttons.A == ButtonState.Pressed && !stoppedHighJumping)
        {
            justJumped -= Time.deltaTime;
            GetComponent<Rigidbody>().AddForce(new Vector2(0, jumpForce * 2) * Time.deltaTime);
        }

        //Attack
        if ((prevControllerState.Buttons.X == ButtonState.Released && controllerState.Buttons.X == ButtonState.Pressed)
            || (prevControllerState.Buttons.B == ButtonState.Released && controllerState.Buttons.B == ButtonState.Pressed))
        {
            if (!isAttacking && (currentAnimationState != animationState.STATE_COUNTER && counterDir.magnitude == 0.0f) && attackTimer <= 0)
            {
                startAttack = true;

                //play sound
                audioSource.playSound(attackSounds[UnityEngine.Random.Range(0, attackSounds.Length)]);
            }
        }
        if (prevControllerState.Buttons.Y == ButtonState.Released && controllerState.Buttons.Y == ButtonState.Pressed
            && special >= amountOfSpecialConsumed
            && currentSpecialCooldown <= 0)
        {
            if (!isAttacking && (currentAnimationState != animationState.STATE_COUNTER && counterDir.magnitude == 0.0f) && attackTimer <= 0)
            {
                startAttack = true;
                nextAttackIsSpecial = true;
                special -= amountOfSpecialConsumed;
                currentSpecialCooldown = specialCooldown;
                audioSource.playSound(specialAttackSound);
            }
        }

        //Jump
        else if ((prevControllerState.Buttons.A == ButtonState.Released && controllerState.Buttons.A == ButtonState.Pressed)
            && IsCurrentAnimationStateCancellable())
        {
            if (isGrounded)
            {
                stoppedHighJumping = false;
                GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 0, GetComponent<Rigidbody>().velocity.z);
                // move from ground and jump
                isGrounded = false;
                justJumped = 0.3f;
                audioSource.playSound(playerAudio.CLIP_JUMP, playerIndex);
                GetComponent<Rigidbody>().AddForce(new Vector2(0, jumpForce * 0.6f));
                //animate
                ChangeState(animationState.STATE_JUMP);
            }
        }
        //do taunt
        else if (prevControllerState.DPad.Up == ButtonState.Released && controllerState.DPad.Up == ButtonState.Pressed)
        {
            ChangeState(animationState.STATE_TAUNT);
            audioSource.playSound(tauntSounds[UnityEngine.Random.Range(0, tauntSounds.Length)]);
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

    //enum used to manage button presses
    public enum GamepadButtons
    {
        A, B, X, Y
    }
    //returns true if button was pressed this frame
    public bool ButtonTapped(GamepadButtons b)
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
    public bool ButtonDown(GamepadButtons b)
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
    public bool ButtonUp(GamepadButtons b)
    {
        switch (b)
        {
            case GamepadButtons.A:
                return controllerState.Buttons.A == ButtonState.Released;
            case GamepadButtons.B:
                return controllerState.Buttons.B == ButtonState.Released;
            case GamepadButtons.X:
                return controllerState.Buttons.X == ButtonState.Released;
            case GamepadButtons.Y:
                return controllerState.Buttons.Y == ButtonState.Released;
        }
        return false;
    }

    //called to kill the player
    public void KillPlayer()
    {
        isDead = true;
        deathZoom = Mathf.Abs(transform.lossyScale.x);

        this.GetComponent<BoxCollider>().enabled = false;
        deathSpinPos = (GetComponent<BoxCollider>().size * 0.5f) + transform.position;
    }

    private void DoDeathZoomRotate()
    {
        Freeze();
        deathZoom -= Time.deltaTime * 0.5f;
        deathSpin = Time.deltaTime * 400;

        if (deathZoom < 0)
        {
            transform.localScale = Vector3.zero;
            return;
        }

        transform.localScale = new Vector3(deathZoom, deathZoom, deathZoom);
        transform.RotateAround(deathSpinPos , new Vector3(0, 0, 1), deathSpin);
    }

    //called when the player wins
    public void WinPlayer()
    {
        isWinning = true;
        Freeze();
        this.GetComponent<BoxCollider>().enabled = false;
        ChangeState(animationState.STATE_TAUNT);
    }
    private void DoWin()
    {
        Freeze();
        if (currentAnimationState < 0)
        {
            ChangeState(animationState.STATE_TAUNT);
        }
    }

    void Attack()
    {
        //first frame of attacking, set stuff up
        if (startAttack && !isAttacking)
        {
            //unity fuck off
            animator.ResetTrigger("IDLE");
            animator.ResetTrigger("WALK");
            animator.ResetTrigger("RUN");
            animator.ResetTrigger("JUMP");
            animator.ResetTrigger("FALL");

            //setup
            hasSpawnedArrow = false;

            //set attack direction
            if (controllerState.Buttons.X == ButtonState.Pressed)
            {
                ChangeState(animationState.STATE_ATTACK_LIGHT);
            }
            else if (controllerState.Buttons.B == ButtonState.Pressed)
            {
                ChangeState(animationState.STATE_ATTACK_HEAVY);
            }
            else if (controllerState.Buttons.Y == ButtonState.Pressed)
            {
                ChangeState(animationState.STATE_ATTACK_SPECIAL);

                hasSpawnedArrow = true;
                if (savedThumbState.x < 0.3f && savedThumbState.x > -0.3f &&
                    savedThumbState.y < 0.3f && savedThumbState.y > -0.3f)
                {
                    if (transform.localEulerAngles.y < 90)
                        SpawnArrow(new Vector2(1, 0), nextAttackIsSpecial);
                    else
                        SpawnArrow(new Vector2(-1, 0), nextAttackIsSpecial);
                }
                else SpawnArrow(savedThumbState, nextAttackIsSpecial);
            }

            //setup
            startAttack = false;
            isAttacking = true;
            attackTimer = 1;

            //set up stuff
            savedHeavyAttack = (controllerState.Buttons.B == ButtonState.Pressed);
            savedThumbState.x = controllerState.ThumbSticks.Left.X;
            savedThumbState.y = controllerState.ThumbSticks.Left.Y;
            savedTriggerState.x = controllerState.Triggers.Left;
            savedTriggerState.y = controllerState.Triggers.Right;
        }
        else
        {
            attackTimer = (1 - animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
        }

        if ((attackTimer <= 0 && attackTimer > -0.2f) && isAttacking)
        {
            isAttacking = false;
            attackTimer = 0;
            //return;
        }
        else
        {
            animator.ResetTrigger("IDLE");
            animator.ResetTrigger("WALK");
            animator.ResetTrigger("RUN");
            animator.ResetTrigger("JUMP");
            animator.ResetTrigger("FALL");
        }

        //make sure the attack timer is fine
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.LightHit")
            || animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.HeavyHit")
            || animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.SpecialHit"))
        {
            //attackTimer = (1 - animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
            #region Create Arrow
            //spawn the arrow
            if (!hasSpawnedArrow && (attackTimer < 0.5f && attackTimer > 0.1f)
                && !nextAttackIsSpecial)
            {
                hasSpawnedArrow = true;

                if (savedThumbState.x < 0.3f && savedThumbState.x > -0.3f &&
                    savedThumbState.y < 0.3f && savedThumbState.y > -0.3f)
                {
                    if (transform.localEulerAngles.y < 90)
                        SpawnArrow(new Vector2(1, 0), nextAttackIsSpecial);
                    else
                        SpawnArrow(new Vector2(-1, 0), nextAttackIsSpecial);
                }

                else SpawnArrow(savedThumbState, nextAttackIsSpecial);

            }
            #endregion
        }
        //this partially stops animation locking
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Idle")
                 || animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Walk")
                 || animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Run"))
        {
            attackTimer = 1;
            isAttacking = true;
        }
    }

    //spawns an arrow with the stats based on playercontrols fields, 
    void SpawnArrow(Vector2 arrowDir, bool special)
    {
        int newArrowDamage = savedHeavyAttack ? heavyArrowDamage : arrowDamage;
        GameObject newArrow;
        if (special)
        {
            newArrow = Instantiate(specialArrow);
            newArrow.GetComponent<ArrowMovement>().SpecialScript = specialAttackScript;
            GameObject specialGlow = (GameObject)Instantiate(specialGlowPrefab);
            specialGlow.transform.position = this.transform.position;
            specialGlow.transform.Translate(0, GetComponent<BoxCollider>().size.y * 0.5f, 0, 0);
            specialGlow.transform.parent = this.transform;
            specialGlow.GetComponent<SpecialGlowScript>().TurnOn(GetAnimationClip("Special").length);
        }
        else
            newArrow = savedHeavyAttack ? Instantiate(heavyArrow) : Instantiate(arrow);

        newArrow.transform.position = arrowSpawner.transform.position;
        newArrow.GetComponent<ArrowMovement>().SetVars(arrowDir, arrowSpeed, arrowHitTime, enemy, newArrowDamage);
        newArrow.GetComponent<ArrowMovement>().heavy = savedHeavyAttack;

        //reset special
        if (nextAttackIsSpecial)
            nextAttackIsSpecial = false;
    }

    //zeroes the players velocity
    public void Freeze()
    {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    bool CheckCounterFreeze()
    {
        if (freezeTimer > 0)
        {
            freezeTimer -= Time.deltaTime;
            animator.enabled = false;
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            counterDir.x = controllerState.ThumbSticks.Right.X;
            counterDir.y = controllerState.ThumbSticks.Right.Y;
            return true;
        }

        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        animator.enabled = true;
        return false;
    }

    public void CounterFreeze()
    {
        //start of the freeze
        freezeTimer = freezeTimeWhenCountered;
    }

    void CheckTrail()
    {
        if (currentAnimationState == animationState.STATE_ATTACK_LIGHT
            || currentAnimationState == animationState.STATE_ATTACK_HEAVY
            || currentAnimationState == animationState.STATE_ATTACK_SPECIAL)
        {
            trailRenderer.GetComponent<TrailRenderer>().enabled = true;
        }
        else trailRenderer.GetComponent<TrailRenderer>().enabled = false;
    }
}