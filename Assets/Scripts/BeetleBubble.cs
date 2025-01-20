using UnityEngine;
using UnityEngine.InputSystem;

public class BeetleBubble : MonoBehaviour
{
    #region Serialized Fields
    [Header("Input Properties")]
    [SerializeField] private InputActionAsset m_InputActions;
    
    [Header("Movement Properties")]
    [SerializeField, Range(0.1f, 10f)] private float m_BaseWeight = 1f;
    [SerializeField, Range(1f, 10f)] private float m_BopForce = 2f;
    [SerializeField, Range(1f, 25f)] private float m_ChargeRate = 2f;
    [SerializeField, Range(0f, 10f)] private float m_BaseDamping = 5f;
    
    [Header("Size Properties")]
    [SerializeField] private float m_MinSize = 0.5f;
    [SerializeField] private float m_MaxSize = 5f;  // Increased max size for more dramatic effect
    [SerializeField] private float m_ChargeGrowthRate = 0.5f;  // Fast growth while charging
    [SerializeField] private float m_DischargeShrinkRate = 0.1f;  // Smaller decrease when bursting
    
    [Header("Visual Properties")]
    [SerializeField, Range(0.2f, 1f)] private float m_ChargeTransparency = 0.5f;
    #endregion

    #region Private Fields
    private Rigidbody2D m_Rigidbody;
    private SpriteRenderer m_SpriteRenderer;
    private Vector2 m_MoveDirection;
    private Vector2 m_LastValidMoveDirection = Vector2.right;
    private float m_CurrentCharge;
    private bool m_IsCharging;
    private float m_CurrentSize = 1f;
    private Color m_OriginalColor;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        Debug.Log("BeetleBubble: Awake");
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
        
        if (m_SpriteRenderer == null)
        {
            Debug.LogError("BeetleBubble: No SpriteRenderer found!");
            return;
        }
        
        m_OriginalColor = m_SpriteRenderer.color;
        m_Rigidbody.mass = m_BaseWeight;
        m_Rigidbody.linearDamping = m_BaseDamping;
        m_Rigidbody.gravityScale = 0f;
        
        // Enable the input actions
        var playerInput = gameObject.AddComponent<PlayerInput>();
        playerInput.actions = m_InputActions;
        playerInput.defaultActionMap = "Player";  // Use the "Player" action map
        playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
        
        Debug.Log($"BeetleBubble: Input Actions loaded: {playerInput.actions != null}");
        
        // Debug available actions
        if (playerInput.actions != null)
        {
            foreach (var actionMap in playerInput.actions.actionMaps)
            {
                Debug.Log($"Action Map: {actionMap.name}");
                foreach (var action in actionMap.actions)
                {
                    Debug.Log($"  Action: {action.name}");
                }
            }
        }

        // Enable action map and subscribe to events
        var playerMap = playerInput.actions.FindActionMap("Player");
        playerMap.Enable();
        
        var chargeAction = playerMap.FindAction("Charge");
        var moveAction = playerMap.FindAction("Move");
        
        chargeAction.performed += ctx => OnCharge(ctx);
        chargeAction.canceled += ctx => OnCharge(ctx);
        moveAction.performed += ctx => OnMove(ctx);
        moveAction.canceled += ctx => OnMove(ctx);
    }

    private void OnDestroy()
    {
        // Clean up subscriptions if the object is destroyed
        if (m_InputActions != null)
        {
            var playerMap = m_InputActions.FindActionMap("Player");
            if (playerMap != null)
            {
                playerMap.Disable();
                var chargeAction = playerMap.FindAction("Charge");
                var moveAction = playerMap.FindAction("Move");
                
                if (chargeAction != null)
                {
                    chargeAction.performed -= ctx => OnCharge(ctx);
                    chargeAction.canceled -= ctx => OnCharge(ctx);
                }
                
                if (moveAction != null)
                {
                    moveAction.performed -= ctx => OnMove(ctx);
                    moveAction.canceled -= ctx => OnMove(ctx);
                }
            }
        }
    }

    private void Update()
    {
        if (m_IsCharging)
        {
            Charge();
        }
        
        // Update damping based on size
        UpdateDamping();
    }
    #endregion

    #region Input Handling
    public void OnMove(InputAction.CallbackContext _context)
    {
        m_MoveDirection = _context.ReadValue<Vector2>();
        
        // Store last valid direction
        if (m_MoveDirection != Vector2.zero)
        {
            m_LastValidMoveDirection = m_MoveDirection.normalized;
        }
        
        Debug.Log($"BeetleBubble: Move input received: {m_MoveDirection}");
    }

    public void OnCharge(InputAction.CallbackContext _context)
    {
        m_IsCharging = _context.performed;
        Debug.Log($"BeetleBubble: Charge input received: {m_IsCharging}");
        
        if (!m_IsCharging && m_CurrentCharge > 0)
        {
            ApplyBurst();
        }
        
        // Reset transparency when not charging
        if (!m_IsCharging)
        {
            UpdateVisuals(1f);
        }
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
        
        Debug.Log($"BeetleBubble: Charging - Size: {m_CurrentSize:F2}, Charge: {m_CurrentCharge:F2}");
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
        Debug.Log($"BeetleBubble: Burst applied - Direction: {burstDirection}, Force: {totalForce:F2}");
        
        // Decrease size after burst with smaller rate
        m_CurrentSize = Mathf.Max(m_CurrentSize - m_DischargeShrinkRate, m_MinSize);
        transform.localScale = Vector3.one * m_CurrentSize;
        
        // Reset charge
        m_CurrentCharge = 0f;
    }
    
    private void UpdateVisuals(float _alpha)
    {
        if (m_SpriteRenderer != null)
        {
            Color newColor = m_OriginalColor;
            newColor.a = _alpha;
            m_SpriteRenderer.color = newColor;
        }
    }
    #endregion
} 