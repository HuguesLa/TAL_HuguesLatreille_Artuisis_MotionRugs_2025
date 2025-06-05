using System.Collections.Generic;
using UnityEngine;

public class ListTools
{
    public static float Mean(List<float> l)
    {
        float mean = 0.0f;
        foreach(float f in l)
        {
            mean += f;
        }
        return (mean / l.Count);
    }
    public static float StandardDeviation(List<float> l)
    {
        //Calcul de la moyenne
        float mean = Mean(l);

        float variance = 0.0f;
        foreach (float v in l)
        {
            float val = (v - mean);
            val = Mathf.Pow(val, 2);
            variance += val;
        }

        variance /= (l.Count - 1);

        float standardDeviation = Mathf.Sqrt(variance);
        return standardDeviation;
    }

    public static float Median(List<float> l)
    {
        float median;
        l.Sort(new GFG());

        int n = l.Count;
        if (n % 2 != 0)
        {
            n = n + 1;
            median = l[n / 2 - 1];
        }
        else
        {
            median = (l[n / 2 - 1] + l[n / 2]) / 2;
        }


        return median;
    }

    //Class allowing to sort float in a list using : list.Sort(new GFG());  
    public class GFG : IComparer<float>
    {
        public int Compare(float x, float y)
        {
            if (x == 0 || y == 0)
            {
                return 0;
            }

            // CompareTo() method
            return x.CompareTo(y);
        }
    }
}
