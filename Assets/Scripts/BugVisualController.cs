using UnityEngine;

public class BugVisualController : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private SpriteRenderer m_BugRenderer;
    [SerializeField] private Sprite m_BugSprite;
    [SerializeField] private float m_BaseScale = 2f;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        SetupBugSprite();
    }
    #endregion

    #region Public Methods
    public void UpdateScale(float _bubbleScale)
    {
        if (m_BugRenderer != null)
        {
            transform.localScale = Vector3.one * (m_BaseScale / _bubbleScale);
        }
    }
    #endregion

    #region Private Methods
    private void SetupBugSprite()
    {
        if (m_BugRenderer == null)
        {
            m_BugRenderer = GetComponent<SpriteRenderer>();
        }

        if (m_BugRenderer == null)
        {
            m_BugRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        if (m_BugSprite != null)
        {
            m_BugRenderer.sprite = m_BugSprite;
        }
    }
    #endregion
} 