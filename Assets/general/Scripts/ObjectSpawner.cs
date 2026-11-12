using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Prefabs de Objetos")]
    public GameObject[] objectPrefabs; 

    [Header("Configuración de Spawn")]
    public float spawnInterval = 2.0f; // Tiempo entre cada objeto que cae
    public float spawnRangeX = 0.5f;   // Rango horizontal en el que aparecen
    public float spawnHeight = 1.0f;   // Altura desde donde caen

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnObject();
            timer = 0f;
        }
    }

    void SpawnObject()
    {
        if (objectPrefabs == null || objectPrefabs.Length == 0) return;

       
        int randomIndex = Random.Range(0, objectPrefabs.Length);
        GameObject selectedPrefab = objectPrefabs[randomIndex];

        
        Vector3 randomOffset = new Vector3(Random.Range(-spawnRangeX, spawnRangeX), spawnHeight, Random.Range(-spawnRangeX, spawnRangeX));
        Vector3 spawnPosition = transform.position + randomOffset;

     
        Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
    }
}