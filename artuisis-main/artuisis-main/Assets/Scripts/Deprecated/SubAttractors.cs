using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubAttractors 
{
    /*
    private void FindSubAttractors()
    {
        List<Vector3> intersectionPoints = GetIntersectionPoints();
        intersectionPoints = RemovingPointOutsideMapBoundaries(intersectionPoints);
        //intersectionPoints = IdentifySpatialPointClusters(intersectionPoints, thresholdDistance);
        MassCentersAndWeights res = MergePoints(intersectionPoints, thresholdDistance);
        intersectionPoints = res.massCenters;

        //(Temporaire) afficher les points pour vérifier que ça fonctionne
        foreach (GameObject t in subAttractorPoints)
        {
            Destroy(t);
        }

        subAttractorPoints.Clear();

        int j = 0;
        foreach (Vector3 p in intersectionPoints)
        {
            GameObject t = Instantiate(prefab);
            t.transform.parent = null;
            Vector3 pt = new Vector3(p.x, 0.2f, p.z);

            //Vector3 st = new Vector3(Mathf.Clamp(res.weights[j]* 0.01f, 0.01f , 0.3f), 0.01f, Mathf.Clamp(res.weights[j] * 0.01f, 0.01f, 0.3f));
            Vector3 st = new Vector3(0.05f, 0.01f, 0.05f);

            t.transform.position = pt;
            t.transform.localScale = st;
            subAttractorPoints.Add(t);
            j++;
        }
        //--
    }

    private List<Vector3> GetIntersectionPoints()
    {
        int n = agents.Count;

        List<Vector3> intersectionPoints = new List<Vector3>();

        for (int i = 0; i < n; i++)
        {
            Vector3 speedI = agents[i].GetComponent<Agent>().GetSpeed();
            if (speedI.magnitude == 0.0f) continue; //If the agent doesn't move, go to the next

            Vector3 positionI = agents[i].transform.position;

            //Calculate the coefficients of the equation of the first line
            float a1 = speedI.z / speedI.x;
            float b1 = positionI.z - a1 * positionI.x;
            //--

            for (int j = i + 1; j < n; j++)
            {
                Vector3 speedJ = agents[j].GetComponent<Agent>().GetSpeed();
                if (speedJ.magnitude == 0.0f) continue; //If the agent doesn't move, go to the next

                Vector3 positionJ = agents[j].transform.position;

                //Calculate the coefficients of the equation of the second line
                float a2 = speedJ.z / speedJ.x;
                float b2 = positionJ.z - a2 * positionJ.x;
                //--

                if ((a1 == a2) && (b1 != b2)) continue; //If both line are parallel but distinct, continue

                if (a1 != a2)
                {
                    float x = (b2 - b1) / (a1 - a2);

                    float z = a1 * x + b1;

                    if (float.IsNaN(x) || float.IsNaN(z)) continue;

                    if ((Mathf.Abs(x) > manager.GetMapSizeX()) || (Mathf.Abs(z) > manager.GetMapSizeZ())) continue;

                    Vector3 newIntersection = new Vector3(x, 0.0f, z);

                    if (Vector3.Distance(positionI, newIntersection) > parameterManager.GetFieldOfViewSize()) continue;
                    if (Vector3.Distance(positionI, positionJ) > parameterManager.GetFieldOfViewSize()) continue;

                    Vector3 tempI = newIntersection - positionI;
                    Vector3 tempJ = newIntersection - positionJ;
                    float angleI = Vector3.Angle(speedI, tempI);
                    float angleJ = Vector3.Angle(speedJ, tempJ);

                    if (angleI > 90 || angleJ > 90) continue;

                    intersectionPoints.Add(newIntersection);
                }
            }
        }
        return intersectionPoints;
    }

    private List<Vector3> RemovingPointOutsideMapBoundaries(List<Vector3> positions)
    {
        List<Vector3> clearedPositions = new List<Vector3>();
        foreach (Vector3 p in positions)
        {
            if (p.x > manager.GetMapSizeX() || p.x < 0.0f || p.z > manager.GetMapSizeZ() || p.z < 0.0f) continue;
            clearedPositions.Add(p);
        }
        return clearedPositions;
    }

    private MassCentersAndWeights MergePoints(List<Vector3> points, float threshold)
    {
        //--Part 1, identify neighbourhood
        List<List<int>> allNeighbourhoods = new List<List<int>>();

        for (int i = 0; i < points.Count; i++)
        {
            List<int> neighbourhood = new List<int>();
            for (int j = 0; j < points.Count; j++)
            {
                //Check if we are comparing the same two points
                if (i == j) continue;

                //Check distance between the two points based on a threshold value
                float dist = Vector3.Distance(points[i], points[j]);
                if (dist <= threshold)
                {
                    //That's a neighbour
                    neighbourhood.Add(j);
                }
            }
            //Adding the point i' neighbourhood at the neighbourhood list
            allNeighbourhoods.Add(neighbourhood);
        }


        if (allNeighbourhoods.Count != points.Count) Debug.LogError("Il y a une erreur de la recherche de voisinnage. Il n'y a pas autant de voisinnage que d'agents");

        List<Vector3> massCenters = new List<Vector3>();
        List<int> weights = new List<int>();
        for (int i = 0; i < allNeighbourhoods.Count; i++)
        {
            Vector3 massCenter = points[i];
            int weight = allNeighbourhoods[i].Count;
            foreach (int n in allNeighbourhoods[i])
            {
                massCenter += points[n] * allNeighbourhoods[n].Count;
                weight += allNeighbourhoods[n].Count;
            }
            if (weight == 0.0f) continue;
            massCenter /= weight;
            massCenters.Add(massCenter);
            weights.Add(weight);
        }
        MassCentersAndWeights res = new MassCentersAndWeights();
        res.massCenters = massCenters;
        res.weights = weights;

        return res;


    }

    private List<Vector3> IdentifySpatialPointClusters(List<Vector3> points, float threshold)
    {
        //--Part 1, identify neighbourhood
        List<List<int>> allNeighbourhoods = new List<List<int>>();

        for (int i = 0; i < points.Count; i++)
        {
            List<int> neighbourhood = new List<int>();
            for (int j = 0; j < points.Count; j++)
            {
                //Check if we are comparing the same two points
                if (i == j) continue;

                //Check distance between the two points based on a threshold value
                float dist = Vector3.Distance(points[i], points[j]);
                if (dist <= threshold)
                {
                    //That's a neighbour
                    neighbourhood.Add(j);
                }
            }
            //Adding the point i' neighbourhood at the neighbourhood list
            allNeighbourhoods.Add(neighbourhood);
        }


        if (allNeighbourhoods.Count != points.Count) Debug.LogError("Il y a une erreur de la recherche de voisinnage. Il n'y a pas autant de voisinnage que d'agents");



        //--Part 2, identify clusters
        List<List<int>> pointClusters = new List<List<int>>();

        //Create a clone of the points list, to manipulate it
        List<Vector3> pointsClone = new List<Vector3>(points);

        while (pointsClone.Count > 0)
        {
            //Add first agent in the new cluster
            List<int> newCluster = new List<int>();
            Vector3 firstPoint = pointsClone[0];
            pointsClone.Remove(firstPoint);
            newCluster.Add(points.IndexOf(firstPoint));

            int i = 0;
            while (i < newCluster.Count)
            {
                List<int> temp = allNeighbourhoods[i];
                foreach (int g in temp)
                {
                    if (!newCluster.Contains(g))
                    {
                        bool res = pointsClone.Remove(points[g]);
                        if (res) newCluster.Add(g);
                    }
                }
                i++;

            }
            pointClusters.Add(newCluster);
        }


        int count = 0;
        foreach (List<int> l in pointClusters)
        {
            count += l.Count;
        }

        if (count != points.Count) Debug.LogError("Erreur dans l'identification des clusters, nb de point attendus: " + points.Count + " et nombre de point obtenus: " + count);


        //--Part 3, identify mass center of clusters
        List<Vector3> clusterMassCenters = new List<Vector3>();

        foreach (List<int> l in pointClusters)
        {
            Vector3 massCenter = new Vector3();
            float total = 0.0f;
            foreach (int v in l)
            {
                massCenter += points[v] * allNeighbourhoods[v].Count;
                total += allNeighbourhoods[v].Count;
            }
            if (total == 0.0f) continue;
            massCenter /= total;
            clusterMassCenters.Add(massCenter);
        }

        return clusterMassCenters;
    }



#endregion


    */

    public class MassCentersAndWeights
    {
        public List<Vector3> massCenters = new List<Vector3>();
        public List<int> weights = new List<int>();
    }

}


