using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TitleScreenManager : MonoBehaviour
{
    public string mainGameSceneName = "MainGame";

    public void StartGame()
    {
        SceneManager.LoadScene(mainGameSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
}