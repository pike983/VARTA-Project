using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuControls : MonoBehaviour
{
    public void LoadMenuScene()
    {
        ChangeScene("MainMenuScene");
    }

    public void LoadStoryScene()
    {
        ChangeScene("StoryModeScene");
    }

    public void LoadPracticeScene()
    {
        ChangeScene("MainLibraryScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void RotateAttachedObject180Y()
    {
        this.transform.Rotate(0.0f, 180.0f, 0.0f, Space.Self);
    }
}
