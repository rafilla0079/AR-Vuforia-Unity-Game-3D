using UnityEngine;

public class FloatingText : MonoBehaviour
{
    void Start()
    {
        Destroy(gameObject, 1f); // Se destruye a 1 segundo
    }

    void Update()
    {
        transform.Translate(Vector3.up * Time.deltaTime * 2f); // Sube flotando
    }
}