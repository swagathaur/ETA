using UnityEngine;
using System.Collections;
using System;

public enum ArrowDirState
{
    NOCOUNTER = 0,
    RightUp = 1,
    Right,
    RightDown,
    Down,
    LeftDown,
    Left,
    LeftUp,
    Up
};

public class ArrowMovement : simpleMove
{

    float Speed = 4;
    public float deathtime = 0.3f;
    bool collided = false;

    public GameObject shine;

    public void SetVars(ArrowDirState direction, float speed, float deathTimer, GameObject Enemy)
    {
        this.direction = direction;
        this.Speed = speed;
        this.deathtime = deathTimer;
        this.target = Enemy;

        DoRotation();
    }

    public override void DoRotation()
    {
        switch (direction)
        {
            case ArrowDirState.RightUp:
                transform.localEulerAngles = new Vector3(0, 0, 45);
                break;
            case ArrowDirState.Right:
                transform.localEulerAngles = new Vector3(0, 0, 0);
                break;

            case ArrowDirState.RightDown:
                transform.localEulerAngles = new Vector3(0, 0, 315);
                break;

            case ArrowDirState.Down:
                transform.localEulerAngles = new Vector3(0, 0, 270);
                break;

            case ArrowDirState.LeftDown:
                transform.localEulerAngles = new Vector3(0, 0, 225);
                break;

            case ArrowDirState.Left:
                transform.localEulerAngles = new Vector3(0, 0, 180);
                break;

            case ArrowDirState.LeftUp:
                transform.localEulerAngles = new Vector3(0, 0, 135);
                break;

            case ArrowDirState.Up:
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
    }

    // Update is called once per frame
    void Update()
    {
        if (collided)
        {
            float counterTimer = target.GetComponent<PlayerControls>().CheckCounter(this);
            if (counterTimer != 0)
            {
                target.GetComponent<PlayerControls>().DidCounter(true, heavy);
                if (counterTimer > target.GetComponent<PlayerControls>().timeToCounter * 0.8)
                {
                    target = target.GetComponent<PlayerControls>().Enemy;
                    SwapDirection();
                    collided = false;
                    Debug.Log("perfect");
                    GetComponent<SpriteRenderer>().enabled = true;
                }
                else DestroyImmediate(this.gameObject);

            }
            deathtime -= Time.deltaTime;
        }

        if (!heavy)
            transform.Translate((Vector3.right) * Speed * Time.deltaTime);
        else
            transform.Translate((Vector3.right) * Speed * 0.5f * Time.deltaTime);

        if (deathtime < 0)
        {
            target.GetComponent<PlayerControls>().DidCounter(false, heavy);
            DestroyImmediate(this.gameObject);
        }
    }

    private void SwapDirection()
    {
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
            Instantiate(shine, transform.position, new Quaternion());
        }
    }
}
