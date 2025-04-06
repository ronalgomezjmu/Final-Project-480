using UnityEngine;

public class DestroyOnCollision : MonoBehaviour
{
    public bool destroyWithAnyObject = false;
    public string collisionTag = "";
    void OnCollisionEnter(Collision collision)
    {
        if (destroyWithAnyObject || collision.gameObject.CompareTag(collisionTag))
        {
            Destroy(gameObject);
        }
    }

}
