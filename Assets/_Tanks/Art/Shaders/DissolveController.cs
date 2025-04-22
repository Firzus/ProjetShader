using System.Collections;
using UnityEngine;

public class DissolveController : MonoBehaviour
{
    // Référence au material qui utilise le shader de dissolve
    [SerializeField] private Material dissolveMaterial;

    // Référence directe au Renderer (plus performant que GetComponent à chaque frame)
    [SerializeField] private Renderer targetRenderer;

    // Options de configuration
    [SerializeField] private float dissolveSpeed = 1.0f;
    [SerializeField] private bool reverseOnComplete = false;
    [SerializeField] private bool startDissolvedOut = false;

    // ID de propriété du shader (plus performant que d'utiliser des chaînes de caractères)
    private static readonly int DissolveAmountProperty = Shader.PropertyToID("_DissolveAmount");

    // Variables de suivi
    private float currentDissolveAmount;
    private bool isDissolving = false;
    private Coroutine dissolveCoroutine;

    private void Start()
    {
        // Si aucun renderer n'est assigné, utilisez celui de cet objet
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        // Si aucun material n'est assigné, utilisez celui du renderer
        if (dissolveMaterial == null && targetRenderer != null)
            dissolveMaterial = targetRenderer.material;

        // Initialisez la valeur de départ
        currentDissolveAmount = startDissolvedOut ? 0.0f : 1.0f;
        UpdateDissolveAmount(currentDissolveAmount);
    }

    // Méthode publique pour déclencher l'effet de dissolve
    public void StartDissolve(bool dissolveIn = true)
    {
        // Arrêtez toute coroutine en cours
        if (dissolveCoroutine != null)
            StopCoroutine(dissolveCoroutine);

        // Démarrez la nouvelle coroutine
        dissolveCoroutine = StartCoroutine(DissolveEffect(dissolveIn));
    }

    // Méthode pour démarrer un dissolve depuis le code
    public void TriggerDissolve()
    {
        StartDissolve(true);
    }

    // Méthode pour inverser l'effet (réapparition)
    public void TriggerReappear()
    {
        StartDissolve(false);
    }

    // Coroutine qui gère l'animation de l'effet dissolve
    private IEnumerator DissolveEffect(bool dissolveIn)
    {
        isDissolving = true;

        float targetValue = dissolveIn ? 0.0f : 1.0f;
        float startValue = currentDissolveAmount;
        float elapsed = 0;

        while (elapsed < 1)
        {
            elapsed += Time.deltaTime * dissolveSpeed;
            currentDissolveAmount = Mathf.Lerp(startValue, targetValue, elapsed);

            // Met à jour la valeur dans le shader
            UpdateDissolveAmount(currentDissolveAmount);

            yield return null;
        }

        // Assure que la valeur finale est exactement celle souhaitée
        currentDissolveAmount = targetValue;
        UpdateDissolveAmount(currentDissolveAmount);

        isDissolving = false;

        // Si l'option est activée, inverse automatiquement l'effet
        if (reverseOnComplete)
            StartDissolve(!dissolveIn);
    }

    // Met à jour la propriété dans le material de manière optimisée
    private void UpdateDissolveAmount(float value)
    {
        if (dissolveMaterial != null)
            dissolveMaterial.SetFloat(DissolveAmountProperty, value);

        // Si on utilise des MaterialPropertyBlocks pour plusieurs instances
        // (méthode plus optimisée pour de nombreux objets)
        /*
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(DissolveAmountProperty, value);
        targetRenderer.SetPropertyBlock(propertyBlock);
        */
    }

    // Méthodes utilitaires pour vérifier l'état
    public bool IsDissolving() => isDissolving;
    public float GetDissolveAmount() => currentDissolveAmount;

    // Exemple d'utilisation avec des déclencheurs (triggers)
    private void OnTriggerEnter(Collider other)
    {
        // Exemple: Si le joueur entre en contact, déclenchez le dissolve
        if (other.CompareTag("Player"))
        {
            StartDissolve(true);
        }
    }
}