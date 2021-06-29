using UnityEngine;
using System.Collections;

public class NOFABPLS : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        GameObject.FindGameObjectWithTag("Selections").GetComponent<SelectionScript>().P1prefab = null;
        GameObject.FindGameObjectWithTag("Selections").GetComponent<SelectionScript>().P2prefab = null;
        GameObject.FindGameObjectWithTag("Selections").GetComponent<SelectionScript>().loaded = false;
    }
}
