using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    public void LoadLevel()
    {
        TransitionsSceneManger.Get().LoadLevel();
    }

    public void LoadMenu()
    {
        TransitionsSceneManger.Get().LoadMenu();
    }
}