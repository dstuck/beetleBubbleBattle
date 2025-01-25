using UnityEngine;
using System.Collections;

public abstract class ItemEffect : MonoBehaviour
{
    [SerializeField] protected float m_Duration = 7f;
    protected BeetleBubble m_TargetBeetle;
    
    public virtual void Initialize(BeetleBubble _targetBeetle)
    {
        m_TargetBeetle = _targetBeetle;
        StartCoroutine(EffectRoutine());
    }

    protected virtual IEnumerator EffectRoutine()
    {
        OnEffectStart();
        yield return new WaitForSeconds(m_Duration);
        OnEffectEnd();
        Destroy(this);
    }

    protected abstract void OnEffectStart();
    protected abstract void OnEffectEnd();
} 