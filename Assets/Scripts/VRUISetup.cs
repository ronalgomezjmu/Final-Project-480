using UnityEngine;
using UnityEngine.XR;

public class VRUISetup : MonoBehaviour 
{
    void Start()
    {
        // Make sure Canvas renders on both eyes in VR
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            
            // Adjust scale for comfortable viewing
            transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        }
    }
}