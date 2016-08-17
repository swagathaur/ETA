using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BattleClick : MonoBehaviour {

    public void OnClick()
    {
        SceneManager.LoadScene("Anarchy Level");
        Time.timeScale = 1;
    }
}
