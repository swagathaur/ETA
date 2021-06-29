using UnityEngine;
using System.Collections;

public class AdamArrowMovementDummy : ArrowMovement
{
    private PlayerControls user;

    [SerializeField]
    private Sprite sprite0;
    [SerializeField]
    private Sprite sprite1;
    [SerializeField]
    private Sprite sprite2;
    [SerializeField]
    private Sprite sprite3;
    [SerializeField]
    private Sprite sprite4;

    [SerializeField]
    private GameObject shadowPrefab;

    private bool isRunning = true;

    private float phaseTime = 0;
    private int currPhase = 0;

    [SerializeField]
    private float timeBetweenPhases = 0.08f;
    [SerializeField]
    private float numPhases = 5;
    [SerializeField]
    private float distanceToMove = 1.5f;

    private float dir;

    void Start()
    {

    }

    public override void SetVars(Vector2 direction, float speed, float deathTimer, GameObject Enemy, int damage)
    {
        base.isSpecial = true;
        PlayerControls[] pcs = FindObjectsOfType<PlayerControls>();
        foreach (PlayerControls pc in pcs)
        {
            if (Enemy.GetComponent<PlayerControls>().playerIndex != pc.playerIndex)
            {
                user = pc;
            }
        }

        isRunning = true;
        dir = direction.x;
    }
    // Update is called once per frame
    void Update()
    {
        if (isRunning)
        {
            user.Freeze();
            //phase transition
            if (phaseTime > timeBetweenPhases)
            {
                //increment the phasetime
                phaseTime = 0;
                currPhase++;

                //end
                if (currPhase >= numPhases)
                {
                    isRunning = false;
                    Cleanup();
                    return;
                }

                //do normal stuff
                //todo: instantiate a game object at user's current pos
                GameObject shadow = (GameObject)(Instantiate(shadowPrefab));
                shadow.transform.Translate(user.transform.position);
                if (dir > 0)
                    shadow.GetComponent<SpriteRenderer>().flipX = !shadow.GetComponent<SpriteRenderer>().flipX;
                Destroy(shadow, 0.5f);

                switch (currPhase)
                {
                    case 0: shadow.GetComponent<SpriteRenderer>().sprite = sprite0; break;
                    case 1: shadow.GetComponent<SpriteRenderer>().sprite = sprite1; break;
                    case 2: shadow.GetComponent<SpriteRenderer>().sprite = sprite2; break;
                    case 3: shadow.GetComponent<SpriteRenderer>().sprite = sprite3; break;
                    case 4: shadow.GetComponent<SpriteRenderer>().sprite = sprite4; break;
                }

                RaycastHit info = new RaycastHit();
                Debug.DrawRay(user.transform.position, new Vector3(dir, 0));

                float newDirection = user.transform.localEulerAngles.y < 90 ? 1 : -1;
                if (Physics.Raycast(user.transform.position, new Vector3(newDirection, 0), out info, distanceToMove + user.GetComponent<BoxCollider>().size.x))
                {
                    if (info.collider.tag == "Wall")
                    {
                        user.transform.position =
                            new Vector3((user.GetComponent<BoxCollider>().size.x * -newDirection) + info.collider.transform.position.x + 0.1f, user.transform.position.y, user.transform.position.z);
                        SpecialScript.RunAttack(user.enemy.GetComponent<PlayerControls>().playerIndex);
                        return;
                    }
                }
                user.transform.Translate(Vector3.right * newDirection * distanceToMove, Space.World);
                SpecialScript.RunAttack(user.enemy.GetComponent<PlayerControls>().playerIndex);
            }

            phaseTime += Time.deltaTime;
        }
    }

    void Cleanup()
    {
        Destroy(this.gameObject);
    }
}
