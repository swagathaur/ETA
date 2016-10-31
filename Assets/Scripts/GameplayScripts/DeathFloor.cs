using UnityEngine;
using System.Collections;

public class DeathFloor : MonoBehaviour {

	void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {
            col.GetComponent<PlayerControls>().health = 0;
        }
    }
}
