using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    #region Events & Static State
    public static event Action<Platform> OnPLatformClicked;
    public static bool towerPanelOpen { get; set; } = false;
    #endregion

    #region Public Properties
    public Tower CurrentTower { get; private set; }
    #endregion

    #region Private Fields
    private SpriteRenderer _spriteRenderer;
    private Color _startColor;
    private Color _hoverColor;
    #endregion

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
        {
            _startColor = _spriteRenderer.color;
            // set colour brighter
            _hoverColor = new Color(_startColor.r, _startColor.g, _startColor.b, 0.7f);
        }
    }

    //====INPUT HANDLE
    private void OnMouseDown()
    {
        // check condition prevent click
        if (towerPanelOpen || Time.timeScale == 0f)
        {
            return;
        }

        // prevent click throught UI
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        OnPLatformClicked?.Invoke(this);
    }
    private void OnMouseEnter()
    {
        if (towerPanelOpen || Time.timeScale == 0f)
        {
            return;
        }
        if (_spriteRenderer != null && CurrentTower == null)
        {
            _spriteRenderer.color = _hoverColor;
        }
    }
    private void OnMouseExit()
    {
        if (_spriteRenderer != null && CurrentTower == null)
        {
            _spriteRenderer.color = _startColor;
        }
    }

    //====LOGIC
    public void PlaceTower(TowerData data)
    {
        if (data == null || data.Prefab == null)
        {
            return;
        }
        GameObject newTowerObj = Instantiate(data.Prefab,transform.position, Quaternion.identity,transform);

        CurrentTower = newTowerObj.GetComponent<Tower>();

        if (_spriteRenderer != null)
        {
            _spriteRenderer.enabled = false;
            _spriteRenderer.color = _startColor;
        }
    }
    public void ResetPlatform()
    {
        CurrentTower = null;
        if (_spriteRenderer != null)
        {
            _spriteRenderer.enabled = true;
            _spriteRenderer.color = _startColor;
        }
    }
    public void SetTower(Tower tower)
    {
        CurrentTower = tower;
        if (_spriteRenderer != null)
        {
            _spriteRenderer.enabled = false;
        }
    }
}
