using UnityEngine;

public class CactusWind3D : MonoBehaviour
{
    [Header("Dirección y Fuerza del Viento")]
    public Vector3 direccionViento = new Vector3(1f, 0f, 0.35f);
    public float fuerzaVientoBase = 4.5f;     
    public float velocidadOndulacion = 1.6f;

    [Header("Ráfagas de Viento")]
    public bool usarRafagas = true;
    public float fuerzaRafaga = 7.5f;
    public float frecuenciaRafagas = 0.45f;
    public float suavizadoRafaga = 3f;

    [Header("Respuesta Física / Rebote del Cactus")]
    public float rigidez = 14f;            
    public float amortiguacion = 4.5f;     
    public float anguloMaximoTotal = 22f;   

    private Quaternion rotacionInicial;
    private Vector2 rotacionActualGrados;   
    private Vector2 velocidadAngularGrados;
    private float rafagaActual = 0f;
    private float semillaRuido;

    void Awake()
    {
        rotacionInicial = transform.localRotation;
        semillaRuido = Random.Range(0f, 100f);
    }

    void Update()
    {
        float tiempo = Time.time * velocidadOndulacion;

       
        float ruidoX = (Mathf.PerlinNoise(semillaRuido, tiempo) * 2f) - 1f;
        float ruidoZ = (Mathf.PerlinNoise(semillaRuido + 37.3f, tiempo) * 2f) - 1f;

       
        float objetivoRafaga = 0f;
        if (usarRafagas)
        {
            float pulso = Mathf.PerlinNoise(semillaRuido + 91.7f, Time.time * frecuenciaRafagas);
            
            float intensidadPulso = Mathf.Clamp01((pulso - 0.52f) / 0.48f);
            objetivoRafaga = intensidadPulso * fuerzaRafaga;
        }

        rafagaActual = Mathf.Lerp(rafagaActual, objetivoRafaga, Time.deltaTime * suavizadoRafaga);

        Vector3 dir = direccionViento.sqrMagnitude > 0.0001f ? direccionViento.normalized : Vector3.right;

   
        float empujeTotal = fuerzaVientoBase + rafagaActual;

        float objetivoGradosZ = (-dir.x * empujeTotal) + (ruidoZ * (fuerzaVientoBase * 0.35f + rafagaActual * 0.25f));
        float objetivoGradosX = (dir.z * empujeTotal) + (ruidoX * (fuerzaVientoBase * 0.35f + rafagaActual * 0.25f));

        Vector2 objetivoGrados = new Vector2(objetivoGradosX, objetivoGradosZ);

    
        Vector2 desplazamiento = objetivoGrados - rotacionActualGrados;
        Vector2 aceleracion = (desplazamiento * rigidez) - (velocidadAngularGrados * amortiguacion);

        velocidadAngularGrados += aceleracion * Time.deltaTime;
        rotacionActualGrados += velocidadAngularGrados * Time.deltaTime;

      
        rotacionActualGrados.x = Mathf.Clamp(rotacionActualGrados.x, -anguloMaximoTotal, anguloMaximoTotal);
        rotacionActualGrados.y = Mathf.Clamp(rotacionActualGrados.y, -anguloMaximoTotal, anguloMaximoTotal);

      
        Quaternion rotacionViento = Quaternion.Euler(rotacionActualGrados.x, 0f, rotacionActualGrados.y);
        transform.localRotation = rotacionInicial * rotacionViento;
    }

   
    public void AplicarGolpeDeViento(Vector3 direccionGolpe, float intensidad)
    {
        Vector3 dir = direccionGolpe.sqrMagnitude > 0.0001f ? direccionGolpe.normalized : direccionViento.normalized;
        velocidadAngularGrados.y += -dir.x * intensidad * 10f;
        velocidadAngularGrados.x += dir.z * intensidad * 10f;
    }
}