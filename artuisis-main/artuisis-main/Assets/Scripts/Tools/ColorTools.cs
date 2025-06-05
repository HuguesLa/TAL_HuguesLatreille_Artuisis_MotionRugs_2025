using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class ColorTools
{
    #region Private fields
    private static float sectionSize = 1.0f / 6.0f;
    #endregion

    #region Methods - Private
    /// <summary>
    /// This method is an equation corresponding to a trapezoidale.
    /// Take in entry a float value belonging to the interval [O.Of,1.0f].
    /// If a value greater than 1.0f is set in parameter, a modulo will be done on it.
    /// If a value less than 0.0f, a logErrror will appear and an exception will be raised.
    /// </summary>
    /// <param name="x"> The <see cref="float"/> instance that represent a threshold value between [0.0f,1.0f]</param>
    /// <returns> A <see cref="float"/> value between [0.0f,1.0f] </returns>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// <paramref name="x"/> is less than 0.0f.
    /// </exception>
    private static float Equation(float x)
    {
        if (x < 0.0f)
        {
            Debug.LogError("Equation method can't take value less than 0.0f in parameter. Allowed interval [0.0f,1.0f]");
            throw new System.ArgumentOutOfRangeException();
        }

        x = x % 1.0f; //Protection against greater values than 1.0f
        float res = -1.0f;

        if (x >= 0.0f && x < sectionSize)
        {
            float a = 1.0f / 0.17f;
            res = a * x;
        }

        if (x >= sectionSize && x <= sectionSize * 3.0f)
        {
            res = 1.0f;
        }

        if (x > sectionSize * 3.0f && x < sectionSize * 4.0f)
        {
            float a = -(1.0f / 0.17f);
            float b = 1.0f;
            float xTemp = x - sectionSize * 3.0f;
            res = a * xTemp + b;
        }

        if (x >= sectionSize * 4.0f && x <= 1.0f)
        {
            res = 0.0f;
        }

        return res;
    }
    #endregion

    #region Methods - Public
   

    /// <summary>
    /// This method return a color based on the parameter value.
    /// </summary>
    /// <param name="val"> 
    /// The <see cref="float"/> instance that represent a threshold value between [0.0f,1.0f] 
    /// corresponding to the color circle (without black and white constrast) 
    /// </param>
    /// <returns>
    /// A <see cref="UnityEngine.Color"/> value. The corresponding color depends on the parameter value. Starting whith green color for a parameter value at 0.0f, Red at 0.33f and blue at 0.66f
    /// </returns>
    public static Color GetColor(float val)
    {
        return new Color(Equation(val), Equation((2 * sectionSize + val) % 1.0f), Equation((4 * sectionSize + val) % 1.0f));
    }

    /// <summary>
    /// This method create a list of colors, spaced at a fixed distance from each other.
    /// </summary>
    /// <param name="nbColor"> The <see cref="int"/> instance of the number of color generated </param>
    /// <param name="start"> The <see cref="int"/> instance of the starting color of the list, based on <see cref="ColorTools.GetColor(float)"/> </param>
    /// <returns> A <see cref="System.Collections.Generic.List{T}"/> of <see cref="UnityEngine.Color"/>.</returns>
    public static List<Color> GetColorPalette(int nbColor, float start = 0.0f)
    {
        List<Color> colorPalette = new List<Color>();

        float palier = 1.0f / nbColor;
        for (float i = 0.0f; i < 1.0f; i += palier)
        {
            colorPalette.Add(GetColor(i+start));        
        }

        return colorPalette;
    }

    /// <summary>
    /// This method create a list of colors, shuffled, based on the method <see cref="ColorTools.GetColorPalette(int, float)"/>.
    /// </summary>
    /// <param name="nbColor"> The <see cref="int"/> instance of the number of color generated </param>
    /// <param name="start"> The <see cref="int"/> instance of the starting color of the list, based on <see cref="ColorTools.GetColor(float)"/> </param>
    /// <returns> A shuffled <see cref="System.Collections.Generic.List{T}"/> of <see cref="UnityEngine.Color"/>.</returns>
    public static List<Color> GetShuffledColorPalette(int nbColor, float start = 0.0f)
    {
        List<Color> colorPalette = GetColorPalette(nbColor, start);
        var rnd = new System.Random();
        colorPalette = colorPalette.OrderBy(item => rnd.Next()).ToList<Color>();

        return colorPalette;
    }
    #endregion
}
