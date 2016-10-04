using UnityEngine;
using System.Collections;

public class DickDummyArrowMovementDummy : ArrowMovement
{
    
	// Use this for initialization
	void Start () {
	
	}

    override public void SetVars(Vector2 direction, float speed, float deathTimer, GameObject Enemy, int damage)
    {
        SpecialScript.RunAttack(Enemy.GetComponent<PlayerControls>().playerIndex);
        StartCoroutine(Cleanup());
    }

    IEnumerator Cleanup()
    {
        yield return new WaitForSeconds(10);
        ((DickSpecial)(SpecialScript)).Cleanup();
        Destroy(this.gameObject);
    }


    // Update is called once per frame
    void Update () {
	
	}
}
