using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

public class GunMobileController : MonoBehaviour
{
    [Header("Configuración de Disparo")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.2f;
    private float nextFireTime = 0f;

    [Header("Efecto de Fogonazo con Imagen")]
    public Sprite fogonazoSprite;
    public float duracionFogonazo = 0.08f;
    public float multiplicadorTamanoFirePoint = 1f;
    public AnimationCurve curvaTamanoFogonazo = new AnimationCurve(
        new Keyframe(0f, 0.4f),
        new Keyframe(0.5f, 1.35f),
        new Keyframe(1f, 0.8f)
    );
    public float rotacionOffsetZ = 0f;
    public bool giroAleatorioSuave = false;
    public Color tintFogonazo = Color.white;

    [Header("Configuración de Temblor de Cámara (Shake)")]
    public float shakeDuration = 0.08f;
    public float shakeMagnitude = 0.1f;

    [Header("Configuración de Munición")]
    public int capacidadCargador = 10;
    private int balasActuales;
    public float tiempoRecarga = 1.5f;
    private bool estaRecargando = false;

    [Header("Animación de Recarga Externa")]
    public GunReloadFlipAnimation reloadFlipAnimation;

    [Header("Interfaz de Usuario (UI)")]
    public TextMeshProUGUI textoBalasUI;

    [Header("Configuración de Audio")]
    public AudioClip shootSound;
    public AudioClip reloadSound;
    private AudioSource audioSource;

    [Header("Configuración de Animación (Retroceso)")]
    public float recoilAmount = 0.15f;
    public float recoilSpeed = 15f;
    private Vector3 localOriginalPos;

    private GameObject flashObject;
    private SpriteRenderer flashSpriteRenderer;
    private Coroutine flashRoutine;

    void Start()
    {
        localOriginalPos = transform.localPosition;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (reloadFlipAnimation == null)
        {
            reloadFlipAnimation = GetComponent<GunReloadFlipAnimation>();
        }

        PrepararFogonazoImagen();

        balasActuales = capacidadCargador;
        ActualizarTextoBalas();
    }

    void PrepararFogonazoImagen()
    {
        if (fogonazoSprite == null) return;

        flashObject = new GameObject("MuzzleFlashImage");
        flashSpriteRenderer = flashObject.AddComponent<SpriteRenderer>();
        flashSpriteRenderer.sprite = fogonazoSprite;
        flashSpriteRenderer.color = tintFogonazo;
        flashSpriteRenderer.sortingOrder = 20;

        SincronizarConFirePoint(1f);
        flashObject.SetActive(false);
    }

    void SincronizarConFirePoint(float factorCurva)
    {
        if (flashObject == null || firePoint == null) return;

        flashObject.transform.position = firePoint.position;

        if (Camera.main != null)
        {
            Vector3 dirToCamera = Camera.main.transform.position - flashObject.transform.position;
            if (dirToCamera.sqrMagnitude > 0.0001f)
            {
                flashObject.transform.rotation = Quaternion.LookRotation(-dirToCamera.normalized, Camera.main.transform.up);
            }
        }

        if (Mathf.Abs(rotacionOffsetZ) > 0.001f)
        {
            flashObject.transform.Rotate(0f, 0f, rotacionOffsetZ, Space.Self);
        }

        if (giroAleatorioSuave)
        {
            flashObject.transform.Rotate(0f, 0f, Random.Range(-10f, 10f), Space.Self);
        }

        Vector3 escalaFirePoint = firePoint.lossyScale;
        if (escalaFirePoint.sqrMagnitude <= 0.000001f) escalaFirePoint = Vector3.one;

        flashObject.transform.localScale = escalaFirePoint * (multiplicadorTamanoFirePoint * factorCurva);
    }

    void Update()
    {
        if (estaRecargando) return;

        if (balasActuales <= 0)
        {
            StartCoroutine(RecargarRutina());
            return;
        }

        if (Time.time < nextFireTime) return;

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
            nextFireTime = Time.time + fireRate;
        }
    }

    void DispararHacia(Vector2 screenPos)
    {
        if (bulletPrefab == null || firePoint == null || Camera.main == null) return;

        balasActuales--;
        ActualizarTextoBalas();

        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;

        Vector3 targetDirection;
        if (Physics.Raycast(ray, out hit, 50f))
        {
            targetDirection = (hit.point - firePoint.position).normalized;

            DestructibleObject destructible = hit.collider.GetComponent<DestructibleObject>();
            if (destructible != null)
            {
                destructible.RecibirImpacto();
            }
        }
        else
        {
            targetDirection = ray.direction;
        }

        Quaternion shotRotation = Quaternion.LookRotation(targetDirection);

        MostrarFogonazoImagen();
        StartCoroutine(CameraShakeRoutine());

        Instantiate(bulletPrefab, firePoint.position, shotRotation);

        StartCoroutine(RecoilRoutine());
    }

    void MostrarFogonazoImagen()
    {
        if (fogonazoSprite == null || firePoint == null) return;

        if (flashObject == null || flashSpriteRenderer == null)
        {
            PrepararFogonazoImagen();
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FogonazoRutina());
    }

    IEnumerator FogonazoRutina()
    {
        flashObject.SetActive(true);
        flashSpriteRenderer.sprite = fogonazoSprite;
        flashSpriteRenderer.color = tintFogonazo;

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, duracionFogonazo);

        while (elapsed < duration)
        {
            float normalizedTime = Mathf.Clamp01(elapsed / duration);
            float curveFactor = curvaTamanoFogonazo != null ? curvaTamanoFogonazo.Evaluate(normalizedTime) : 1f;

            SincronizarConFirePoint(curveFactor);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (flashObject != null)
        {
            flashObject.SetActive(false);
        }
    }

    IEnumerator CameraShakeRoutine()
    {
        Transform camTransform = Camera.main.transform;
        Vector3 originalPos = camTransform.localPosition;

        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            camTransform.localPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        camTransform.localPosition = originalPos;
    }

    IEnumerator RecargarRutina()
    {
        estaRecargando = true;

        if (textoBalasUI != null)
        {
            textoBalasUI.text = "Recargando...";
        }

        if (reloadSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }

        if (reloadFlipAnimation != null)
        {
            reloadFlipAnimation.ReproducirAnimacionRecarga(tiempoRecarga);
        }

        yield return new WaitForSeconds(tiempoRecarga);

        balasActuales = capacidadCargador;
        estaRecargando = false;

        ActualizarTextoBalas();
    }

    void ActualizarTextoBalas()
    {
        if (textoBalasUI != null)
        {
            textoBalasUI.text = "Balas: " + balasActuales + " / " + capacidadCargador;
        }
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