using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsSceneController : MonoBehaviour
{
    [SerializeField] private string lobbySceneName = "Lobby";

    public void LoadLobby()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadLobbyScene();
            return;
        }

        SceneManager.LoadScene(lobbySceneName);
    }
}
