using System;
using System.Collections.Generic;
using UnityEngine;

public class SwarmMetrics
{
    #region Methods - Clusters

    /// <summary>
    /// Compute the expected size of the swarm clusters.
    /// To do it, divide the number of agents by the number of identified clusters
    /// </summary>
    /// <param name="swarmData"></param>
    /// <returns>A ratio (number of swarm agents)/(number of clusters) </returns>
    public static float ExpectedClusterSize(SwarmData swarmData)
    {
        List<List<AgentData>> clusters = SwarmTools.GetClusters(swarmData);

        int count = swarmData.GetAgentsData().Count;
        count /= clusters.Count;
        return count;
    }


    /// <summary>
    /// Compute the biggest cluster size
    /// </summary>
    /// <param name="swarmData"></param>
    /// <returns></returns>
    public static int LargestClusterSize(SwarmData swarmData)
    {
        List<List<AgentData>> clusters = SwarmTools.GetOrderedClusters(swarmData);
        if (clusters.Count > 0)
            return clusters[0].Count;
        else
            return 0;
    }

    /// <summary>
    /// Compare the size of the biggest cluster with the total number of agents of the swarm. 
    /// </summary>
    /// <param name="swarmData"></param>
    /// <returns>A ratio (cluster size)/(number of swarm agents)</returns>
    public static float LargestClusterSizeRatio(SwarmData swarmData)
    {
        float res = (float) LargestClusterSize(swarmData) / swarmData.GetAgentsData().Count;
        return res;
    }


    /// <summary>
    /// Compute the number of clusters
    /// </summary>
    /// <param name="swarmData"></param>
    /// <returns></returns>
    public static int ClusterNumber(SwarmData swarmData)
    {
        List<List<AgentData>> clusters = SwarmTools.GetOrderedClusters(swarmData);
        return clusters.Count;
    }
    #endregion

    #region Methods - Swarm motion
    /// <summary>
    /// Compute the difference between the distance covered by the swarm agents and the swarm.
    /// </summary>
    /// <param name="swarmData"></param>
    /// <param name="pastSwarmData"></param>
    /// <returns></returns>
    public static float EffectiveGroupMotion(SwarmData swarmData, SwarmData pastSwarmData)
    {
        Vector3 currentCenterOfMass = SwarmTools.GetCenterOfMass(swarmData);
        Vector3 pastCenterOfMass = SwarmTools.GetCenterOfMass(pastSwarmData);


        float distCM = Vector3.Distance(currentCenterOfMass, pastCenterOfMass);

        float meanDist = 0.0f;

        List<AgentData> currentAgents = swarmData.GetAgentsData();
        List<AgentData> pastAgents = pastSwarmData.GetAgentsData();
        for(int i=0; i<currentAgents.Count; i++)
        {
            meanDist += Vector3.Distance(pastAgents[i].GetPosition(), currentAgents[i].GetPosition());
        }

        meanDist /= currentAgents.Count;

        if (meanDist == 0.0f) return 0.0f; //Protect from division by 0
        else return (distCM / meanDist);
    }
    #endregion

    #region Methods - Swarm distance
    /// <summary>
    /// Compute the total distance between all swam agents, by summing the distances between each of them
    /// </summary>
    /// <param name="swarmData"></param>
    /// <returns></returns>
    public static float TotalDistance(SwarmData swarmData)
    {
        List<AgentData> agents = swarmData.GetAgentsData();
        float total = 0;
        int i, j;
        for (i = 0; i < agents.Count; i++)
        {
            for (j = i; j < agents.Count; j++)
            {
                if (i != j) total += Vector3.Distance(agents[i].GetPosition(), agents[j].GetPosition());
            }
        }
        return total;
    }


    /// <summary>
    /// Compute the average distance of each agent from the center of mass
    /// </summary>
    /// <param name="swarm"></param>
    /// <returns></returns>
    public static float MeanSquareDistanceFromCenterOfMass(SwarmData swarm)
    {
        float res = 0;
        Vector3 centerOfMass = SwarmTools.GetCenterOfMass(swarm);

        List<AgentData> agents = swarm.GetAgentsData();

        foreach (AgentData a in agents)
        {
            float val = Vector3.Distance(a.GetPosition(), centerOfMass);
            res += val * val;
        }

        res /= agents.Count;

        return res;
    }

