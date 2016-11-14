using UnityEngine;
using System.Collections;

public class MouseLock : MonoBehaviour {

	// Update is called once per frame
	void Start()
    {
	    Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
	}
}
