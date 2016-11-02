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

            if (one.Buttons.B == ButtonState.Pressed || two.Buttons.B == ButtonState.Pressed)
            {
                GameObject.Find("Controls").gameObject.SetActive(false);
                open = false;
            }
        }
    }

    public void Start()
    {
        GameObject.Find("Controls").gameObject.SetActive(false);
    }

    public void Pressed()
    {
        GameObject.Find("Controls").gameObject.SetActive(true);
        open = true;
    }
}
