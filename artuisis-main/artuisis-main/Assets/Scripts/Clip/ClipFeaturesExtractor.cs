using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class ClipFeaturesExtractor : MonoBehaviour
{
    string filePath = "/Clips/"; //The folder containing clip files

    private List<SwarmClip> clips = new List<SwarmClip>();

    private int currentClip = 0;
    // Start is called before the first frame update
    void Start()
    {
        filePath = Application.dataPath + filePath;
        Debug.Log(filePath);

        string[] filePaths = Directory.GetFiles(filePath, "*.dat",
                                         SearchOption.TopDirectoryOnly);



        string resultFilePathCSV = Application.dataPath + "/Results/clip_features.csv";

        StringBuilder sb = new StringBuilder();

        //Prepare csv result file
        string line = "Filename;" +
            "Duration;" +
            "FinalExpansionDistance;"+
            "ExpansionSpeed;"+
            "MeanStandardDeviationOfKnnDirection;"+
            "MeanEffectiveGroupMotion;" +
            "MeanKNNOrder;" +
            "MeanTowardsCenterOfMass;" +
            "AverageDistanceTravelledByAgentsPerSecond;" +
            "ChangeOfAgentsLinks;" +
            "ConvexHulArea; " +
            "MeanEmptySpaces(1s); " +
            "CohesionIntensity; " +
            "SeparationIntensity; " +
            "AlignmentIntensity; " +
            "RandomIntensity; " +
            "MaxSpeed;" +
            "FieldOfVision;" +
            "PerceptionRange\r";
        sb.Append(line);

        //Load all clip
        for (int i = 0; i < filePaths.Length; i++)
        {
            //Loading clip from full file path
            SwarmClip clip = ClipTools.LoadClip(filePaths[i]);

            if (clip != null)
            {
                clips.Add(clip); //Add the loaded clip to the list
                Debug.Log("Clip " + filePaths[i] + " Loaded.");
            }
            else
                Debug.LogError("Clip can't be load from " + filePaths[i], this);
        }

        Debug.Log("Clips are loaded.");


      
        //Analyse part
        foreach (SwarmClip c in clips)
        {
            int fractureFrame = ClipMetrics.GetFractureFrame(c);


            string s = filePaths[currentClip];
            int pos = s.IndexOf("/");
            while(pos!=-1)
            {
                s = s.Substring(pos + 1);
                pos = s.IndexOf("/");
            }
            line = s + ";"
                + (c.GetFrames().Count * Time.fixedDeltaTime) + ";"
                + SwarmMetrics.TotalDistance(c.GetFrames()[c.GetFrames().Count - 1]) + ";"
                + GetExpansionSpeed(c) + ";"
                + ClipMetrics.MeanStandardDeviationOfKnnDirection(c) + ";"
                + ClipMetrics.MeanEffectiveGroupMotion(c) + ";"
                + ClipMetrics.MeanKNNOrder(c,3) + ";"
                + ClipMetrics.MeanTowardsCenterOfMass(c) + ";"
                + AverageDistanceTravelledByAgentsPerSecond(c) + ";"
                + ChangeOfAgentsLinks(c) + ";"
                + SwarmTools.GetConvexHulArea(c.GetFrames()[c.GetFrames().Count - 1]) + ";"
                + ClipMetrics.GetMeanEmptySpacesWithinLastNSecond(c, 1.0f) + ";"
                + (c.GetFrames()[0].GetParameters().GetCohesionIntensity()) + ";"
                + (c.GetFrames()[0].GetParameters().GetSeparationIntensity()) + ";"
                + (c.GetFrames()[0].GetParameters().GetAlignmentIntensity()) + ";"
                + (c.GetFrames()[0].GetParameters().GetRandomMovementIntensity()) + ";"
                + (c.GetFrames()[0].GetParameters().GetMaxSpeed()) + ";"
                + (360 - c.GetFrames()[0].GetParameters().GetBlindSpotSize()) + ";"
                + (c.GetFrames()[0].GetParameters().GetFieldOfViewSize())  + "\r";
            sb.Append(line);
         
            currentClip++;
            
        }

        string res = sb.ToString();
        res= res.Replace(",", ".");
        res = res.Replace(";", ",");

        //Save result
        File.AppendAllText(resultFilePathCSV, res);
        sb.Clear();

        Debug.Log("Results saved.");
    }

    
    private float GetExpansionSpeed(SwarmClip c)
    {
        float delay = c.GetFrames().Count * Time.fixedDeltaTime;
        float finalDistance = SwarmMetrics.TotalDistance(c.GetFrames()[c.GetFrames().Count - 1]);
        float startDistance = SwarmMetrics.TotalDistance(c.GetFrames()[0]);

        float speed = (finalDistance - startDistance) / delay;

        return speed;
    }

    private float AverageDistanceTravelledByAgentsPerSecond(SwarmClip c)
    {
        float meanDistance = 0;
        List<SwarmData> swarms = c.GetFrames();

        for(int i=0;i<swarms.Count-1;i++)
        {
            int agentCount = swarms[i].GetAgentsData().Count;

            float totalDistance = 0;
            for(int j=0;j<agentCount;j++)
            {
                totalDistance += Vector3.Distance(swarms[i].GetAgentsData()[j].GetPosition(), swarms[i+1].GetAgentsData()[j].GetPosition());
            }
            meanDistance += totalDistance / agentCount;
        }

        meanDistance = meanDistance/ ((c.GetFrames().Count-1) * Time.fixedDeltaTime);

        return meanDistance;

    }


    private float ChangeOfAgentsLinks (SwarmClip c)
    {
        //TO DO
        List<SwarmData> swarms = c.GetFrames();
        float meanChanges = 0.0f;
        for (int i = 0; i < swarms.Count - 1; i++)
        {
            int agentCount = swarms[i].GetAgentsData().Count;

            float totalChanges = 0.0f;
            for (int j = 0; j < agentCount; j++)
            {
                List<int> neighbours = SwarmTools.GetNeighboursIDs(swarms[i].GetAgentsData()[j], swarms[i].GetAgentsData(), swarms[i].GetParameters().GetFieldOfViewSize(), swarms[i].GetParameters().GetBlindSpotSize());

                List<int> nextNeighbours = SwarmTools.GetNeighboursIDs(swarms[i+1].GetAgentsData()[j], swarms[i+1].GetAgentsData(), swarms[i+1].GetParameters().GetFieldOfViewSize(), swarms[i+1].GetParameters().GetBlindSpotSize());

                int count = 0;
                foreach(int id in neighbours)
                {
                    if(nextNeighbours.Contains(id))
                    {
                        count++;
                    }
                }

                totalChanges += neighbours.Count + nextNeighbours.Count - (2 * count);

            }
            meanChanges += totalChanges / agentCount;

        }
        meanChanges = meanChanges / ((c.GetFrames().Count - 1) * Time.fixedDeltaTime);

        return meanChanges;
    }







    //Trouver la frame de la fracture
    //R�cuperer le premier groupe de la fracture
    //Mesurer la distance la plus petite entre les deux groupes
    //Identifier les deux agents les plus proches par leur ID
    //Mesurer sur les 10 frames pr�c�dentes la vitesse d'�loignement
    //retourner cette vitesse
    //Faire �a sur tous les groupes?

    private float BestSeparationSpeed(SwarmClip c)
    {
        int fracFrame = ClipMetrics.GetFractureFrame(c);
        if (fracFrame == -1) throw new System.Exception("Il n'y a pas de fracture dans ce clip");

        float maxSepSpeed = float.MinValue;

        for (int i = fracFrame; i < c.GetFrames().Count; i++)
        {
            float res = SeparationSpeed(c, i);
            if (res > maxSepSpeed)
            {
                maxSepSpeed = res;
            }
        }

        return maxSepSpeed;
    }

    private float SeparationSpeed(SwarmClip c, int frame)
    {

        List<AgentData> agents = c.GetFrames()[frame].GetAgentsData();

        List<List<AgentData>> clusters = SwarmTools.GetOrderedClusters(c.GetFrames()[frame]);

        float maxSepSpeed = float.MinValue;

        foreach(List<AgentData> cluster in clusters)
        {
            List<AgentData> temp = new List<AgentData>(agents);

            foreach (AgentData a in cluster)
            {
                temp.Remove(a);
            }

            AgentData a1 = null;
            AgentData a2 = null;
            float minDist = float.MaxValue;

            foreach (AgentData a in cluster)
            {
                foreach (AgentData b in temp)
                {
                    float dist = Vector3.Distance(a.GetPosition(), b.GetPosition());
                    if (dist < minDist)
                    {
                        minDist = dist;
                        a1 = a;
                        a2 = b;
                    }
                }
            }

            if (a1 == null || a2 == null) return -1;
            int id1 = agents.IndexOf(a1);
            int id2 = agents.IndexOf(a2);

            float sepSpeed = SeparationSpeed(c, frame, id1, id2);
            if(sepSpeed>maxSepSpeed)
            {
                maxSepSpeed = sepSpeed;
            }
        }
        return maxSepSpeed;
    }



    private float SeparationSpeed(SwarmClip c, int frame, int id1, int id2)
    {
        List<AgentData> agentsAtFrac = c.GetFrames()[frame].GetAgentsData();
        float distAtFrac = Vector3.Distance(agentsAtFrac[id1].GetPosition(), agentsAtFrac[id2].GetPosition());

        int n = 10;

        if (frame < n) n = frame;

        List<AgentData> pastAgents = c.GetFrames()[frame-n].GetAgentsData();
        float pastDist = Vector3.Distance(pastAgents[id1].GetPosition(), pastAgents[id2].GetPosition());


        float change = distAtFrac - pastDist;

        float res = (change / (float)n) * (1.0f / Time.fixedDeltaTime); //To get a value per second

        return res;

    }

    private float DistanceClusterCMFromRemainingSwarmCM(List<AgentData> cluster, List<AgentData> agents)
    {
        //Remove cluster agents from the swarm agents list
        List<AgentData> temp = new List<AgentData>(agents);

        foreach (AgentData a in cluster)
        {
            temp.Remove(a);
        }

        Vector3 clusterCM = SwarmTools.GetCenterOfMass(cluster);
        Vector3 remSwarmCM = SwarmTools.GetCenterOfMass(temp);

        float dist = Vector3.Distance(clusterCM, remSwarmCM);

        return dist;
    }


    private List<AgentData> GetPreviousAgentsState(List<AgentData> cluster, List<AgentData> agents, List<AgentData> pastAgents)
    {
        List<AgentData> res = new List<AgentData>();

        foreach(AgentData l in cluster)
        {
            int pos = agents.IndexOf(l);
            res.Add(pastAgents[pos]);
        }

        return res;
    }


    private float SeparationSpeed2(SwarmClip c, int frame, int k)
    {
        if (frame < k) throw new System.Exception("Can't calculate on the k previous frame, because there is less past frame.");

        List<AgentData> agents = c.GetFrames()[frame].GetAgentsData();

        List<AgentData> pastAgents = c.GetFrames()[frame-k].GetAgentsData();

        //Identifying clusters
        List<List<AgentData>> clusters = SwarmTools.GetOrderedClusters(c.GetFrames()[frame]);

        float maxSepSpeed = float.MinValue;

        //For each cluster
        foreach(List<AgentData> cluster in clusters)
        {
            //Get the current distance between the cluster and the remaining of the swarm
            float currentDist = DistanceClusterCMFromRemainingSwarmCM(cluster, agents);

            List<AgentData> pastCluster = GetPreviousAgentsState(cluster, agents, pastAgents);

            float pastDist = DistanceClusterCMFromRemainingSwarmCM(pastCluster, pastAgents);

            float sepSpeed = ((currentDist - pastDist) / k ) * (1.0f / Time.fixedDeltaTime);

            if(sepSpeed > maxSepSpeed)
            {
                maxSepSpeed = sepSpeed;
            }
        }

        return maxSepSpeed;

    }


    private float BestSeparationSpeed2(SwarmClip c, int k)
    {
        int fracFrame = ClipMetrics.GetFractureFrame(c);
        if (fracFrame == -1) throw new System.Exception("Il n'y a pas de fracture dans ce clip");

        float maxSepSpeed = float.MinValue;

        for(int i = fracFrame; i < c.GetFrames().Count; i++)
        {
            float res = SeparationSpeed2(c, i, k);
            if(res > maxSepSpeed)
            {
                maxSepSpeed = res;
            }
        }

        return maxSepSpeed;
    }

    private float SeparationSpeed3(SwarmClip c, int frame, int k)
    {
        if (frame < k) throw new System.Exception("Can't calculate on the k previous frame, because there is less past frame.");

        List<AgentData> agents = c.GetFrames()[frame].GetAgentsData();

        List<AgentData> pastAgents = c.GetFrames()[frame - k].GetAgentsData();

        //Identifying clusters
        List<List<AgentData>> clusters = SwarmTools.GetOrderedClusters(c.GetFrames()[frame]);

        if (clusters.Count < 2) return float.MinValue;

        float maxSepSpeed = float.MinValue;
        List<AgentData> bestCluster = null;
        List<AgentData> bestPastCluster = null;

        //For each cluster
        foreach (List<AgentData> cluster in clusters)
        {
            //Get the current distance between the cluster and the remaining of the swarm
            float currentDist = DistanceClusterCMFromRemainingSwarmCM(cluster, agents);

            List<AgentData> pastCluster = GetPreviousAgentsState(cluster, agents, pastAgents);

            float pastDist = DistanceClusterCMFromRemainingSwarmCM(pastCluster, pastAgents);

            float sepSpeed = ((currentDist - pastDist) / k) * (1.0f/Time.fixedDeltaTime);

            if (sepSpeed > maxSepSpeed)
            {
                maxSepSpeed = sepSpeed;
                bestCluster = cluster;
                bestPastCluster = pastCluster;
            }
        }

        float densityChangeSpeed = DensityChangeSpeed(bestCluster, bestPastCluster, k);

        maxSepSpeed += densityChangeSpeed * 2; //fois 2 car on prend en compte le changement de densit� du reste de l'essaim de fa�on simplifi� (essaim homog�ne)


        return maxSepSpeed;

    }

    private float DensityChangeSpeed(List<AgentData> cluster,  List<AgentData> pastCluster, int k)
    {
        float currentDist = MeanDistFromCM(cluster);
        float pastDist = MeanDistFromCM(pastCluster);

        float densityChangeSpeed = ((pastDist - currentDist) /(float) k) * (1.0f / Time.fixedDeltaTime);

        return densityChangeSpeed;
    }

    private float MeanDistFromCM(List<AgentData> cluster)
    {
        Vector3 cm = SwarmTools.GetCenterOfMass(cluster);

        float meanDist = 0.0f;

        foreach(AgentData a in cluster)
        {
            meanDist += Vector3.Distance(cm, a.GetPosition());
        }

        meanDist /= cluster.Count;

        return meanDist;
    }

    private float BestSeparationSpeed3(SwarmClip c, int k)
    {
        int fracFrame = ClipMetrics.GetFractureFrame(c);
        if (fracFrame == -1) throw new System.Exception("Il n'y a pas de fracture dans ce clip");

        float maxSepSpeed = float.MinValue;

        for (int i = fracFrame; i < c.GetFrames().Count; i++)
        {
            float res = SeparationSpeed3(c, i, k);
            if (res > maxSepSpeed)
            {
                maxSepSpeed = res;
            }
        }

        return maxSepSpeed;
    }

}
