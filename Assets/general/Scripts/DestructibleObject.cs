using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    public enum ObjectType { Good, Bad }
    public ObjectType type;

    public int pointsValue = 10;
    public int healthChange = 1;

   
    public void RecibirImpacto()
    {
        if (type == ObjectType.Good)
        {
            Debug.Log("¡Disparaste a un objeto BUENO! Pierdes vida.");
           
        }
        else
        {
            Debug.Log("¡Destruiste un objeto MALO! ¡Puntos sumados!");
            
        }

        Destroy(gameObject);
    }

    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.transform.parent != null)
        {
            Destroy(gameObject);
        }
    }
}