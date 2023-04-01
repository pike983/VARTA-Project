using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiDisplayScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // This code is taken from the Unity documentation.
        // https://docs.unity3d.com/Manual/MultiDisplay.html
        // Display.displays[0] is the primary, default display and is always ON, so start at index 1.
        // Check if additional displays are available and activate each.
        // Stop after the first display is activated.
        for (int i = 1; i < Display.displays.Length; i++)
        {
            if (i >= 2)
            {
                break;
            }
            Display.displays[i].Activate();
        }
        // Break after the first display is activated as only one display is needed to show the game
        // to external observers. This could be modified to allow for multiple displays to be used at
        // the user's discretion. This is not currently implemented.
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
