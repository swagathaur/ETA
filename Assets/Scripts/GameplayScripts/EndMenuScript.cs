using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using XInputDotNetPure;

public class EndMenuScript : MonoBehaviour {

    bool running = false;

    private GamePadState[] state;
    private GamePadState[] prevState;

    public enum MenuNames
    {
        Rematch = 0,
        CharacterSelect = 1,
        MainMenu = 2
    }
    public MenuNames selectedIndex = 0;

    private GameObject endGameMenu;
    private GameObject rematch;
    private GameObject rematchSelected;
    private GameObject charSelect;
    private GameObject charSelectSelected;
    private GameObject mainMenu;
    private GameObject mainMenuSelected;

    private GameObject player1Wins;
    private GameObject player2Wins;

    private float timeToRematch = 0.75f;

    // Use this for initialization
    void Start ()
    {
        prevState = new GamePadState[2];
        state = new GamePadState[2];

        endGameMenu = GameObject.Find("EndGameMenu");

        rematch             = endGameMenu.transform.Find("Rematch").gameObject;
        rematchSelected     = endGameMenu.transform.Find("RematchSelected").gameObject;
        charSelect          = endGameMenu.transform.Find("CharSelect").gameObject;
        charSelectSelected  = endGameMenu.transform.Find("CharSelectSelected").gameObject;
        mainMenu            = endGameMenu.transform.Find("MainMenu").gameObject;
        mainMenuSelected    = endGameMenu.transform.Find("MainMenuSelected").gameObject;

        rematch.SetActive(false);
        charSelect.SetActive(false);
        mainMenu.SetActive(false);
    }
	
    public void Enable()
    {
        running = true;
    }

	// Update is called once per frame
	void Update ()
    {
	    if (running)
        {
            timeToRematch -= Time.deltaTime;
            for (int i = 0; i < 2; ++i)
            {
                prevState[i] = state[i];
                state[i] = GamePad.GetState((PlayerIndex)i);

                //up
                if ((prevState[i].DPad.Up == ButtonState.Released
                    && state[i].DPad.Up == ButtonState.Pressed)
                    || (prevState[i].ThumbSticks.Left.Y < 0.4f
                    && state[i].ThumbSticks.Left.Y >= 0.4f))
                {
                    if (selectedIndex == MenuNames.Rematch)
                    {
                        selectedIndex = MenuNames.MainMenu;
                    }
                    else
                    {
                        selectedIndex--;
                        selectedIndex = (MenuNames)Mathf.Clamp((int)selectedIndex, 0, 2);
                    }
                }

                //down
                else if ((prevState[i].DPad.Down == ButtonState.Released
                    && state[i].DPad.Down == ButtonState.Pressed)
                    || (prevState[i].ThumbSticks.Left.Y > -0.4f
                    && state[i].ThumbSticks.Left.Y <= -0.4f))
                {
                    if (selectedIndex == MenuNames.MainMenu)
                    {
                        selectedIndex = MenuNames.Rematch;
                    }
                    else
                    {
                        selectedIndex++;
                        selectedIndex = (MenuNames)Mathf.Clamp((int)selectedIndex, 0, 2);
                    }
                }

                if (timeToRematch <= 0)
                {
                    if (prevState[i].Buttons.A == ButtonState.Released
                    && state[i].Buttons.A == ButtonState.Pressed)
                    {
                        switch (selectedIndex)
                        {
                            case MenuNames.Rematch:
                                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                                break;
                            case MenuNames.CharacterSelect:
                                FindObjectOfType<AudioScript>().StopSound();
                                SceneManager.LoadScene("Character Select");

                                break;
                            case MenuNames.MainMenu:
                                GameObject.Find("SELECTIONS").GetComponent<WinCounter>().Clear();
                                FindObjectOfType<AudioScript>().StopSound();
                                SceneManager.LoadScene("MainMenu");
                                break;
                        }
                    }
                }
            }

            HighlightSelected();
        }
	}

    private void HighlightSelected()
    {
        rematch.SetActive(true);
        charSelect.SetActive(true);
        mainMenu.SetActive(true);

        rematchSelected.SetActive(false);
        charSelectSelected.SetActive(false);
        mainMenuSelected.SetActive(false);

        switch(selectedIndex)
        {
            case MenuNames.Rematch:
                rematch.SetActive(false);
                rematchSelected.SetActive(true);
                break;
            case MenuNames.CharacterSelect:
                charSelect.SetActive(false);
                charSelectSelected.SetActive(true);
                break;
            case MenuNames.MainMenu:
                mainMenu.SetActive(false);
                mainMenuSelected.SetActive(true);
                break;
        }
    }
}
