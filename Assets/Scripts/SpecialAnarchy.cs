using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpecialAnarchy : SpecialBase
{

    GameObject projectilePrefab;
    GameObject missPrefab;
    GameObject goodPrefab;
    GameObject greatPrefab;
    GameObject perfectPrefab;
    GameObject UI;

    List<GameObject> arrows;

    public float yClamp;

    float arrowSpeed = 5;
    float attackTimer = 10;

    bool running;

    public override void RunAttack(PlayerControls otherPlayer)
    {
        if (!running)
        {
            Instantiate(UI);
            running = true;
            otherPlayer.paused = true;
        }
        else
        {
            Vector3 spawnPos = new Vector3(0, otherPlayer.transform.position.y + 10, 0);

            //spawn random arrows with directions
            #region Arrow Switch Statement
            GameObject newArrow = (GameObject)Instantiate(projectilePrefab, spawnPos, new Quaternion());
            simpleMove arrow = newArrow.GetComponent<simpleMove>();
            switch (Random.Range(0, 3))
            {
                case 0:
                    {
                        spawnPos.x -= -3;
                        arrow.direction = ArrowDirState.Left;
                        arrow.DoRotation();
                        arrow.speed = arrowSpeed;
                    }
                    break;
                case 1:
                    {
                        spawnPos.x -= -1;
                        arrow.direction = ArrowDirState.Right;
                        arrow.DoRotation();
                        arrow.speed = arrowSpeed;
                    }
                    break;
                case 2:
                    {
                        spawnPos.x -= 1;
                        arrow.direction = ArrowDirState.Up;
                        arrow.DoRotation();
                        arrow.speed = arrowSpeed;
                    }
                    break;
                case 3:
                    {
                        spawnPos.x -= 3;
                        arrow.direction = ArrowDirState.Down;
                        arrow.DoRotation();
                        arrow.speed = arrowSpeed;
                    }
                    break;
            }
            arrows.Add(newArrow);
            #endregion

            foreach (GameObject i in arrows)
            {
                if (i.transform.position.y < yClamp && otherPlayer.CheckCounter(i.GetComponent<simpleMove>().direction))
                {
                    if (otherPlayer.CheckCounter(i.GetComponent<simpleMove>().direction))
                    {
                        if (yClamp - i.transform.position.y < 0.2f)
                        {
                            Instantiate(perfectPrefab, transform.position, new Quaternion());
                        }
                        else if (yClamp - i.transform.position.y < 0.5f)
                        {
                            Instantiate(greatPrefab, transform.position, new Quaternion());
                            otherPlayer.health -= 1;
                        }
                        else if (yClamp - i.transform.position.y < 1.5f)
                        {
                            Instantiate(goodPrefab, transform.position, new Quaternion());
                            otherPlayer.health -= 3;
                        }
                    }
                    else if (yClamp - i.transform.position.y >= 1.5f)
                    {
                        otherPlayer.health -= 5;
                        Instantiate(missPrefab, transform.position, new Quaternion());
                    }
                }
            }

        }
    }
}
