using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

public class MenuCanvasController : MonoBehaviour
{
    [Header("Botones del Canvas (UI)")]
    public Button botonJugar;
    public Button botonSalir;

    [Header("Configuración de Escena")]
    public string nombreEscenaJuego = "Juego3D";
    public float retrasoAccion = 0.18f;

    [Header("Animación de Escala")]
    public float escalaHoverOPresion = 1.15f;
    public float escalaPulso = 0.92f;
    public float velocidadEscala = 12f;

    [Header("Efecto de Partículas Sprite al Pulsar")]
    public Sprite spriteParticulaClick;
    [Min(1)]
    public int cantidadParticulasClick = 10;
    public Vector2 tamanoParticulaClick = new Vector2(28f, 28f);
    public float velocidadParticulaClick = 220f;
    public float duracionParticulaClick = 0.45f;
    public bool rotarParticulasHaciaDireccion = true;

    [Header("Animación Bala Sprite 2D (Botón Jugar)")]
    public Sprite spriteBala2D;
    public Vector2 tamanoBala = new Vector2(120f, 36f);
    [Tooltip("Rotación")]
    public float rotacionBalaZ = 0f;
    [Tooltip("Altura relativa en pantalla (0 = abajo, 0.5 = centro, 1 = arriba).")]
    [Range(0f, 1f)]
    public float alturaRelativaPantalla = 0.5f;
    public float duracionCruceBala = 0.45f;
    public float margenLateral = 150f;
    public float retrasoTrasBala = 0.05f;
    public AudioClip sonidoBala;

    [Header("Audio Opcional")]
    public AudioClip sonidoClick;
    private AudioSource audioSource;

    private Vector3 escalaOriginalJugar;
    private Vector3 escalaOriginalSalir;
    private bool resaltadoJugar = false;
    private bool resaltadoSalir = false;
    private bool accionEnProceso = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (sonidoClick != null || sonidoBala != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (botonJugar != null)
        {
            escalaOriginalJugar = botonJugar.transform.localScale;
            ConfigurarEventosBoton(botonJugar, true);
            botonJugar.onClick.AddListener(OnClickJugar);
        }

        if (botonSalir != null)
        {
            escalaOriginalSalir = botonSalir.transform.localScale;
            ConfigurarEventosBoton(botonSalir, false);
            botonSalir.onClick.AddListener(OnClickSalir);
        }
    }

    void Update()
    {
        if (accionEnProceso) return;

        if (botonJugar != null)
        {
            AnimarEscala(botonJugar.transform, escalaOriginalJugar, resaltadoJugar);
        }

        if (botonSalir != null)
        {
            AnimarEscala(botonSalir.transform, escalaOriginalSalir, resaltadoSalir);
        }
    }

    private void ConfigurarEventosBoton(Button boton, bool esJugar)
    {
        EventTrigger trigger = boton.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = boton.gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry pointerEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        pointerEnter.callback.AddListener((_) => SetResaltado(esJugar, true));
        trigger.triggers.Add(pointerEnter);

        EventTrigger.Entry pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        pointerExit.callback.AddListener((_) => SetResaltado(esJugar, false));
        trigger.triggers.Add(pointerExit);

        EventTrigger.Entry pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        pointerDown.callback.AddListener((_) => SetResaltado(esJugar, true));
        trigger.triggers.Add(pointerDown);

        EventTrigger.Entry pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        pointerUp.callback.AddListener((_) => SetResaltado(esJugar, false));
        trigger.triggers.Add(pointerUp);
    }

    private void SetResaltado(bool esJugar, bool estado)
    {
        if (esJugar)
            resaltadoJugar = estado;
        else
            resaltadoSalir = estado;
    }

    private void AnimarEscala(Transform objetivo, Vector3 escalaBase, bool resaltado)
    {
        float multiplicador = resaltado ? escalaHoverOPresion : 1f;
        Vector3 escalaObjetivo = escalaBase * multiplicador;

        objetivo.localScale = Vector3.Lerp(
            objetivo.localScale,
            escalaObjetivo,
            Time.unscaledDeltaTime * velocidadEscala
        );
    }

