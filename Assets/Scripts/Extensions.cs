using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class Extensions
{
    const string
        ext_timeDays = "d ",
        ext_timeHours = "h ",
        ext_timeMinutes = "m ",
        ext_timeSeconds = "s ",
        ext_comma = ", ",
        ext_leftBracket = "(",
        ext_rightBracket = ")",
        ext_notApplicable = "n/a",
        ext_zeroSec = "0s";

    /// <summary>
    /// Sets this transform and all its children to the specified layer
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="layer"></param>
    public static void SetChildLayers(this Transform t, int layer)
    {
        for (int i = 0; i < t.childCount; i++)
        {
            Transform child = t.GetChild(i);
            child.gameObject.layer = layer;
            SetChildLayers(child, layer);
        }
    }
    public static Quaternion ReflectRotation(this Quaternion source, Vector3 normal)
    {
        return Quaternion.LookRotation(Vector3.Reflect(source * Vector3.forward, normal), Vector3.Reflect(source * Vector3.up, normal));
    }
    /// <summary>
    /// Converts a time given in seconds to days, hours, minutes and seconds (e.g. 1d 19h 35m 7s)
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    public static string ConvertTime(this double seconds)
    {
        TimeSpan ts = TimeSpan.FromSeconds((int)seconds);
        return
            (ts.Days > 0 ? ts.Days.ToString() + ext_timeDays : string.Empty) +
            (ts.Hours > 0 ? ts.Hours.ToString() + ext_timeHours : string.Empty) +
            (ts.Minutes > 0 ? ts.Minutes.ToString() + ext_timeMinutes : string.Empty) +
            (ts.Seconds > 0 ? ts.Seconds.ToString() + ext_timeSeconds : ext_zeroSec);
    }
    /// <summary>
    /// Calculates the greatest common denominator of a and b
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static float CalcGCD(float a, float b)
    {
        while (a != 0f && b != 0f)
        {
            if (a > b)
                a %= b;
            else
                b %= a;
        }
        if (a == 0f)
            return b;
        else
            return a;
    }
    /// <summary>
    /// Converts a Vector3 to a StringBuilder containing the values like this (x, y, z)
    /// </summary>
    /// <param name="vector3"></param>
    /// <returns></returns>
    public static StringBuilder ToStringBuilder(this Vector3 vector3)
    {
        return new StringBuilder().Append(ext_leftBracket)
            .Append(vector3.x.ToString()).Append(ext_comma)
            .Append(vector3.y.ToString()).Append(ext_comma)
            .Append(vector3.z.ToString()).Append(ext_rightBracket);
    }
    /// <summary>
    /// Converts a Vector3 to a StringBuilder containing the values like this (x, y, z)
    /// </summary>
    /// <param name="vector2"></param>
    /// <returns></returns>
    public static StringBuilder ToStringBuilder(this Vector2 vector2)
    {
        return new StringBuilder().Append(ext_leftBracket)
            .Append(vector2.x.ToString()).Append(ext_comma)
            .Append(vector2.y.ToString()).Append(ext_rightBracket);
    }
    /// <summary>
    /// Converts an array of Colliders to a StringBuilder containing a comma-separated list of all the Collider's GameObject's names
    /// </summary>
    /// <param name="colliders"></param>
    /// <param name="ifNullText"></param>
    /// <param name="ifEmptyText"></param>
    /// <returns></returns>
    public static StringBuilder ToStringBuilder(this Collider[] colliders, string ifNullText = ext_notApplicable, string ifEmptyText = ext_notApplicable)
    {
        StringBuilder a = new();
        if (colliders == null) { return a.Append(ifNullText); }
        if (colliders.Length == 0) { return a.Append(ifEmptyText); }
        foreach (Collider collider in colliders) { if (collider != null) { a.Append(collider.gameObject.name).Append(ext_comma); } }
        return a;
    }
    public static bool AllCharsAreDigits(this char[] chars)
    {
        List<bool> isDigitBools = new();
        foreach (char _char in chars) { isDigitBools.Add(char.IsDigit(_char)); }
        return isDigitBools.All(x => x);
    }
    /// <summary>
    /// Converts a horizontal field of view value to a vertical field of view value (viewportHeight == Camera.pixelHeight)
    /// </summary>
    /// <param name="horizontalFOV"></param>
    /// <param name="viewportHeight"></param>
    /// <param name="viewportWidth"></param>
    /// <param name="clampFOV1to179"></param>
    /// <returns>
    /// Vertical field of view value
    /// </returns>
    public static float FOVHorizontalToVertical(float horizontalFOV, float viewportHeight, float viewportWidth, bool clampFOV1to179 = true)
    {
        return 2 * Mathf.Atan(Mathf.Tan((clampFOV1to179 ? Mathf.Clamp(horizontalFOV, 1, 179) : horizontalFOV) * Mathf.PI / 180 / 2) * viewportHeight / viewportWidth) * 180 / Mathf.PI;
    }
    /// <summary>
    /// Converts a horizontal field of view value to a vertical field of view value
    /// </summary>
    /// <param name="horizontalFOV"></param>
    /// <param name="viewportHeight"></param>
    /// <param name="viewportWidth"></param>
    /// <param name="clampFOV1to179"></param>
    /// <returns>
    /// Vertical field of view value
    /// </returns>
    public static float FOVHorizontalToVertical(float horizontalFOV, Camera camera, bool clampFOV1to179 = true)
    {
        return 2 * Mathf.Atan(Mathf.Tan((clampFOV1to179 ? Mathf.Clamp(horizontalFOV, 1, 179) : horizontalFOV) * Mathf.PI / 180 / 2) * camera.pixelHeight / camera.pixelWidth) * 180 / Mathf.PI;
    }
    public static Vector3 DirectionFromAngleY(float eulerY, float angleInDegrees) 
    {
        angleInDegrees += eulerY;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
    public static Vector3 DirectionFromAngleX(float eulerX, float angleInDegrees) 
    {
        angleInDegrees += eulerX;
        return new Vector3(0, Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0);
    }
    public static Vector3 Round(this Vector3 vector3, int digits = 0)
    {
        return new Vector3(MathF.Round(vector3.x, digits), MathF.Round(vector3.y, digits), MathF.Round(vector3.z, digits));
    }
    public static float FloatFromAxis(bool positive, bool negative) =>
    (positive, negative) switch
    {
        (true, false) => 1f,
        (false, true) => -1f,
        _ => 0f
    };
}