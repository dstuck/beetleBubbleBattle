using UnityEngine;

public class Spike : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.TryGetComponent<BeetleBubble>(out var bubble))
            return;
            
        if (bubble.IsShielded)
            return;
            
        PopBubble(bubble);
    }
    
    private void PopBubble(BeetleBubble bubble)
    {
        // For now, just destroy the bubble
        Destroy(bubble.gameObject);
        
        // TODO: Add pop effect, sound, etc.
    }
} 