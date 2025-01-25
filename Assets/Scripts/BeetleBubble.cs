using UnityEngine;
using UnityEngine.InputSystem;

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

    [SerializeField] private float m_BounceForce = 1f;  // Multiplier for bounce force
    #endregion

    #region Private Fields
    private Rigidbody2D m_Rigidbody;
    private SpriteRenderer m_SpriteRenderer;
    private Vector2 m_MoveDirection;
    private Vector2 m_LastValidMoveDirection = Vector2.right;
    private float m_CurrentCharge;
    private bool m_IsCharging;
    private float m_CurrentSize = 1f;
    private Color m_BaseColor = Color.white;  // Base color is white
    private Vector3 m_StartPosition;
    private bool m_IsShielded;
    #endregion

    #region Public Properties
    public float ChargeRate
    {
        get => m_ChargeRate;
        set => m_ChargeRate = value;
    }

    public bool IsShielded
    {
        get => m_IsShielded;
        set => m_IsShielded = value;
    }
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        Debug.Log($"BeetleBubble: Awake for Player {GetComponent<PlayerInput>()?.playerIndex ?? -1}");
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
        
        if (m_SpriteRenderer == null)
        {
            Debug.LogError("BeetleBubble: No SpriteRenderer found!");
            return;
        }
        
        m_Rigidbody.mass = m_BaseWeight;
        m_Rigidbody.linearDamping = m_BaseDamping;
        m_Rigidbody.gravityScale = 0f;
        m_StartPosition = transform.position;

        // Configure physics for bouncing
        if (m_Rigidbody != null)
        {
            m_Rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            m_Rigidbody.sharedMaterial = CreateBouncyMaterial();
        }
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
        transform.localScale = Vector3.one * m_CurrentSize;
        
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
        transform.localScale = Vector3.one * m_CurrentSize;
        
        // Reset charge
        m_CurrentCharge = 0f;
    }
    
    private void UpdateVisuals(float _chargePercent)
    {
        Color targetColor = m_BaseColor;
        
        // Apply charge effect (only modify alpha)
        if (_chargePercent > 0)
        {
            targetColor.a = Mathf.Lerp(1f, m_ChargeTransparency, _chargePercent);
        }
        
        // Apply shield effect on top if active
        if (m_IsShielded)
        {
            Color shieldTint = new Color(0, 0.7f, 1f, 0.5f);
            targetColor = Color.Lerp(targetColor, shieldTint, 0.5f);
        }

        // Apply the final color
        if (m_SpriteRenderer != null)
        {
            m_SpriteRenderer.color = targetColor;
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
        transform.localScale = Vector3.one;
        UpdateVisuals(1f);
    }

    private PhysicsMaterial2D CreateBouncyMaterial()
    {
        var material = new PhysicsMaterial2D("BubbleBounce")
        {
            friction = 0f,
            bounciness = 1f
        };
        return material;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (m_IsShielded) return; // Ignore collisions when shielded
        
        // Handle other collision logic here
    }
    #endregion
} 