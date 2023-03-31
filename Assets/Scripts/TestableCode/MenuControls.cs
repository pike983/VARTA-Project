using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuControls : MonoBehaviour
{
    // Loads the main menu scene.
    public void LoadMenuScene()
    {
        ChangeScene("MainMenuScene");
    }

    // Loads the game storymode first scene.
    public void LoadStoryScene()
    {
        ChangeScene("MainStory1");
    }

    // Loads the games practice mode scene.
    public void LoadPracticeScene()
    {
        ChangeScene("MainLibraryScene");
    }

    // Quits the game.
    public void QuitGame()
    {
        Application.Quit();
    }

    // Loads the scene with the given name.
    private void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Rotates the object this script is attached to.
    // Meant to show the options menu to the player, but it's not used.
    public void RotateAttachedObject180Y()
    {
        this.transform.Rotate(0.0f, 180.0f, 0.0f, Space.Self);
    }
}
