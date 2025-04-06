using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    private float timeAlive = 0f;
    public float maxTimeAlive = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        timeAlive += Time.deltaTime;

        if (timeAlive >= maxTimeAlive)
        {
            Destroy(gameObject);
        }

    }
}
