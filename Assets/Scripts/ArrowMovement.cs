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
    public Vector2 direction;
    public SpecialBase SpecialScript = null;
    public GameObject target;
    public GameObject explosionAnim;
    public GameObject sparkAnim;
    public bool heavy = false;

    private Vector3 pointOfContact;
    private UnityEngine.GameObject activeShine;

    //variables for Growing on creation, and Clamping to a Max Size
    public float normScale;
    public float maxScale;
    public float growRate;

    public float freezeTimeWhenCountered = 0.3f;
    private float freezeTimer = 0;

    AudioScript audioSource;
    float Speed = 4;
    bool collided = false;

    float speedGainWhenCountered = 1.3f;
    float sizeGainWhenCountered = 1.2f;
    float damageGainWhenCountered = 4f;

    public int damage;

    public float deathtime = 0.3f;
    public GameObject shine;

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
        if (CheckFreeze())
            return;
        Grow();
        Move();
        CheckCollision();
    }

    public void SetVars(Vector2 direction, float speed, float deathTimer, GameObject Enemy, int damage)
    {
        this.direction = direction;
        this.Speed = speed;
        this.deathtime = deathTimer;
        this.target = Enemy;
        this.damage = damage;

        DoRotation();
    }

    public void DoRotation()
    {
        float angleDegs = Mathf.Rad2Deg * (Mathf.Atan2(direction.y, direction.x));
        transform.localEulerAngles = new Vector3(0, 0, angleDegs);

        //flip the sprite if traveling left
        if (Vector3.Dot(direction, Vector3.left) > 0)
        {
            GetComponent<SpriteRenderer>().flipY = true;
        }
        else //necessary because of countering
        {
            GetComponent<SpriteRenderer>().flipY = false;
        }
    }

    private void Grow()
    {
        Vector3 currentScale = transform.localScale;
        if (currentScale.x < normScale)
        {
            currentScale += new Vector3(growRate, growRate, growRate) * Time.deltaTime;
        }
        else
        {
            currentScale.x = Mathf.Clamp(currentScale.x, normScale, maxScale);
            currentScale.y = Mathf.Clamp(currentScale.y, normScale, maxScale);
        }
        transform.localScale = new Vector3(currentScale.x, currentScale.y, 1);
    }

    private void CheckCollision()
    {
        if (collided)
        {
            bool counterSuccess = target.GetComponent<PlayerControls>().CheckCounterSuccess(this);
            if (counterSuccess)
            {
                target.GetComponent<PlayerControls>().DoCounter(true, heavy, damage);
                SwapDirection(target.GetComponent<PlayerControls>().counterDir);

                target = target.GetComponent<PlayerControls>().enemy;
                collided = false;
                audioSource.playSound(baseAudio.CLIP_COUNTER_PERFECT);

                PlayExplosion(sparkAnim, 0.3f);
                GetComponent<SpriteRenderer>().enabled = true;
            }
            else
            {
                target.GetComponent<PlayerControls>().DoCounter(false, heavy, damage);
                PlayExplosion(explosionAnim, 1);
                DestroyImmediate(this.gameObject);
            }
        }
    }

    private void Move()
    {
        if (!heavy && !collided)
            transform.Translate((Vector3.right) * Speed * Time.deltaTime);
        else if (!collided)
            transform.Translate((Vector3.right) * Speed * 0.5f * Time.deltaTime);
    }

    //todo: change function name to "Countered()"
    private void SwapDirection(Vector2 dir)
    {
        Speed *= speedGainWhenCountered;
        this.transform.localScale *= sizeGainWhenCountered;
        damage = Mathf.RoundToInt(damage + damageGainWhenCountered);

        freezeTimer = freezeTimeWhenCountered;
        target.GetComponent<PlayerControls>().CounterFreeze();

        GetComponent<Animator>().SetTrigger("Change");

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
                pointOfContact = transform.position + (target.transform.position - transform.position) * 0.1f;
            }
            else if (coll.gameObject.tag.CompareTo("Terrain") == 1)
            {
                collided = true;
                Destroy(activeShine);
                Destroy(this.gameObject);
            }
        }
    }

    void PlayExplosion(GameObject animationPrefab, float length)
    {
        Destroy(Instantiate(animationPrefab, pointOfContact, new Quaternion()), length);
    }

    bool CheckFreeze()
    {
        if (freezeTimer > 0)
        {
            freezeTimer -= Time.deltaTime;
            GetComponentInChildren<ParticleSystem>().enableEmission = false;
            return true;
        }

        if (SpecialScript == null)
            GetComponentInChildren<ParticleSystem>().enableEmission = true;
        return false;
    }
}
