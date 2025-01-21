using UnityEngine;

public class Spike : MonoBehaviour
{
    [SerializeField] private Vector2 m_SpikeDirection = Vector2.up;
    [SerializeField] private float m_DeadZoneAngle = 90f; // Angle range that counts as "hitting the spike"
    
    private void OnDrawGizmos()
    {
        // Draw spike direction in editor
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, m_SpikeDirection.normalized);
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.TryGetComponent<BeetleBubble>(out var bubble))
            return;
            
        // Get the direction the bubble is coming from
        Vector2 collisionDirection = collision.GetContact(0).point - (Vector2)transform.position;
        float angle = Vector2.Angle(m_SpikeDirection, collisionDirection);
        
        // If the bubble hits within our dead zone angle, pop it
        if (angle <= m_DeadZoneAngle / 2)
        {
            PopBubble(bubble);
        }
    }
    
    private void PopBubble(BeetleBubble bubble)
    {
        // For now, just destroy the bubble
        Destroy(bubble.gameObject);
        
        // TODO: Add pop effect, sound, etc.
    }
} 