using System.Collections;
using System.Diagnostics;
using Meta.XR.Util;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class CheckKeyboard : MonoBehaviour
{
    // Start is called before the first frame update
    public OVRTrackedKeyboard trackedKeyboard;

    void Start()
    {

        UnityEngine.Debug.Log(trackedKeyboard.TrackingEnabled);

        
    }

    // Update is called once per frame
    void Update()
    {
        var text_val = trackedKeyboard.SystemKeyboardInfo.Name;
        UnityEngine.Debug.Log("System Name");
        UnityEngine.Debug.Log(text_val);
        var text_2 =
            ((bool)((trackedKeyboard.SystemKeyboardInfo.KeyboardFlags & OVRPlugin.TrackedKeyboardFlags.Connected) > 0))
            .ToString();
        UnityEngine.Debug.Log("Is keyboard Connected");
        UnityEngine.Debug.Log(text_2);
        
        var text_3 = trackedKeyboard.TrackingState.ToString();
        UnityEngine.Debug.Log("Current State");
        UnityEngine.Debug.Log(text_3);
        var text_4 = "Select " + trackedKeyboard.KeyboardQueryFlags.ToString() + " Keyboard";
        UnityEngine.Debug.Log(text_4);
        var text_5 = trackedKeyboard.KeyboardQueryFlags.ToString();
        UnityEngine.Debug.Log("Query Status");
        UnityEngine.Debug.Log(text_5);
        switch (trackedKeyboard.TrackingState)
        {
            case OVRTrackedKeyboard.TrackedKeyboardState.Uninitialized:
            case OVRTrackedKeyboard.TrackedKeyboardState.Error:
            case OVRTrackedKeyboard.TrackedKeyboardState.ErrorExtensionFailed:
            case OVRTrackedKeyboard.TrackedKeyboardState.StartedNotTracked:
            case OVRTrackedKeyboard.TrackedKeyboardState.Stale:
                UnityEngine.Debug.Log("It isn't working");
                break;
            default:
                UnityEngine.Debug.Log("It IS working");
                break;
        }

    }
}
