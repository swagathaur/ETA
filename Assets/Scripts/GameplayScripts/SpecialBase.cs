using UnityEngine;
using System.Collections;
using XInputDotNetPure;

public abstract class SpecialBase : MonoBehaviour
{
    public abstract void RunAttack(PlayerIndex otherPlayer);
}
