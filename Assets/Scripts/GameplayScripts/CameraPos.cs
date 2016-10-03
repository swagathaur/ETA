using UnityEngine;
using System.Collections.Generic;


public class CameraPos : MonoBehaviour
{
    public Vector2 cameraBuffer = new Vector2(2, 6);

    public float minSize = 2.5f;
    public float maxSize = 4;
    public float lerpScale = 1;

    public float yAdd;

    float minX;
    float maxX;
    float minY;
    float maxY;

    private class Shake
    {
        public float duration;
        public float elapsed;
        public float intensity;

        public float shakeFrequency;
        public float lastShakeTime;
        public Vector3 lastShake;

        public void Update(float dt)
        {
            elapsed += dt;
        }
    }
    private List<Shake> shakes;

    GameObject[] players;

    Vector3 lineSegment;
    // Use this for initialization
    void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        shakes = new List<Shake>();
    }

    // Update is called once per frame
    void Update()
    {
        if (players.Length == 0)
        {
            players = GameObject.FindGameObjectsWithTag("Player");
            return;
        }

        //0.05 for low intensity attack
        if (Input.GetKeyDown(KeyCode.A))
        {
            ShakeTheCamera(1.25f, 0.3f, 0.1f);
        }

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
        //Size
        float sizeX = (maxX - minX) * 0.7f + cameraBuffer.x;
        float sizeY = maxY - minY + cameraBuffer.y;
        float camSize = (sizeX > sizeY ? sizeX : sizeY);

        Vector2 center = new Vector2(minX, minY) + ((new Vector2(maxX, maxY) - new Vector2(minX, minY)) * 0.5f);
        Vector3 finalCameraCenter = new Vector3(center.x, center.y + (yAdd * Camera.main.orthographicSize));
        Vector3 pos;

        //Rotates and Positions camera around a point
        finalCameraCenter.z = 0;
        pos = finalCameraCenter;
        pos -= transform.forward * 50;

        transform.position = pos;
        transform.LookAt(finalCameraCenter);

        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, camSize, lerpScale * Time.deltaTime);
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, minSize, maxSize);

        pos.y += Camera.main.orthographicSize * 0.5f;

        Vector3 totalShake = new Vector3();

        //calculate shakes
        for (var i = 0; i < shakes.Count; ++i)
        {
            shakes[i].Update(Time.deltaTime);

            if (shakes[i].elapsed >= shakes[i].duration)
            {
                shakes.RemoveAt(i);
                continue;
            }

            //do frequency
            if (shakes[i].shakeFrequency != 0)
            {
                if ((shakes[i].lastShakeTime - shakes[i].elapsed) < -shakes[i].shakeFrequency)
                {
                    shakes[i].lastShakeTime = shakes[i].elapsed;
                    float shakeX = Random.Range(0, shakes[i].intensity * 2) - shakes[i].intensity;
                    float shakeY = Random.Range(0, shakes[i].intensity * 2) - shakes[i].intensity;
                    shakes[i].lastShake = new Vector3(shakeX, shakeY);
                }
                totalShake += shakes[i].lastShake;
            }
            //shake every frame
            else
            {
                float shakeX = Random.Range(0, shakes[i].intensity * 2) - shakes[i].intensity;
                float shakeY = Random.Range(0, shakes[i].intensity * 2) - shakes[i].intensity;
                totalShake += new Vector3(shakeX, shakeY, 0);
            }
        }

        transform.position = pos += totalShake;
    }

    /*shakes the camera, 0 = nothing, 1 = 1 pixel left/right
     * duration is in seconds
     */
    public void ShakeTheCamera(float intensity, float duration, float shakeFrequency = 0)
    {
        Shake s = new Shake();
        s.duration = duration;
        s.intensity = intensity;
        s.elapsed = 0;
        s.shakeFrequency = shakeFrequency;
        s.lastShake = new Vector3();
        shakes.Add(s);
    }
}