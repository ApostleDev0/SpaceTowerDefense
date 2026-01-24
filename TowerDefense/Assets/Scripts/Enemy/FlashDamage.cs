using System.Collections;
using UnityEngine;

public class FlashDamage : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private Color flashColor = Color.red; 
    [SerializeField] private float duration = 0.2f;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    #endregion

    #region Private Fields
    private Color _originalColor;
    private Coroutine _flashRoutine;
    private WaitForSeconds _wait;
    #endregion

    void Awake()
    {
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        _originalColor = Color.white;

        // setup before start
        _wait = new WaitForSeconds(duration);
    }
    private void OnEnable()
    {
        ResetColor();
        _flashRoutine = null;
    }
    private void OnDisable()
    {
        ResetColor();
        if (_flashRoutine != null)
        {
            StopCoroutine(_flashRoutine);
            _flashRoutine = null;
        }
    }

    public void Flash()
    {
        if (_spriteRenderer == null || !gameObject.activeInHierarchy)
        {
            return;
        }

        // reset time
        if (_flashRoutine != null)
        {
            StopCoroutine(_flashRoutine); 
        }

        _flashRoutine = StartCoroutine(FlashRoutine());
    }
    private IEnumerator FlashRoutine()
    {
        _spriteRenderer.color = flashColor;
        yield return _wait;
        _spriteRenderer.color = _originalColor;
        _flashRoutine = null;
    }
    private void ResetColor()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _originalColor;
        }
    }
}
