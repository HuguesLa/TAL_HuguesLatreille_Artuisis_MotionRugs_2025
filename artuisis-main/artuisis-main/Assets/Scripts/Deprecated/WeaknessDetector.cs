using System.Collections.Generic;
using UnityEngine;
using System;

public class WeaknessDetector : MonoBehaviour
{
    /*
    SwarmManager swarmManager;

    public GameObject prefab;
    public GameObject prefabArrow;

    public float timestep=1;

    public float threshold = 0.97f;

    private Gradient gradient;
    GradientColorKey[] colorKey;
    GradientAlphaKey[] alphaKey;

    List<GameObject> displayCube = new List<GameObject>();

    List<Tuple<Agent, Agent, float>> links = new List<Tuple<Agent, Agent, float>>();

    List<Color> colorPalette;

    // Start is called before the first frame update
    void Start()
    {
        swarmManager = FindObjectOfType<SwarmManager>();
        if (swarmManager == null) Debug.LogError("AgentManager is missing in the scene", this);


        gradient = new Gradient();

        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        colorKey = new GradientColorKey[2];
        colorKey[0].color = Color.red;
        colorKey[0].time = 0.0f;
        colorKey[1].color = Color.green;
        colorKey[1].time = 1.0f;




        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 1.0f;


        gradient.SetKeys(colorKey, alphaKey);

        colorPalette = ColorTools.GetShuffledColorPalette(10);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach(GameObject g in displayCube)
        {
            Destroy(g);
        }
        displayCube.Clear();

        List<AgentData> agents = swarmManager.GetSwarmData().GetAgentsData();


        UpdateLinksList(agentsGO);

        //List<Tuple<Agent, Agent, float>> criticalLinks = GetCriticalLinks(links,agents);

        //List<List<Agent>> communities = ModularityOptimisation.GetCommunities(agents);
        List<List<Agent>> communities = new List<List<Agent>>();


        foreach (List<Agent> c in communities)
        {
            Vector3 meanSpeed = Vector3.zero;
            Vector3 meanPos = Vector3.zero;
            foreach(Agent a in c)
            {
                meanSpeed += a.GetSpeed();
                meanPos += a.transform.position;
            }
            meanSpeed /= c.Count;
            meanPos /= c.Count;

            GameObject temp = GameObject.Instantiate(prefabArrow);
            temp.transform.position = meanPos;

            float agentDirection_YAxis = 180 - (Mathf.Acos(meanSpeed.normalized.x) * 180.0f / Mathf.PI);
            if (meanSpeed.z < 0.0f) agentDirection_YAxis = agentDirection_YAxis * -1;
            temp.transform.rotation = Quaternion.Euler(0.0f, agentDirection_YAxis, 0.0f);

            displayCube.Add(temp);
        }

        
    */

