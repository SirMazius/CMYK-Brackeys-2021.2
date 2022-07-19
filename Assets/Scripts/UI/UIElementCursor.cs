using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIElementCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public static UIElementCursor CurrentHovered = null;
    public static UIElementCursor CurrentPressed = null;

    public bool IsHovered = false;
    public bool IsPressed = false;

    [HideInInspector]
    public UnityEvent OnPressed = new UnityEvent();
    [HideInInspector]
    public UnityEvent OnHoverStarts = new UnityEvent();


    public void OnPointerEnter(PointerEventData eventData)
    {
        IsHovered = true;
        CurrentHovered = this;
        OnHoverStarts.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsHovered = false;
        if (CurrentHovered == this)
            CurrentHovered = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        IsPressed = true;
        CurrentPressed = this;
        OnPressed.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsPressed = false;
        if (CurrentPressed == this)
            CurrentPressed = null;
    }
}
