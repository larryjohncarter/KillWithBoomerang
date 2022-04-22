using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class InputManager : SingletonBehaviour<InputManager>
{
#pragma warning disable 0649
    [SerializeField] private InputReceiver _inputReceive;
#pragma warning restore 0649
    public Action<PointerEventData> OnPointerDownEvent;
    public Action<PointerEventData> OnPointerUpEvent;
    public Action<PointerEventData> OnDragEvent;

    protected override void Awake()
    {
        base.Awake();
        _inputReceive.OnPointerDownEvent = OnPointDown;
        _inputReceive.OnPointerUpEvent = OnPointUp;
        _inputReceive.OnDragEvent = OnDrag;
    }

    private void OnDrag(PointerEventData data)
    {
        OnDragEvent?.Invoke(data);
    }

    private void OnPointDown(PointerEventData data)
    {
        OnPointerDownEvent?.Invoke(data);
    }
    private void OnPointUp(PointerEventData data)
    {
        OnPointerUpEvent?.Invoke(data);
    }
}