        /*
        
        for (int i=0; i<criticalLinks.Count; i++)
        {
            GameObject temp = GameObject.Instantiate(prefab);
            temp.transform.position = (criticalLinks[i].Item1.transform.position + criticalLinks[i].Item2.transform.position) / 2;
            //temp.GetComponent<Renderer>().material.color = gradient.Evaluate(agentScore);
            displayCube.Add(temp);
        }
        */
        /*

        foreach (GameObject g in agentsGO)
        {
            g.GetComponent<Agent>().SavePosition();
        }

        //List<List<GameObject>> neighbours = GetAgentLinks(agents[0].GetComponent<Agent>());

        //GetAgentLinksNumber(neighbours);

        //DisplayLinkTension(agents[0].GetComponent<Agent>(), neighbours);
        //DisplayLinkAngle(agents[0].GetComponent<Agent>(), neighbours);
    }


    private List<Tuple<Agent, Agent, float>> GetCriticalLinks(List<Tuple<Agent, Agent, float>> linksList, List<Agent> agents)
    {
        List<Tuple<Agent, Agent, float>> criticalLinks = new List<Tuple<Agent, Agent, float>>();


        List<List<Agent>> clusters = GetClustersFromLinks(linksList, agents);
        int count = clusters.Count;

        foreach(Tuple<Agent, Agent, float> l in links)
        {
            //If the score link isn't critical, stop
            if (l.Item3 < threshold) break;

            List<Tuple<Agent, Agent, float>> clone = new List<Tuple<Agent, Agent, float>>(links);
            clone.Remove(l);

            List<List<Agent>> temp = GetClustersFromLinks(clone, agents);

            if (temp.Count > count) criticalLinks.Add(l);
        }

        return criticalLinks;

    }



    private List<List<Agent>> GetClustersFromLinks(List<Tuple<Agent, Agent, float>> linksList, List<Agent> agents)
    {
        //Clone the links list to modify it
        List<Tuple<Agent, Agent, float>> clone = new List<Tuple<Agent, Agent, float>>(linksList);


        //Create the list that will store the different clusters
        List<List<Agent>> clusters = new List<List<Agent>>();


        while(clone.Count > 0) 
        { 
            //Take the first link to start a new cluster
            List<Agent> newCluster = new List<Agent>();

            //Add the two agents of the link
            newCluster.Add(clone[0].Item1);
            newCluster.Add(clone[0].Item2);

            //Remove the two agents from the list
            agents.Remove(clone[0].Item1);
            agents.Remove(clone[0].Item2);

            //Remove the link from the list
            clone.RemoveAt(0);

            int i = 0;
            while(i<newCluster.Count)
            {
                List<Tuple<Agent, Agent, float>> temp = new List<Tuple<Agent, Agent, float>>();
                foreach(Tuple<Agent, Agent, float> l in clone)
                {
                    if(System.Object.ReferenceEquals(l.Item1, newCluster[i]))
                    {
                        temp.Add(l);
                        if (!newCluster.Contains(l.Item2))
                        {
                            newCluster.Add(l.Item2);
                            agents.Remove(l.Item2);
                        }
                    }

                    if (System.Object.ReferenceEquals(l.Item2, newCluster[i]))
                    {
                        temp.Add(l);
                        if (!newCluster.Contains(l.Item1))
                        {
                            newCluster.Add(l.Item1);
                            agents.Remove(l.Item1);
                        }
                    }
                } 
                
                foreach(Tuple<Agent, Agent, float> t in temp)
                {
                    clone.Remove(t);
                }
                i++;
            }
            clusters.Add(newCluster);
        }

        //Now, all the clusters are defined from the links. However, some agents are missing because they are isolated.
        //Complete the list of cluster by adding isolated agents
        foreach(Agent a in agents)
        {
            List<Agent> temp = new List<Agent>();
            temp.Add(a);
            clusters.Add(temp);
        }

        return clusters;
    }

    private void SortLinksUsingScore()
    {
        for (int i = 1; i < links.Count; i++)
        {
            for (int j = 0; j < links.Count - i; j++)
            {
                if (links[j].Item3 < links[j + 1].Item3)
                {
                    Tuple<Agent, Agent, float> temp = links[j];
                    links[j] = links[j + 1];
                    links[j + 1] = temp;
                }
            }
        }
    }

    private void UpdateLinksList(List<GameObject> agents)
    {
        links.Clear();

        foreach(GameObject g in agents)
        {
            Agent agent = g.GetComponent<Agent>();
            List<GameObject> neigbours = SwarmAnalyserTools.GetNeighbours(g, agents, agent.GetFieldOfViewSize(), agent.GetBlindSpotSize());

            foreach(GameObject n in neigbours)
            {
                Agent neighbour = n.GetComponent<Agent>();
                if(!ContainsLink(agent,neighbour))
                {
                    float score = ComputeLinkTensionScore(agent, neighbour);
                    Tuple<Agent, Agent, float> link = new Tuple<Agent, Agent, float>(agent, neighbour,score);
                    links.Add(link);
                }
            }
        }

        SortLinksUsingScore();
    }


    private bool ContainsLink(Agent agent, Agent neighbour)
    {
        bool exists = false;
        foreach(Tuple<Agent,Agent,float> t in links)
        {
            if (System.Object.ReferenceEquals(t.Item1, agent) && System.Object.ReferenceEquals(t.Item2, neighbour)) exists = true;
            if (System.Object.ReferenceEquals(t.Item1, neighbour) && System.Object.ReferenceEquals(t.Item2, agent)) exists = true;
        }

        return exists;
    }
        */

    /*
    private float ComputeAgentScore(Agent agent, List<GameObject> agents)
    {
        List<GameObject> neigbours = SwarmAnalyserTools.GetNeighbours(agent.gameObject, agents, agent.GetFieldOfViewSize(), agent.GetBlindSpotSize());

        float score = 0;
        foreach(GameObject g in neigbours)
        {
            score += ComputeLinkScore(agent, g.GetComponent<Agent>());
        }

        if(neigbours.Count<0)  score /= neigbours.Count;


        return score;
    }
    */

    /*

    private float ComputeLinkTensionScore(Agent agent, Agent neighbour)
    {
        float score = ComputeLinkTension(agent, neighbour) + (ComputeLinkTensionEvolution(agent, neighbour) * timestep);

        return score;
    }


    private float ComputeLinkTension(Agent agent, Agent neighbour)
    {
        float dist = Vector3.Distance(neighbour.transform.position, agent.transform.position);
        float ratio = dist / agent.GetFieldOfViewSize();

        return ratio;
    }

    private float ComputeLinkTensionEvolution(Agent agent, Agent neighbour)
    {
        float dist = Vector3.Distance(neighbour.transform.position, agent.transform.position);
        float pastDist = Vector3.Distance(neighbour.GetSavedPosition(), agent.GetSavedPosition());

        float change = dist - pastDist;

        float ratio = change / agent.GetFieldOfViewSize();

        ratio /= Time.fixedDeltaTime;

        return ratio;
    }
    */

