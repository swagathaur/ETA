using UnityEngine;
using System.Collections;

public class SpriteFacing : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (transform.rotation.y > 89 && transform.rotation.y < 89)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
        else
            GetComponent<SpriteRenderer>().flipX = false;

        transform.LookAt(transform.position + Vector3.back);

    }
}
