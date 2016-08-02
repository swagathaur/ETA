using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BattleClick : MonoBehaviour {

    public void OnClick()
    {
        SceneManager.LoadScene("main");
        Time.timeScale = 1;
    }
}
