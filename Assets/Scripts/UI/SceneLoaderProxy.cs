using UnityEngine;

/// <summary>
/// SceneLoader가 DontDestroyOnLoad이기 때문에 버튼에 직접 연결하면
/// 씬 이동 후 참조가 끊어지는 문제를 해결하기 위한 프록시 스크립트.
/// 이 스크립트를 각 씬의 버튼에 연결하면 SceneLoader.Instance를 통해 호출합니다.
/// </summary>
public class SceneLoaderProxy : MonoBehaviour
{
    public void LoadLobbyScene()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadLobbyScene();
        }
        else
        {
            Debug.LogError("SceneLoader.Instance를 찾을 수 없습니다!");
        }
    }

    public void LoadClassicMode()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadClassicMode();
        }
        else
        {
            Debug.LogError("SceneLoader.Instance를 찾을 수 없습니다!");
        }
    }

    public void LoadAdventureMode()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadAdventureMode();
        }
        else
        {
            Debug.LogError("SceneLoader.Instance를 찾을 수 없습니다!");
        }
    }

    public void LoadCreditsScene()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadCreditsScene();
        }
        else
        {
            Debug.LogError("SceneLoader.Instance를 찾을 수 없습니다!");
        }
    }

    public void QuitGame()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.QuitGame();
        }
        else
        {
            Debug.LogError("SceneLoader.Instance를 찾을 수 없습니다!");
        }
    }
}
