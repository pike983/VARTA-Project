using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChangeScript : MonoBehaviour
{
    public string nextLevel;

    // Loads the next level when the player enters the trigger, this being the opened doorway.
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            // Load the next level.
            SceneManager.LoadScene(nextLevel);
        }
    }
}
