using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SpecialBarScript : MonoBehaviour {

    public PlayerControls player;

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        GetComponent<Image>().fillAmount = player.special / (float)player.maxSpecial;
	}
}
