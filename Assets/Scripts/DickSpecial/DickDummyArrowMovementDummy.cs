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
        DickDummyArrowMovementDummy[] derp = FindObjectsOfType<DickDummyArrowMovementDummy>();
        
        if (derp.Length > 1)
        {
            SpecialScript.RunAttack(Enemy.GetComponent<PlayerControls>().playerIndex);
            ((DickSpecial)SpecialScript).AttachDummy(this.gameObject);
        }
    }

    // Update is called once per frame
    void Update () {
	    
	}
}
