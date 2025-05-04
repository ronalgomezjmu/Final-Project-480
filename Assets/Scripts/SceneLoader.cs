using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadZombieScene()
    {
        SceneManager.LoadScene("Zombiespawnertest");
    }
}
