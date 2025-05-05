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
}
