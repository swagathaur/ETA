using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SmokeCloudScript : MonoBehaviour {

    [SerializeField]private Sprite[] frames;

    private short currentFrame;
    private float totalTime;
    private float elapsedTime;
    private float timePerFrame;

    // Use this for initialization
    void Start () {
        totalTime = 1;
        currentFrame = 0;
        elapsedTime = 0;
        timePerFrame = 1f / 6f;
	}
	
	// Update is called once per frame
	void Update () {
        elapsedTime += Time.deltaTime;
        if (elapsedTime > (currentFrame + 1) * timePerFrame)
        {
            currentFrame++;
            if (currentFrame > 5)
            {
                Destroy(gameObject);
                return;
            }
            GetComponent<Image>().sprite = frames[currentFrame];
        }
	}
}
