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

    [Header("Efecto de Partículas al Pulsar")]
    public ParticleSystem particulasClick; 
    public float offsetZParticulas = -1f; 
    public bool instanciarPrefabSiEsPrefab = true;

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
        if (audioSource == null && sonidoClick != null)
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
            Time.deltaTime * velocidadEscala
        );
    }

    private void OnClickJugar()
    {
        if (accionEnProceso || botonJugar == null) return;

        StartCoroutine(EjecutarAccionBoton(botonJugar.transform, escalaOriginalJugar, () =>
        {
            if (!string.IsNullOrEmpty(nombreEscenaJuego))
            {
                SceneManager.LoadScene(nombreEscenaJuego);
            }
        }));
    }

    private void OnClickSalir()
    {
        if (accionEnProceso || botonSalir == null) return;

        StartCoroutine(EjecutarAccionBoton(botonSalir.transform, escalaOriginalSalir, SalirDelJuego));
    }

    private void EmitirParticulasEnBoton(Transform botonTransform)
    {
        if (particulasClick == null || botonTransform == null) return;

        Vector3 posicionSpawn = botonTransform.position + new Vector3(0f, 0f, offsetZParticulas);

        if (instanciarPrefabSiEsPrefab && !particulasClick.gameObject.scene.IsValid())
        {
            ParticleSystem instancia = Instantiate(particulasClick, posicionSpawn, Quaternion.identity);
            instancia.Play();
            float duracionTotal = instancia.main.duration + instancia.main.startLifetime.constantMax;
            Destroy(instancia.gameObject, Mathf.Max(0.5f, duracionTotal));
        }
        else
        {
            particulasClick.transform.position = posicionSpawn;
            particulasClick.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particulasClick.Play();
        }
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
            tiempo += Time.deltaTime;
            yield return null;
        }

        tiempo = 0f;
        Vector3 escalaRebote = escalaBase * escalaHoverOPresion;
        while (tiempo < duracionPaso)
        {
            tr.localScale = Vector3.Lerp(escalaPulsada, escalaRebote, tiempo / duracionPaso);
            tiempo += Time.deltaTime;
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