    private void OnClickJugar()
    {
        if (accionEnProceso || botonJugar == null) return;

        StartCoroutine(SecuenciaJugarConBala());
    }

    private IEnumerator SecuenciaJugarConBala()
    {
        yield return StartCoroutine(EjecutarAccionBoton(botonJugar.transform, escalaOriginalJugar, null));

        yield return StartCoroutine(AnimarBalaCruzandoPantalla());

        if (retrasoTrasBala > 0f)
        {
            yield return new WaitForSecondsRealtime(retrasoTrasBala);
        }

        if (!string.IsNullOrEmpty(nombreEscenaJuego))
        {
            SceneManager.LoadScene(nombreEscenaJuego);
        }
    }

    private IEnumerator AnimarBalaCruzandoPantalla()
    {
        if (spriteBala2D == null)
        {
            yield break;
        }

        Canvas canvasPadre = GetComponentInParent<Canvas>();
        if (canvasPadre == null)
        {
            canvasPadre = FindObjectOfType<Canvas>();
        }
        if (canvasPadre == null)
        {
            yield break;
        }

        RectTransform rectCanvas = canvasPadre.GetComponent<RectTransform>();
        GameObject goBala = new GameObject("Bala2D_Transicion", typeof(RectTransform), typeof(Image));
        goBala.transform.SetParent(rectCanvas, false);

        Image balaInstancia = goBala.GetComponent<Image>();
        balaInstancia.sprite = spriteBala2D;
        balaInstancia.raycastTarget = false;

        RectTransform rectBala = balaInstancia.rectTransform;
        rectBala.SetAsLastSibling();
        rectBala.sizeDelta = tamanoBala;
        rectBala.localRotation = Quaternion.Euler(0f, 0f, rotacionBalaZ);

        float anchoCanvas = rectCanvas.rect.width;
        float altoCanvas = rectCanvas.rect.height;

        float posXInicio = -anchoCanvas * 0.5f - margenLateral;
        float posXFin = anchoCanvas * 0.5f + margenLateral;
        float posY = (alturaRelativaPantalla - 0.5f) * altoCanvas;

        Vector2 inicio = new Vector2(posXInicio, posY);
        Vector2 fin = new Vector2(posXFin, posY);
        rectBala.anchoredPosition = inicio;

        if (sonidoBala != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoBala);
        }

