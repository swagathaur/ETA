using UnityEngine;
using System.Collections;

public class AnarchySpecialArrow : MonoBehaviour {

    [HideInInspector] public bool collided;
    [HideInInspector] public SpecialAnarchy.TriggerButtons button;
    [HideInInspector] public bool missed;

	// Use this for initialization
	void Start () {
        collided = false;
        missed = false;
	}

    void OnTriggerEnter(Collider c)
    {
        if (c.name == "Goal(Clone)")
            collided = true;
    }
    void OnTriggerExit(Collider c)
    {
        if (c.name == "Goal(Clone)" && collided)
        {
            collided = false;
            missed = true;
        }
    }
}
