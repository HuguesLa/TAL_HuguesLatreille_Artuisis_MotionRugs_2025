using System.Collections.Generic;
using UnityEngine;
using System;


public class DisplayerSwarm : Displayer
{
    private enum DisplayType
    {
        Simple,
        Clusters,
        Communities,
        LeavesBranchesAndTrunk
    }

    #region Serialized fields
    [SerializeField]
    private DisplayType displayType = DisplayType.Simple;
    [SerializeField]
    private GameObject actorPrefab;
    #endregion

    #region Private fields

    private List<GameObject> actors = new List<GameObject>();

    private Vector3 spatialOrigin;

    private int nbColorCommunities = 10;
    private int nbColorClusters = 40;

    private List<Color> clusterPalette;
    private List<Color> communitiesPalette;
    private List<Color> lbtPalette;
    #endregion

    #region Methods - Monobehaviour callbacks
    // Start is called before the first frame update
    void Start()
    {
        this.spatialOrigin = Vector3.zero;

        clusterPalette = ColorTools.GetShuffledColorPalette(nbColorClusters);
        communitiesPalette = ColorTools.GetColorPalette(nbColorCommunities);
        lbtPalette = ColorTools.GetColorPalette(3);
        if(actorPrefab.GetComponent<Renderer>() == null)
            Debug.LogWarning("The \"ActorPrefab\" as no component \"Renderer\", no color will be displayed.", this);
    }
    #endregion

    #region Methods - Displayer override
    public override void DisplayVisual(SwarmData swarmData)
    {
        switch (displayType)
        {
            case DisplayType.Simple:
                DisplaySimple(swarmData);
                break;
            case DisplayType.Clusters:
                DisplayColoredCluster(swarmData);
                break;
            case DisplayType.Communities:
                DisplayCommunities(swarmData);
                break;
            case DisplayType.LeavesBranchesAndTrunk:
                DisplayLeavesBranchesAndTrunk(swarmData);
                break;
            default:
                Debug.LogError("Display unimplemented yet.", this);
                break;
        }
    }


    public override void ClearVisual()
    {
        ClearActors();
    }
    #endregion

    #region Methods - Swarm display
    /// <summary>
    /// Displays the <see cref="SwarmData"/> set in parameter,, 
    /// by displaying the position of saved agents in the clip using actors.
    /// It display the frame in the simpliest way possible, meaning that all actor are the same <see cref="UnityEngine.Color"/>.
    /// </summary>
    /// /// <param name="frame"> The <see cref="LogClipFrame"/> value correspond to the frame which must be displayed.</param>
    public void DisplaySimple(SwarmData swarmData)
    {
        int numberOfAgents = swarmData.GetAgentsData().Count;

        AdjustActorNumber(numberOfAgents);


        //Update actors position
        for (int i = 0; i < numberOfAgents; i++)
        {
            AgentData a = swarmData.GetAgentsData()[i];

            //Update actor position and direction
            UpdateActorPositionAndDirection(i, a.GetPosition(), a.GetDirection());

            //Update actor color
            Renderer actorRenderer = actors[i].GetComponent<Renderer>();
            if(actorRenderer != null)
                actorRenderer.material.color = Color.black;
        }
    }

    /// <summary>
    /// Displays the <see cref="LogClipFrame"/> set in parameter,
    /// by displaying the position of saved agents in the clip using actors.
    /// It display the frame coloring actors from the same clusters in an unique <see cref="UnityEngine.Color"/>, allowing an user to identify groups visually.
    /// </summary>
    /// <param name="frame"> The <see cref="LogClipFrame"/> value correspond to the frame which must be displayed.</param>
    public void DisplayColoredCluster(SwarmData swarmData)
    {
        AdjustActorNumber(swarmData.GetAgentsData().Count);

        //Searching for fracture
        List<List<AgentData>> clusters = SwarmTools.GetOrderedClusters(swarmData);

        int i = 0;
        int c = 0;
        foreach (List<AgentData> l in clusters)
        {
            foreach (AgentData a in l)
            {
                //Update actor position and direction
                UpdateActorPositionAndDirection(i, a.GetPosition(), a.GetDirection());

                //Update actor color
                Renderer actorRenderer = actors[i].GetComponent<Renderer>();
                if (actorRenderer != null)
                {
                    if (clusters.Count > nbColorClusters)
                    {
                        actorRenderer.material.color = Color.black;
                    }
                    else
                    {
                        actorRenderer.material.color = clusterPalette[c];
                    }
                } 
                i++;
            }
            c++;
        }
        //Debug.Log(i);
    }


