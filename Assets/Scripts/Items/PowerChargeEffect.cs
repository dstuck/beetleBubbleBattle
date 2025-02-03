using UnityEngine;

public class PowerChargeEffect : ItemEffect
{
    #region Constants
    private const float c_ChargeMultiplier = 2f;
    private static readonly Color c_NormalColor = new Color(1f, 1f, 0f, 1f); // Bright yellow
    private static readonly Color c_WarningColor = new Color(1f, 0f, 0f, 1f); // Bright red
    #endregion

    #region Private Fields
    private float m_OriginalChargeRate;
    private float m_OriginalGrowthRate;
    private ParticleSystem m_PowerParticles;
    private static readonly string c_ParticlePrefabPath = "Prefabs/PowerParticles";
    #endregion

    protected override void OnEffectStart()
    {
        if (m_TargetBeetle == null)
        {
            Debug.LogError("PowerChargeEffect: No target beetle!");
            return;
        }
        
        // Store original rates
        m_OriginalChargeRate = m_TargetBeetle.ChargeRate;
        m_OriginalGrowthRate = m_TargetBeetle.ChargeGrowthRate;

        // Boost both charge and growth rates
        m_TargetBeetle.ChargeRate *= c_ChargeMultiplier;
        m_TargetBeetle.ChargeGrowthRate *= c_ChargeMultiplier;
        
        // Load and instantiate particle system
        var particlePrefab = Resources.Load<ParticleSystem>(c_ParticlePrefabPath);
        if (particlePrefab != null)
        {
            m_PowerParticles = Instantiate(particlePrefab, m_TargetBeetle.transform);
            m_PowerParticles.transform.localPosition = Vector3.zero;
            m_PowerParticles.Play();
        }
        else
        {
            Debug.LogError($"PowerChargeEffect: Could not load particle prefab from {c_ParticlePrefabPath}");
        }
    }

    private void ConfigureParticleSystem()
    {
        var main = m_PowerParticles.main;
        main.playOnAwake = false;
        main.loop = true;
        main.startLifetime = 0.5f;
        main.startSpeed = 2f;
        main.startSize = 0.2f;
        main.startColor = c_NormalColor;
        
        var emission = m_PowerParticles.emission;
        emission.rateOverTime = 20;
        
        var shape = m_PowerParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.5f;
        shape.radiusThickness = 1f;
        
        var colorOverLifetime = m_PowerParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(c_NormalColor, 0.0f), // Bright yellow
                new GradientColorKey(new Color(1f, 0.7f, 0f), 1.0f) // Orange-yellow fade
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorOverLifetime.color = gradient;
    }

    protected override void OnEffectEnd()
    {
        if (m_TargetBeetle != null)
        {
            // Restore original rates
            m_TargetBeetle.ChargeRate = m_OriginalChargeRate;
            m_TargetBeetle.ChargeGrowthRate = m_OriginalGrowthRate;
        }

        if (m_PowerParticles != null)
        {
            m_PowerParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            Destroy(m_PowerParticles.gameObject);
        }
    }

    protected override void OnFlickerChange(bool isVisible)
    {
        if (m_PowerParticles != null)
        {
            var main = m_PowerParticles.main;
            main.startColor = isVisible ? c_NormalColor : c_WarningColor;
        }
    }
} 