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

public class ArrowMovement : MonoBehaviour
{

    ArrowDirState direction;
    float Speed = 4;
    public float deathtime = 0.1f;
    bool collided = false;
    public GameObject target;
    public bool heavy = false;

    public SpecialBase SpecialScript = null;


    public void SetVars(ArrowDirState direction, float speed, float deathTimer, GameObject Enemy)
    {
        this.direction = direction;
        this.Speed = speed;
        this.deathtime = deathTimer;
        this.target = Enemy;

        DoRotation();
    }

    private void DoRotation()
    {
        switch (direction)
        {
            case ArrowDirState.RightUp:
                transform.Rotate(0, 0, 45);
                break;
            case ArrowDirState.Right:
                transform.Rotate(0, 0, 0);
                break;

            case ArrowDirState.RightDown:
                transform.Rotate(0, 0, 315);
                break;

            case ArrowDirState.Down:
                transform.Rotate(0, 0, 270);
                break;

            case ArrowDirState.LeftDown:
                transform.Rotate(0, 0, 225);
                break;

            case ArrowDirState.Left:
                transform.Rotate(0, 0, 180);
                break;

            case ArrowDirState.LeftUp:
                transform.Rotate(0, 0, 135);
                break;

            case ArrowDirState.Up:
                transform.Rotate(0, 0, 90);
                break;
        }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (heavy)
        {
            GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 1);
        }

        if (collided)
        {
            deathtime -= Time.deltaTime;
            if (target.GetComponent<PlayerControls>().CheckCounter(direction, heavy, SpecialScript))
            {
                target.GetComponent<PlayerControls>().DidCounter(true, heavy);
                DestroyImmediate(this.gameObject);
            }
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

    void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject == target)
        {
            collided = true;
        }
    }
}
