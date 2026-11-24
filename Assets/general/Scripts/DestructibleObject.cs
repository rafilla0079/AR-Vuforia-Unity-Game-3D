using UnityEngine;
using TMPro;
using System.Collections;

public class DestructibleObject : MonoBehaviour
{
    public enum ObjectType { Good, Bad }
    public ObjectType type;

    public int pointsValue = 10;
    public int healthChange = 1;

    [Header("Configuración de Texto Flotante")]
    public GameObject floatingTextPrefab;

    [Header("Efecto de Destrucción (Partículas)")]
    public GameObject particleEffectPrefab;

    [Header("Efecto de Destrucción con Sprite")]
    public Sprite destructionSprite;
    public float duracionSpriteDestruccion = 0.25f;
    public float multiplicadorTamanoObjeto = 1f;
    public AnimationCurve curvaTamanoSprite = new AnimationCurve(
        new Keyframe(0f, 0.5f),
        new Keyframe(0.5f, 1.3f),
        new Keyframe(1f, 0f)
    );
    public float rotacionOffsetZ = 0f;
    public bool giroAleatorioSuave = false;
    public Color tintSprite = Color.white;

    [Header("Panel de Daño (Objeto Malo)")]
    public GameObject damagePanelObject; // Opcional; si queda vacío en el prefab se busca por tag en escena
    public string tagPanelDano = "DamagePanel";
    public float duracionPanelDano = 0.25f;

    [Header("Configuración de Audio")]
    public AudioClip hitSound;
    [Range(0f, 1f)] public float soundVolume = 1f;

    private static int comboCount = 0;
    private static float lastHitTime = 0f;
    private float resetComboTime = 1.5f;

    public void RecibirImpacto()
    {
        Vector3 impactPos = transform.position;
        Vector3 objectWorldScale = transform.lossyScale;

        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, impactPos, soundVolume);
        }

        if (particleEffectPrefab != null)
        {
            GameObject particles = Instantiate(particleEffectPrefab, impactPos, Quaternion.identity);

            ParticleSystem ps = particles.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();

            Destroy(particles, 2f);
        }

        if (destructionSprite != null)
        {
            SpawnDestructionSprite(impactPos, objectWorldScale);
        }

        if (Time.time - lastHitTime > resetComboTime)
        {
            comboCount = 0;
        }

        comboCount++;
        lastHitTime = Time.time;

        MostrarTextoFlotante(impactPos + Vector3.up * 0.5f, "x" + comboCount);

        if (GameManager.Instance != null)
        {
            if (type == ObjectType.Good)
            {
                GameManager.Instance.ModificarVidas(healthChange);
            }
            else
            {
                GameManager.Instance.ModificarVidas(-healthChange);
            }
        }

        if (type == ObjectType.Bad)
        {
            ActivarPanelDano();
        }

        Destroy(gameObject);
    }

    void ActivarPanelDano()
    {
        if (damagePanelObject == null && !string.IsNullOrEmpty(tagPanelDano))
        {
            damagePanelObject = GameObject.FindGameObjectWithTag(tagPanelDano);
        }

        if (damagePanelObject != null)
        {
            DamagePanelSimpleRunner.ActivarTemporalmente(damagePanelObject, duracionPanelDano);
        }
    }

    void SpawnDestructionSprite(Vector3 position, Vector3 worldScale)
    {
        GameObject spriteObj = new GameObject("DestructionSprite_FX");
        spriteObj.transform.position = position;

        SpriteRenderer sr = spriteObj.AddComponent<SpriteRenderer>();
        sr.sprite = destructionSprite;
        sr.color = tintSprite;
        sr.sortingOrder = 25;

        DestructionSpriteRunner runner = spriteObj.AddComponent<DestructionSpriteRunner>();
        runner.IniciarAnimacion(
            sr,
            position,
            worldScale,
            multiplicadorTamanoObjeto,
            duracionSpriteDestruccion,
            curvaTamanoSprite,
            rotacionOffsetZ,
            giroAleatorioSuave
        );
    }

    void MostrarTextoFlotante(Vector3 position, string textContent)
    {
        if (floatingTextPrefab != null)
        {
            GameObject textObj = Instantiate(floatingTextPrefab, position, Quaternion.identity);

            TextMeshPro tmp = textObj.GetComponentInChildren<TextMeshPro>();
            if (tmp != null)
            {
                tmp.text = textContent;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.transform.parent != null)
        {
            Destroy(gameObject);
        }
    }
}

public class DamagePanelSimpleRunner : MonoBehaviour
{
    private static DamagePanelSimpleRunner instance;
    private Coroutine activeRoutine;

    public static void ActivarTemporalmente(GameObject panel, float duracion)
    {
        if (panel == null) return;

        if (instance == null)
        {
            GameObject go = new GameObject("DamagePanelSimpleRunner_Singleton");
            instance = go.AddComponent<DamagePanelSimpleRunner>();
            DontDestroyOnLoad(go);
        }

        if (instance.activeRoutine != null)
        {
            instance.StopCoroutine(instance.activeRoutine);
        }

        instance.activeRoutine = instance.StartCoroutine(instance.RutinaPanel(panel, duracion));
    }

    private IEnumerator RutinaPanel(GameObject panel, float duracion)
    {
        panel.SetActive(true);
        yield return new WaitForSeconds(Mathf.Max(0.05f, duracion));
        if (panel != null)
        {
            panel.SetActive(false);
        }
        activeRoutine = null;
    }
}

public class DestructionSpriteRunner : MonoBehaviour
{
    public void IniciarAnimacion(
        SpriteRenderer sr,
        Vector3 position,
        Vector3 objectWorldScale,
        float sizeMultiplier,
        float duration,
        AnimationCurve sizeCurve,
        float zOffset,
        bool softRandomTilt)
    {
        StartCoroutine(AnimarRutina(sr, position, objectWorldScale, sizeMultiplier, duration, sizeCurve, zOffset, softRandomTilt));
    }

    private IEnumerator AnimarRutina(
        SpriteRenderer sr,
        Vector3 position,
        Vector3 objectWorldScale,
        float sizeMultiplier,
        float duration,
        AnimationCurve sizeCurve,
        float zOffset,
        bool softRandomTilt)
    {
        float safeDuration = Mathf.Max(0.02f, duration);
        float elapsed = 0f;

        Vector3 baseScale = objectWorldScale;
        if (baseScale.sqrMagnitude <= 0.000001f) baseScale = Vector3.one;

        float extraTilt = softRandomTilt ? Random.Range(-10f, 10f) : 0f;

        while (elapsed < safeDuration)
        {
            if (sr == null) yield break;

            transform.position = position;

            if (Camera.main != null)
            {
                Vector3 dirToCamera = Camera.main.transform.position - transform.position;
                if (dirToCamera.sqrMagnitude > 0.0001f)
                {
                    transform.rotation = Quaternion.LookRotation(-dirToCamera.normalized, Camera.main.transform.up);
                }
            }

            if (Mathf.Abs(zOffset + extraTilt) > 0.001f)
            {
                transform.Rotate(0f, 0f, zOffset + extraTilt, Space.Self);
            }

            float t = Mathf.Clamp01(elapsed / safeDuration);
            float curveFactor = sizeCurve != null ? sizeCurve.Evaluate(t) : 1f;

            transform.localScale = baseScale * (sizeMultiplier * curveFactor);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}