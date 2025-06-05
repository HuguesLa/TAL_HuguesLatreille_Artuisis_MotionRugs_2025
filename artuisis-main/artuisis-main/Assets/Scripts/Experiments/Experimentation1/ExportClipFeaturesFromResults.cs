using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

public class ExportClipFeaturesFromResults : MonoBehaviour
{
    string clipFilesPath = "/Clips/"; //The folder containing clip files
    string resultFilesPath = "/Results/"; //The folder containing clip files

    private List<SwarmClip> clips = new List<SwarmClip>();
    private List<ExperimentationResult> results = new List<ExperimentationResult>();

    private int currentClip = 0;



    int[] clipCategory = new int[] { 1, 4, 4, 3, 4, 2, 1, 1, 2, 2, 3, 1, 3, 3, 2, 2, 1, 4, 4, 4, 2, 3, 3, 1, 2, 3, 4, 3, 3, 2, 3, 2, 1, 1, 3, 1, 2, 1, 1, 4, 4, 4, 4, 4, 2, 2, 1, 3 };


    // Start is called before the first frame update
    void Start()
    {

        //Search for clip filepaths
        clipFilesPath = Application.dataPath + clipFilesPath;
        Debug.Log(clipFilesPath);

        string[] clipFilesPaths = Directory.GetFiles(clipFilesPath, "*.dat",
                                 SearchOption.TopDirectoryOnly);

        //Search for results filepaths
        resultFilesPath = Application.dataPath + resultFilesPath;
        Debug.Log(resultFilesPath);

        string[] resultFilesPaths = Directory.GetFiles(resultFilesPath, "*.dat",
                                         SearchOption.TopDirectoryOnly);



        string resultFilePathCSV = Application.dataPath + "/Results/detailed_results.csv";


        //=========================================================================
        //===========================Loading part==================================
        //=========================================================================
        
        //Load all clip
        for (int i = 0; i < clipFilesPaths.Length; i++)
        {
            //Loading clip from full file path
           SwarmClip clip = ClipTools.LoadClip(clipFilesPaths[i]);

            if (clip != null)
            {
                clips.Add(clip); //Add the loaded clip to the list
                Debug.Log("Clip " + clipFilesPaths[i] + " Loaded.");
            }
            else
                Debug.LogError("Clip can't be load from " + clipFilesPaths[i], this);
        }

        Debug.Log("Clips are loaded.");
        
        //Load all results
        for (int i = 0; i < resultFilesPaths.Length; i++)
        {
          
            if (File.Exists(resultFilesPaths[i]))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(resultFilesPaths[i], FileMode.Open);
                ExperimentationResult res = null;
                try
                {
                    res = (ExperimentationResult)bf.Deserialize(file);
                    results.Add(res); //Add the loaded clip to the list
                    Debug.Log("Result " + resultFilesPaths[i] + " Loaded.");
                }
                catch (Exception)
                {
                    Debug.LogError("Result can't be load from " + resultFilesPaths[i], this);
                }

                file.Close();
            }
                          
        }

        Debug.Log("Result are loaded.");


        //=========================================================================
        //===========================Compute part==================================
        //=========================================================================

        StringBuilder sb = new StringBuilder();

        //Prepare csv result file
        string line = "Filename"+
                       ";ClipCategory" +
                       ";FracturedClip" +               //Le clip contient-il une fracture? 1 : oui, 0 : non
                       ";Participant" +
                       ";Anticipation" +
                       ";CorrectAnswer" +               //Le participant à t-il donné la bonne réponse ? 1 : oui, 0 : non
                       ";AnswerFrame" +                 //Nombre de frames écoulées depuis le début du clip au moment de la réponse
                       ";TimeFromFracture" +            //Nombre de frames depuis la fracture que ce soit positif ou négatif (positif = après, négatif = avant)
                       ";FrameRemaining" +              //Nombre de frames restantes avant la fin du clip au moment de la réponse
                       ";ClipPercentageRemaining" +     //Pourcentage de clip restant au moment de la réponse
                       ";DistanceScoreAtAnswer" +
                       ";SepSpeedAtAnswer" +
                       ";SepSpeed2AtAnswer" +
                       ";SepSpeed3AtAnswer" +
                       ";BestDistanceScore" +
                       ";BestSepSpeed" +
                       ";BestSepSpeed2" +
                       ";BestSepSpeed3" +
                       ";DistanceScoreAtFracture" +
                       ";SepSpeedAtFracture" +
                       ";SepSpeed2AtFracture" +
                       ";SepSpeed3AtFracture" +
                       ";MeanTowardCenterOfMass" +
                       ";MeanEffectiveGroupMotion" +
                       ";StdKNNDirection" +
                       "\r";



