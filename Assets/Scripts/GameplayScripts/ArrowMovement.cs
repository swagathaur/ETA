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

    //variables for Growing on creation, and Clamping to a Max Size
    public float normScale;
    public float maxScale;
    public float growRate;

    public float freezeTimeWhenCountered = 0.3f;
    private float freezeTimer = 0;

    AudioScript audioSource;
    float Speed = 4;
    bool collided = false;
    bool canCounter = false;

    float speedGainWhenCountered = 1.3f;
    float sizeGainWhenCountered = 1.2f;
    float damageGainWhenCountered = 4f;

    public int damage;

    public float deathtime = 0.3f;
    public GameObject glow;

    private int numReflections;
    private bool blackened = true;

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
        ChangeGlow();
        if (CheckFreeze())
            return;
        Grow();
        if (!DoPrediction())
            Move();
        CheckCollision();
    }

    virtual public void SetVars(Vector2 direction, float speed, float deathTimer, GameObject Enemy, int damage)
    {
        this.direction = direction;
        this.Speed = speed;
        this.deathtime = deathTimer;
        this.target = Enemy;
        this.damage = damage;
        numReflections = 0;
        transform.FindChild("Glow").GetComponent<SpriteRenderer>().color = target.GetComponent<PlayerControls>().enemy.GetComponent<PlayerControls>().glowColor;

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
                blackened = false;
                target.GetComponent<PlayerControls>().DoCounter(true, heavy, damage);
                SwapDirection(target.GetComponent<PlayerControls>().counterDir);

                target = target.GetComponent<PlayerControls>().enemy;
                collided = false;
                audioSource.playSound(baseAudio.CLIP_COUNTER_PERFECT);

                PlayExplosion(sparkAnim, 0.58f, false);
                GetComponent<SpriteRenderer>().enabled = true;
            }
            else
            {
                target.GetComponent<PlayerControls>().DoCounter(false, heavy, damage);
                PlayExplosion(explosionAnim, 0.6f);
                Destroy(this.gameObject);
            }
        }
        if (canCounter)
        {
            bool counterSuccess = target.GetComponent<PlayerControls>().enemy.GetComponent<PlayerControls>().CheckCounterSuccess(this);
            if (counterSuccess)
            {
                target.GetComponent<PlayerControls>().enemy.GetComponent<PlayerControls>().DoCounter(true, heavy, damage, false);
                SwapDirection(target.GetComponent<PlayerControls>().enemy.GetComponent<PlayerControls>().counterDir);
                PlayExplosion(sparkAnim, 0.58f, false);
                canCounter = false;
                collided = false;
                audioSource.playSound(baseAudio.CLIP_COUNTER_PERFECT);
                //GetComponent<SpriteRenderer>().enabled = true;
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
        if (collided)
        {
            if (transform.lossyScale.x < 2)
                Camera.main.GetComponent<CameraPos>().ShakeTheCamera(0.1f, 0.3f, 0.1f);
            else if (transform.lossyScale.x < 16)
                Camera.main.GetComponent<CameraPos>().ShakeTheCamera(0.2f, 0.3f, 0.1f);
            else //> 16
                Camera.main.GetComponent<CameraPos>().ShakeTheCamera(0.4f, 0.3f, 0.1f);

            Speed *= speedGainWhenCountered;
            this.transform.localScale *= sizeGainWhenCountered;
            damage = Mathf.RoundToInt(damage + damageGainWhenCountered);
            GetComponent<Animator>().SetTrigger("Change");
        }
        freezeTimer = freezeTimeWhenCountered;
        if (collided)
            target.GetComponent<PlayerControls>().CounterFreeze();
        else if (canCounter)
            target.GetComponent<PlayerControls>().enemy.GetComponent<PlayerControls>().CounterFreeze();

        numReflections = 0;

        DoRotation();
    }

    void OnTriggerEnter(Collider coll)
    {
        Hit(coll);
    }
    void OnTriggerExit(Collider coll)
    {
        if (coll.tag == "Player")
            canCounter = false;
    }

    /*raycasts for physics prediction.
     * Returns true if a collision was found
     * Returns false if no collision was found
     */
    bool DoPrediction()
    {
        RaycastHit rayHitInfo;
        Ray ray = new Ray(new Vector3(transform.position.x + GetComponent<BoxCollider>().size.x * 0.5f * transform.right.x, transform.position.y), 
            transform.right);

        if (Physics.Raycast(ray, out rayHitInfo, Speed * Time.deltaTime * (heavy ? 0.5f : 1)))
        {
            if (rayHitInfo.collider.tag == "Terrain"
                || rayHitInfo.collider.tag == "Wall"
                || (rayHitInfo.collider.tag == "Player" && rayHitInfo.collider.gameObject == target))
            {
                //move the position by half the hitbox along the normal
                float xAmount = (rayHitInfo.normal.x / rayHitInfo.normal.magnitude) * (GetComponent<BoxCollider>().size.x * 0.25f);
                float yAmount = (rayHitInfo.normal.y / rayHitInfo.normal.magnitude) * (GetComponent<BoxCollider>().size.y * 0.25f);
                transform.position += new Vector3(xAmount, yAmount, 0);
                Hit(rayHitInfo.collider);
                return true;
            }
        }
        return false;
    }

    //hit something
    void Hit(Collider coll)
    {
        if (!collided)
        {
            if (coll.gameObject == target)
            {
                //does actually check counter
                collided = true;
                GetComponent<SpriteRenderer>().enabled = false;
                pointOfContact = transform.position + (target.transform.position - transform.position) * 0.1f;
            }
            else if (coll.gameObject.tag == "Player" && numReflections > 0)
            {
                canCounter = true;
                //GetComponent<SpriteRenderer>().enabled = false;
                pointOfContact = transform.position + (target.transform.position - transform.position) * 0.1f;
            }
            //reflections
            else if (coll.tag == "Terrain" || coll.tag == "Wall")
            {
                if (coll.tag == "Terrain" && numReflections < 2)
                {
                    //stop it colliding with ground on grow
                    Vector2 newDirection = Vector2.Reflect(direction, Vector3.up);
                    if (Vector2.Dot(newDirection, direction) == 1)
                    {
                        return;
                    }
                    direction = newDirection;
                }
                else if (coll.tag == "Wall" && numReflections < 2)
                    direction = Vector3.Reflect(direction, direction.x > 0 ? Vector3.left : Vector3.right);
                else if (numReflections >= 2)
                {
                    Destroy(this.gameObject);
                }
                DoRotation();
                numReflections++;
            }
        }
    }

    void PlayExplosion(GameObject animationPrefab, float length, bool useOffset = true)
    {
        Vector3 contact = pointOfContact;
        if (useOffset)
            contact += transform.right * 0.75f;
        else
            contact -= transform.right * 0.25f;
        Destroy(Instantiate(animationPrefab, contact, new Quaternion()), length);
    }

    bool CheckFreeze()
    {
        if (freezeTimer > 0)
        {
            freezeTimer -= Time.deltaTime;
            GetComponentInChildren<ParticleSystem>().enableEmission = false;

            if (target.GetComponent<PlayerControls>().enemy.GetComponent<PlayerControls>().counterDir.magnitude > 0.8f)
            {
                direction = target.GetComponent<PlayerControls>().enemy.GetComponent<PlayerControls>().counterDir;
                DoRotation();
            }
            return true;
        }

        if (SpecialScript == null)
            GetComponentInChildren<ParticleSystem>().enableEmission = true;
        return false;
    }

    private void ChangeGlow()
    {

        if (transform.FindChild("Glow").GetComponent<SpriteRenderer>().color != target.GetComponent<PlayerControls>().enemy.GetComponent<PlayerControls>().glowColor)
        {
            Color temp = transform.FindChild("Glow").GetComponent<SpriteRenderer>().color;
            if (!blackened)
            {
                temp.a -= Time.deltaTime * 12;

                if (temp.a <= 0)
                {
                    blackened = true;
                    temp = target.GetComponent<PlayerControls>().enemy.GetComponent<PlayerControls>().glowColor;
                    temp.a = 0;
                }
            }
            else
            {
                temp.a += Time.deltaTime * 12;
                if (temp.a >= 1)
                    temp.a = 1;
            }
            transform.FindChild("Glow").GetComponent<SpriteRenderer>().color = temp;
        }
    }
}