    /// <summary>
    /// Compute the area of the box containing the swarm. 
    /// Then compare this area with the size of the map, to obtain a percentage of the area covered.
    /// </summary>
    /// <param name="swarmData"></param>
    /// <returns></returns>
    public static float BBR(SwarmData swarmData) //BoundingBoxRatio
    {
        List<AgentData> agents = swarmData.GetAgentsData();
        float bbr = 0.0f;

        if (agents.Count > 0)
        {
            float xMin = agents[0].GetPosition().x;
            float xMax = agents[0].GetPosition().x;
            float zMin = agents[0].GetPosition().z;
            float zMax = agents[0].GetPosition().z;
            int n = agents.Count;

            int i;
            for (i = 0; i < n; i++)
            {
                float xTemp = agents[i].GetPosition().x;
                float zTemp = agents[i].GetPosition().z;
                if (xTemp > xMax) xMax = xTemp;
                else if (xTemp < xMin) xMin = xTemp;

                if (zTemp > zMax) zMax = zTemp;
                else if (zTemp < zMin) zMin = zTemp;
            }

            bbr = ((xMax - xMin) * (zMax - zMin)) / (swarmData.GetParameters().GetMapSizeX() * swarmData.GetParameters().GetMapSizeZ());
        }
        return bbr;
    }
    #endregion

    #region Methods - Swarm speed

    /// <summary>
    /// Compute the average speed of the swarm
    /// </summary>
    /// <param name="swarmData"></param>
    /// <returns></returns>
    public static float AverageSpeed(SwarmData swarmData)
    {
        List<AgentData> agents = swarmData.GetAgentsData();
        float averageSpeed = 0.0f;
        foreach (AgentData a in agents)
        {
            Vector3 speed = a.GetSpeed();
            averageSpeed += speed.magnitude;
        }

        averageSpeed /= agents.Count;
        return averageSpeed;
    }

    /// <summary>
    /// Compute the average speed of the largest cluster of the swarm
    /// </summary>
    /// <param name="swarmData"></param>
    /// <returns></returns>
    public static float LargestClusterAverageSpeed(SwarmData swarmData)
    {
        List<List<AgentData>> clusters = SwarmTools.GetOrderedClusters(swarmData);

        if(clusters.Count>0)
        {
            List<AgentData> largestCluster = clusters[0];
            //Get average speed of the largest cluster
            float averageSpeed = 0.0f;
            foreach (AgentData a in largestCluster)
            {
                Vector3 speed = a.GetSpeed();
                averageSpeed += speed.magnitude;
            }

            averageSpeed /= largestCluster.Count;
            return averageSpeed;
        } else
        {
            return -1;
        }
    }


    /// <summary>
    /// Compute the ratio between the current speed and the max speed of the agents
    /// </summary>
    /// <param name="swarmData"></param>
    /// <param name="speed"></param>
    /// <returns></returns>
    public static float RescaledSpeed(SwarmData swarmData, float speed)
    {
        float res = speed / swarmData.GetParameters().GetMaxSpeed();
        return res;
    }

    #endregion

    #region Methods - Swarm direction

    /// <summary>
    /// Calculate whether the swarm is oriented towards the center of mass or not.
    /// </summary>
    /// <param name="swarmData"></param>
    /// <returns>A value between 0 and 1, 1 meaning towards the center of mass, 0 meaning away of the center of mass.</returns>
    public static float TowardsCenterOfMass(SwarmData swarmData)
    {
        List<AgentData> agents = swarmData.GetAgentsData();
        int n = agents.Count;

        Vector3 centerOfMass = SwarmTools.GetCenterOfMass(swarmData);
        float b = 0.0f;

        foreach (AgentData a in agents)
        {
            Vector3 speed = a.GetSpeed();
            Vector3 temp = centerOfMass - a.GetPosition();
            float angle = 0.0f;
            if (speed.magnitude == 0.0f)
            {
                angle = 90; //Represent the neutral angle, if the agent isn't moving.
            }
            else
            {
                angle = Vector3.Angle(speed, temp);
            }


            b += angle;
        }
        float res = 1 - ((b / n) / 180);
        return res;
    }

    public static float TowardsCenterOfMassStandardDeviation(SwarmData s)
    {
        List<AgentData> agents = s.GetAgentsData();

        Vector3 centerOfMass = SwarmTools.GetCenterOfMass(s);
        List<float> l = new List<float>();

        foreach (AgentData a in agents)
        {
            Vector3 speed = a.GetSpeed();
            Vector3 temp = centerOfMass - a.GetPosition();
            float angle = 0.0f;
            if (speed.magnitude == 0.0f)
            {
                angle = 90; //Represent the neutral angle, if the agent isn't moving.
            }
            else
            {
                angle = Vector3.Angle(speed, temp);
            }


            l.Add(1 - (angle / 180));
        }
        return ListTools.StandardDeviation(l);
    }


