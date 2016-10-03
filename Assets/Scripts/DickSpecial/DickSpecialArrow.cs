using UnityEngine;
using System.Collections;

public class DickSpecialArrow : MonoBehaviour {

    private GameObject enemy;
    private float speed;
    private short damage;

	// Use this for initialization
	void Start ()
    {
	    
	}

    void SetVars(float speed, GameObject enemy, short damage)
    {
        this.speed = speed;
        this.enemy = enemy;
        this.damage = damage;
    }
	
	// Update is called once per frame
	void Update ()
    {
        transform.Translate((Vector3.down) * speed * Time.deltaTime);
    }

    //this manages player hit
    void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject == enemy)
        {
            coll.GetComponent<PlayerControls>().health -= damage;
            Destroy(this.gameObject);
        }
    }
    //this manages destruction
    void OnTriggerExit(Collider coll)
    {
        if (coll.tag == "Terrain")
        {
            Destroy(this.gameObject);
        }
    }
}