    public void DisplayCommunities(SwarmData swarmData)
    {
        AdjustActorNumber(swarmData.GetAgentsData().Count);

        List<List<AgentData>> communities = SwarmTools.GetOrderedCommunities(swarmData);

        int i = 0;
        int c = 0;
        foreach (List<AgentData> l in communities)
        {
            foreach (AgentData a in l)
            {
                //Update actor position and direction
                UpdateActorPositionAndDirection(i, a.GetPosition(), a.GetDirection());

                Renderer actorRenderer = actors[i].GetComponent<Renderer>();
                if (actorRenderer != null)
                    actorRenderer.material.color = communitiesPalette[c % nbColorCommunities];

                i++;
            }
            c++;
        }
    }

    public void DisplayLeavesBranchesAndTrunk(SwarmData swarmData)
    {
        AdjustActorNumber(swarmData.GetAgentsData().Count);
        Tuple<List<AgentData>, List<AgentData>, List<AgentData>> tuple = SwarmTools.SeparateLeavesBranchesAndTrunk(swarmData);

        List<List<AgentData>> groups = new List<List<AgentData>>();

        groups.Add(tuple.Item1);
        groups.Add(tuple.Item2);
        groups.Add(tuple.Item3);

        int i = 0;
        int c = 0;
        foreach (List<AgentData> l in groups)
        {
            foreach (AgentData a in l)
            {
                //Update actor position and direction
                UpdateActorPositionAndDirection(i, a.GetPosition(), a.GetDirection());

                Renderer actorRenderer = actors[i].GetComponent<Renderer>();
                if (actorRenderer != null)
                    actorRenderer.material.color = lbtPalette[c];
                i++;
            }
            c++;
        }
    }
    #endregion

    #region Methods - Actors management
    /// <summary>
    /// This method check if there is the right amount of actors (<see cref="GameObject"/>) to simulate each agent of a clip.
    /// If there is more, it deletes the surplus actors. 
    /// Is there is less, it create new <see cref="GameObject"/> to fit the right amount of agents.
    /// </summary>
    /// <param name="numberOfAgents"> A <see cref="int"/> value that represent the right amount of actor needed.</param>
    public void AdjustActorNumber(int numberOfAgents)
    {
        int numberOfActors = actors.Count;

        //Create missing actors
        if (numberOfActors < numberOfAgents)
        {
            for (int i = 0; i < (numberOfAgents - numberOfActors); i++)
            {
                GameObject newAgent = GameObject.Instantiate(actorPrefab);
                newAgent.transform.localPosition = Vector3.zero;
                newAgent.transform.localRotation = Quaternion.Euler(0.0f, UnityEngine.Random.Range(0.0f, 359.0f), 0.0f);
                newAgent.transform.parent = this.transform;
                actors.Add(newAgent);
            }
        }

        //Destroy surplus actors
        if (numberOfActors > numberOfAgents)
        {
            for (int i = numberOfAgents; i < numberOfActors; i++)
            {
                GameObject.Destroy(actors[i].gameObject);
            }
            actors.RemoveRange(numberOfAgents, numberOfActors - numberOfAgents);
        }
    }

    /// <summary>
    /// Remove all actors and destroy their gameObject.
    /// </summary>
    private void ClearActors()
    {
        foreach (GameObject a in actors)
        {
            GameObject.Destroy(a.gameObject);
        }
        actors.Clear();
    }

    private void UpdateActorDirection(int actorId, Vector3 direction)
    {
        float agentDirection_YAxis = 180 - (Mathf.Acos(direction.normalized.x) * 180.0f / Mathf.PI);
        if (direction.z < 0.0f) agentDirection_YAxis = agentDirection_YAxis * -1;
        actors[actorId].transform.localRotation = Quaternion.Euler(0.0f, agentDirection_YAxis, 0.0f);
    }

    private void UpdateActorPosition(int actorId, Vector3 position)
    {
        actors[actorId].transform.localPosition = position + spatialOrigin;
    }

    private void UpdateActorPositionAndDirection(int actorId, Vector3 position, Vector3 direction)
    {
        UpdateActorPosition(actorId, position);
        UpdateActorDirection(actorId, direction);
    }
    #endregion

    #region Methods - Setter
    public void SetSpatialOrigin(Vector3 origin)
    {
        this.spatialOrigin = origin;
    }

    public void setActorPrefab(GameObject prefab)
    {
        this.actorPrefab = prefab;
        ClearActors();
    }
    #endregion
}
