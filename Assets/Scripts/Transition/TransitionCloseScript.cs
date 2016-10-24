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

    public static string[] GetNamesToLoad()
    {
        GameObject p1Prefab = GameObject.Find("SELECTIONS").GetComponent<SelectionScript>().P1prefab;
        GameObject p2Prefab = GameObject.Find("SELECTIONS").GetComponent<SelectionScript>().P2prefab;

        string[] ret = new string[2];

        string p1Name = "";
        string p2Name = "";

        if (p1Prefab.name.Contains("Anarchy"))
            p1Name = "CaptainAnarchy";
        else if (p1Prefab.name.Contains("Richard"))
            p1Name = "RichardHardwood";
        else if (p1Prefab.name.Contains("Sam"))
            p1Name = "SAM";
        else { }
        //todo: ADAM
        if (p2Prefab.name.Contains("Anarchy"))
            p2Name = "CaptainAnarchy";
        else if (p2Prefab.name.Contains("Richard"))
            p2Name = "RichardHardwood";
        else if (p2Prefab.name.Contains("Sam"))
            p2Name = "SAM";
        else { }
        //todo: ADAM

        ret[0] = "Prefabs/Transition/VSName_" + p1Name + "Left";
        ret[1] = "Prefabs/Transition/VSName_" + p2Name + "Right";

        return ret;
    }

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

        string[] names = GetNamesToLoad();

        left.transform.FindChild("LeftText").GetComponent<Image>().sprite =
                Resources.Load<Sprite>(names[0]);

        right.transform.FindChild("RightText").GetComponent<Image>().sprite =
            Resources.Load<Sprite>(names[1]);
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
