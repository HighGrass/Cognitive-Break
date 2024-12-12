using UnityEngine;

public static class ColorUtils
{
    public static string ColorToHex(Color color)
    {
        // Converte cada componente da cor para um inteiro (0-255) e formata no estilo #RRGGBBAA
        int r = Mathf.RoundToInt(color.r * 255);
        int g = Mathf.RoundToInt(color.g * 255);
        int b = Mathf.RoundToInt(color.b * 255);
        int a = Mathf.RoundToInt(color.a * 255);

        // Formata como uma string hexadecimal
        return $"#{r:X2}{g:X2}{b:X2}{a:X2}";
    }
}