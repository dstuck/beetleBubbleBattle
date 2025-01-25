using UnityEngine;
using System.Collections;

public abstract class ItemEffect : MonoBehaviour
{
    [SerializeField] protected float m_Duration = 7f;
    [SerializeField] private float m_FlickerDuration = 1.5f;
    [SerializeField] private float m_FlickerRate = 0.1f;
    
    protected BeetleBubble m_TargetBeetle;
    private bool m_IsFlickering;
    
    public virtual void Initialize(BeetleBubble _targetBeetle)
    {
        m_TargetBeetle = _targetBeetle;
        StartCoroutine(EffectRoutine());
    }

    protected virtual IEnumerator EffectRoutine()
    {
        OnEffectStart();
        
        // Wait until it's time to start flickering
        yield return new WaitForSeconds(m_Duration - m_FlickerDuration);
        
        // Start flickering
        StartCoroutine(FlickerRoutine());
        
        // Wait for the remaining duration
        yield return new WaitForSeconds(m_FlickerDuration);
        
        OnEffectEnd();
        Destroy(this);
    }
    
    private IEnumerator FlickerRoutine()
    {
        m_IsFlickering = true;
        bool isVisible = true;
        
        while (m_IsFlickering)
        {
            OnFlickerChange(isVisible);
            isVisible = !isVisible;
            yield return new WaitForSeconds(m_FlickerRate);
        }
    }
    
    protected virtual void OnFlickerChange(bool isVisible)
    {
        // Override in derived classes to handle visibility changes
    }

    protected abstract void OnEffectStart();
    protected abstract void OnEffectEnd();
} 