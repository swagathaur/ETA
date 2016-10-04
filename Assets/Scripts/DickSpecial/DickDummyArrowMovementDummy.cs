using UnityEngine;
using System.Collections;

public class DickDummyArrowMovementDummy : ArrowMovement
{
	// Use this for initialization
	void Start ()
    {
	}

    override public void SetVars(Vector2 direction, float speed, float deathTimer, GameObject Enemy, int damage)
    {
        DickDummyArrowMovementDummy[] dummys = FindObjectsOfType<DickDummyArrowMovementDummy>();
        if (dummys.Length > 1)
        {
            PlayerControls[] pcs = FindObjectsOfType<PlayerControls>();
            foreach (PlayerControls pc in pcs)
            {
                if (Enemy.GetComponent<PlayerControls>().playerIndex != pc.playerIndex)
                {
                    pc.special += pc.amountOfSpecialConsumed;
                    break;
                }
            }

            Destroy(this.gameObject);
        }
        else
        {
            SpecialScript.RunAttack(Enemy.GetComponent<PlayerControls>().playerIndex);
        }
    }

    // Update is called once per frame
    void Update () {
	
	}
}
