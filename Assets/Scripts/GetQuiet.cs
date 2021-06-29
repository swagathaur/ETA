using UnityEngine;
using System.Collections;

public class GetQuiet : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        GetComponent<AudioSource>().volume -= Time.deltaTime * 0.1f;
    }
}
