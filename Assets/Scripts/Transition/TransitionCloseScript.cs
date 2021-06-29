using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class TransitionCloseScript : MonoBehaviour
{
    private GameObject left;
    private GameObject right;

    private GameObject leftImage;
    private GameObject rightImage;

    private Vector3 leftStart;
    private Vector3 rightStart;
    private Vector3 leftClosed;
    private Vector3 rightClosed;

    private bool finalBounce = false;

    private bool started = false;
    private string levelToLoad = "";

    private float totalDuration = 2;
    private float currentDuration = 0;

    [SerializeField]
    private AudioClip VsSound;

    private bool hasPlayedSound = false;

    public static string[] GetNamesToLoad()
    {
        GameObject p1Prefab = GameObject.Find("SELECTIONS").GetComponent<SelectionScript>().P1prefab;
        GameObject p2Prefab = GameObject.Find("SELECTIONS").GetComponent<SelectionScript>().P2prefab;

        string[] ret = new string[4];

        string p1Name = "";
        string p1ImageName = "";
        string p2Name = "";
        string p2ImageName = "";

        if (p1Prefab.name.Contains("Anarchy"))
        {
            p1Name = "CaptainAnarchy";
            p1ImageName = "Anarchy";
        }
        else if (p1Prefab.name.Contains("Richard"))
        {
            p1Name = "RichardHardwood";
            p1ImageName = "Richard";
        }
        else if (p1Prefab.name.Contains("Sam"))
        {
            p1Name = "SAM";
            p1ImageName = "Sam";
        }
        else if (p1Prefab.name.Contains("Adam"))
        {
            p1Name = "TeacherAdam";
            p1ImageName = "Adam";
        }
        if (p2Prefab.name.Contains("Anarchy"))
        {
            p2Name = "CaptainAnarchy";
            p2ImageName = "Anarchy";
        }
        else if (p2Prefab.name.Contains("Richard"))
        {
            p2Name = "RichardHardwood";
            p2ImageName = "Richard";
        }
        else if (p2Prefab.name.Contains("Sam"))
        {
            p2Name = "SAM";
            p2ImageName = "Sam";
        }
        else if (p2Prefab.name.Contains("Adam"))
        {
            p2Name = "TeacherAdam";
            p2ImageName = "Adam";
        }

        ret[0] = "Prefabs/Transition/VSName_" + p1Name + "Left";
        ret[1] = "Prefabs/Transition/" + p1ImageName + "VsImage";
        ret[2] = "Prefabs/Transition/VSName_" + p2Name + "Right";
        ret[3] = "Prefabs/Transition/" + p2ImageName + "VsImage";
        return ret;
    }

    // Use this for initialization
    void Start()
    {
        //end point left = 0.099, -0.9
        //end point right = 0.15 -0.9
        left = transform.Find("Left").gameObject;
        right = transform.Find("Right").gameObject;

        leftClosed = new Vector3(0, 0, 0);
        rightClosed = new Vector3(0, 0, 0);
        leftStart = new Vector3(-7.19f, 0, 0);
        rightStart = new Vector3(7.19f, 0, 0);

        left.transform.position = new Vector3(-10000, -10000, 0);
        right.transform.position = new Vector3(-10000, -10000, 0);

        string[] names = GetNamesToLoad();

        left.transform.Find("LeftText").GetComponent<Image>().sprite =
            Resources.Load<Sprite>(names[0]);
        left.transform.Find("LeftImage").GetComponent<Image>().sprite =
            Resources.Load<Sprite>(names[1]);

        right.transform.Find("RightText").GetComponent<Image>().sprite =
            Resources.Load<Sprite>(names[2]);
        right.transform.Find("RightImage").GetComponent<Image>().sprite =
            Resources.Load<Sprite>(names[3]);
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
            if (!hasPlayedSound)
            {
                FindObjectOfType<AudioScript>().playSound(VsSound);
                hasPlayedSound = true;
            }
            currentDuration += Time.deltaTime;
            //first close
            if (currentDuration < 1)
            {
                left.transform.localPosition = Vector3.Lerp(leftStart, leftClosed, currentDuration + 0.1f);
                right.transform.localPosition = Vector3.Lerp(rightStart, rightClosed, currentDuration + 0.1f);
            }
/*            else if (currentDuration < 1.25)
            {
                left.transform.localPosition = Vector3.Lerp(leftClosed, leftStart, currentDuration - 1);
                right.transform.localPosition = Vector3.Lerp(rightClosed, rightStart, currentDuration - 1);
            }
            else if (currentDuration < 1.9f)
            {
                left.transform.localPosition = Vector3.Lerp(leftStart, leftClosed, currentDuration - 0.5f);
                right.transform.localPosition = Vector3.Lerp(rightStart, rightClosed, currentDuration - 0.5f);
            }*/
            else if (currentDuration >= 1)
            {
                left.transform.localPosition = leftClosed;
                right.transform.localPosition = rightClosed;
                //when done
                SceneManager.LoadScene(levelToLoad);
            }
        }
    }
}
