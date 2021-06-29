using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TransitionOpenScript : MonoBehaviour {

    private GameObject left;
    private GameObject right;

    private Vector3 leftOpen;
    private Vector3 rightOpen;
    private Vector3 leftClosed;
    private Vector3 rightClosed;

    public bool opening;
    public bool startPhase;
    public float currentOpeningTime;

    // Use this for initialization
	void Start ()
    {
        opening = true;

        left = transform.Find("Left").gameObject;
        right = transform.Find("Right").gameObject;

        leftClosed = new Vector3(0, 0, 0);
        rightClosed = new Vector3(0, 0, 0);
        left.transform.localPosition = leftClosed;
        right.transform.localPosition = rightClosed;

        leftOpen  = new Vector3(-7.19f, 0, 0);
        rightOpen = new Vector3(7.19f, 0, 0);

        startPhase = true;

        string[] names = TransitionCloseScript.GetNamesToLoad();

        left.transform.Find("LeftText").GetComponent<Image>().sprite =
            Resources.Load<Sprite>(names[0]);
        left.transform.Find("LeftImage").GetComponent<Image>().sprite =
            Resources.Load<Sprite>(names[1]);

        right.transform.Find("RightText").GetComponent<Image>().sprite =
            Resources.Load<Sprite>(names[2]);
        right.transform.Find("RightImage").GetComponent<Image>().sprite =
            Resources.Load<Sprite>(names[3]);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (startPhase)
        {
            Time.timeScale = 0;
            currentOpeningTime += Time.unscaledDeltaTime;
            if (currentOpeningTime > 3)
            {
                currentOpeningTime = 0;
                startPhase = false;
            }
        }
	    else if (opening)
        {
            Time.timeScale = 1;
            currentOpeningTime += Time.unscaledDeltaTime;
            left.transform.localPosition = Vector3.Lerp(leftClosed, leftOpen, currentOpeningTime - 0.1f);
            right.transform.localPosition = Vector3.Lerp(rightClosed, rightOpen, currentOpeningTime - 0.1f);
                
            if (currentOpeningTime > 2)
            {
                opening = false;
            }
        }

	}
}
