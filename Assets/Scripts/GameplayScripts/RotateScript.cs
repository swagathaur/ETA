using UnityEngine;
using System.Collections;

public class RotateScript : MonoBehaviour {

	// Update is called once per frame
	void Update ()
    {
        if (transform.localRotation.y == 0)
            transform.Rotate(0, 90, 0);
        if (transform.localRotation.y == 180)
            transform.Rotate(0, 90, 0);
    }
}
