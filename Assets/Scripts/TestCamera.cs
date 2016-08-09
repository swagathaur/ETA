using UnityEngine;
using System.Collections;

public class TestCamera : MonoBehaviour {

    bool plus = true;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (GetComponent<Camera>().orthographicSize > 4)
        {
            plus = !plus;
            GetComponent<Camera>().orthographicSize = 4;
        }
        if (GetComponent<Camera>().orthographicSize < 2.5f)
        {
            plus = !plus;
            GetComponent<Camera>().orthographicSize = 2.5f;
        }

        if (plus)
        {
            GetComponent<Camera>().orthographicSize += Time.deltaTime * 0.5f;
        }
        else
        {

            GetComponent<Camera>().orthographicSize -= Time.deltaTime * 0.5f;
        }

    }
}
