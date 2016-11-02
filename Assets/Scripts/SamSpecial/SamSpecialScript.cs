using UnityEngine;
using System.Collections;
using XInputDotNetPure;

public class SamSpecialScript : SpecialBase
{

    private bool running = false;

    [SerializeField]
    private float length = 5;
    private float currentLength;

    // Use this for initialization
    void Start()
    {
        currentLength = length;
    }

    // Update is called once per frame
    void Update()
    {
        if (running)
        {
            GetComponent<PlayerControls>().enemy.GetComponent<PlayerControls>().Freeze();
            GetComponent<PlayerControls>().enemy.GetComponent<PlayerControls>().useGravity = false;

            currentLength -= Time.deltaTime;
            if (currentLength < 0)
            {
                GetComponent<PlayerControls>().enemy.GetComponent<PlayerControls>().useGravity = true;
                currentLength = 5;
                running = false;
            }
        }
    }

    public override void RunAttack(PlayerIndex otherPlayer)
    {
        running = true;
        currentLength = length;
        Destroy(Instantiate((GameObject)Resources.Load("Prefabs/Sam/JoshHug", typeof(GameObject)), 
            GetComponent<PlayerControls>().enemy.transform.position + new Vector3(0,0.5f,0), new Quaternion()), length);
    }
}
