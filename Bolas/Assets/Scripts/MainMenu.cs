using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        Debug.Log("Play button pressed!");
        SceneManager.LoadScene("GameScene");
    }
}
