using UnityEngine;
using System.Collections;

public class AnarchySpecialGoalScript : MonoBehaviour {

    [HideInInspector]public GameObject YGoal;
    [HideInInspector]public GameObject XGoal;
    [HideInInspector]public GameObject BGoal;
    [HideInInspector]public GameObject AGoal;

    // Use this for initialization
    void Start () {
        YGoal = transform.Find("YGoal").gameObject;
        XGoal = transform.Find("XGoal").gameObject;
        BGoal = transform.Find("BGoal").gameObject;
        AGoal = transform.Find("AGoal").gameObject;
    }
	
    public void YToggle()
    {
        YGoal.GetComponent<Animator>().SetTrigger("Toggle");
    }
    public void XToggle()
    {
        XGoal.GetComponent<Animator>().SetTrigger("Toggle");
    }
    public void BToggle()
    {
        BGoal.GetComponent<Animator>().SetTrigger("Toggle");
    }
    public void AToggle()
    {
        AGoal.GetComponent<Animator>().SetTrigger("Toggle");
    }

    // Update is called once per frame
    void Update () {
	
	}
}
