using UnityEngine;
using System.Collections;

public class AdamArrowMovementDummy : ArrowMovement {

    public PlayerControls user;
    private GameObject[] shadows;

    private bool isRunning = true;

    private float phaseTime = 0;
    private float timeBetweenPhases = 0.08f;
    private float currPhase = 0;
    private float numPhases = 5;
    private float distanceToMove = 5;

    private int dir;

    void Start()
    {

    }
    
    public override void SetVars(Vector2 direction, float speed, float deathTimer, GameObject Enemy, int damage)
    {
        base.isSpecial = true;
        DickDummyArrowMovementDummy[] dummys = FindObjectsOfType<DickDummyArrowMovementDummy>();
        var derp = dummys.Length;
        PlayerControls[] pcs = FindObjectsOfType<PlayerControls>();
        foreach (PlayerControls pc in pcs)
        {
            if (Enemy.GetComponent<PlayerControls>().playerIndex != pc.playerIndex)
            {
                user = pc;
            }
        }


        isRunning = true;
        dir = direction.x > 0 ? 1 : -1;
    }
    // Update is called once per frame
    void Update ()
    {
        if (isRunning)
        {
            //phase transition
            if (phaseTime > timeBetweenPhases)
            {
                //increment the phasetime
                phaseTime = 0;
                currPhase++;

                //end
                if (currPhase >= numPhases)
                {
                    SpecialScript.RunAttack(user.enemy.GetComponent<PlayerControls>().playerIndex);
                    isRunning = false;
                    Cleanup();
                    return;
                }

                //do normal stuff
                //todo: instantiate a game object at user's current pos
                user.transform.Translate(distanceToMove * dir, 0, 0);
            }

            phaseTime += Time.deltaTime;
        }
	}

    void Cleanup()
    {
        Destroy(this.gameObject);
    }
}
