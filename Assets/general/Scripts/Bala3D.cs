using UnityEngine;

public class Bullet3D : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 3f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        DestructibleObject target = collision.gameObject.GetComponent<DestructibleObject>();
        if (target != null)
        {
            target.RecibirImpacto();
        }

        Destroy(gameObject);
    }
}