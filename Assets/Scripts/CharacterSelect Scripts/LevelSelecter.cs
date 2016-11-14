using UnityEngine;
using System.Collections;
using XInputDotNetPure;
using System;

public class LevelSelecter : MonoBehaviour {

    public GameObject[] levels;

    private short selectedIndex = 0;
    private GamePadState controllerState1;
    private GamePadState prevControllerState1;
    private GamePadState controllerState2;
    private GamePadState prevControllerState2;

    void Update()
    {

        prevControllerState1 = controllerState1;
        controllerState1 = GamePad.GetState(PlayerIndex.One);
        prevControllerState2 = controllerState2;
        controllerState2 = GamePad.GetState(PlayerIndex.Two);

        CheckInput(prevControllerState1, controllerState1);
        CheckInput(prevControllerState2, controllerState2);
    }

    private void CheckInput(GamePadState prevControllerState, GamePadState controllerState)
    {
        transform.position = levels[selectedIndex].transform.position;

        if ((controllerState.ThumbSticks.Left.Y > 0.6f || controllerState.ThumbSticks.Left.Y < -0.6f)
            && (prevControllerState.ThumbSticks.Left.Y < 0.6f && prevControllerState.ThumbSticks.Left.Y > -0.6f))
        {
            selectedIndex += 2;
        }
        if (controllerState.ThumbSticks.Left.X > 0.6f && prevControllerState.ThumbSticks.Left.X < 0.6f)
        {
            selectedIndex += 1;
        }
        if (controllerState.ThumbSticks.Left.X < -0.6f && prevControllerState.ThumbSticks.Left.X > -0.6f)
        {
            selectedIndex -= 1;
        }

        if (selectedIndex > levels.Length - 1)
            selectedIndex -= (short)levels.Length;

        if (selectedIndex < 0)
            selectedIndex += 2;

        if (controllerState.Buttons.A == ButtonState.Pressed && prevControllerState.Buttons.A == ButtonState.Released)
        {
            GameObject.Find("Transition").GetComponent<TransitionCloseScript>().Start(levels[selectedIndex].GetComponent<LevelSelectHolder>().level);
            FindObjectOfType<AudioScript>().levelAudio = levels[selectedIndex].GetComponent<LevelSelectHolder>().levelAudio;
        }
    }
}
