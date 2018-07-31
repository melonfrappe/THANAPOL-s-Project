using UnityEngine;

public static class ConvertExtensions
{
    public static Sprite ToSprite(this Texture2D texture2D)
    {
        return texture2D.ToSprite(new Rect(0.0f, 0.0f, texture2D.width, texture2D.height),
                new Vector2(0.5f, 0.5f),
                100.0f);
    }

    public static Sprite ToSprite(this Texture2D texture2D, Rect rect, Vector2 pivot, float pixelsPerUnit = 100f)
    {
        return Sprite.Create(texture2D, rect, pivot, pixelsPerUnit);
    }
}