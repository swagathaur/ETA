using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XInputDotNetPure;
using System;

public class CharacterSelecter : MonoBehaviour
{
    public GameObject selecter;
    public List<GameObject> colors;
    private short selectedIndex = 0;

    public PlayerIndex playerIndex;
    private GamePadState controllerState;
    private GamePadState prevControllerState;

    public short speed = 200;
    public short rotationSpeed = 90;

    public GameObject spawnPos;
    public LayerMask layer;

    private GameObject preview;
    private GameObject tempPrefab;
    private GameObject prefab;
    private SelectionScript SELECTION;

    private bool showing = false;
    private bool alternate = false;

    // Use this for initialization
    void Start()
    {
        SELECTION = GameObject.FindGameObjectWithTag("Selections").GetComponent<SelectionScript>();
    }

    // Update is called once per frame
    void Update()
    {
        prevControllerState = controllerState;
        controllerState = GamePad.GetState(playerIndex);

        if (prefab == null)
        {
            transform.position += new Vector3(controllerState.ThumbSticks.Left.X, controllerState.ThumbSticks.Left.Y, 0) * Time.deltaTime * speed;

            if (showing)
                CheckSelection();
        }
        else
        {
            if (prevControllerState.Buttons.B == ButtonState.Released && controllerState.Buttons.B == ButtonState.Pressed)
            {
                prefab = null;
                GetComponent<Animator>().SetTrigger("Change");
            }
        }
        DrawPreview();

        #region select color
        if (playerIndex == PlayerIndex.One)
            SELECTION.P1Color = colors[selectedIndex].GetComponent<SpriteRenderer>().color;
        else
            SELECTION.P2Color = colors[selectedIndex].GetComponent<SpriteRenderer>().color;

        if (prefab != null)
        {
            selecter.transform.position = colors[selectedIndex].transform.position;

            if ((controllerState.ThumbSticks.Left.Y > 0.6f || controllerState.ThumbSticks.Left.Y < -0.6f)
                && (prevControllerState.ThumbSticks.Left.Y < 0.6f && prevControllerState.ThumbSticks.Left.Y > -0.6f))
            {
                selectedIndex += 4;
            }
            if (controllerState.ThumbSticks.Left.X > 0.6f && prevControllerState.ThumbSticks.Left.X < 0.6f)
            {
                selectedIndex += 1;
            }
            if (controllerState.ThumbSticks.Left.X < -0.6f && prevControllerState.ThumbSticks.Left.X > -0.6f)
            {
                selectedIndex -= 1;
            }

            if (selectedIndex > 7)
                selectedIndex -= 8;

            if (selectedIndex < 0)
                selectedIndex += 8;
        }
        #endregion
    }

    private void CheckSelection()
    {
        if (prevControllerState.Buttons.Y == ButtonState.Released && controllerState.Buttons.Y == ButtonState.Pressed)
        {
            alternate = !alternate;
            showing = false;
            Destroy(preview.gameObject);
        }
        if (prevControllerState.Buttons.A == ButtonState.Released && controllerState.Buttons.A == ButtonState.Pressed)
        {
            Select();
        }
    }

    private void Select()
    {
        if (!(tempPrefab == SELECTION.P1prefab || tempPrefab == SELECTION.P2prefab))
        {
            GetComponent<Animator>().SetTrigger("Change");
            prefab = tempPrefab;
            if (playerIndex == PlayerIndex.One)
            {
                SELECTION.P1prefab = prefab;
            }
            else
            {
                SELECTION.P2prefab = prefab;
            }
        }
    }

    private bool DrawPreview()
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
                    showing = true;


                    if (alternate)
                    {
                        tempPrefab = (hit.transform.GetComponent<CharacterSelectHolder>().characterColor2);

                        if (playerIndex == PlayerIndex.One)
                        {
                            if (SELECTION.P2prefab == tempPrefab)
                            {
                                tempPrefab = (hit.transform.GetComponent<CharacterSelectHolder>().characterColor1);
                                preview = Instantiate(hit.transform.GetComponent<CharacterSelectHolder>().preview1);
                            }
                        }
                        else if (playerIndex == PlayerIndex.Two)
                        {
                            if (SELECTION.P1prefab == tempPrefab)
                            {
                                tempPrefab = (hit.transform.GetComponent<CharacterSelectHolder>().characterColor1);
                                preview = Instantiate(hit.transform.GetComponent<CharacterSelectHolder>().preview1);
                            }
                        }
                        else
                            preview = Instantiate(hit.transform.GetComponent<CharacterSelectHolder>().preview2);

                        preview.transform.position = spawnPos.transform.position;
                        preview.transform.rotation = spawnPos.transform.rotation;
                        preview.transform.localScale *= 2f;
                    }
                    else
                    {
                        tempPrefab = (hit.transform.GetComponent<CharacterSelectHolder>().characterColor1);

                        if (playerIndex == PlayerIndex.One && SELECTION.P2prefab == tempPrefab)
                        {
                            tempPrefab = (hit.transform.GetComponent<CharacterSelectHolder>().characterColor2);
                            preview = Instantiate(hit.transform.GetComponent<CharacterSelectHolder>().preview2);

                        }
                        else if (playerIndex == PlayerIndex.Two && SELECTION.P1prefab == tempPrefab)
                        {
                            tempPrefab = (hit.transform.GetComponent<CharacterSelectHolder>().characterColor2);
                            preview = Instantiate(hit.transform.GetComponent<CharacterSelectHolder>().preview2);
                        }
                        else
                            preview = Instantiate(hit.transform.GetComponent<CharacterSelectHolder>().preview1);

                        preview.transform.position = spawnPos.transform.position;
                        preview.transform.rotation = spawnPos.transform.rotation;
                        preview.transform.localScale *= 2f;
                    }
                    return true;
                }
            }
        }
        return false;
    }
}
