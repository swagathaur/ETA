using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class TransitionCloseScript : MonoBehaviour
{

    private GameObject left;
    private GameObject right;

    private Vector3 leftStart;
    private Vector3 rightStart;
    private Vector3 leftClosed;
    private Vector3 rightClosed;

    private bool finalBounce = false;

    private bool started = false;
    private string levelToLoad = "";

    private float totalDuration = 2;
    private float currentDuration = 0;

    // Use this for initialization
    void Start()
    {
        //end point left = 0.099, -0.9
        //end point right = 0.15 -0.9
        left = transform.FindChild("Left").gameObject;
        right = transform.FindChild("Right").gameObject;

        leftClosed = new Vector3(0, 0, 0);
        rightClosed = new Vector3(0, 0, 0);
        leftStart = new Vector3(-6.86f, 0, 0);
        rightStart = new Vector3(7.19f, 0, 0);

        left.transform.position = leftStart;
        right.transform.position = rightStart;

        //GameObject selection = GameObject.Find("SELECTIONS").GetComponent<>
        //
        //left.transform.FindChild("LeftText").GetComponent<Image>().sprite =
        //    Resources.Load<Sprite>("Prefabs\VSName_" + string + "Left");
        //right.transform.FindChild("RightText").GetComponent<Image>().sprite =
        //    Resources.Load<Sprite>("Prefabs\VSName_" + string + "Right");
    }

    public void Start(string levelToLoad)
    {
        started = true;
        this.levelToLoad = levelToLoad;
    }

    // Update is called once per frame
    void Update()
    {
        if (started)
        {
            currentDuration += Time.deltaTime;
            //first close
            if (currentDuration < 1)
            {
                left.transform.localPosition = Vector3.Lerp(leftStart, leftClosed, currentDuration + 0.1f);
                right.transform.localPosition = Vector3.Lerp(rightStart, rightClosed, currentDuration + 0.1f);
            }
            else if (currentDuration < 1.25)
            {
                left.transform.localPosition = Vector3.Lerp(leftClosed, leftStart, currentDuration - 1);
                right.transform.localPosition = Vector3.Lerp(rightClosed, rightStart, currentDuration - 1);
            }
            else if (currentDuration < 1.9f)
            {
                left.transform.localPosition = Vector3.Lerp(leftStart, leftClosed, currentDuration - 0.5f);
                right.transform.localPosition = Vector3.Lerp(rightStart, rightClosed, currentDuration - 0.5f);
            }
            else if (currentDuration >= 1.9f)
            {
                //when done
                SceneManager.LoadScene(levelToLoad);
            }
        }
    }
}
