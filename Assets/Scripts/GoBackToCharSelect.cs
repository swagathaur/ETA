using UnityEngine;
using System.Collections;
using XInputDotNetPure;

public class GoBackToCharSelect : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        GamePadState one = GamePad.GetState(PlayerIndex.One);
        GamePadState two = GamePad.GetState(PlayerIndex.Two);

        if (one.Buttons.B == ButtonState.Pressed || two.Buttons.B == ButtonState.Pressed)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Character Select");
        }
    }
}
