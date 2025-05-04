using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    private Transform target;

    void Start()
    {
        if (Camera.main != null)
        {
            target = Camera.main.transform;
        }
        else
        {
            Debug.LogWarning("Main Camera not found!");
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        transform.LookAt(target);
        transform.Rotate(0, 180f, 0); // Flip so it faces the camera properly
    }
}
