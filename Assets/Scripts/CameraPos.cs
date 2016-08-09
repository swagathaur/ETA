using UnityEngine;
using System.Collections;

public class CameraPos : MonoBehaviour
{

    public GameObject P1;
    public GameObject P2;

    public Vector2 cameraBuffer = new Vector2(2, 6);

    public float minSize = 2.5f;
    public float maxSize = 4;

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
        if (!P1.activeInHierarchy|| !P2.activeInHierarchy)
            return;

        CalculateBounds();
        CalculateCameraPosAndSize();
    }

    void CalculateBounds()
    {
        minX = Mathf.Infinity;
        maxX = -Mathf.Infinity;
        minY = Mathf.Infinity;
        maxY = -Mathf.Infinity;

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
        Vector2 center = new Vector2(minX, minY) + ((new Vector2(maxX, maxY) - new Vector2(minX, minY)) * 0.5f);
        Vector3 finalCameraCenter = new Vector3(center.x, center.y);
        Vector3 pos;

        //Rotates and Positions camera around a point
        finalCameraCenter.z = 0;
        pos = finalCameraCenter;
        pos -= transform.forward * 50;

        transform.position = pos;
        transform.LookAt(finalCameraCenter);

        //Size
        float sizeX = (maxX - minX) * 0.7f + cameraBuffer.x;
        float sizeY = maxY - minY + cameraBuffer.y;
        float camSize = (sizeX > sizeY ? sizeX : sizeY);

        
        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, camSize, 2 * Time.deltaTime);
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, minSize, maxSize);

        pos.y += Camera.main.orthographicSize * 0.5f; 
        transform.position = pos;
    }
}