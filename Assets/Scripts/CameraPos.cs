using UnityEngine;
using System.Collections;

public class CameraPos : MonoBehaviour
{

    public GameObject P1;
    public GameObject P2;

    public Vector2 cameraBuffer = new Vector2(2, 6);
    public float yAdd;

    float minX;
    float maxX;
    float minY;
    float maxY;

    GameObject[] players;

    Vector3 lineSegment;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        CalculateBounds();
        CalculateCameraPosAndSize();
    }

    void CalculateBounds()
    {
        minX = Mathf.Infinity; maxX = -Mathf.Infinity; minY = Mathf.Infinity; maxY = -Mathf.Infinity;

        players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            Vector3 tempPlayer = player.transform.position;
            //X Bounds
            if (tempPlayer.x < minX)
                minX = tempPlayer.x;
            if (tempPlayer.x > maxX)
                maxX = tempPlayer.x;
            //Y Bounds
            if (tempPlayer.y < minY)
                minY = tempPlayer.y;
            if (tempPlayer.y > maxY)
                maxY = tempPlayer.y;
        }

    }

    void CalculateCameraPosAndSize()
    {
        Vector3 cameraCenter = Vector3.zero;
        Vector3 pos;

        foreach (GameObject player in players)
        {
            cameraCenter += player.transform.position;
        }
        Vector3 finalCameraCenter = cameraCenter / players.Length;

        //Rotates and Positions camera around a point
        finalCameraCenter.z = 0;
        finalCameraCenter.y = 3;
        pos = finalCameraCenter;
        pos -= transform.forward * 50;

        transform.position = pos;
        transform.LookAt(finalCameraCenter);

        //Size
        float sizeX = maxX - minX + cameraBuffer.x;
        float sizeY = maxY - minY + cameraBuffer.y;
        float camSize = (sizeX > sizeY ? sizeX : sizeY);
        Camera.main.orthographicSize = camSize * yAdd;

        pos.y += camSize * 0.5f; 
        transform.position = pos;
    }
}