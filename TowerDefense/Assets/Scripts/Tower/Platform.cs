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
                Platform platform = raycastHit.collider.GetComponent<Platform>();
                if(platform != null)
                {
                    OnPLatformClicked?.Invoke(platform);
                }
            }

        }
    }
    public void PlaceTower(TowerData data)
    {
        Instantiate(data.prefab,transform.position, Quaternion.identity,transform);
        if(_spriteRenderer != null)
        {
            _spriteRenderer.enabled = false;
        }
    }
}
