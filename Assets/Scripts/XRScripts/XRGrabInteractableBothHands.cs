using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


// This script is based off of the tutorial from Valem Tutorials found at the following link:
// https://www.youtube.com/watch?v=nowlPXuPEG8&list=PLpEoiloH-4eN0_53RMyv9p2oG2BB8uasX&index=25
public class XRGrabInteractableBothHands : XRGrabInteractable
{
    public Transform leftHandTransform;
    public Transform rightHandTransform;

    // This function is called when the object is grabbed.
    // It tells the object which hand is grabbing it.
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        if(args.interactorObject.transform.CompareTag("Left Hand"))
        {
            attachTransform = leftHandTransform;
        }
        else if (args.interactorObject.transform.CompareTag("Right Hand"))
        {
            attachTransform = rightHandTransform;
        }

        base.OnSelectEntered(args);
    }
}
