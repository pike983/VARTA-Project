using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InterfaceController : MonoBehaviour
{
    public Canvas interfaceCanvas;

    public void OpenClose(InputAction.CallbackContext context)
    {
        if (interfaceCanvas != null)
        {
            interfaceCanvas.enabled = !interfaceCanvas.enabled;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
