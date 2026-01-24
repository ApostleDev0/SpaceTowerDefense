using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenuController : MonoBehaviour
{
    public void StartNewGame()
    {
        // reset time
        Time.timeScale = 1f;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        // check before load level
        if (LevelManager.Instance != null && LevelManager.Instance.allLevels.Length > 0)
        {
            // Load first index level
            LevelManager.Instance.LoadLevel(LevelManager.Instance.allLevels[0]);
        }
        else
        {
            Debug.LogError("MainMenu: Level Not Found in LevelManager!");
        }
    }
    public void QuitGame()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
        Debug.Log("Quitting Game...");

        // handle quit game for Editor & Build
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
