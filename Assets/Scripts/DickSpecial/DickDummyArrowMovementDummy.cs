using UnityEngine;
using System.Collections;

public class DickDummyArrowMovementDummy : ArrowMovement
{
    // Use this for initialization
    public PlayerControls user;

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

        SpecialScript.RunAttack(Enemy.GetComponent<PlayerControls>().playerIndex);
    }

    public void Cleanup()
    {
        user.currentSpecialCooldown = -1;
    }
}