        float tiempo = 0f;
        float duracion = Mathf.Max(0.05f, duracionCruceBala);
        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tiempo / duracion);
            rectBala.anchoredPosition = Vector2.Lerp(inicio, fin, t);
            yield return null;
        }

        rectBala.anchoredPosition = fin;
        Destroy(goBala);
    }

    private void OnClickSalir()
    {
        if (accionEnProceso || botonSalir == null) return;

        StartCoroutine(EjecutarAccionBoton(botonSalir.transform, escalaOriginalSalir, SalirDelJuego));
    }

    private void EmitirParticulasEnBoton(Transform botonTransform)
    {
        if (spriteParticulaClick == null || botonTransform == null) return;

        Canvas canvasPadre = botonTransform.GetComponentInParent<Canvas>();
        if (canvasPadre == null)
        {
            canvasPadre = FindObjectOfType<Canvas>();
        }
        if (canvasPadre == null) return;

        RectTransform rectCanvas = canvasPadre.GetComponent<RectTransform>();
        RectTransform rectBoton = botonTransform as RectTransform;

        Vector2 puntoLocalEnCanvas;
        if (rectBoton != null)
        {
            Vector3 centroMundo = rectBoton.TransformPoint(rectBoton.rect.center);
            Vector2 puntoPantalla = RectTransformUtility.WorldToScreenPoint(
                canvasPadre.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvasPadre.worldCamera,
                centroMundo
            );
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectCanvas,
                puntoPantalla,
                canvasPadre.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvasPadre.worldCamera,
                out puntoLocalEnCanvas
            );
        }
        else
        {
            puntoLocalEnCanvas = Vector2.zero;
        }

        int total = Mathf.Max(1, cantidadParticulasClick);
        for (int i = 0; i < total; i++)
        {
            float anguloBase = (360f / total) * i;
            float angulo = (anguloBase + Random.Range(-18f, 18f)) * Mathf.Deg2Rad;
            Vector2 direccion = new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)).normalized;
            float velocidad = velocidadParticulaClick * Random.Range(0.75f, 1.25f);

            StartCoroutine(AnimarParticulaSprite(
                rectCanvas,
                puntoLocalEnCanvas,
                direccion,
                velocidad,
                duracionParticulaClick * Random.Range(0.8f, 1.15f)
            ));
        }
    }

    private IEnumerator AnimarParticulaSprite(
        RectTransform rectCanvas,
        Vector2 posicionInicial,
        Vector2 direccion,
        float velocidad,
        float duracion
    )
    {
        GameObject goParticula = new GameObject("ParticulaSprite_Click", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        goParticula.transform.SetParent(rectCanvas, false);

        RectTransform rectP = goParticula.GetComponent<RectTransform>();
        rectP.SetAsLastSibling();
        rectP.anchoredPosition = posicionInicial;
        rectP.sizeDelta = tamanoParticulaClick;

        if (rotarParticulasHaciaDireccion)
        {
            float anguloZ = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
            rectP.localRotation = Quaternion.Euler(0f, 0f, anguloZ);
        }

        Image imgP = goParticula.GetComponent<Image>();
        imgP.sprite = spriteParticulaClick;
        imgP.raycastTarget = false;

        CanvasGroup cg = goParticula.GetComponent<CanvasGroup>();
        cg.alpha = 1f;

        float tiempo = 0f;
        float duracionSegura = Mathf.Max(0.05f, duracion);
        Vector3 escalaInicial = rectP.localScale;

        while (tiempo < duracionSegura)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tiempo / duracionSegura);

            rectP.anchoredPosition += direccion * velocidad * Time.unscaledDeltaTime;
            rectP.localScale = Vector3.Lerp(escalaInicial, escalaInicial * 0.35f, t);
            cg.alpha = 1f - t;

            yield return null;
        }

        Destroy(goParticula);
    }

    private IEnumerator EjecutarAccionBoton(Transform tr, Vector3 escalaBase, System.Action accionFinal)
    {
        accionEnProceso = true;
        SetBotonesInteractuables(false);

        EmitirParticulasEnBoton(tr);

        if (sonidoClick != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoClick);
        }

        float duracionPaso = Mathf.Max(0.02f, retrasoAccion * 0.5f);

        float tiempo = 0f;
        Vector3 escalaInicial = tr.localScale;
        Vector3 escalaPulsada = escalaBase * escalaPulso;
        while (tiempo < duracionPaso)
        {
            tr.localScale = Vector3.Lerp(escalaInicial, escalaPulsada, tiempo / duracionPaso);
            tiempo += Time.unscaledDeltaTime;
            yield return null;
        }

        tiempo = 0f;
        Vector3 escalaRebote = escalaBase * escalaHoverOPresion;
        while (tiempo < duracionPaso)
        {
            tr.localScale = Vector3.Lerp(escalaPulsada, escalaRebote, tiempo / duracionPaso);
            tiempo += Time.unscaledDeltaTime;
            yield return null;
        }

        accionFinal?.Invoke();
    }

    private void SetBotonesInteractuables(bool interactuable)
    {
        if (botonJugar != null) botonJugar.interactable = interactuable;
        if (botonSalir != null) botonSalir.interactable = interactuable;
    }

    private void SalirDelJuego()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void OnDestroy()
    {
        if (botonJugar != null) botonJugar.onClick.RemoveListener(OnClickJugar);
        if (botonSalir != null) botonSalir.onClick.RemoveListener(OnClickSalir);
    }
}