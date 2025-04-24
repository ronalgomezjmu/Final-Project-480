using UnityEngine;

public class VRCanvasFollowCamera : MonoBehaviour
{
    public Camera vrCamera;
    public float distance = 1f;
    public Vector3 offsetPosition = new Vector3(0, 0, 0);
    
    void Update()
    {
        if (vrCamera != null)
        {
            // Position the canvas in front of the camera
            transform.position = vrCamera.transform.position + vrCamera.transform.forward * distance;
            transform.position += vrCamera.transform.right * offsetPosition.x;
            transform.position += vrCamera.transform.up * offsetPosition.y;
            
            // Make the canvas face the camera
            transform.rotation = vrCamera.transform.rotation;
        }
    }
    
    void Start()
    {
        // Try to find the camera if not assigned
        if (vrCamera == null)
        {
            vrCamera = Camera.main;
        }
    }
}