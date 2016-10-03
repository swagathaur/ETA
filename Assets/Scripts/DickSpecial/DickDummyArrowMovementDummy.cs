using UnityEngine;
using System.Collections;

public class DickDummyArrowMovementDummy : ArrowMovement
{
    
	// Use this for initialization
	void Start () {
	
	}

    override public void SetVars(Vector2 direction, float speed, float deathTimer, GameObject Enemy, int damage)
    {
        UnityEditor.EditorApplication.isPaused = true;
        SpecialScript.RunAttack(Enemy.GetComponent<PlayerControls>().playerIndex);
    }

    // Update is called once per frame
    void Update () {
	
	}
}
