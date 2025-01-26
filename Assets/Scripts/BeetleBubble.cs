using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class BeetleBubble : MonoBehaviour
{
    #region Serialized Fields
    [Header("Movement Properties")]
    [SerializeField, Range(0.1f, 10f)] private float m_BaseWeight = 1f;
    [SerializeField, Range(1f, 10f)] private float m_BopForce = 2f;
    [SerializeField, Range(1f, 25f)] private float m_ChargeRate = 2f;
    [SerializeField, Range(0f, 10f)] private float m_BaseDamping = 5f;
    
    [Header("Size Properties")]
    [SerializeField] private float m_MinSize = 0.5f;
    [SerializeField] private float m_MaxSize = 5f;
    [SerializeField] private float m_ChargeGrowthRate = 0.5f;
    [SerializeField] private float m_DischargeShrinkRate = 0.1f;
    
    [Header("Visual Properties")]
    [SerializeField, Range(0.2f, 1f)] private float m_ChargeTransparency = 0.5f;
    [SerializeField] private float m_BounceForce = 1f;

    [Header("References")]
    [SerializeField] private Transform m_BubbleTransform;
    [SerializeField] private SpriteRenderer m_BeetleRenderer;

    [Header("Death Properties")]
    [SerializeField] private float m_RespawnDelay = 1f;
    [SerializeField] private int m_MaxLives = 3;
    private bool m_IsDead = false;
    private int m_LivesRemaining;

    #endregion

    #region Private Fields
    private Rigidbody2D m_Rigidbody;
    private Vector2 m_MoveDirection;
    private Vector2 m_LastValidMoveDirection = Vector2.right;
    private float m_CurrentCharge;
    private bool m_IsCharging;
    private float m_CurrentSize = 1f;
    private Vector3 m_StartPosition;
    private bool m_IsShielded;
    #endregion

    #region Public Properties
    public float ChargeRate
    {
        get => m_ChargeRate;
        set => m_ChargeRate = value;
    }

    public float ChargeGrowthRate
    {
        get => m_ChargeGrowthRate;
        set => m_ChargeGrowthRate = value;
    }

    public bool IsShielded
    {
        get => m_IsShielded;
        set => m_IsShielded = value;
    }

    public SpriteRenderer BeetleRenderer => m_BeetleRenderer;
    #endregion

    #region Public Events
    public System.Action<int> OnLivesChanged;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        var playerInput = GetComponent<PlayerInput>();
        Debug.Log($"BeetleBubble: Awake for Player {playerInput?.playerIndex ?? -1}. " +
                  $"BeetleRenderer null? {m_BeetleRenderer == null}, " +
                  $"Current sprite null? {m_BeetleRenderer?.sprite == null}, " +
                  $"Sprite name: {m_BeetleRenderer?.sprite?.name ?? "none"}");
        
        m_Rigidbody = GetComponent<Rigidbody2D>();
        
        if (m_Rigidbody == null)
        {
            Debug.LogError("BeetleBubble: No Rigidbody2D found!");
            return;
        }
        
        m_Rigidbody.mass = m_BaseWeight;
        m_Rigidbody.linearDamping = m_BaseDamping;
        m_Rigidbody.gravityScale = 0f;
        m_StartPosition = transform.position;
        m_LivesRemaining = m_MaxLives;

        // Configure physics for bouncing
        if (m_Rigidbody != null)
        {
            m_Rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            m_Rigidbody.sharedMaterial = CreateBouncyMaterial();
        }
    }

    private void Start()
    {
        var playerInput = GetComponent<PlayerInput>();
        Debug.Log($"BeetleBubble: Start for Player {playerInput?.playerIndex ?? -1}. " +
                  $"BeetleRenderer null? {m_BeetleRenderer == null}, " +
                  $"Current sprite null? {m_BeetleRenderer?.sprite == null}, " +
                  $"Sprite name: {m_BeetleRenderer?.sprite?.name ?? "none"}");
    }

    private void Update()
    {
        if (m_IsCharging)
        {
            Charge();
        }
        UpdateDamping();
    }
    #endregion

    #region Input Event Handlers
    public void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        m_MoveDirection = context.ReadValue<Vector2>();
        if (m_MoveDirection != Vector2.zero)
        {
            m_LastValidMoveDirection = m_MoveDirection.normalized;
            
            // Calculate rotation based on movement direction, offset by -90 to account for initial upward orientation
            float angle = Mathf.Atan2(m_MoveDirection.y, m_MoveDirection.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public void OnCharge(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        bool isPressed = context.ReadValueAsButton();
        m_IsCharging = isPressed;
        
        if (!isPressed && m_CurrentCharge > 0)
        {
            ApplyBurst();
        }
        UpdateVisuals(isPressed ? m_ChargeTransparency : 1f);
    }
    #endregion

    #region Private Methods
    private void Charge()
    {
        m_CurrentCharge += m_ChargeRate * Time.deltaTime;
        
        // Increase size while charging
        m_CurrentSize = Mathf.Min(m_CurrentSize + m_ChargeGrowthRate * Time.deltaTime, m_MaxSize);
        UpdateSize(m_CurrentSize);
        
        // Update mass based on size
        m_Rigidbody.mass = m_BaseWeight * m_CurrentSize;
        
        // Update transparency while charging
        float chargePercent = m_CurrentSize / m_MaxSize;
        float transparency = Mathf.Lerp(m_ChargeTransparency, 1f, chargePercent);
        UpdateVisuals(transparency);        
    }

    private void UpdateDamping()
    {
        // Smaller size = more damping, larger size = less damping
        float dampingMultiplier = Mathf.Lerp(1.5f, 0.5f, (m_CurrentSize - m_MinSize) / (m_MaxSize - m_MinSize));
        m_Rigidbody.linearDamping = m_BaseDamping * dampingMultiplier;
    }

    private void ApplyBurst()
    {
        float totalForce = m_BopForce * (1f + m_CurrentCharge);
        Vector2 burstDirection = m_MoveDirection.normalized;
        
        if (burstDirection == Vector2.zero)
        {
            burstDirection = m_LastValidMoveDirection;
        }

        // Scale force based on size (smaller = less force)
        float sizeMultiplier = Mathf.Lerp(0.5f, 1f, (m_CurrentSize - m_MinSize) / (m_MaxSize - m_MinSize));
        totalForce *= sizeMultiplier;

        m_Rigidbody.AddForce(burstDirection * totalForce, ForceMode2D.Impulse);
        
        // Decrease size after burst with smaller rate
        m_CurrentSize = Mathf.Max(m_CurrentSize - m_DischargeShrinkRate, m_MinSize);
        UpdateSize(m_CurrentSize);
        
        // Reset charge
        m_CurrentCharge = 0f;
    }
    
    private void UpdateVisuals(float _chargePercent)
    {
        if (m_BubbleTransform != null && m_BubbleTransform.TryGetComponent<SpriteRenderer>(out var bubbleRenderer))
        {
            Color color = bubbleRenderer.color;
            
            if (_chargePercent > 0)
            {
                color.a = Mathf.Lerp(1f, m_ChargeTransparency, _chargePercent);
            }
            
            bubbleRenderer.color = color;
        }
    }

    public void Pop()
    {
        // TODO: Add pop effect, particles, sound
        Respawn();
    }

    private void Respawn()
    {
        // Reset position
        transform.position = m_StartPosition;
        
        // Reset physics
        m_Rigidbody.linearVelocity = Vector2.zero;
        m_Rigidbody.angularVelocity = 0f;
        
        // Reset size and visuals
        m_CurrentSize = 1f;
        m_CurrentCharge = 0f;
        UpdateSize(m_CurrentSize);
        UpdateVisuals(1f);
    }

    private PhysicsMaterial2D CreateBouncyMaterial()
    {
        var material = new PhysicsMaterial2D("BeetleBounce")
        {
            friction = 0f,
            bounciness = 0.2f
        };
        return material;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (m_IsShielded) return;

        // Check if we hit another beetle's bubble
        var otherBeetle = collision.gameObject.GetComponentInParent<BeetleBubble>();
        if (otherBeetle != null)
        {
            Debug.Log($"Beetle collision! Self size: {m_CurrentSize}, Other size: {otherBeetle.m_CurrentSize}");
            
            // Calculate relative velocity
            Vector2 relativeVelocity = m_Rigidbody.linearVelocity - otherBeetle.m_Rigidbody.linearVelocity;
            Vector2 collisionNormal = (transform.position - collision.transform.position).normalized;
            
            // Base bounce with coefficient of restitution of 1.2
            float bounceMultiplier = 1.2f;
            Vector2 bounceForce = collisionNormal * relativeVelocity.magnitude * bounceMultiplier;
            
            // Apply size difference as a small modifier
            float sizeDifference = m_CurrentSize - otherBeetle.m_CurrentSize;
            float bonusForce = m_BounceForce * sizeDifference;
            bounceForce += collisionNormal * bonusForce;
            
            // Apply the forces
            m_Rigidbody.AddForce(bounceForce, ForceMode2D.Impulse);
            otherBeetle.m_Rigidbody.AddForce(-bounceForce, ForceMode2D.Impulse);
            
            Debug.Log($"Applied bounce force: {bounceForce.magnitude}");
        }

        // Check if we hit a spike
        if (collision.gameObject.TryGetComponent<Spike>(out _))
        {
            OnHitSpike();
        }
    }

    private void UpdateSize(float _newSize)
    {
        m_CurrentSize = _newSize;
        // Scale the bubble
        if (m_BubbleTransform != null)
        {
            m_BubbleTransform.localScale = Vector3.one * m_CurrentSize;
        }
    }

    public void ResetState()
    {
        var playerInput = GetComponent<PlayerInput>();
        // Store the current beetle sprite before re-enabling children
        Sprite currentSprite = null;
        if (m_BeetleRenderer != null)
        {
            currentSprite = m_BeetleRenderer.sprite;
            Debug.Log($"BeetleBubble: ResetState - Player {playerInput?.playerIndex ?? -1} - " +
                      $"Stored current sprite. Sprite null? {currentSprite == null}, " +
                      $"Sprite name: {currentSprite?.name ?? "none"}");
        }

        // Re-enable visuals and physics
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
        
        // Restore the beetle sprite
        if (m_BeetleRenderer != null && currentSprite != null)
        {
            Debug.Log($"BeetleBubble: ResetState - Restoring sprite");
            m_BeetleRenderer.sprite = currentSprite;
        }
        else
        {
            Debug.LogWarning($"BeetleBubble: ResetState - Failed to restore sprite. BeetleRenderer null? {m_BeetleRenderer == null}, Stored sprite null? {currentSprite == null}");
        }
        
        if (m_Rigidbody != null)
        {
            m_Rigidbody.simulated = true;
            m_Rigidbody.linearVelocity = Vector2.zero;
            m_Rigidbody.angularVelocity = 0f;
        }
        
        // Reset size and visuals
        m_CurrentSize = 1f;
        m_CurrentCharge = 0f;
        UpdateSize(m_CurrentSize);
        UpdateVisuals(1f);
    }

    public void OnHitSpike()
    {
        if (IsShielded || m_IsDead) return;
        m_IsDead = true;
        m_LivesRemaining--;
        OnLivesChanged?.Invoke(m_LivesRemaining);

        // Disable visuals and physics
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        
        if (m_Rigidbody != null)
        {
            m_Rigidbody.simulated = false;
        }

        if (m_LivesRemaining <= 0)
        {
            GameManager.Instance.OnPlayerEliminated(GetComponent<PlayerInput>());
            // Don't start respawn sequence if eliminated
            return;
        }

        // Start respawn sequence
        StartCoroutine(RespawnSequence());
    }

    private System.Collections.IEnumerator RespawnSequence()
    {
        yield return new WaitForSeconds(m_RespawnDelay);
        
        // Request respawn from GameManager
        GameManager.Instance.RespawnPlayer(GetComponent<PlayerInput>());
        m_IsDead = false;
    }
    #endregion
} 