using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtonActions : MonoBehaviour
{
    public void LoadTutorialScene()
    {
        SceneManager.LoadScene("tutorial scene");
    }

    public void LoadZombieScene()
    {
        SceneManager.LoadScene("Zombiespawnertest");
    }

    public void CloseGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
