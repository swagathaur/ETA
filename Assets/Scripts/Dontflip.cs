using UnityEngine;
using System.Collections;

public class Dontflip : MonoBehaviour {
    	
	// Update is called once per frame
	void Update () {

        GetComponent<SpriteRenderer>().flipX = true;
        if (transform.rotation.eulerAngles.z == 180)
        {
            GetComponent<SpriteRenderer>().flipY = true;
        }
        else
        {
            GetComponent<SpriteRenderer>().flipY = false;
        }
	}
}
