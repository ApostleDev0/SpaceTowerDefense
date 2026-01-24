using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerCard : MonoBehaviour
{
    #region Static Events
    public static event Action<TowerData> OnTowerSelected;
    #endregion

    #region Serialized Fields
    [SerializeField] private Image towerImage;
    [SerializeField] private TMP_Text costText;
    #endregion

    #region Private Fields
    private TowerData _towerData;
    #endregion

    public void Initialize(TowerData data)
    {
        if (data == null)
        {
            return;
        }
        _towerData = data;

        if (towerImage != null)
        {
            towerImage.sprite = data.sprite;
        }
        if(costText != null)
        {
            costText.text = $"${data.cost}";
        }
    }
    public void PlaceTower()
    {
        if (_towerData != null)
        {
            OnTowerSelected?.Invoke(_towerData);
        }
    }
}
