using UnityEngine;
using TMPro; 

public class GameManager : MonoBehaviour
{
    
    public static GameManager Instance;

    [Header("Configuración de Vidas")]
    public int vidasMaximas = 3;
    private int vidasActuales;

    [Header("Interfaz de Usuario (UI)")]
    public TextMeshProUGUI textoVidasUI; 

    void Awake()
    {
        
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        vidasActuales = vidasMaximas;
        ActualizarTextoVidas();
    }

   
    public void ModificarVidas(int cantidad)
    {
        vidasActuales += cantidad;

        
        if (vidasActuales < 0)
        {
            vidasActuales = 0;
        }

        
        if (vidasActuales > vidasMaximas)
        {
            vidasActuales = vidasMaximas;
        }

        ActualizarTextoVidas();

        
        if (vidasActuales <= 0)
        {
            GameOver();
        }
    }

    void ActualizarTextoVidas()
    {
        if (textoVidasUI != null)
        {
            textoVidasUI.text = "Vidas: " + vidasActuales;
        }
    }

    void GameOver()
    {
        Debug.Log("¡JUEGO TERMINADO! Te has quedado sin vidas.");
        
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}