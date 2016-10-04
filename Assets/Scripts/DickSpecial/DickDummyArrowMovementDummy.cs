using UnityEngine;
using System.Collections;

public class DickDummyArrowMovementDummy : ArrowMovement
{
    [HideInInspector]public System.Guid id;
    
	// Use this for initialization
	void Start ()
    {
        id = System.Guid.NewGuid();
	}

    override public void SetVars(Vector2 direction, float speed, float deathTimer, GameObject Enemy, int damage)
    {
        SpecialScript.RunAttack(Enemy.GetComponent<PlayerControls>().playerIndex);
    }

    // Update is called once per frame
    void Update () {
	
	}
}
