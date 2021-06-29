using UnityEngine;
using System.Collections;

public class RandomLevelSelectHolder : LevelSelectHolder {

	// Use this for initialization
	void Start ()
    {
        GameObject temp = FindObjectOfType<LevelSelecter>().levels[Random.Range(1, FindObjectOfType<LevelSelecter>().levels.Length)];
        level = temp.GetComponent<LevelSelectHolder>().level;
        levelAudio = temp.GetComponent<LevelSelectHolder>().levelAudio;
    }	
}
