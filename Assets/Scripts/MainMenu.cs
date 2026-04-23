using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneFader.Instance.FadeToScene("MainGame");
    }

    public void Pause()
    {
        Time.timeScale=0;
    }

    public void Resume()
    {
        Time.timeScale=1;
    }
}
