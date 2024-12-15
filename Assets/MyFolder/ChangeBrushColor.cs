using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChangeBrushColor : MonoBehaviour, IPointerDownHandler
{
    private Color _color;
    private PenManager _penManager;
    private Button _button;

    [SerializeField] private bool isEraser;
    private void Awake()
    {
        _color = GetComponent<Image>().color;
        if (isEraser)
        {
            _color = Color.clear;
        }
    }

    private void Start()
    {
        _penManager = PenManager.Instance;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _penManager.SetPenColor(_color);
    }
}
