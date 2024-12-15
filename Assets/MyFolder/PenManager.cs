// PenManager.cs
using UnityEngine;

public class PenManager : MonoBehaviour
{
    public static PenManager Instance;

    [SerializeField] private Color penColor = Color.white;
    [SerializeField] private int penWidth = 5;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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
    }
    
    public void SetPenWidth(float width)
    {
        penWidth = Mathf.RoundToInt(width * 10f); // 예: width=1이면 penWidth=10 정도로 확대
    }
}