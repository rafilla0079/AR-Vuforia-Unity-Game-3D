using UnityEngine;

public class Bullet3D : MonoBehaviour
{
    public float speed = 40f;
    public float lifetime = 2f; 

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    
}