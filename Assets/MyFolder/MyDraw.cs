using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class MyDraw : MonoBehaviour
{
    // 이 스크립트에서는 펜 색상과 두께를 PenManager에서 가져옴
    // 초기 penColor, penWidth 변수를 두지 않고, PenManager.Instance를 통해 참조
    
    private SpriteRenderer _spriteRenderer; // 스프라이트 렌더러 참조
    private Texture2D _drawTexture;         // 그림을 그릴 텍스처
    private Vector2 _previousPixelPos = Vector2.zero; // 이전 드래그 픽셀 위치 (선 연결용)
    private Color32[] _currentPixels;       // 텍스처의 픽셀 데이터(수정용)
    private Camera mainCam;                 // 메인 카메라 참조
    
    private void Awake()
    {
        // 이 오브젝트에 있는 SpriteRenderer 참조 획득
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 스프라이트에서 텍스처를 얻음
        // 이 텍스처는 반드시 Read/Write Enabled여야 픽셀 수정 가능
        _drawTexture = _spriteRenderer.sprite.texture;
    }

    private void Start()
    {
        // 메인 카메라 참조
        mainCam = Camera.main;
    }

    private void Update()
    {
        // 마우스 왼쪽 버튼 눌림 상태 확인
        bool mouseDown = Input.GetMouseButton(0);

        if (mouseDown)
        {
            // 카메라가 없으면 처리 종료
            if (!mainCam) return;
            
            // 마우스 스크린 좌표 → 월드 좌표
            Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0; // 2D 상에서 z는 0으로 고정

            // 마우스 위치가 이 오브젝트(스프라이트) 콜라이더 위인지 체크
            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);
            if (hit && hit.transform == this.transform)
            {
                // 스프라이트 위에서 마우스가 눌려있음
                // 월드 좌표를 텍스처 픽셀 좌표로 변환
                Vector2 pixelPos = WorldToPixelCoordinates(mouseWorldPos);

                // 현재 텍스처의 픽셀 데이터 가져오기 (처음 그릴 때만 GetPixels32)
                _currentPixels ??= _drawTexture.GetPixels32();

                // 펜 속성(PenColor, PenWidth)을 PenManager에서 획득
                Color penColor = PenManager.Instance ? PenManager.Instance.GetPenColor() : Color.white;
                int penWidth = PenManager.Instance ? PenManager.Instance.GetPenWidth() : 5;

                // 이전 드래그 위치(_previousPixelPos)가 없었다면(즉, 드래그 시작)
                // 현재 점 주변만 칠함
                if (_previousPixelPos == Vector2.zero)
                {
                    MarkPixelsToColor(pixelPos, penWidth, penColor);
                }
                else
                {
                    // 이전 점과 현재 점 사이를 선으로 연결해서 픽셀 칠하기
                    DrawLine(_previousPixelPos, pixelPos, penWidth, penColor);
                }

                // 변경된 픽셀 정보를 텍스처에 적용
                ApplyMarkedPixels();

                // 현재 픽셀 위치를 이전 위치로 저장 (다음 프레임 드래그 연결용)
                _previousPixelPos = pixelPos;
            }
            else
            {
                // 스프라이트 영역 밖이면 드래그 정보 초기화
                _previousPixelPos = Vector2.zero;
            }
        }
        else
        {
            // 마우스 버튼을 뗐다면 드래그 종료
            _previousPixelPos = Vector2.zero;
        }
    }

    /// <summary>
    /// 월드 좌표를 이 텍스처 내의 픽셀 좌표로 변환하는 함수
    /// 마우스 위치에 대응하는 텍스처상의 픽셀 인덱스를 얻기 위함
    /// </summary>
    private Vector2 WorldToPixelCoordinates(Vector3 worldPos)
    {
        float pixelWidth = _spriteRenderer.sprite.rect.width;   // 스프라이트 픽셀 너비
        float pixelHeight = _spriteRenderer.sprite.rect.height; // 스프라이트 픽셀 높이

        // 스프라이트의 월드 크기 대비 픽셀 크기 비율
        float unitsToPixels = pixelWidth / _spriteRenderer.bounds.size.x;

        // 월드 → 로컬 좌표 변환
        Vector3 localPos = transform.InverseTransformPoint(worldPos);

        // 로컬 좌표를 픽셀 좌표로 변환
        float x = localPos.x * unitsToPixels + pixelWidth / 2;
        float y = localPos.y * unitsToPixels + pixelHeight / 2;

        // 소수점 반올림해 정수 픽셀 좌표로 변환
        return new Vector2(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
    }

    /// <summary>
    /// 중심 픽셀 주변 영역(penRadius 반경)을 지정한 색으로 칠하는 함수
    /// </summary>
    private void MarkPixelsToColor(Vector2 centerPixel, int penRadius, Color color)
    {
        int centerX = (int)centerPixel.x;
        int centerY = (int)centerPixel.y;

        int width = _drawTexture.width;
        int height = _drawTexture.height;

        int radiusSquared = penRadius * penRadius; // 반지름 제곱값 미리 계산

        for (int x = centerX - penRadius; x <= centerX + penRadius; x++)
        {
            if (x < 0 || x >= width) continue;
            for (int y = centerY - penRadius; y <= centerY + penRadius; y++)
            {
                if (y < 0 || y >= height) continue;

                // 원 범위 체크
                int dx = x - centerX;
                int dy = y - centerY;
                int distSquared = dx * dx + dy * dy;

                if (distSquared <= radiusSquared)
                {
                    // 원 내부 픽셀에만 색칠
                    int index = y * width + x;
                    _currentPixels[index] = color;
                }
            }
        }
    }


    /// <summary>
    /// 변경된 _currentPixels 배열을 실제 텍스처에 적용
    /// 텍스처를 갱신해 화면에 반영한다.
    /// </summary>
    private void ApplyMarkedPixels()
    {
        _drawTexture.SetPixels32(_currentPixels);
        _drawTexture.Apply();
    }

    /// <summary>
    /// startPixel에서 endPixel까지 선을 그리듯 픽셀을 칠하는 함수
    /// startPixel → endPixel 사이를 lerp로 보간하며 MarkPixelsToColor 호출
    /// </summary>
    private void DrawLine(Vector2 startPixel, Vector2 endPixel, int width, Color color)
    {
        float distance = Vector2.Distance(startPixel, endPixel);
        float step = 1 / distance; // 픽셀 간 간격을 설정하기 위한 비율

        // 0부터 1까지 step 단위로 진행하며 점 사이의 픽셀을 채워
        for (float t = 0; t <= 1; t += step)
        {
            Vector2 pos = Vector2.Lerp(startPixel, endPixel, t);
            MarkPixelsToColor(pos, width, color);
        }
    }
}
