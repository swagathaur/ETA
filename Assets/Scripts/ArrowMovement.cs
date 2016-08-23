using UnityEngine;
using System.Collections;
using System;

//order is important! Logic in SpawnArrow (PlayerController) relies on it
public enum ArrowDirState
{
    NOCOUNTER = 0,
    Up = 1,
    LeftUp = 2,
    RightUp = 3,
    Right = 4,
    Down = 5,
    LeftDown = 6,
    RightDown = 7,
    Left = 8,
    Countered = 9
};

public class ArrowMovement : MonoBehaviour
{
    public float speed;
    public Vector2 direction;
    public SpecialBase SpecialScript = null;
    public GameObject target;
    public bool heavy = false;

    private UnityEngine.GameObject activeShine;

    AudioScript audioSource;
    float Speed = 4;
    bool collided = false;

    float speedGainWhenCountered = 2f;
    float sizeGainWhenCountered = 1.2f;
    float damageGainWhenCountered = 1.5f;

    public int damage;

    public float deathtime = 0.3f;
    public GameObject shine;

    public int arrowID;

    public void SetVars(Vector2 direction, float speed, float deathTimer, GameObject Enemy, int arrowID, int damage)
    {
        this.direction = direction;
        this.Speed = speed;
        this.deathtime = deathTimer;
        this.target = Enemy;
        this.arrowID = arrowID;
        this.damage = damage;

        DoRotation();
    }

    public void DoRotation()
    {
        float angleDegs = Mathf.Rad2Deg * (Mathf.Atan2(direction.y, direction.x));

        transform.localEulerAngles = new Vector3(0, 0, angleDegs);
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
                target.GetComponent<PlayerControls>().DidCounter(true, heavy, arrowID, damage);
                if (counterTimer > target.GetComponent<PlayerControls>().timeToCounter * 0.8)
                {
                    target = target.GetComponent<PlayerControls>().enemy;
                    SwapDirection(target.GetComponent<PlayerControls>().counterDir);
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
            target.GetComponent<PlayerControls>().DidCounter(false, heavy, arrowID, damage);
            Destroy(activeShine);
            DestroyImmediate(this.gameObject);
        }
    }

    //todo: change function name to "Countered()"
    private void SwapDirection(Vector2 dir)
    {
        speed *= speedGainWhenCountered;
        this.transform.localScale *= sizeGainWhenCountered;
        damage = Mathf.RoundToInt(damage * damageGainWhenCountered);

        direction = dir;
        DoRotation();
    }

    void OnTriggerEnter(Collider coll)
    {
        if (!collided)
        {
            if (coll.gameObject == target)
            {
                collided = true;
                GetComponent<SpriteRenderer>().enabled = false;
                Vector3 shinePos;
                //Aidan don't bother re-writing i'll do it - Sean.
                //make the shine appear in the right spot
                //if (direction == ArrowDirState.Left || direction == ArrowDirState.LeftDown || direction == ArrowDirState.LeftUp)
                //    shinePos = transform.position + GetComponent<BoxCollider>().center;
                //else if (direction == ArrowDirState.Right || direction == ArrowDirState.RightDown || direction == ArrowDirState.RightUp)
                //    shinePos = new Vector3(transform.position.x + GetComponent<BoxCollider>().size.x, transform.position.y, transform.position.z);
                //else if (direction == ArrowDirState.Down)
                //    shinePos = new Vector3(transform.position.x + (GetComponent<BoxCollider>().size.x * 0.5f),
                //        transform.position.y - GetComponent<BoxCollider>().size.y, transform.position.z);
                //else
                shinePos = new Vector3(transform.position.x + (GetComponent<BoxCollider>().size.x * 0.5f),
                    transform.position.y + GetComponent<BoxCollider>().size.y, transform.position.z);

                activeShine = Instantiate(shine, shinePos, new Quaternion()) as UnityEngine.GameObject;
            }
            else if (coll.gameObject.tag.CompareTo("Terrain") == 1)
            {
                collided = true;
                Destroy(activeShine);
                DestroyImmediate(this.gameObject);
            }
        }
    }
}
