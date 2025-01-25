using UnityEngine;

public class ShieldEffect : ItemEffect
{
    #region Serialized Fields
    [SerializeField] private Color m_ShieldColor = new Color(0, 0.7f, 1f, 0.5f);
    #endregion

    #region Private Fields
    private SpriteRenderer m_BubbleRenderer;
    private GameObject m_ShieldVisual;
    #endregion

    protected override void OnEffectStart()
    {
        if (m_TargetBeetle == null)
        {
            Debug.LogError("ShieldEffect: No target beetle!");
            return;
        }

        // Find the bubble's SpriteRenderer
        m_BubbleRenderer = m_TargetBeetle.GetComponentInChildren<Transform>().Find("Bubble").GetComponent<SpriteRenderer>();
        if (m_BubbleRenderer == null)
        {
            Debug.LogError("ShieldEffect: No SpriteRenderer found on bubble!");
            return;
        }

        // Create shield visual
        m_ShieldVisual = new GameObject("ShieldVisual");
        m_ShieldVisual.transform.SetParent(m_BubbleRenderer.transform, false);
        m_ShieldVisual.transform.localPosition = Vector3.zero;

        SpriteRenderer shieldRenderer = m_ShieldVisual.AddComponent<SpriteRenderer>();
        shieldRenderer.sprite = m_BubbleRenderer.sprite;
        shieldRenderer.color = m_ShieldColor;
        shieldRenderer.sortingOrder = m_BubbleRenderer.sortingOrder + 1;
        shieldRenderer.material = m_BubbleRenderer.material;

        // Make it slightly larger than the bubble
        m_ShieldVisual.transform.localScale = Vector3.one * 1.2f;
    }

    protected override void OnEffectEnd()
    {
        if (m_ShieldVisual != null)
        {
            Destroy(m_ShieldVisual);
        }
    }

    protected override void OnFlickerChange(bool isVisible)
    {
        if (m_ShieldVisual != null)
        {
            m_ShieldVisual.SetActive(isVisible);
        }
    }
} 