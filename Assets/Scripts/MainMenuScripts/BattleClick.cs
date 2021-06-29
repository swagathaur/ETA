using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BattleClick : MonoBehaviour {

    public void OnClick()
    {
        SceneManager.LoadScene("Character Select");
        Time.timeScale = 1;
    }
}
