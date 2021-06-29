using UnityEngine;
using System.Collections;

public class cameraScale : MonoBehaviour {

	// Update is called once per frame
	void Update ()
    {
        Camera.main.aspect = 16f / 9f;
    }
}
