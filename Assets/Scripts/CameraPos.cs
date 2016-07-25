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

        Vector3 newPos = P1.transform.position + (lineSegment.normalized * (lineSegment).magnitude) * 0.5f;
        newPos.z = -Mathf.Clamp(lineSegment.magnitude * 0.7f, 15, 300);
        newPos.y = Mathf.Clamp(newPos.y, 3, 20);

        transform.position = newPos;
	}
}
