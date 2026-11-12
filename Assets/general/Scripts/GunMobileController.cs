using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class GunMobileController : MonoBehaviour
{
    [Header("Configuración de Disparo")]
    public GameObject bulletPrefab; // Prefab de la bala 3D
    public Transform firePoint;     // El punto de salida en la punta del cañón

    [Header("Configuración de Animación (Retroceso)")]
    public float recoilAmount = 0.15f;   // Cuánto se echa hacia atrás la pistola
    public float recoilSpeed = 15f;     // Velocidad del retroceso y recuperación
    private Vector3 localOriginalPos;   // Posición local original de la pistola

    void Start()
    {
       
        localOriginalPos = transform.localPosition;
    }

    void Update()
    {
        // Detectar si el usuario toca la pantalla o hace clic con el ratón
        bool isTouching = false;
        Vector2 touchPosition = Vector2.zero;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            isTouching = true;
            touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            isTouching = true;
            touchPosition = Mouse.current.position.ReadValue();
        }

        if (isTouching)
        {
            DispararHacia(touchPosition);
        }
    }

    void DispararHacia(Vector2 screenPos)
    {
        if (bulletPrefab == null || firePoint == null || Camera.main == null) return;

       
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;

        Vector3 targetDirection;
        if (Physics.Raycast(ray, out hit, 50f))
        {
            targetDirection = (hit.point - firePoint.position).normalized;
        }
        else
        {
            targetDirection = ray.direction;
        }

        
        Quaternion bulletRotation = Quaternion.LookRotation(targetDirection);

        // 3. Instanciar la bala 3D
        Instantiate(bulletPrefab, firePoint.position, bulletRotation);

        
        StopAllCoroutines();
        StartCoroutine(RecoilRoutine());
    }

    IEnumerator RecoilRoutine()
    {
        

        Vector3 recoilPos = localOriginalPos - new Vector3(0f, 0f, recoilAmount);

        
        float elapsed = 0f;
        while (elapsed < 0.05f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, recoilPos, Time.deltaTime * recoilSpeed);
            elapsed += Time.deltaTime;
            yield return null;
        }

       
        while (Vector3.Distance(transform.localPosition, localOriginalPos) > 0.001f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, localOriginalPos, Time.deltaTime * recoilSpeed);
            yield return null;
        }

        transform.localPosition = localOriginalPos; 
    }
}