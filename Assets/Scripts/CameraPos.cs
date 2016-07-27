using UnityEngine;
using System.Collections;

public class CameraPos : MonoBehaviour {

    public GameObject P1;
    public GameObject P2;

    Vector3 lineSegment;
    // Use this for initialization
    void Start ()
    {
	   
	}
	
	// Update is called once per frame
	void Update ()
    {
        lineSegment = (P2.transform.position - P1.transform.position);

        Vector3 newPos = P1.transform.position + (lineSegment * 0.5f);

        GetComponent<Camera>().orthographicSize = Mathf.Clamp(lineSegment.magnitude * 0.8f, 3, 10);

        newPos.y += 2;
        transform.position = newPos - transform.forward * 10;
	}
}
