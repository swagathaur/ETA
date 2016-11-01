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

    bool canPause;

    PlayerIndex pausingPlayer;

    //menu
    private enum MenuNames
    {
        Resume = 0,
        Restart = 1,
        Quit = 2
    }
    private MenuNames selectedIndex = 0;

    private GameObject Pause;

    private GameObject pauseMenu;
    private GameObject resumeRed;
    private GameObject resumeYellow;
    private GameObject restartRed;
    private GameObject restartYellow;
    private GameObject exitRed;
    private GameObject exitYellow;

    // Use this for initialization
    void Start()
    {
        //init input
        state = new GamePadState[2];
        prevState = new GamePadState[2];

        //setup input for two players
        state = new GamePadState[2];
        prevState = new GamePadState[2];
        canPause = true;

        Pause = GameObject.Find("Pause");

        //get the things
        pauseMenu       = Pause.transform.Find("PauseMenu").gameObject;
        resumeRed       = Pause.transform.Find("ResumeRed").gameObject;
        resumeYellow    = Pause.transform.Find("ResumeYellow").gameObject;
        restartRed      = Pause.transform.Find("OptionsRed").gameObject;
        restartYellow   = Pause.transform.Find("OptionsYellow").gameObject;
        exitRed         = Pause.transform.Find("ExitRed").gameObject;
        exitYellow      = Pause.transform.Find("ExitYellow").gameObject;

        //disable pause stuff
        ShowMenu(false);
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
                selectedIndex = MenuNames.Resume;
            }
        }

        //menu options
        if (isPaused)
        {
            HighlightSelection();
            //up
            if ((prevState[(int)pausingPlayer].DPad.Up == ButtonState.Released
                && state[(int)pausingPlayer].DPad.Up == ButtonState.Pressed)
                || (prevState[(int)pausingPlayer].ThumbSticks.Left.Y < 0.4f
                && state[(int)pausingPlayer].ThumbSticks.Left.Y >= 0.4f))
            {
                if (selectedIndex == MenuNames.Resume)
                {
                    selectedIndex = MenuNames.Quit;
                }
                else
                {
                    selectedIndex--;
                    Mathf.Clamp((int)selectedIndex, 0, 2);
                }
            }
            //down
            else if ((prevState[(int)pausingPlayer].DPad.Down == ButtonState.Released
                && state[(int)pausingPlayer].DPad.Down == ButtonState.Pressed)
                || (prevState[(int)pausingPlayer].ThumbSticks.Left.Y > -0.4f
                && state[(int)pausingPlayer].ThumbSticks.Left.Y <= -0.4f))
            {
                if (selectedIndex == MenuNames.Quit)
                {
                    selectedIndex = MenuNames.Resume;
                }
                else
                {
                    selectedIndex++;
                    Mathf.Clamp((int)selectedIndex, 0, 2);
                }
            }

            if (prevState[(int)pausingPlayer].Buttons.A == ButtonState.Pressed
                && state[(int)pausingPlayer].Buttons.A == ButtonState.Released)
            {
                switch (selectedIndex)
                {
                    case MenuNames.Resume:
                        Toggle(pausingPlayer);
                        break;
                    case MenuNames.Restart:
                        Toggle(pausingPlayer);
                        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                        break;
                    case MenuNames.Quit:
                        Time.timeScale = 1;
                        FindObjectOfType<AudioScript>().StopSound();
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
        }
    }

    //highlight the currently selected menu item
    private void HighlightSelection()
    {
        //disable all yellows
        resumeYellow.SetActive(false);
        restartYellow.SetActive(false);
        exitYellow.SetActive(false);
        switch (selectedIndex)
        {
            case MenuNames.Resume:
                resumeYellow.SetActive(true);
                break;
            case MenuNames.Restart:
                restartYellow.SetActive(true);
                break;
            case MenuNames.Quit:
                exitYellow.SetActive(true);
                break;
        }
    }

    //toggle pause
    private void Toggle(PlayerIndex playerIndex)
    {
        bool started = gameObject.GetComponent<GameController>().started;
        bool ended = gameObject.GetComponent<GameController>().ended;
        if (started && !ended)
        {
            //pause
            if (!isPaused)
            {
                pausingPlayer = playerIndex;
                Time.timeScale = 0;
                selectedIndex = 0;
                isPaused = true;
                ShowMenu(true);

                GamePad.SetVibration(PlayerIndex.One, 0, 0);
                GamePad.SetVibration(PlayerIndex.Two, 0, 0);
            }
            //unpause
            else
            {
                Time.timeScale = 1;
                isPaused = false;
                ShowMenu(false);
            }
        }
    }

    public void Disable()
    {
        canPause = false;
    }

    //calls .SetActive(val) on all text
    private void ShowMenu(bool val)
    {
        if (canPause)
        {
            pauseMenu.SetActive(val);
            resumeRed.SetActive(val);
            restartRed.SetActive(val);
            exitRed.SetActive(val);

            if (!val)
            {
                resumeYellow.SetActive(false);
                restartYellow.SetActive(false);
                exitYellow.SetActive(false);
            }
        }
    }
}
