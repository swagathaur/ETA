using UnityEngine;
using XInputDotNetPure;

public class StopControllerVibration : MonoBehaviour {

	// Use this for initialization
	void Start () {
        //dirty, filthy, hacky.
        GamePad.SetVibration(PlayerIndex.One, 0, 0);
        GamePad.SetVibration(PlayerIndex.Two, 0, 0);
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
