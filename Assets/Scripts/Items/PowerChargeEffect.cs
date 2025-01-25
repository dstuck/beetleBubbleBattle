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
    private ParticleSystem m_PowerParticles;
    #endregion

    protected override void OnEffectStart()
    {
        if (m_TargetBeetle == null)
        {
            Debug.LogError("PowerChargeEffect: No target beetle!");
            return;
        }
        
        // Store original charge rate
        m_OriginalChargeRate = m_TargetBeetle.ChargeRate;
        m_TargetBeetle.ChargeRate *= c_ChargeMultiplier;
        
        // Create particle system
        var particleObj = new GameObject("PowerParticles");
        particleObj.transform.SetParent(m_TargetBeetle.transform, false);
        particleObj.transform.localPosition = Vector3.zero;
        
        m_PowerParticles = particleObj.AddComponent<ParticleSystem>();
        var particleRenderer = m_PowerParticles.GetComponent<ParticleSystemRenderer>();
        particleRenderer.material = Resources.GetBuiltinResource<Material>("Sprite-Default.mat");
        
        ConfigureParticleSystem();
        m_PowerParticles.Play();
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
        if (m_TargetBeetle == null)
        {
            Debug.LogError("PowerChargeEffect: No target beetle on end!");
            return;
        }
        
        m_TargetBeetle.ChargeRate = m_OriginalChargeRate;
        Debug.Log($"PowerChargeEffect: Charge rate restored to {m_OriginalChargeRate}");

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