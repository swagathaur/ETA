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

    public void DoAnimations(bool aDown, bool bDown, bool xDown, bool yDown)
    {
        AToggle(aDown);
        BToggle(bDown);
        XToggle(xDown);
        YToggle(yDown);
    }
	
    private void YToggle(bool down)
    {
        YGoal.GetComponent<Animator>().SetBool("Down", down);
    }
    private void XToggle(bool down)
    {
        XGoal.GetComponent<Animator>().SetBool("Down", down);
    }
    private void BToggle(bool down)
    {
        BGoal.GetComponent<Animator>().SetBool("Down", down);
    }
    private void AToggle(bool down)
    {
        AGoal.GetComponent<Animator>().SetBool("Down", down);
    }

    // Update is called once per frame
    void Update () {
	
	}
}
