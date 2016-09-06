using UnityEngine;
using System.Collections;
using XInputDotNetPure;
using System;

public class CharacterSelecter : MonoBehaviour
{

    public PlayerIndex playerIndex;
    private GamePadState controllerState;
    private GamePadState prevControllerState;

    public short speed = 200;
    public short rotationSpeed = 90;

    public GameObject spawnPos;
    public LayerMask layer;

    private GameObject preview;
    private bool showing = false;
    private bool alternate = false;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        prevControllerState = controllerState;
        controllerState = GamePad.GetState(playerIndex);

        transform.position += new Vector3(controllerState.ThumbSticks.Left.X, controllerState.ThumbSticks.Left.Y, 0) * Time.deltaTime * speed;

        if (prevControllerState.Buttons.Y == ButtonState.Released && controllerState.Buttons.Y == ButtonState.Pressed)
        {
            alternate = !alternate;
            showing = false;
            Destroy(preview.gameObject);
        }

        DrawPreview();
    }

    private void DrawPreview()
    {
        RaycastHit hit;
        if (showing)
        {
            spawnPos.transform.Rotate(0, Time.deltaTime * rotationSpeed, 0);
            preview.transform.position = spawnPos.transform.position;
            preview.transform.rotation = spawnPos.transform.rotation;

            if (!Physics.Raycast(transform.position, Vector3.forward, out hit, 20, layer))
            {
                showing = false;
                Destroy(preview.gameObject);
            }
        }

        else
        {
            Debug.DrawRay(transform.position, Vector3.forward * 20);
            if (Physics.Raycast(transform.position, Vector3.forward, out hit, 20, layer))
            {
                if (hit.transform.tag == "CharacterButton")
                {
                    Debug.Log("Hit");
                    showing = true;
                    if (alternate)
                    {
                        preview = Instantiate(hit.transform.GetComponent<CharacterSelectHolder>().preview2);
                        preview.transform.position = spawnPos.transform.position;
                        preview.transform.rotation = spawnPos.transform.rotation;
                        preview.transform.localScale *= 2f;
                    }
                    else
                    {
                        preview = Instantiate(hit.transform.GetComponent<CharacterSelectHolder>().preview1);
                        preview.transform.position = spawnPos.transform.position;
                        preview.transform.rotation = spawnPos.transform.rotation;
                        preview.transform.localScale *= 2f;
                    }
                }
            }
        }
    }
}
