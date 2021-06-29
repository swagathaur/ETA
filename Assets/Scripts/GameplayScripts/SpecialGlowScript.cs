using UnityEngine;
using System.Collections;

public class SpecialGlowScript : MonoBehaviour
{

    float length, currTime;
    bool half, hasHitHalf;
    // Use this for initialization
    void Start()
    {
        half = false;
        hasHitHalf = false;
    }

    public void TurnOn(float length)
    {
        this.length = length;
        currTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (currTime < 0)
        {
            Destroy(this.gameObject);
        }

        half = currTime * 2 > length ? true : false;
        if (half)
            hasHitHalf = true;

        if (!hasHitHalf)
        {
            currTime += Time.deltaTime;
        }
        else
        {
            currTime -= Time.deltaTime;
        }

        float amount = currTime * 2 / length;

        gameObject.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, amount);
    }
}
