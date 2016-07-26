using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using XInputDotNetPure;
using UnityEngine.UI;

public class PauseScript : MonoBehaviour
{
    //input
    GamePadState[] state;
    GamePadState[] prevState;

    //pausing
    bool isPaused = false;
    bool canUnpause = false;

    PlayerIndex pausingPlayer;
    public GameObject pauseMenu;

    //menu
    private enum MenuNames
    {
        Resume = 0,
        Options = 1,
        Quit = 2
    }
    private MenuNames selectedIndex = 0;
    public GameObject resumeText;
    public GameObject optionsText;
    public GameObject quitText;

    // Use this for initialization
    void Start()
    {
        //disable pause stuff
        ShowText(false);

        //setup input for two players
        state = new GamePadState[2];
        prevState = new GamePadState[2];
    }

    // Update is called once per frame
    void Update()
    {
        //update states
        for (int i = 0; i < 2; ++i)
        {
            prevState[i] = state[i];
            state[i] = GamePad.GetState((PlayerIndex)i);

            //pause
            if (state[i].Buttons.Start == ButtonState.Pressed
                && prevState[i].Buttons.Start == ButtonState.Released)
            {
                Toggle((PlayerIndex)i);
            }
        }

        //menu options
        if (isPaused)
        {
            //up
            if ((prevState[(int)pausingPlayer].DPad.Up == ButtonState.Released
                && state[(int)pausingPlayer].DPad.Up == ButtonState.Pressed)
                || (prevState[(int)pausingPlayer].ThumbSticks.Left.Y < 0.4f
                && state[(int)pausingPlayer].ThumbSticks.Left.Y >= 0.4f))
            {
                selectedIndex--;
            }
            //down
            else if ((prevState[(int)pausingPlayer].DPad.Down == ButtonState.Released
                && state[(int)pausingPlayer].DPad.Down == ButtonState.Pressed)
                || (prevState[(int)pausingPlayer].ThumbSticks.Left.Y > -0.4f
                && state[(int)pausingPlayer].ThumbSticks.Left.Y <= -0.4f))
            {
                selectedIndex++;
            }

            if (prevState[(int)pausingPlayer].Buttons.A == ButtonState.Pressed
                && state[(int)pausingPlayer].Buttons.A == ButtonState.Released)
            {
                switch (selectedIndex)
                {
                    case MenuNames.Resume:
                        Toggle(pausingPlayer);
                        break;
                    case MenuNames.Options:
                        //todo
                        break;
                    case MenuNames.Quit:
                        Time.timeScale = 1;
                        SceneManager.LoadScene("MainMenu");
                        break;
                }
            }

            if (prevState[(int)pausingPlayer].Buttons.B == ButtonState.Pressed
                && state[(int)pausingPlayer].Buttons.B == ButtonState.Released)
            {
                Toggle(pausingPlayer);
                return;
            }
            selectedIndex = (MenuNames)Mathf.Clamp((int)selectedIndex, 0, 2);
            HighlightSelection();
        }
    }

    //highlight the currently selected menu item
    private void HighlightSelection()
    {
        resumeText.GetComponent<Text>().color = Color.blue;
        optionsText.GetComponent<Text>().color = Color.blue;
        quitText.GetComponent<Text>().color = Color.blue;

        switch (selectedIndex)
        {
            case MenuNames.Resume:
                resumeText.GetComponent<Text>().color = Color.red;
                break;
            case MenuNames.Options:
                optionsText.GetComponent<Text>().color = Color.red;
                break;
            case MenuNames.Quit:
                quitText.GetComponent<Text>().color = Color.red;
                break;
        }
    }

    //toggle pause
    private void Toggle(PlayerIndex playerIndex)
    {
        //pause
        if (!isPaused)
        {
            pausingPlayer = playerIndex;
            Time.timeScale = 0;
            isPaused = true;
            selectedIndex = 0;
            ShowText(true);
        }
        //unpause
        else
        {
            Time.timeScale = 1;
            isPaused = false;
            ShowText(false);
        }
    }

    //calls .SetActive(val) on all text
    private void ShowText(bool val)
    {
        pauseMenu.SetActive(val);
        resumeText.SetActive(val);
        optionsText.SetActive(val);
        quitText.SetActive(val);
    }
}
