using UnityEngine;
using System.Collections;

public class PlantaRodadora2D : MonoBehaviour
{
    [Header("Movimiento Horizontal (2D)")]
    public float velocidadMovimiento = 3f;
    public bool idaYVuelta = false; 
    public bool usarLimites = true;
    public float limiteIzquierdo = -6f;
    public float limiteDerecho = 6f;

    [Header("Animación de Rodar y Bote")]
    public float velocidadRotacion = 260f; 
    public bool animarBoteSuave = true;
    public float alturaBote = 0.08f;
    public float frecuenciaBote = 8f;

    [Header("Respawn con Tiempo")]
    public bool respawnActivado = true;
    public float tiempoRespawnMin = 2f;
    public float tiempoRespawnMax = 4.5f;
    public bool respawnearEnLimiteIzquierdo = true;

    private int direccion = 1; 
    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;
    private Vector3 escalaInicial;
    private float offsetYActual = 0f;
    private bool estaEsperandoRespawn = false;
    private Coroutine respawnCoroutine;

    private Renderer[] renderers;
    private Collider2D[] colliders2D;

    void Start()
    {
        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;
        escalaInicial = transform.localScale;

        renderers = GetComponentsInChildren<Renderer>(true);
        colliders2D = GetComponentsInChildren<Collider2D>(true);
    }

    void Update()
    {
        if (estaEsperandoRespawn) return;

        float dt = Time.deltaTime;

        // 1. Mover en X
        float pasoX = direccion * velocidadMovimiento * dt;
        Vector3 pos = transform.position;
        pos.x += pasoX;

        if (usarLimites)
        {
            if (pos.x >= limiteDerecho)
            {
                pos.x = limiteDerecho;
                if (idaYVuelta)
                {
                    direccion = -1;
                }
                else if (respawnActivado)
                {
                    transform.position = pos;
                    IniciarRespawn();
                    return;
                }
                else
                {
                    pos.x = limiteIzquierdo;
                }
            }
            else if (pos.x <= limiteIzquierdo)
            {
                pos.x = limiteIzquierdo;
                direccion = 1;
            }
        }

       
        if (animarBoteSuave)
        {
            float nuevoOffsetY = Mathf.Abs(Mathf.Sin(Time.time * frecuenciaBote)) * alturaBote;
            pos.y += (nuevoOffsetY - offsetYActual);
            offsetYActual = nuevoOffsetY;

            float squash = Mathf.Sin(Time.time * frecuenciaBote * 2f) * 0.05f;
            transform.localScale = new Vector3(
                escalaInicial.x * (1f + squash),
                escalaInicial.y * (1f - squash),
                escalaInicial.z
            );
        }

        transform.position = pos;

        // 3. Rotar para simular que rueda
        float giroZ = -direccion * velocidadRotacion * dt;
        transform.Rotate(0f, 0f, giroZ, Space.Self);
    }

    public void IniciarRespawn()
    {
        if (!respawnActivado || estaEsperandoRespawn) return;

        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
        }

        respawnCoroutine = StartCoroutine(RespawnRutina());
    }

    private IEnumerator RespawnRutina()
    {
        estaEsperandoRespawn = true;
        EstablecerVisibleYActivo(false);

        float minT = Mathf.Min(tiempoRespawnMin, tiempoRespawnMax);
        float maxT = Mathf.Max(tiempoRespawnMin, tiempoRespawnMax);
        float tiempoEspera = Random.Range(minT, maxT);

        yield return new WaitForSeconds(tiempoEspera);

        
        Vector3 nuevaPos = posicionInicial;
        if (respawnearEnLimiteIzquierdo && usarLimites)
        {
            nuevaPos.x = limiteIzquierdo;
            nuevaPos.y = posicionInicial.y;
        }

        offsetYActual = 0f;
        direccion = 1;
        transform.position = nuevaPos;
        transform.rotation = rotacionInicial;
        transform.localScale = escalaInicial;

        EstablecerVisibleYActivo(true);
        estaEsperandoRespawn = false;
    }

    private void EstablecerVisibleYActivo(bool estado)
    {
        if (renderers != null)
        {
            foreach (Renderer r in renderers)
            {
                if (r != null) r.enabled = estado;
            }
        }

        if (colliders2D != null)
        {
            foreach (Collider2D c in colliders2D)
            {
                if (c != null) c.enabled = estado;
            }
        }
    }
}