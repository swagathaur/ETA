using UnityEngine;
using System.Collections;

public class EasyRotate : MonoBehaviour {
    [SerializeField]
    private float rotateSpeed = 2;

	// Update is called once per frame
	void Update ()
    {
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
	}
}
