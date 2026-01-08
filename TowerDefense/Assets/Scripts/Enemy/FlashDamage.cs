using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashDamage : MonoBehaviour
{
    [SerializeField] private Color flashColor = Color.red; 
    [SerializeField] private float duration = 0.2f;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private Color _originalColor;
    private Coroutine _flashRoutine;

    void Awake()
    {
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        //if (_spriteRenderer != null)
        //{
        //    _originalColor = _spriteRenderer.color;
        //}
        _originalColor = Color.white;
    }
    void OnEnable()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _originalColor;
            _flashRoutine = null;
        }
    }
    void OnDisable()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _originalColor;
        }
    }

    public void Flash()
    {
        if (_spriteRenderer == null)
        {
            return;
        }

        if (_flashRoutine != null)
        {
            StopCoroutine(_flashRoutine); 
        }

        _flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        _spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(duration);
        _spriteRenderer.color = _originalColor;
        _flashRoutine = null;
    }
}