    /*
    public List<List<GameObject>> GetAgentLinks(Agent agent)
    {
        List<GameObject> reciprocalNeigbours = SwarmAnalyserTools.GetNeighboursBasedOnTypeLink(agent.gameObject, agentManager.GetAgents(), agent.GetFieldOfViewSize(), agent.GetBlindSpotSize(), SwarmAnalyserTools.LinkType.reciprocal);
        List<GameObject> unidirectional_to_neighbour = SwarmAnalyserTools.GetNeighboursBasedOnTypeLink(agent.gameObject, agentManager.GetAgents(), agent.GetFieldOfViewSize(), agent.GetBlindSpotSize(), SwarmAnalyserTools.LinkType.unidirectional);
        List<GameObject> unidirectional_to_agent = SwarmAnalyserTools.GetNeighboursBasedOnTypeLink(agent.gameObject, agentManager.GetAgents(), agent.GetFieldOfViewSize(), agent.GetBlindSpotSize(), SwarmAnalyserTools.LinkType.unidirectional_reverse);

        List<List<GameObject>> neighbours = new List<List<GameObject>>();
        neighbours.Add(reciprocalNeigbours);
        neighbours.Add(unidirectional_to_neighbour);
        neighbours.Add(unidirectional_to_agent);

        return neighbours;
    }

    private float GetAgentLinksNumber(List<List<GameObject>> neighbours)
    {

        Debug.Log(neighbours[0].Count + "," + neighbours[1].Count + "," + neighbours[2].Count);

        return neighbours[0].Count + neighbours[1].Count + neighbours[2].Count;
    }


    private void DisplayLinkTension(Agent agent, List<List<GameObject>> neighbours)
    {
        string line = "{";
       foreach (List<GameObject> l in neighbours)
       {
            line += "{";
            foreach(GameObject a in l)
            {
                //Vector3 link = a.transform.position - agent.transform.position;
                float dist = Vector3.Distance(a.transform.position, agent.transform.position);
                float ratio = dist / agent.GetFieldOfViewSize();
                line += ratio +";";
            }
            line += "}";
       }
        line += "}";
        Debug.Log(line);
    }

    private void DisplayLinkAngle(Agent agent, List<List<GameObject>> neighbours)
    {
        string line = "{";
        for(int i=0;i< neighbours.Count; i++)
        {
            line += "{";
            foreach (GameObject a in neighbours[i])
            {
                float val;
                float angle;
                float blindSpotSize;
                if (i<2)
                {
                    Vector3 dir = a.transform.position - agent.transform.position;
                    angle = Vector3.Angle(agent.GetSpeed(), dir);
                    blindSpotSize = agent.GetBlindSpotSize();
                } else
                {
                    Vector3 dir = agent.transform.position - a.transform.position;
                    angle = Vector3.Angle(a.GetComponent<Agent>().GetSpeed(), dir);
                    blindSpotSize = a.GetComponent<Agent>().GetBlindSpotSize();

                }
                if (blindSpotSize == 0) val = -1;
                else val = 180 - (blindSpotSize / 2) - angle;
                line += val + ";";

            }
            line += "}";
        }
        line += "}";
        Debug.Log(line);
    }*/

    /*
    private float ComputeAngleScore(Agent agent, Agent neighbour)
    {
        float val1 = 0, val2=0 ;
        float angle;
        float blindSpotSize;

        Vector3 dir = neighbour.transform.position - agent.transform.position;
        angle = Vector3.Angle(agent.GetSpeed(), dir);
        blindSpotSize = agent.GetBlindSpotSize();


        if (angle <= (180 - (blindSpotSize / 2)))
        {
            if (blindSpotSize == 0) val1 = 180;
            else val1 = 180 - (blindSpotSize / 2) - angle;
        }


        dir = agent.transform.position - neighbour.transform.position;
        angle = Vector3.Angle(neighbour.GetSpeed(), dir);
        blindSpotSize = neighbour.GetBlindSpotSize();


        if (angle <= (180 - (blindSpotSize / 2)))
        {
            if (blindSpotSize == 0) val2 = 180;
            else val2 = 180 - (blindSpotSize / 2) - angle;
        }
        val1 /= 180;
        val2 /= 180;

        return Mathf.Max(val1, val2);

    }
    */


}
