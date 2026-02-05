using UnityEngine;

namespace LWT.UnityWorkbench.Utilities
{
    /// <summary>
    /// Extension methods for Color.
    /// </summary>
    public static class ColorExtensions
    {
        #region Component Modification

        /// <summary>
        /// Returns a new Color with modified red value.
        /// </summary>
        public static Color WithR(this Color c, float r) => new Color(r, c.g, c.b, c.a);

        /// <summary>
        /// Returns a new Color with modified green value.
        /// </summary>
        public static Color WithG(this Color c, float g) => new Color(c.r, g, c.b, c.a);

        /// <summary>
        /// Returns a new Color with modified blue value.
        /// </summary>
        public static Color WithB(this Color c, float b) => new Color(c.r, c.g, b, c.a);

        /// <summary>
        /// Returns a new Color with modified alpha value.
        /// </summary>
        public static Color WithAlpha(this Color c, float a) => new Color(c.r, c.g, c.b, a);

        #endregion

        #region Conversion

        /// <summary>
        /// Converts to Color32.
        /// </summary>
        public static Color32 ToColor32(this Color c) => c;

        /// <summary>
        /// Converts to Vector3 (RGB).
        /// </summary>
        public static Vector3 ToVector3(this Color c) => new Vector3(c.r, c.g, c.b);

        /// <summary>
        /// Converts to Vector4 (RGBA).
        /// </summary>
        public static Vector4 ToVector4(this Color c) => new Vector4(c.r, c.g, c.b, c.a);

        /// <summary>
        /// Converts to hex string.
        /// </summary>
        public static string ToHex(this Color c, bool includeAlpha = false)
        {
            Color32 c32 = c;
            return includeAlpha
                ? $"#{c32.r:X2}{c32.g:X2}{c32.b:X2}{c32.a:X2}"
                : $"#{c32.r:X2}{c32.g:X2}{c32.b:X2}";
        }

        /// <summary>
        /// Creates a Color from hex string.
        /// </summary>
        public static Color FromHex(string hex)
        {
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);

            if (hex.Length == 6)
            {
                byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                return new Color32(r, g, b, 255);
            }
            else if (hex.Length == 8)
            {
                byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                byte a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                return new Color32(r, g, b, a);
            }

            Debug.LogWarning($"Invalid hex color: {hex}");
            return Color.magenta;
        }

        #endregion

        #region HSV

        /// <summary>
        /// Converts to HSV values.
        /// </summary>
        public static void ToHSV(this Color c, out float h, out float s, out float v)
        {
            Color.RGBToHSV(c, out h, out s, out v);
        }

        /// <summary>
        /// Returns the hue (0-1).
        /// </summary>
        public static float GetHue(this Color c)
        {
            Color.RGBToHSV(c, out float h, out _, out _);
            return h;
        }

        /// <summary>
        /// Returns the saturation (0-1).
        /// </summary>
        public static float GetSaturation(this Color c)
        {
            Color.RGBToHSV(c, out _, out float s, out _);
            return s;
        }

        /// <summary>
        /// Returns the value/brightness (0-1).
        /// </summary>
        public static float GetValue(this Color c)
        {
            Color.RGBToHSV(c, out _, out _, out float v);
            return v;
        }

        /// <summary>
        /// Returns a new Color with modified hue.
        /// </summary>
        public static Color WithHue(this Color c, float h)
        {
            Color.RGBToHSV(c, out _, out float s, out float v);
            return Color.HSVToRGB(h, s, v).WithAlpha(c.a);
        }

        /// <summary>
        /// Returns a new Color with modified saturation.
        /// </summary>
        public static Color WithSaturation(this Color c, float s)
        {
            Color.RGBToHSV(c, out float h, out _, out float v);
            return Color.HSVToRGB(h, s, v).WithAlpha(c.a);
        }

        /// <summary>
        /// Returns a new Color with modified value/brightness.
        /// </summary>
        public static Color WithValue(this Color c, float v)
        {
            Color.RGBToHSV(c, out float h, out float s, out _);
            return Color.HSVToRGB(h, s, v).WithAlpha(c.a);
        }

        #endregion

        #region Manipulation

        /// <summary>
        /// Returns the inverted color (1 - value for RGB).
        /// </summary>
        public static Color Inverted(this Color c)
        {
            return new Color(1f - c.r, 1f - c.g, 1f - c.b, c.a);
        }

        /// <summary>
        /// Returns the grayscale version of the color.
        /// </summary>
        public static Color Grayscale(this Color c)
        {
            float gray = c.grayscale;
            return new Color(gray, gray, gray, c.a);
        }

        /// <summary>
        /// Returns a brighter version of the color.
        /// </summary>
        public static Color Brighten(this Color c, float amount)
        {
            Color.RGBToHSV(c, out float h, out float s, out float v);
            v = Mathf.Clamp01(v + amount);
            return Color.HSVToRGB(h, s, v).WithAlpha(c.a);
        }

        /// <summary>
        /// Returns a darker version of the color.
        /// </summary>
        public static Color Darken(this Color c, float amount)
        {
            return c.Brighten(-amount);
        }

        /// <summary>
        /// Returns a more saturated version of the color.
        /// </summary>
        public static Color Saturate(this Color c, float amount)
        {
            Color.RGBToHSV(c, out float h, out float s, out float v);
            s = Mathf.Clamp01(s + amount);
            return Color.HSVToRGB(h, s, v).WithAlpha(c.a);
        }

        /// <summary>
        /// Returns a less saturated version of the color.
        /// </summary>
        public static Color Desaturate(this Color c, float amount)
        {
            return c.Saturate(-amount);
        }

        /// <summary>
        /// Lerp between two colors.
        /// </summary>
        public static Color LerpTo(this Color from, Color to, float t)
        {
            return Color.Lerp(from, to, t);
        }

        #endregion

        #region Comparison

        /// <summary>
        /// Checks if colors are approximately equal.
        /// </summary>
        public static bool Approximately(this Color a, Color b, float tolerance = 0.01f)
        {
            return Mathf.Abs(a.r - b.r) < tolerance &&
                   Mathf.Abs(a.g - b.g) < tolerance &&
                   Mathf.Abs(a.b - b.b) < tolerance &&
                   Mathf.Abs(a.a - b.a) < tolerance;
        }

        /// <summary>
        /// Returns the perceived luminance (human eye weighted).
        /// </summary>
        public static float GetLuminance(this Color c)
        {
            return 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
        }

        /// <summary>
        /// Checks if the color is considered dark.
        /// </summary>
        public static bool IsDark(this Color c)
        {
            return c.GetLuminance() < 0.5f;
        }

        /// <summary>
        /// Returns black or white depending on which contrasts better.
        /// </summary>
        public static Color GetContrastColor(this Color c)
        {
            return c.IsDark() ? Color.white : Color.black;
        }

        #endregion
    }
}
