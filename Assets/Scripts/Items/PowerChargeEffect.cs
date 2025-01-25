using UnityEngine;

public class PowerChargeEffect : ItemEffect
{
    #region Constants
    private const float c_ChargeMultiplier = 2f;
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
        Debug.Log($"PowerChargeEffect: Charge rate boosted from {m_OriginalChargeRate} to {m_TargetBeetle.ChargeRate}");

        // Create particle system
        var particleObj = new GameObject("PowerParticles");
        particleObj.transform.SetParent(m_TargetBeetle.transform, false);
        particleObj.transform.localPosition = Vector3.zero;
        
        m_PowerParticles = particleObj.AddComponent<ParticleSystem>();
        var particleRenderer = m_PowerParticles.GetComponent<ParticleSystemRenderer>();
        particleRenderer.material = new Material(Shader.Find("Sprites/Default"));
        
        // Configure all particle system properties
        var main = m_PowerParticles.main;
        main.playOnAwake = false;
        main.loop = true;
        main.startLifetime = 0.5f;
        main.startSpeed = 2f;
        main.startSize = 0.2f;
        main.startColor = new Color(1f, 1f, 0f, 1f); // Bright yellow
        
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
                new GradientColorKey(new Color(1f, 1f, 0f), 0.0f), // Bright yellow
                new GradientColorKey(new Color(1f, 0.7f, 0f), 1.0f) // Orange-yellow fade
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorOverLifetime.color = gradient;

        // Start the system
        m_PowerParticles.Play();
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
} 