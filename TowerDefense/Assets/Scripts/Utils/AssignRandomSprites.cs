using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AssignRandomSprites : MonoBehaviour
{
    [System.Serializable]
    public struct SpritePair
    {
        public Sprite normal;
        public Sprite special;
    }

    [SerializeField] private SpritePair[] spritePairs;
    private SpritePair _choosePair;
    private SpriteRenderer _spriteRenderer;
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        ChooseRandomPair();
        SetSpecial(false);
    }
    private void ChooseRandomPair()
    {
        if(spritePairs == null || spritePairs.Length == 0)
        {
            return;
        }
        int index = Random.Range(0, spritePairs.Length);
        _choosePair = spritePairs[index];
    }
    public void SetSpecial(bool useSpecial)
    {
        if(_choosePair.normal == null)
        {
            return;
        }

        if(useSpecial && _choosePair.special != null)
        {
            _spriteRenderer.sprite = _choosePair.special;
        }
        else
        {
            _spriteRenderer.sprite = _choosePair.normal;
        }
    }

}
