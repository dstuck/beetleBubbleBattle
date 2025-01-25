using UnityEngine;

public class PowerChargeEffect : ItemEffect
{
    #region Constants
    private const float c_ChargeMultiplier = 2f;
    #endregion

    #region Private Fields
    private float m_OriginalChargeRate;
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

        // Apply power boost
        m_TargetBeetle.ChargeRate *= c_ChargeMultiplier;
        Debug.Log($"PowerChargeEffect: Charge rate boosted from {m_OriginalChargeRate} to {m_TargetBeetle.ChargeRate}");
    }

    protected override void OnEffectEnd()
    {
        if (m_TargetBeetle == null)
        {
            Debug.LogError("PowerChargeEffect: No target beetle on end!");
            return;
        }
        
        // Restore original charge rate
        m_TargetBeetle.ChargeRate = m_OriginalChargeRate;
        Debug.Log($"PowerChargeEffect: Charge rate restored to {m_OriginalChargeRate}");
    }
} 