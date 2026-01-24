using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PlayButtonClick : MonoBehaviour, IPointerEnterHandler
{
    private Button _button;
    private void Awake()
    {
        _button = GetComponent<Button>();
    }
    private void OnDisable()
    {
        // prevent Memory Leak
        _button.onClick.RemoveListener(OnButtonClick);
    }
    private void OnButtonClick()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_button.interactable && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonHover();
        }
    }
}
