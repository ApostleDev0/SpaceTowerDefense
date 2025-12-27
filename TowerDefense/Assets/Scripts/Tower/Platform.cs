using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public static event Action<Platform> OnPLatformClicked;
    [SerializeField] private LayerMask platformLayerMask;
    public static bool towerPanelOpen { get; set; } = false;
    private SpriteRenderer _spriteRenderer;

    public Tower tower;
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        if (towerPanelOpen || Time.timeScale == 0f)
        {
            return;
        }

        if(Input.GetMouseButtonDown(0))
        {
            Vector3 mouseScreenPosition = Input.mousePosition;
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
            RaycastHit2D raycastHit = Physics2D.Raycast(worldPoint,Vector2.zero,Mathf.Infinity,platformLayerMask);

            if(raycastHit.collider != null)
            {
                Platform hitPlatform = raycastHit.collider.GetComponent<Platform>();
                if(hitPlatform != null && hitPlatform == this)
                {
                    OnPLatformClicked?.Invoke(hitPlatform);
                }
            }

        }
    }
    public void PlaceTower(TowerData data)
    {
        GameObject newTowerObj = Instantiate(data.prefab,transform.position, Quaternion.identity,transform);

        tower = newTowerObj.GetComponent<Tower>();

        if(_spriteRenderer != null)
        {
            _spriteRenderer.enabled = false;
        }
    }
    public void ResetPlatform()
    {
        tower = null; 
        if (_spriteRenderer != null)
        {
            _spriteRenderer.enabled = true;
        }
    }
}
