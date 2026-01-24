using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    #region Singleton
    public static LevelManager Instance { get; private set; }
    #endregion

    #region Serialized Field
    public LevelData[] allLevels;
    #endregion

    #region Public Properties
    public LevelData CurrentLevel { get; private set; }
    #endregion

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    //====PUBLIC
    public void LoadLevel(LevelData levelData)
    {
        // check level data
        if(levelData == null)
        {
            Debug.LogError("Level Manager: LevelData is NULL");
            return;
        }

        // up to date current level
        CurrentLevel = levelData;

        // check scene name data = Scene's name
        if(string.IsNullOrEmpty(levelData.SceneName))
        {
            Debug.LogError($"LevelManager: Scene Name is empty in LevelData '{levelData.name}'");
        }
        SceneManager.LoadScene(levelData.SceneName);
    }
    public void RestartCurrentLevel()
    {
        if (CurrentLevel != null)
        {
            LoadLevel(CurrentLevel);
        }
        else
        {
            // Fallback: no data load current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void LoadNextLevel()
    {
        if (CurrentLevel == null)
        {
            return;
        }

        // get current level index
        int currentIndex = GetCurrentLevelIndex();

        // check level index
        if (currentIndex >= 0 && currentIndex < allLevels.Length - 1)
        {
            LoadLevel(allLevels[currentIndex + 1]);
        }
        else
        {
            Debug.Log("LevelManager: No more levels to load (Game Clear). Returning to Menu.");
            LoadMainMenu();
        }
    }
    public void LoadMainMenu()
    {
        // reset current level
        CurrentLevel = null;
        SceneManager.LoadScene("MainMenu");
    }

    
    public bool HasNextLevel() // check next level available
    {
        int index = GetCurrentLevelIndex();
        return index >= 0 && index < allLevels.Length - 1;
    }

    private int GetCurrentLevelIndex()
    {
        for (int i = 0; i < allLevels.Length; i++)
        {
            if (allLevels[i] == CurrentLevel)
            {
                return i;
            }
        }
        return -1;
    }
}
