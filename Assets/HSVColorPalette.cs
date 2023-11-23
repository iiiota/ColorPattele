using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HSVColorPalette : MonoBehaviour
{
    private int row_size = 100;
    private int column_size = 100;
    private int light_row_size = 20;
    private int light_column_size = 100;
    private List<List<Color>> rgbColors = new List<List<Color>>();
    private List<List<Color>> lightRgbColors = new List<List<Color>>();

    private Texture2D imgTexture;
    private Vector2 imgTextureRtSize;
    private Texture2D lightTexture;
    private Vector2 lightTextureRtSize;

    public Camera Camera;
    public RawImage RawImage;
    public Color CurColor = Color.white;
    public Image ColorKnob;
    public Image PreviewImg;
    public RawImage lightRawImage;
    public Image lightKnob;

    private Vector3 curHsv;
    private Vector3 CurHsv
    {
        get
        {
            Color.RGBToHSV(CurColor, out curHsv.x, out curHsv.y, out curHsv.z);
            return curHsv;
        }
    }

    private Vector2 inputPos;
    private System.Action _update;

    void Start()
    {
        InitColorData();
        InitRtSize();
        InitTexture();
        ResetCurColor();
        ResetLightTexture();
        _update = UpdateIdle;
    }

    void Update()
    {
        _update?.Invoke();
    }

    private void UpdateIdle()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (HitTarget(RawImage.rectTransform, out inputPos))
            {
                _update = UpdatePalette;
            }
            else if (HitTarget(lightRawImage.rectTransform, out inputPos))
            {
                _update = UpdateLight;
            }
        }
    }

    private void UpdatePalette()
    {
        HitTarget(RawImage.rectTransform, out inputPos);
        ColorKnob.transform.localPosition = inputPos;
        ResetCurHsv(inputPos);
        ResetCurColor();
        if (Input.GetMouseButtonUp(0))
        {
            _update = UpdateIdle;
        }
    }

    private void UpdateLight()
    {
        HitTarget(lightRawImage.rectTransform, out inputPos);
        lightKnob.transform.localPosition = inputPos;
        ResetLightValue(inputPos);
        ResetCurColor();
        if (Input.GetMouseButtonUp(0))
        {
            _update = UpdateIdle;
        }
    }

    private void InitColorData()
    {
        rgbColors.Clear();
        for (int y = 0; y < column_size; y++)
        {
            var rowColors = new List<Color>();
            rgbColors.Add(rowColors);
            for (int x = 0; x < row_size; x++)
            {
                var h = (float)x / row_size;
                var s = (float)y / column_size;
                var rgb = Color.HSVToRGB(h, s, CurHsv.z);
                rowColors.Add(rgb);
            }
        }
    }

    private void InitRtSize()
    {
        imgTextureRtSize = RawImage.rectTransform.sizeDelta;
        lightTextureRtSize = lightRawImage.rectTransform.sizeDelta;
    }

    private void InitTexture()
    {
        imgTexture = new Texture2D(row_size, column_size);
        RawImage.texture = imgTexture;
        for (int y = 0; y < imgTexture.height; y++)
        {
            for (int x = 0; x < imgTexture.width; x++)
            {
                imgTexture.SetPixel(x, y, rgbColors[y][x]);
            }
        }
        imgTexture.Apply();
    }

    [ContextMenu("ResetCurColor")]
    private void ResetCurColor()
    {
        Color.RGBToHSV(CurColor, out curHsv.x, out curHsv.y, out curHsv.z);
        ResetColorKnob(CurHsv);
        ResetPreviewImg(CurHsv);
        ResetLightTexture();
    }

    private void ResetColorKnob(Vector3 hsv)
    {
        var color = Color.HSVToRGB(hsv.x, hsv.y, hsv.z);
        ColorKnob.color = color;
        lightKnob.color = color;
    }

    private void ResetPreviewImg(Vector3 hsv)
    {
        PreviewImg.color = Color.HSVToRGB(hsv.x, hsv.y, hsv.z);
    }

    private void ResetLightTexture()
    {
        ResetLightTextureData();
    }

    private void ResetLightTextureData()
    {
        if (lightTexture == null)
        {
            lightTexture = new Texture2D(light_row_size, light_column_size);
            lightRawImage.texture = lightTexture;
        }
        for (int y = 0; y < light_column_size; y++)
        {
            if (lightRgbColors.Count - 1 < y)
                lightRgbColors.Add(new List<Color>());
            for (int x = 0; x < light_row_size; x++)
            {
                var v = (float)y / light_column_size;
                var rgb = Color.HSVToRGB(CurHsv.x, CurHsv.y, v);
                lightRgbColors[y].Add(rgb);
                lightTexture.SetPixel(x, y, rgb);
            }
        }
        lightTexture.Apply();
    }

    private void ResetCurHsv(Vector2 localPos)
    {
        var h = (localPos.x + imgTextureRtSize.x / 2) / imgTextureRtSize.x;
        var s = (localPos.y + imgTextureRtSize.y / 2) / imgTextureRtSize.y;
        CurColor = Color.HSVToRGB(h, s, curHsv.z);
    }

    private void ResetLightValue(Vector2 localPos)
    {
        var v = (localPos.y + lightTextureRtSize.y / 2) / lightTextureRtSize.y;
        CurColor = Color.HSVToRGB(curHsv.x, curHsv.y, v);
    }

    private bool HitTarget(RectTransform rect, out Vector2 localPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, Camera, out localPos);
        var xMin = -rect.sizeDelta.x / 2;
        var xMax = -xMin;
        var yMin = -rect.sizeDelta.y / 2;
        var yMax = -yMin;
        localPos.x = Mathf.Clamp(localPos.x, xMin, xMax);
        localPos.y = Mathf.Clamp(localPos.y, yMin, yMax);
        if (localPos.x > xMin && localPos.x < xMax && localPos.y > yMin && localPos.y < yMax)
            return true;
        return false;
    }
}