    /// <summary>
    /// Compute the ratio indicating whether the directions of swarm agents are aligned or not.
    /// </summary>
    /// <param name="swarmData"></param>
    /// <returns></returns>
    public static float Order(SwarmData swarmData)
    {
        List<AgentData> agents = swarmData.GetAgentsData();
        int n = agents.Count;

        Vector3 b = Vector3.zero;
        foreach (AgentData a in agents)
        {
            Vector3 speed = a.GetSpeed();
            Vector3 direction = speed.normalized;
            b += direction;
        }

        float psi = Vector3.Magnitude(b) / ((float)n);
        return psi;
    }

    public static float AverageSpeedDirection(SwarmData swarmData)
    {
        Vector3 averageOrientation = Vector3.zero;
        foreach (AgentData a in swarmData.GetAgentsData())
        {
            averageOrientation += a.GetSpeed();
        }
        return (Mathf.Atan2(averageOrientation.z, averageOrientation.x) / Mathf.PI) * 180;
    }

    public static float StandardDeviationOfKnnDirection(SwarmData f)
    {
        List<AgentData> agents = f.GetAgentsData();
        List<float> directionDiff = new List<float>();

        foreach (AgentData a in agents)
        {
            List<AgentData> knn = SwarmTools.KNN(a, agents, 3);
            foreach (AgentData n in knn)
            {
                float angleDiff = Vector3.Angle(a.GetSpeed(), n.GetSpeed()) / 180.0f;
                directionDiff.Add(angleDiff);
            }
        }

        return ListTools.StandardDeviation(directionDiff);
    }


    public static float KNNOrder(SwarmData swarmData,int k)
    {
        List<AgentData> agents = swarmData.GetAgentsData();
        int n = agents.Count;
        float total = 0;

        foreach (AgentData a in agents)
        {
            List<AgentData> knn = SwarmTools.KNN(a, agents, k);
            Vector3 b = a.GetSpeed().normalized;
            foreach (AgentData i in knn)
            {
                Vector3 speed = i.GetSpeed();
                Vector3 direction = speed.normalized;
                b += direction;   
            }
            float psi = Vector3.Magnitude(b) / ((float)(knn.Count+1));
            total += psi;
        }

        total = total / n;
        return total;



    }
    #endregion

    #region Methods - Neighbourhood
    /// <summary>
    /// Compute the average number of neighbour of the swarm agents
    /// </summary>
    /// <param name="swarmData"></param>
    /// <returns></returns>
    public static float AverageNeighbourhood(SwarmData swarmData)
    {
        //Create a clone of the agents list, to manipulate it
        List<AgentData> agents = swarmData.GetAgentsData();

        float total = 0.0f;
        foreach (AgentData a in agents)
        {
            List<AgentData> temp = SwarmTools.GetNeighbours(a,agents,swarmData.GetParameters().GetFieldOfViewSize(), swarmData.GetParameters().GetBlindSpotSize());
            total += temp.Count;

        }
        total = total / agents.Count;

        return total;
    }
    #endregion

    #region Methods - Fracture
    public static float FractureVisibilityScore(SwarmData swarmData)
    {
        float meanDist = SwarmTools.MeanKNNDistanceBiggerCluster(swarmData, 3);

        float dist = SwarmTools.GetSignificantDistanceBetweenClusters(swarmData);

        if(meanDist == 0.0f) return -1; //This metric can't handle that situation currently
        float ratio = dist / meanDist;

        return ratio;
    }

    #endregion

    #region Methods - Densities
    // WORK IN PROGRESS


    //Calculer max taille trou
    //Calculer taille moyenne des trous
    //Calculer nombre moyen de trous
    //Calculer nombre moyen de trous sur taille total de l'essaim
    //Tout sur n seconds

    /// <summary>
    /// Get the number of empty spaces within the external enveloppe of the swarm using densities' grid.
    /// </summary>
    /// <param name="swarmData">The analysed swarm state</param>
    /// <returns>Return the number of empty spaces</returns>
    public static int GetNumberOfEmptySpace(SwarmData swarmData)
    {
        int total = 0;

        Tuple<int[,], float> t = SwarmTools.GetDensitiesUsingKNNWithinConvexArea(swarmData);
        int[,] densities = t.Item1;

        for (int i = 0; i < densities.GetLength(0); i++)
        {
            for (int j = 0; j < densities.GetLength(1); j++)
            {
                if (densities[i, j] == 0) total++;
            }
        }

        return total;
    }
    #endregion

}