        Debug.Log(line);

        sb.Append(line);



        //Analyse part
        foreach (SwarmClip c in clips)
        {
            int fractureFrame = ClipMetrics.GetFractureFrame(c);
            

            string s = clipFilesPaths[currentClip];
            int pos = s.IndexOf("/");
            while (pos != -1)
            {
                s = s.Substring(pos + 1);
                pos = s.IndexOf("/");
            }

            float bestDistanceScore = -1;
            float bestSepSpeed = -1;
            float bestSepSpeed2 = -1;
            float bestSepSpeed3 = -1;
            float distanceScoreAtFracture = -1;
            float sepSpeedAtFracture = -1;
            float sepSpeed2AtFracture = -1;
            float sepSpeed3AtFracture = -1;

            if (fractureFrame != -1)
            {
                bestDistanceScore = ClipMetrics.BestFractureVisibilityScore(c);
                bestSepSpeed = BestSeparationSpeed(c);
                bestSepSpeed2 = BestSeparationSpeed2(c, 10);
                bestSepSpeed3 = BestSeparationSpeed3(c, 10);
                distanceScoreAtFracture = SwarmMetrics.FractureVisibilityScore(c.GetFrames()[fractureFrame]);
                sepSpeedAtFracture = SeparationSpeed(c, fractureFrame);
                sepSpeed2AtFracture = SeparationSpeed2(c, fractureFrame,10);
                sepSpeed3AtFracture = SeparationSpeed3(c, fractureFrame,10);
            }


            float meanTowardCenterOfMass = ClipMetrics.MeanTowardsCenterOfMass(c);
            float meanEffectiveGroupMotion = ClipMetrics.MeanEffectiveGroupMotion(c);
            float STDKNNDirection = ClipMetrics.MeanStandardDeviationOfKnnDirection(c);

            int currentParticipant = 0;
            foreach (ExperimentationResult result in results)
            {

                
                string ch = resultFilesPaths[currentParticipant];
                int pos2 = ch.IndexOf("/");
                while (pos2 != -1)
                {
                    ch = ch.Substring(pos2 + 1);
                    pos2 = ch.IndexOf("/");
                }
                pos2 = ch.IndexOf(".");
                ch = ch.Substring(0, pos2);

                float score = - 1;
                float sepSpeed = -1;
                float sepSpeed2 = -1;
                float sepSpeed3 = -1;
                float anticipation = 0;

                int frameNumber = GetAnswerFrameNumber(result, s);
                bool participantAnswer = GetAnswer(result, s);
                if (frameNumber < fractureFrame)
                {
                    anticipation = 1;

                }
                else
                {
                    if(fractureFrame != -1)
                    {
                        score = SwarmMetrics.FractureVisibilityScore(c.GetFrames()[frameNumber]);
                        sepSpeed = SeparationSpeed(c, frameNumber);
                        sepSpeed2 = SeparationSpeed2(c, frameNumber, 10);
                        sepSpeed3 = SeparationSpeed3(c, frameNumber, 10);
                    } else
                    {
                        anticipation = -1;
                    }

                }

                //Does the participant have the correct answer ? 0 : no, 1 : yes
                int correctAnswer = 0;
                if ((participantAnswer && fractureFrame != -1) || (!participantAnswer && fractureFrame == -1))
                {
                    correctAnswer = 1;
                }

                string timeFromFracture = null;
                if (fractureFrame != -1)
                {
                    int delta = frameNumber - fractureFrame;
                    timeFromFracture = delta.ToString();
                }

                int frameRemaining = c.GetFrames().Count - frameNumber - 1;
                float clipPercentageRemaining = (float) frameRemaining / (float) c.GetFrames().Count;

                int fracture = 0;
                if (fractureFrame != -1)
                    fracture = 1;


                line = s
                    + ";" + clipCategory[currentClip]
                    + ";" + fracture
                    + ";" + ch
                    + ";" + anticipation
                    + ";" + correctAnswer
                    + ";" + frameNumber
                    + ";" + timeFromFracture
                    + ";" + frameRemaining
                    + ";" + clipPercentageRemaining
                    + ";" + score
                    + ";" + sepSpeed
                    + ";" + sepSpeed2
                    + ";" + sepSpeed3
                    + ";" + bestDistanceScore
                    + ";" + bestSepSpeed
                    + ";" + bestSepSpeed2
                    + ";" + bestSepSpeed3
                    + ";" + distanceScoreAtFracture
                    + ";" + sepSpeedAtFracture
                    + ";" + sepSpeed2AtFracture
                    + ";" + sepSpeed3AtFracture
                    + ";" + meanTowardCenterOfMass
                    + ";" + meanEffectiveGroupMotion
                    + ";" + STDKNNDirection
                    + "\r";
                //Debug.Log(line);
                sb.Append(line);
                
                
                currentParticipant++;
            }



            currentClip++;
        }


