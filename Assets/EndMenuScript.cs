using UnityEngine;
using System.Collections;
using XInputDotNetPure;

public class EndMenuScript : MonoBehaviour {

    bool running = false;

    private GamePadState[] state;
    private GamePadState[] prevState;

    private enum MenuNames
    {
        Rematch = 0,
        CharacterSelect = 1,
        MainMenu = 2
    }
    private MenuNames selectedIndex = 0;

    private GameObject endGameMenu;
    private GameObject rematch;
    private GameObject rematchSelected;
    private GameObject charSelect;
    private GameObject charSelectSelected;
    private GameObject mainMenu;
    private GameObject mainMenuSelected;

    private GameObject player1Wins;
    private GameObject player2Wins;

    // Use this for initialization
    void Start ()
    {
        gameObject.SetActive(false);

        endGameMenu = GameObject.Find("EndGameMenu");

        rematch             = endGameMenu.transform.Find("Rematch").gameObject;
        rematchSelected     = endGameMenu.transform.Find("RematchSelected").gameObject;
        charSelect          = endGameMenu.transform.Find("CharSelect").gameObject;
        charSelectSelected  = endGameMenu.transform.Find("CharSelectSelected").gameObject;
        mainMenu            = endGameMenu.transform.Find("MainMenu").gameObject;
        mainMenu            = endGameMenu.transform.Find("MainMenuSelected").gameObject;
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
            for (int i = 0; i < 2; ++i)
            {
                prevState[i] = state[i];
                state[i] = GamePad.GetState((PlayerIndex)i);

                //down
                if ((prevState[i].DPad.Down == ButtonState.Released
                    && state[i].DPad.Down == ButtonState.Pressed)
                    || (prevState[i].ThumbSticks.Left.Y > -0.4f
                    && state[i].ThumbSticks.Left.Y <= -0.4f))
                {

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
