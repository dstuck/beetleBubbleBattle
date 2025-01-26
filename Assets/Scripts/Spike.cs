using UnityEngine;

public class Spike : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.TryGetComponent<BeetleBubble>(out var bubble))
            return;
            
        if (bubble.IsShielded)
            return;
            
        bubble.OnHitSpike();
    }
} 