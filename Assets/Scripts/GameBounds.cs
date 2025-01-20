using UnityEngine;

public class GameBounds : MonoBehaviour
{
    [SerializeField] private float m_WallThickness = 1f;
    [SerializeField] private float m_BounceForce = 0.8f;
    
    private void Start()
    {
        CreateWalls();
    }
    
    private void CreateWalls()
    {
        // Get camera bounds
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;
        
        float screenAspect = (float)Screen.width / Screen.height;
        float cameraHeight = mainCamera.orthographicSize * 2;
        float cameraWidth = cameraHeight * screenAspect;
        
        // Create walls
        CreateWall(Vector2.right * (cameraWidth / 2), new Vector2(m_WallThickness, cameraHeight)); // Right wall
        CreateWall(Vector2.left * (cameraWidth / 2), new Vector2(m_WallThickness, cameraHeight));  // Left wall
        CreateWall(Vector2.up * (cameraHeight / 2), new Vector2(cameraWidth, m_WallThickness));    // Top wall
        CreateWall(Vector2.down * (cameraHeight / 2), new Vector2(cameraWidth, m_WallThickness));  // Bottom wall
    }
    
    private void CreateWall(Vector2 _position, Vector2 _size)
    {
        GameObject wall = new GameObject("Wall");
        wall.transform.parent = transform;
        wall.transform.position = _position;
        
        // Add collider
        BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
        collider.size = _size;
        
        // Add physics material
        PhysicsMaterial2D material = new PhysicsMaterial2D
        {
            bounciness = m_BounceForce,
            friction = 0
        };
        collider.sharedMaterial = material;
    }
} 