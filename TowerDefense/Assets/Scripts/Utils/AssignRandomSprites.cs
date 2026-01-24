using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AssignRandomSprites : MonoBehaviour
{
    #region Serialized Fields
    [System.Serializable]
    public struct SpritePair
    {
        public Sprite normal;
        public Sprite special;
    }
    [SerializeField] private SpritePair[] spritePairs;
    #endregion

    #region Private Fields
    private SpritePair _currentPair;
    private SpriteRenderer _spriteRenderer;
    private bool _initialized = false;
    #endregion
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _initialized = true;
    }
    private void OnEnable()
    {
        // random sprite when object turn on
        if (_initialized)
        {
            ChooseRandomPair();
            SetSpecial(false);
        }
    }
    private void ChooseRandomPair()
    {
        if(spritePairs == null || spritePairs.Length == 0)
        {
            Debug.LogWarning($"AssignRandomSprites: Not Adding Sprite Pairs to {gameObject.name}!");
            return;
        }
        int index = Random.Range(0, spritePairs.Length);
        _currentPair = spritePairs[index];
    }
    public void SetSpecial(bool useSpecial)
    {
        if (_spriteRenderer == null)
        {
            return;
        }

        if (useSpecial && _currentPair.special != null)
        {
            _spriteRenderer.sprite = _currentPair.special;
        }
        else if (_currentPair.normal != null)
        {
            _spriteRenderer.sprite = _currentPair.normal;
        }
    }

}
