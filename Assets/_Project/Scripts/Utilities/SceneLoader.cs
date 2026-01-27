using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            // Reset time scale just in case coming from a paused state
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("SceneLoader: Scene name is empty!");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
