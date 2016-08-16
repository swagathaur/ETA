using UnityEngine;
using System.Collections;
using System;

//order is important! Logic in SpawnArrow (PlayerController) relies on it
public enum ArrowDirState
{
    NOCOUNTER     = 0,
    Up            = 1,
    LeftUp        = 2,
    RightUp       = 3,
    Right         = 4,
    Down          = 5,
    LeftDown      = 6,
    RightDown     = 7,
    Left          = 8
};

public class ArrowMovement : simpleMove
{
    AudioScript audioSource;
    float Speed = 4;
    bool collided = false;

    float speedGainWhenCountered = 1.5f;
    float sizeGainWhenCountered = 1.5f;

    public float deathtime = 0.3f;
    public GameObject shine;

    public int arrowID;

    public void SetVars(ArrowDirState direction, float speed, float deathTimer, GameObject Enemy, int arrowID)
    {
        this.direction = direction;
        this.Speed = speed;
        this.deathtime = deathTimer;
        this.target = Enemy;
        this.arrowID = arrowID;

        DoRotation();
    }

    public override void DoRotation()
    {
        switch (direction)
        {
            case ArrowDirState.RightUp:
                GetComponent<SpriteRenderer>().flipY = false;
                transform.localEulerAngles = new Vector3(0, 0, 45);
                break;
            case ArrowDirState.Right:
                GetComponent<SpriteRenderer>().flipY = false;
                transform.localEulerAngles = new Vector3(0, 0, 0);
                break;

            case ArrowDirState.RightDown:
                GetComponent<SpriteRenderer>().flipY = false;
                transform.localEulerAngles = new Vector3(0, 0, 315);
                break;

            case ArrowDirState.Down:
                GetComponent<SpriteRenderer>().flipY = false;
                transform.localEulerAngles = new Vector3(0, 0, 270);
                break;

            case ArrowDirState.LeftDown:
                GetComponent<SpriteRenderer>().flipY = true;
                transform.localEulerAngles = new Vector3(0, 0, 225);
                break;

            case ArrowDirState.Left:
                GetComponent<SpriteRenderer>().flipY = true;
                transform.localEulerAngles = new Vector3(0, 0, 180);
                break;

            case ArrowDirState.LeftUp:
                GetComponent<SpriteRenderer>().flipY = true;
                transform.localEulerAngles = new Vector3(0, 0, 135);
                break;

            case ArrowDirState.Up:
                GetComponent<SpriteRenderer>().flipY = false;
                transform.localEulerAngles = new Vector3(0, 0, 90);
                break;
        }
    }

    // Use this for initialization
    void Start()
    {
        if (heavy)
        {
            GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 1);
        }

        audioSource = FindObjectOfType<AudioScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!heavy)
            transform.Translate((Vector3.right) * Speed * Time.deltaTime);
        else
            transform.Translate((Vector3.right) * Speed * 0.5f * Time.deltaTime);

        if (collided)
        {
            float counterTimer = target.GetComponent<PlayerControls>().CheckCounter(this);
            if (counterTimer != 0)
            {
                target.GetComponent<PlayerControls>().DidCounter(true, heavy, arrowID);
                if (counterTimer > target.GetComponent<PlayerControls>().timeToCounter * 0.8)
                {
                    target = target.GetComponent<PlayerControls>().enemy;
                    SwapDirection();
                    collided = false;
                    audioSource.playSound(baseAudio.CLIP_COUNTER_PERFECT);
                    GetComponent<SpriteRenderer>().enabled = true;
                }
                else
                {
                    audioSource.playSound(baseAudio.CLIP_COUNTER_NORMAL);
                    DestroyImmediate(this.gameObject);
                }

            }
            deathtime -= Time.deltaTime;
        }
        if (deathtime < 0)
        {
            target.GetComponent<PlayerControls>().DidCounter(false, heavy, arrowID);
            DestroyImmediate(this.gameObject);
        }
    }

    //todo: change function name to "Countered()"
    private void SwapDirection()
    {
        speed *= speedGainWhenCountered;
        this.transform.localScale *= sizeGainWhenCountered; 
        switch (direction)
        {
            case ArrowDirState.Left:
                direction = ArrowDirState.Right;
                break;
            case ArrowDirState.LeftUp:
                direction = ArrowDirState.RightUp;
                break;
            case ArrowDirState.LeftDown:
                direction = ArrowDirState.RightDown;
                break;
            case ArrowDirState.Right:
                direction = ArrowDirState.Left;
                break;
            case ArrowDirState.RightUp:
                direction = ArrowDirState.LeftUp;
                break;
            case ArrowDirState.RightDown:
                direction = ArrowDirState.LeftDown;
                break;
            case ArrowDirState.Up:
                direction = ArrowDirState.Down;
                break;
            case ArrowDirState.Down:
                direction = ArrowDirState.Up;
                break;
        }
        DoRotation();
    }

    void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject == target)
        {
            collided = true;
            GetComponent<SpriteRenderer>().enabled = false;
            Vector3 shinePos;
            //make the shine appear in the right spot
            if (direction == ArrowDirState.Left || direction == ArrowDirState.LeftDown || direction == ArrowDirState.LeftUp)
                shinePos = transform.position + GetComponent<BoxCollider>().center;
            else if (direction == ArrowDirState.Right || direction == ArrowDirState.RightDown || direction == ArrowDirState.RightUp)
                shinePos = new Vector3(transform.position.x + GetComponent<BoxCollider>().size.x, transform.position.y, transform.position.z);
            else if (direction == ArrowDirState.Down)
                shinePos = new Vector3(transform.position.x + (GetComponent<BoxCollider>().size.x * 0.5f), 
                    transform.position.y - GetComponent<BoxCollider>().size.y, transform.position.z);
            else 
                shinePos = new Vector3(transform.position.x + (GetComponent<BoxCollider>().size.x * 0.5f), 
                    transform.position.y + GetComponent<BoxCollider>().size.y, transform.position.z);

            Instantiate(shine, shinePos, new Quaternion());
        }
    }
}
