using UnityEngine;
using System.Collections;

public class GunReloadFlipAnimation : MonoBehaviour
{
    public enum EjeGiro
    {
        EjeX, // Giro tipo revólver 
        EjeY, // Giro lateral
        EjeZ  
    }

    [Header("Configuración del Giro")]
    public EjeGiro ejeDeGiro = EjeGiro.EjeX;
    public float vueltasCompletas = 1f; 
    public int direccion = 1; 
    public AnimationCurve curvaGiro = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Pequeño Salto Opcional")]
    public float alturaSalto = 0.08f;

    private Quaternion rotacionInicial;
    private Vector3 posicionInicial;
    private Coroutine animacionActual;

    void Awake()
    {
        rotacionInicial = transform.localRotation;
        posicionInicial = transform.localPosition;
    }

    public void ReproducirAnimacionRecarga(float duracionTotal)
    {
        if (animacionActual != null)
        {
            StopCoroutine(animacionActual);
        }

        animacionActual = StartCoroutine(RutinaGiroRecarga(Mathf.Max(0.1f, duracionTotal)));
    }

    public void CancelarAnimacion()
    {
        if (animacionActual != null)
        {
            StopCoroutine(animacionActual);
            animacionActual = null;
        }

        transform.localRotation = rotacionInicial;
        transform.localPosition = posicionInicial;
    }

    private IEnumerator RutinaGiroRecarga(float duracion)
    {
       
        Quaternion rotInicio = transform.localRotation;
        Vector3 posInicio = transform.localPosition;

        float anguloTotal = 360f * vueltasCompletas * direccion;
        Vector3 vectorEje = ObtenerVectorEje();

        float tiempo = 0f;
        while (tiempo < duracion)
        {
            float t = Mathf.Clamp01(tiempo / duracion);
            float factor = curvaGiro != null ? curvaGiro.Evaluate(t) : t;

           
            transform.localRotation = rotInicio * Quaternion.AngleAxis(anguloTotal * factor, vectorEje);

           
            float arco = Mathf.Sin(t * Mathf.PI) * alturaSalto;
            transform.localPosition = posInicio + Vector3.up * arco;

            tiempo += Time.deltaTime;
            yield return null;
        }

       
        transform.localRotation = rotacionInicial;
        transform.localPosition = posicionInicial;
        animacionActual = null;
    }

    private Vector3 ObtenerVectorEje()
    {
        switch (ejeDeGiro)
        {
            case EjeGiro.EjeY:
                return Vector3.up;
            case EjeGiro.EjeZ:
                return Vector3.forward;
            case EjeGiro.EjeX:
            default:
                return Vector3.right;
        }
    }
}