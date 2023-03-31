using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// This script is based off of the tutorial from Valem Tutorials found at the following link:
// https://www.youtube.com/watch?v=nowlPXuPEG8&list=PLpEoiloH-4eN0_53RMyv9p2oG2BB8uasX&index=25
public class XROffsetGrabInteractable : XRGrabInteractable
{
    // Start is called before the first frame update
    void Start()
    {
        // This sets the location that the object will be grabbed from.
        if (!attachTransform)
        {
            GameObject attachPoint = new GameObject("Offset Grab Pivot");
            attachPoint.transform.SetParent(transform, false);
            attachTransform = attachPoint.transform;
        }
        
    }

    // This function is called when the object is grabbed.
    // It tells the object to move with the hand.
    // The object will stay at it's current position relative to the hand.
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        attachTransform.position = args.interactorObject.transform.position;
        attachTransform.rotation = args.interactorObject.transform.rotation;
        
        base.OnSelectEntered(args);
    }
}
