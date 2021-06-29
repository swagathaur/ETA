using UnityEngine;
using System.Collections;
using XInputDotNetPure;

public class Controls : MonoBehaviour {

    [SerializeField] private AudioClip buttonSound;
    [SerializeField] private AudioClip pressSound;

    private bool open = false;

    public void Update()
    {
        if (open)
        {
            GetComponent<UnityEngine.UI.Button>().Select();

            GamePadState one = GamePad.GetState(PlayerIndex.One);
            GamePadState two = GamePad.GetState(PlayerIndex.Two);

            if (one.Buttons.B == ButtonState.Pressed || two.Buttons.B == ButtonState.Pressed
                || one.Buttons.X == ButtonState.Pressed || two.Buttons.X == ButtonState.Pressed
                || one.Buttons.Y == ButtonState.Pressed || two.Buttons.Y == ButtonState.Pressed
                || one.Buttons.Start == ButtonState.Pressed || two.Buttons.Start == ButtonState.Pressed
                || one.Buttons.Back == ButtonState.Pressed || two.Buttons.Back == ButtonState.Pressed)
            {
                GameObject.Find("Canvas").transform.Find("Controls").gameObject.SetActive(false);
                open = false;
            }
        }
    }

    public void Start()
    {
        GameObject.Find("Canvas").transform.Find("Controls").gameObject.SetActive(false);
    }

    public void Pressed()
    {
        GameObject.Find("Canvas").transform.Find("Controls").gameObject.SetActive(true);
        open = true;
    }
}
