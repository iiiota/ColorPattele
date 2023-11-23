using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UGUIColorPalette : MonoBehaviour
{
    private int circle_in_size = 152;
    private int circle_out_size = 200;
    private int square_size = 100;
    private int rect_row_size = 20;
    private int rect_column_size = 100;
    private List<List<Color>> squareColors = new List<List<Color>>();
    private Texture2D squareTexture;

    public Camera camera;
    public RawImage circleRawImg;
    public RawImage squareRawImg;
    public Image circleKnob;
    public Image squareKnob;
    public Image previewImg;

    private Color paletteColor = Color.red;
    private float H = 0;
    private System.Action _update;

    private void Start()
    {
        InitCirclePatelle();
        _update = UpdateIdle;
    }

    private void Update()
    {
        _update?.Invoke();
    }

    private void UpdateIdle()
    {
        if (Input.GetMouseButton(0))
        {
            if (HitCircleTarget(out Vector2 pos))
            {
                _update = UpdateCircle;
            }
            else if (HitSquareTarget(out pos))
            {
                _update = UpdateSquare;
            }
        }
    }

    private bool HitCircleTarget(out Vector2 localPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(circleRawImg.rectTransform, Input.mousePosition, camera, out localPos);
        var mag = localPos.magnitude;
        var min = circle_in_size / 2;
        var max = circle_out_size / 2;
        localPos = localPos.normalized * (min + (max - min) / 2);
        return mag > min && mag < max;
    }

    private bool HitSquareTarget(out Vector2 targetPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(squareRawImg.rectTransform, Input.mousePosition, camera, out Vector2 localPos);
        var half = square_size / 2;
        targetPos.x = Mathf.Clamp(localPos.x, -half, half);
        targetPos.y = Mathf.Clamp(localPos.y, -half, half);
        return Mathf.Abs(localPos.x) < half && Mathf.Abs(localPos.y) < half;
    }

    private void UpdateCircle()
    {
        HitCircleTarget(out Vector2 localpos);
        circleKnob.transform.localPosition = localpos;
        paletteColor = GetCircleColor(localpos, out H);
        previewImg.color = paletteColor;
        squareKnob.color = H < 0.5 ? Color.black : Color.white;
        RefreshSquareColor();
        if (Input.GetMouseButtonUp(0))
            _update = UpdateIdle;
    }

    private void UpdateSquare()
    {
        HitSquareTarget(out Vector2 localpos);
        squareKnob.transform.localPosition = localpos;
        paletteColor = GetSquareColor(localpos, out float v);
        previewImg.color = paletteColor;
        squareKnob.color = v == 1 ? Color.black : Color.white;
        if (Input.GetMouseButtonUp(0))
            _update = UpdateIdle;
    }

    private void InitCirclePatelle()
    {
        var texure = new Texture2D(circle_out_size, circle_out_size);
        circleRawImg.texture = texure;
        var dMin = circle_in_size / 2;
        var dMax = circle_out_size / 2;
        var center = new Vector2(dMax, dMax);
        var colorsList = new List<List<Color>>();
        for (int y = 0; y < circle_out_size; y++)
        {
            colorsList.Add(new List<Color>());
            for (int x = 0; x < circle_out_size; x++)
            {
                var pos = new Vector2(y, x);
                var distance = Vector2.Distance(pos, center);
                if (distance < dMin || distance > dMax)
                    continue;
                var color = GetCircleColor(pos - center, out H);
                colorsList[y].Add(color);
                texure.SetPixel(y, x, color);
            }
        }
        texure.Apply();
    }

    private void RefreshSquareColor()
    {
        if(squareRawImg.texture == null)
        {
            squareTexture = new Texture2D(square_size, square_size);
            squareRawImg.texture = squareTexture;
        }
        for(int y = 0; y < square_size; y++)
        {
            if (squareColors.Count - 1 < y)
                squareColors.Add(new List<Color>());
            for(int x = 0; x < square_size; x++)
            {
                var s = (float)x / square_size;
                var v = (float)y / square_size;
                var rgb = Color.HSVToRGB(H, s, v);
                squareTexture.SetPixel(x, y, rgb);
                if (x < squareColors[y].Count)
                    squareColors[y][x] = rgb;
                else
                    squareColors[y].Add(rgb);
            }
            squareTexture.Apply();
        }

    }

    private Color GetCircleColor(Vector2 dir, out float h)
    {
        var angle = Vector2.SignedAngle(Vector2.right, dir);
        if (angle < 0)
            angle += 360;
        h = angle / 360;
        var color = Color.HSVToRGB(h, 1, 1);
        return color;
    }

    private Color GetSquareColor(Vector2 localpos, out float v)
    {
        var half = square_size / 2;
        var s = (localpos.x + half) / square_size;
        v = (localpos.y + half) / square_size;
        return Color.HSVToRGB(H, s, v);
    }

}
