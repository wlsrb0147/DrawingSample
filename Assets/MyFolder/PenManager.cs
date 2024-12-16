// PenManager.cs

using System;
using UnityEngine;
using UnityEngine.UI;

public class PenManager : MonoBehaviour
{
    public static PenManager Instance;

    [SerializeField] private Color penColor = Color.white;
    [SerializeField] private int penWidth = 5;
    [SerializeField] private RectTransform penIndicator; // 펜 두께를 표시할 스프라이트 Transform
    private Image image;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        image = penIndicator.GetComponent<Image>();
        SetPenWidth(0.5f);
    }

    public Color GetPenColor()
    {
        return penColor;
    }

    public int GetPenWidth()
    {
        return penWidth;
    }

    public void SetPenColor(Color newColor)
    {
        penColor = newColor;
        image.color = newColor;
    }
    
    public void SetPenWidth(float width)
    {
        penWidth = Mathf.RoundToInt(width * 50f); // 예: width=1이면 penWidth=10 정도로 확
        penIndicator.sizeDelta = new Vector2(penWidth, penWidth);
    }
}