using System.Collections;
using UnityEngine;

public class DissolveController : MonoBehaviour
{
    public enum VisualEffect
    {
        None,
        Dissolve,
        // Ajoutez d'autres effets ici si nécessaire
    }

    private static readonly string[] KeywordNames = new[]
    {
        "_VISUALEFFECT_NONE",
        "_VISUALEFFECT_DISSOLVE",
        // Ajoutez les keywords correspondants aux nouveaux effets ici
    };

    [SerializeField] private Renderer[] targetRenderers;
    [SerializeField] private float dissolveSpeed = 1.0f;
    private readonly string _dissolveProperty = "_DissolveAmount";

    private MaterialPropertyBlock propertyBlock;
    private float currentValue = 1.0f;
    private Coroutine dissolveCoroutine;
    private bool isDissolving = false;
    private VisualEffect currentEffect = VisualEffect.None;

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
    }

    public void StartDissolve(bool dissolveIn = true)
    {
        if (dissolveCoroutine != null)
        {
            StopCoroutine(dissolveCoroutine);
        }

        SetActiveEffect(VisualEffect.Dissolve);
        // Initialise la valeur de dissolve en fonction du type d'effet
        currentValue = dissolveIn ? 1.0f : 0.0f;
        ApplyToAllRenderers(currentValue);
        dissolveCoroutine = StartCoroutine(DissolveRoutine(dissolveIn));
    }

    public void SetVisualEffect(VisualEffect effect)
    {
        if (effect == VisualEffect.Dissolve)
        {
            // Quand on active l'effet manuellement, on met la valeur à 1 (objet totalement visible)
            currentValue = 1.0f;
            ApplyToAllRenderers(currentValue);
        }
        else if (effect == VisualEffect.None)
        {
            // Reset la valeur quand on désactive l'effet
            currentValue = 1.0f;
            ApplyToAllRenderers(currentValue);
        }
        SetActiveEffect(effect);
    }

    public VisualEffect GetCurrentEffect()
    {
        return currentEffect;
    }

    private void SetActiveEffect(VisualEffect effect)
    {
        if (currentEffect == effect) return;

        foreach (var r in targetRenderers)
        {
            if (r == null) continue;

            foreach (var mat in r.sharedMaterials)
            {
                if (mat == null) continue;

                // Désactive tous les keywords
                foreach (string keyword in KeywordNames)
                {
                    mat.DisableKeyword(keyword);
                }

                // Active uniquement le keyword correspondant à l'effet souhaité
                if (effect != VisualEffect.None)
                {
                    mat.EnableKeyword(KeywordNames[(int)effect]);
                }
            }
        }

        currentEffect = effect;
    }

    private IEnumerator DissolveRoutine(bool dissolveIn)
    {
        isDissolving = true;
        float start = currentValue;
        float target = dissolveIn ? 0f : 1f;
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * dissolveSpeed;
            currentValue = Mathf.Lerp(start, target, t);
            ApplyToAllRenderers(currentValue);
            yield return null;
        }

        currentValue = target;
        ApplyToAllRenderers(currentValue);
        isDissolving = false;
    }

    private void ApplyToAllRenderers(float value)
    {
        if (targetRenderers == null || targetRenderers.Length == 0) return;

        foreach (var r in targetRenderers)
        {
            if (r == null) continue;

            r.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(_dissolveProperty, value);
            r.SetPropertyBlock(propertyBlock);
        }
    }

    public bool IsDissolving()
    {
        return isDissolving;
    }
}
