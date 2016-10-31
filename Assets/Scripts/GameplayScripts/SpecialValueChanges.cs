using UnityEngine;
using System.Collections;

public class SpecialValueChanges : MonoBehaviour
{


    // Update is called once per frame
    void Update()
    {
        GetComponent<ArrowMovement>().numReflections = int.MaxValue;
    }
}
