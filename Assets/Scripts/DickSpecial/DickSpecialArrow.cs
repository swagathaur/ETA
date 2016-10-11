using UnityEngine;
using System.Collections;

public class DickSpecialArrow : MonoBehaviour {

    private DickSpecial caller;
    private GameObject enemy;
    private float speed;
    private short damage;

	// Use this for initialization
	void Start ()
    {
	    
	}

    public void SetVars(float speed, GameObject enemy, short damage, DickSpecial caller)
    {
        this.speed = speed;
        this.enemy = enemy;
        this.damage = damage;
        this.caller = caller;
    }
	
	//Update is called once per frame
	void Update ()
    {
        transform.position = transform.position + ((Vector3.down) * speed * Time.deltaTime);
    }

    //this manages player hit
    void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject == enemy)
        {
            if (!caller.hit)
            {
                coll.GetComponent<PlayerControls>().health -= damage;
                caller.hit = true;
            }
            Destroy(this.gameObject);
        }
        if (coll.tag == "Terrain")
            caller.Cleanup();
    }
    //this manages destruction
    void OnTriggerExit(Collider coll)
    {
        if (coll.tag == "Terrain")
        {
            caller.Cleanup();
        }
    }
}
