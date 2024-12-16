using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TransparentBackground : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Texture2D _texture;
    private Color32[] _cleanPixels; // 초기화용 투명 픽셀 배열
    private MyDraw _draw;
    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _texture = _spriteRenderer.sprite.texture;

        // 텍스처 전체를 투명한 픽셀로 채울 배열 준비
        _cleanPixels = new Color32[_texture.width * _texture.height];
        for (int i = 0; i < _cleanPixels.Length; i++)
        {
            // 모든 픽셀을 투명 (0,0,0,0)으로 설정
            _cleanPixels[i] = new Color32(0,0,0,0);
        }
        _draw = GetComponent<MyDraw>();
    }

    void Start()
    {
        // 게임 시작 시 텍스처를 투명하게 리셋
        ResetCanvas();
    }

    public void ResetCanvas()
    {
        _texture.SetPixels32(_cleanPixels);
        _texture.Apply();
        _draw.SetCurrentPixels(null);
    }
}
