using UnityEngine;

public class ShieldEffect : ItemEffect
{
    #region Private Fields
    private SpriteRenderer m_SpriteRenderer;
    private Color m_ShieldColor = new Color(0, 0.7f, 1f, 0.5f); // Light blue, semi-transparent
    private Color m_OriginalColor;
    #endregion

    protected override void OnEffectStart()
    {
        if (m_TargetBeetle == null)
        {
            Debug.LogError("ShieldEffect: No target beetle!");
            return;
        }
        
        // Store original values and setup
        m_SpriteRenderer = m_TargetBeetle.GetComponent<SpriteRenderer>();
        if (m_SpriteRenderer == null)
        {
            Debug.LogError("ShieldEffect: No SpriteRenderer found on beetle!");
            return;
        }

        Debug.Log($"ShieldEffect: Starting effect. Original color: {m_SpriteRenderer.color}");
        m_OriginalColor = m_SpriteRenderer.color;
        
        // Set shielded state only
        m_TargetBeetle.IsShielded = true;
        Debug.Log("ShieldEffect: Shield activated");

        // Create child object for shield visual
        CreateShieldVisual();
    }

    protected override void OnEffectEnd()
    {
        if (m_TargetBeetle == null)
        {
            Debug.LogError("ShieldEffect: No target beetle on end!");
            return;
        }

        // Remove shielded state only
        m_TargetBeetle.IsShielded = false;
        Debug.Log("ShieldEffect: Shield deactivated");

        // Remove shield visual
        Transform shieldVisual = m_TargetBeetle.transform.Find("ShieldVisual");
        if (shieldVisual != null)
        {
            Destroy(shieldVisual.gameObject);
        }
    }

    private void CreateShieldVisual()
    {
        GameObject shieldVisual = new GameObject("ShieldVisual");
        shieldVisual.transform.parent = m_TargetBeetle.transform;
        shieldVisual.transform.localPosition = Vector3.zero;

        SpriteRenderer shieldRenderer = shieldVisual.AddComponent<SpriteRenderer>();
        shieldRenderer.sprite = m_SpriteRenderer.sprite;
        shieldRenderer.color = m_ShieldColor;
        shieldRenderer.sortingOrder = m_SpriteRenderer.sortingOrder + 1;
        shieldRenderer.material = m_SpriteRenderer.material;
        
        // Make it slightly larger than the beetle
        shieldVisual.transform.localScale = Vector3.one * 1.2f;
    }
} 