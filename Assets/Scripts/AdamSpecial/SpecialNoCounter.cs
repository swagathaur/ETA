using UnityEngine;
using System.Collections;

public class SpecialNoCounter : MonoBehaviour
{
    GameObject target;
    
    void Start()
    {
        target = GetComponent<ArrowMovement>().target;
    }
    void Update()
    {

        if (GetComponent<ArrowMovement>().target != target)
        {
            Destroy(this.gameObject);
        }
    }    
}