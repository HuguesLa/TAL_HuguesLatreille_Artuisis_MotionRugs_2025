using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

public class ClipMetrics
{
    #region Methods - Fracture frame
    public static int GetFractureFrame(SwarmClip c, float fMinTime = 2.0f)
    {
        int res = -1;
        int i = 0;

        int validationFrames = (int)((1.0f / Time.fixedDeltaTime) * 2.0f);
        int fractureDuration = 0;
        foreach (SwarmData f in c.GetFrames())
        {
            List<List<AgentData>> clusters = SwarmTools.GetClusters(f);
            if (clusters.Count > 1)
            {
                if (fractureDuration == 0)
                    res = i;

                fractureDuration++;

                if (fractureDuration > validationFrames)
                    break;
            }
            else
            {
                fractureDuration = 0;
                res = -1;
            }
            i++;
        }
        return res;
    }
    #endregion

    #region Methods - Effective group motion
    public static float MeanEffectiveGroupMotion(SwarmClip c)
    {
        List<SwarmData> frames = c.GetFrames();
        int nbFrames = frames.Count;

        float meanValue = 0.0f;
        int i = 1;
        while (true)
        {
            float temp = SwarmMetrics.EffectiveGroupMotion(frames[i], frames[i-1]);

            meanValue += temp;
            i++;
            if (i >= nbFrames) break; ;
        }

        meanValue /= (i - 1);

        return meanValue;
    }
    #endregion

    #region Methods - Order
    public static float MeanOrder(SwarmClip c)
    {
        List<float> orders = new List<float>();
        foreach (SwarmData s in c.GetFrames())
        {
           orders.Add(SwarmMetrics.Order(s));
        }
        
        return ListTools.Mean(orders);
    }

    public static float MeanKNNOrder(SwarmClip c,int k)
    {
        List<float> orders = new List<float>();
        foreach (SwarmData s in c.GetFrames())
        {
            orders.Add(SwarmMetrics.KNNOrder(s,k));
        }

        return ListTools.Mean(orders);
    }

    public static float MedianOrder(SwarmClip c)
    {
        List<float> orders = new List<float>();
        foreach (SwarmData s in c.GetFrames())
        {
            orders.Add(SwarmMetrics.Order(s));
        }

        return ListTools.Median(orders);
    }
    #endregion

    #region Methods - Towards center of mass
    public static float MeanTowardsCenterOfMass(SwarmClip c)
    {
        List<float> tcm = new List<float>();
        foreach (SwarmData s in c.GetFrames())
        {
            if (SwarmTools.GetClusters(s).Count > 1) break;

            tcm.Add(SwarmMetrics.TowardsCenterOfMass(s));
        }

        return ListTools.Mean(tcm);
    }

    public static float MeanTowardsCenterOfMassStandardDeviation(SwarmClip c)
    {
        List<float> tcmStd = new List<float>();
        foreach (SwarmData s in c.GetFrames())
        {
            if (SwarmTools.GetClusters(s).Count > 1) break;

            tcmStd.Add(SwarmMetrics.TowardsCenterOfMassStandardDeviation(s));
        }

        return ListTools.Mean(tcmStd);
    }
    #endregion

    #region Methods - Expansion score
    public static float ExpansionScore(SwarmClip c)
    {
        float startValue = SwarmTools.MeanKNNDistanceBiggerCluster(c.GetFrames()[0], 3);
        float endValue = SwarmTools.MeanKNNDistanceBiggerCluster(c.GetFrames()[c.GetFrames().Count - 1], 3);

        float res = (endValue / startValue);

        return res;
    }
    #endregion

    #region Methods - Fracture visibility score

    public static float BestFractureVisibilityScore(SwarmClip c)
    {
        float bestScore = -1;
        int startFrame = GetFractureFrame(c);

        for (int i = startFrame; i < c.GetFrames().Count; i++)
        {
            float score = SwarmMetrics.FractureVisibilityScore(c.GetFrames()[i]);
            if (score > bestScore)
            {
                bestScore = score;
            }
        }

        return bestScore;
    }

    #endregion

    #region Methods - Knn direction
    public static float MeanStandardDeviationOfKnnDirection(SwarmClip c)
    {
        List<SwarmData> frames = c.GetFrames();
        int n = ClipMetrics.GetFractureFrame(c);

        if (n == -1) n = frames.Count;

        List<float> stdKnnDir = new List<float>();
        for (int i = 0; i < n; i++)
        {
            stdKnnDir.Add(SwarmMetrics.StandardDeviationOfKnnDirection(frames[i]));
        }

        return ListTools.Mean(stdKnnDir);
     }
    #endregion

    #region - Empty spaces

    public static float GetMeanEmptySpacesWithinLastNSecond(SwarmClip c, float seconds)
    {
        int nbFrames = (int)(seconds / Time.fixedDeltaTime);

        if(c.GetFrames().Count<nbFrames) nbFrames = c.GetFrames().Count;

        int start = (c.GetFrames().Count - nbFrames);

        int total = 0;
        for (int i = start;i<c.GetFrames().Count;i++ )
        {
            total += SwarmMetrics.GetNumberOfEmptySpace(c.GetFrames()[i]);
        }

        float res = total/ nbFrames;

        return res;
    }

    #endregion
}
