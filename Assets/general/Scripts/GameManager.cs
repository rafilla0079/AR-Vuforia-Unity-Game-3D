using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Configuración de Vidas")]
    public int vidasMaximas = 3;
    private int vidasActuales;

    [Header("Interfaz de Usuario (UI)")]
    public TextMeshProUGUI textoVidasUI;

    [Header("Game Over - Sangre 2D")]
    public Sprite[] spritesGotasSangre;
    public Canvas canvasObjetivo;
    [Min(1)]
    public int cantidadGotas = 45;
    public float duracionEmisionGotas = 1.2f;
    public float retrasoReinicio = 2.0f;

    [Header("Ajustes de Caída de las Gotas")]
    public Vector2 rangoTamanoGota = new Vector2(20f, 52f);
    public float velocidadInicialMin = 220f;
    public float velocidadInicialMax = 480f;
    public float gravedadCaida = 650f;
    public float margenHorizontal = 40f;
    public bool rotarSegunCaida = true;
    public bool oscurecerPantallaConSangre = true;
    public Color colorTintePantalla = new Color(0.35f, 0f, 0f, 0.45f);

    [Header("Audio Opcional")]
    public AudioClip sonidoGameOver;
    private AudioSource audioSource;

    private bool gameOverActivo = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && sonidoGameOver != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Start()
    {
        vidasActuales = vidasMaximas;
        gameOverActivo = false;
        ActualizarTextoVidas();
    }

    public void ModificarVidas(int cantidad)
    {
        if (gameOverActivo) return;

        vidasActuales = Mathf.Clamp(vidasActuales + cantidad, 0, vidasMaximas);
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
        if (gameOverActivo) return;
        gameOverActivo = true;

        Debug.Log("¡JUEGO TERMINADO! Te has quedado sin vidas.");

        if (sonidoGameOver != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoGameOver);
        }

        StartCoroutine(SecuenciaGameOverConSangre());
    }

    private IEnumerator SecuenciaGameOverConSangre()
    {
        Canvas canvas = ObtenerCanvas();
        if (canvas != null)
        {
            RectTransform rectCanvas = canvas.GetComponent<RectTransform>();

            if (oscurecerPantallaConSangre)
            {
                CrearOverlaySangre(rectCanvas);
            }

            if (TieneSpritesGotasValidos())
            {
                yield return StartCoroutine(EmitirGotasDeSangre(rectCanvas));
            }
        }

        float esperaRestante = Mathf.Max(0f, retrasoReinicio - duracionEmisionGotas);
        if (esperaRestante > 0f)
        {
            yield return new WaitForSecondsRealtime(esperaRestante);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private bool TieneSpritesGotasValidos()
    {
        if (spritesGotasSangre == null || spritesGotasSangre.Length == 0)
            return false;

        for (int i = 0; i < spritesGotasSangre.Length; i++)
        {
            if (spritesGotasSangre[i] != null)
                return true;
        }

        return false;
    }

    private Sprite ObtenerSpriteGotaAleatorio()
    {
        if (!TieneSpritesGotasValidos())
            return null;

        Sprite elegido = null;
        int intentos = 0;
        while (elegido == null && intentos < 20)
        {
            elegido = spritesGotasSangre[Random.Range(0, spritesGotasSangre.Length)];
            intentos++;
        }

        if (elegido == null)
        {
            for (int i = 0; i < spritesGotasSangre.Length; i++)
            {
                if (spritesGotasSangre[i] != null)
                    return spritesGotasSangre[i];
            }
        }

        return elegido;
    }

    private Canvas ObtenerCanvas()
    {
        if (canvasObjetivo != null) return canvasObjetivo;

        if (textoVidasUI != null)
        {
            Canvas canvasTexto = textoVidasUI.GetComponentInParent<Canvas>();
            if (canvasTexto != null) return canvasTexto;
        }

        return FindFirstObjectByType<Canvas>();
    }

    private void CrearOverlaySangre(RectTransform rectCanvas)
    {
        GameObject goOverlay = new GameObject("OverlaySangre_GameOver", typeof(RectTransform), typeof(Image));
        goOverlay.transform.SetParent(rectCanvas, false);
        goOverlay.transform.SetAsLastSibling();

        RectTransform rectOverlay = goOverlay.GetComponent<RectTransform>();
        rectOverlay.anchorMin = Vector2.zero;
        rectOverlay.anchorMax = Vector2.one;
        rectOverlay.offsetMin = Vector2.zero;
        rectOverlay.offsetMax = Vector2.zero;

        Image imgOverlay = goOverlay.GetComponent<Image>();
        imgOverlay.color = colorTintePantalla;
        imgOverlay.raycastTarget = false;
    }

    private IEnumerator EmitirGotasDeSangre(RectTransform rectCanvas)
    {
        float anchoCanvas = rectCanvas.rect.width;
        float altoCanvas = rectCanvas.rect.height;

        float limiteX = Mathf.Max(0f, (anchoCanvas * 0.5f) - margenHorizontal);
        float yInicial = (altoCanvas * 0.5f) + 60f;
        float yLimiteInferior = -(altoCanvas * 0.5f) - 80f;

        int total = Mathf.Max(1, cantidadGotas);
        float intervalo = duracionEmisionGotas / total;

        for (int i = 0; i < total; i++)
        {
            Sprite spriteElegido = ObtenerSpriteGotaAleatorio();
            if (spriteElegido != null)
            {
                float posX = Random.Range(-limiteX, limiteX);
                float tamano = Random.Range(rangoTamanoGota.x, rangoTamanoGota.y);
                float velInicial = Random.Range(velocidadInicialMin, velocidadInicialMax);

                StartCoroutine(AnimarGotaCayendo(
                    rectCanvas,
                    spriteElegido,
                    new Vector2(posX, yInicial),
                    yLimiteInferior,
                    tamano,
                    velInicial
                ));
            }

            if (intervalo > 0f)
            {
                yield return new WaitForSecondsRealtime(intervalo);
            }
        }
    }

    private IEnumerator AnimarGotaCayendo(
        RectTransform rectCanvas,
        Sprite spriteGota,
        Vector2 posicionInicial,
        float yLimiteInferior,
        float tamano,
        float velocidadInicial
    )
    {
        GameObject goGota = new GameObject("GotaSangre_2D", typeof(RectTransform), typeof(Image));
        goGota.transform.SetParent(rectCanvas, false);
        goGota.transform.SetAsLastSibling();

        RectTransform rectGota = goGota.GetComponent<RectTransform>();
        rectGota.anchoredPosition = posicionInicial;
        rectGota.sizeDelta = new Vector2(tamano * 0.7f, tamano);

        Image imgGota = goGota.GetComponent<Image>();
        imgGota.sprite = spriteGota;
        imgGota.raycastTarget = false;

        float velocidadActual = velocidadInicial;
        float derivaHorizontal = Random.Range(-18f, 18f);

        while (rectGota != null && rectGota.anchoredPosition.y > yLimiteInferior)
        {
            float dt = Time.unscaledDeltaTime;
            velocidadActual += gravedadCaida * dt;

            Vector2 nuevaPos = rectGota.anchoredPosition;
            nuevaPos.x += derivaHorizontal * dt;
            nuevaPos.y -= velocidadActual * dt;
            rectGota.anchoredPosition = nuevaPos;

            if (rotarSegunCaida)
            {
                float angulo = Mathf.Atan2(-velocidadActual, derivaHorizontal) * Mathf.Rad2Deg + 90f;
                rectGota.localRotation = Quaternion.Euler(0f, 0f, angulo);
            }

            yield return null;
        }

        if (goGota != null)
        {
            Destroy(goGota);
        }
    }
}