        //Save result
        File.AppendAllText(resultFilePathCSV, sb.ToString());
        sb.Clear();

        Debug.Log("Results saved.");
        
    }


    private int GetAnswerFrameNumber(ExperimentationResult result, String filename)
    {
        foreach (ClipResult c in result.results)
        {
            if (c.filename == filename)
            {
                return c.frameNumber;
            }
        }
        return -2;
    }

    private bool GetAnswer(ExperimentationResult result, String filename)
    {
        foreach (ClipResult c in result.results)
        {
            if (c.filename == filename)
            {
                return c.fracture;
            }
        }
        throw new Exception("Clip " + filename +" not found in participant results.");
    }


    #region Methods - Separation speed

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

        foreach (List<AgentData> cluster in clusters)
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
            if (sepSpeed > maxSepSpeed)
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

        List<AgentData> pastAgents = c.GetFrames()[frame - n].GetAgentsData();
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

        foreach (AgentData l in cluster)
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

        List<AgentData> pastAgents = c.GetFrames()[frame - k].GetAgentsData();

        //Identifying clusters
        List<List<AgentData>> clusters = SwarmTools.GetOrderedClusters(c.GetFrames()[frame]);

        float maxSepSpeed = float.MinValue;

        //For each cluster
        foreach (List<AgentData> cluster in clusters)
        {
            //Get the current distance between the cluster and the remaining of the swarm
            float currentDist = DistanceClusterCMFromRemainingSwarmCM(cluster, agents);

            List<AgentData> pastCluster = GetPreviousAgentsState(cluster, agents, pastAgents);

            float pastDist = DistanceClusterCMFromRemainingSwarmCM(pastCluster, pastAgents);

            float sepSpeed = ((currentDist - pastDist) / k) * (1.0f / Time.fixedDeltaTime);

            if (sepSpeed > maxSepSpeed)
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

        for (int i = fracFrame; i < c.GetFrames().Count; i++)
        {
            float res = SeparationSpeed2(c, i, k);
            if (res > maxSepSpeed)
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

            float sepSpeed = ((currentDist - pastDist) / k) * (1.0f / Time.fixedDeltaTime);

            if (sepSpeed > maxSepSpeed)
            {
                maxSepSpeed = sepSpeed;
                bestCluster = cluster;
                bestPastCluster = pastCluster;
            }
        }

        float densityChangeSpeed = DensityChangeSpeed(bestCluster, bestPastCluster, k);

        maxSepSpeed += densityChangeSpeed * 2; //fois 2 car on prend en compte le changement de densité du reste de l'essaim de façon simplifié (essaim homogène)


        return maxSepSpeed;

    }

    private float DensityChangeSpeed(List<AgentData> cluster, List<AgentData> pastCluster, int k)
    {
        float currentDist = MeanDistFromCM(cluster);
        float pastDist = MeanDistFromCM(pastCluster);

        float densityChangeSpeed = ((pastDist - currentDist) / (float)k) * (1.0f / Time.fixedDeltaTime);

        return densityChangeSpeed;
    }

    private float MeanDistFromCM(List<AgentData> cluster)
    {
        Vector3 cm = SwarmTools.GetCenterOfMass(cluster);

        float meanDist = 0.0f;

        foreach (AgentData a in cluster)
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

    #endregion


}


