using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class SwarmTools
{
    #region Methods - Agent perception
    /// <summary>
    /// Check if an agent perceive another another, based on its field of view distance and its blind spot size.
    /// </summary>
    /// <param name="agent">The agent perceiving.</param>
    /// <param name="potentialNeighbour"> The agent potentially perceived.</param>
    /// <param name="fieldOfViewSize">The distance of perception of the agent.</param>
    /// <param name="blindSpotSize">The blind angle behind the agent where neigbours are not perceived.</param>
    /// <returns> A <see cref="bool"/> value set � True if the agent if perceived by the other agent. False ohterwise.</returns>
    private static bool Perceive(AgentData agent, AgentData potentialNeighbour, float fieldOfViewSize, float blindSpotSize)
    {
        //Check whether the potential neighbour is close enough (at a distance shorter than the perception distance).
        if (Vector3.Distance(potentialNeighbour.GetPosition(), agent.GetPosition()) <= fieldOfViewSize)
        {
            Vector3 dir = potentialNeighbour.GetPosition() - agent.GetPosition();
            float angle = Vector3.Angle(agent.GetDirection(), dir);
            //Check whether the potential neighbour is visible by the current agent (not in the blind spot of the current agent)
            if (angle <= 180 - (blindSpotSize / 2))
            {
                //Agent is a neighbour
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Check if at least one of the two agent perceive the other one.
    /// </summary>
    /// <param name="agent1">The first agent tested.</param>
    /// <param name="agent2"> The second agent tested.</param>
    /// <param name="fieldOfViewSize">The distance of perception of both agent. If different, use <see cref="Perceive(AgentData, AgentData, float, float)"/> instead </param>
    /// <param name="blindSpotSize">The blind angle behind the agent where neigbours are not perceived. If different, use <see cref="Perceive(AgentData, AgentData, float, float)"/> instead</param>
    /// <returns> A <see cref="bool"/> value set � True if the agent if perceived by the other agent. False ohterwise.</returns>
    public static bool Linked(AgentData agent1, AgentData agent2, float fieldOfViewSize, float blindSpotSize)
    {
        return (Perceive(agent1, agent2, fieldOfViewSize, blindSpotSize) || Perceive(agent2, agent1, fieldOfViewSize, blindSpotSize));
    }

    /// <summary>
    /// Detect all perceived agents of an agent based on its perception, and return them.
    /// </summary>
    /// <param name="agent"> A <see cref="AgentData"/> representing the agent searching its perceveid agents.</param>
    /// <param name="agentList"> A <see cref="List{T}"/>  of all the agent, containing the possible perceveid agents.</param>
    /// <param name="fieldOfViewSize"> A <see cref="float"/> value representing the distance of perception of the agent.</param>
    /// <param name="blindSpotSize"> A <see cref="float"/> value representing the blind angle behind the agent where neigbours are not perceived.</param>
    /// <returns> The <see cref="List{T}"/> of perceived agent.</returns>
    public static List<AgentData> GetPerceivedAgents(AgentData agent, List<AgentData> agentList, float fieldOfViewSize, float blindSpotSize)
    {
        //Create a list that will store the perceived agent
        List<AgentData> detectedAgents = new List<AgentData>();

        //Compare current agent with all agents
        foreach (AgentData g in agentList)
        {
            //Check if the current agent is compared with itself
            if (System.Object.ReferenceEquals(g, agent)) continue;

            if (Perceive(agent, g, fieldOfViewSize, blindSpotSize))
            {
                detectedAgents.Add(g);
            }
        }
        return detectedAgents;
    }



    /// <summary>
    /// Detect all neighbours of an agent based on its perception, and return them.
    /// A neighbour here mean that the agent perceived, or is perceived by the neighbour.
    /// </summary>
    /// <param name="agent"> A <see cref="AgentData"/> representing the agent searching its neighbours.</param>
    /// <param name="agentList"> A <see cref="List{T}"/>  of all the agent, containing the possible neighbours.</param>
    /// <param name="fieldOfViewSize"> A <see cref="float"/> value representing the distance of perception of the agent.</param>
    /// <param name="blindSpotSize"> A <see cref="float"/> value representing the blind angle behind the agent where neigbours are not perceived.</param>
    /// <returns> The <see cref="List{T}"/> of neighbours.</returns>
    public static List<AgentData> GetNeighbours(AgentData agent, List<AgentData> agentList, float fieldOfViewSize, float blindSpotSize)
    {
        //Create a list that will store the perceived agent
        List<AgentData> neighbours = new List<AgentData>();

        //Compare current agent with all agents
        foreach (AgentData g in agentList)
        {
            //Check if the current agent is compared with itself
            if (System.Object.ReferenceEquals(g, agent)) continue;

            if (Linked(agent, g, fieldOfViewSize, blindSpotSize))
            {
                neighbours.Add(g);
            }
        }
        return neighbours;
    }


    /// <summary>
    /// Detect all neighbours of an agent based on its perception, and return their position in the list.
    /// A neighbour here mean that the agent perceived, or is perceived by the neighbour.
    /// </summary>
    /// <param name="agent"> A <see cref="AgentData"/> representing the agent searching its neighbours.</param>
    /// <param name="agentList"> A <see cref="List{T}"/>  of all the agent, containing the possible neighbours.</param>
    /// <param name="fieldOfViewSize"> A <see cref="float"/> value representing the distance of perception of the agent.</param>
    /// <param name="blindSpotSize"> A <see cref="float"/> value representing the blind angle behind the agent where neigbours are not perceived.</param>
    /// <returns> The <see cref="List{T}"/> of the position of agents in the agents list.</returns>
    public static List<int> GetNeighboursIDs(AgentData agent, List<AgentData> agentList, float fieldOfViewSize, float blindSpotSize)
    {
        //Create a list that will store the perceived agent
        List<int> neighboursIDs = new List<int>();

        //Compare current agent with all agents
        for (int i=0; i<agentList.Count; i++)
        {
            //Check if the current agent is compared with itself
            if (System.Object.ReferenceEquals(agentList[i], agent)) continue;

            if (Linked(agent, agentList[i], fieldOfViewSize, blindSpotSize))
            {
                neighboursIDs.Add(i);
            }
        }
        return neighboursIDs;
    }

    #endregion

    #region Methods - Agents links
    /// <summary>
    /// From a swarm, get all unique pairs of agents based on their perception.
    /// This method uses <see cref="GetLinksList(List{AgentData}, float, float)"/> method.
    /// </summary>
    /// <param name="swarm">The analysed swarm</param>
    /// <returns>The <see cref="List{T}"/> of pairs of agents.</returns>
    public static List<Tuple<AgentData, AgentData>> GetLinksList(SwarmData swarm)
    {
        List<Tuple<AgentData, AgentData>> links = GetLinksList(swarm.GetAgentsData(), swarm.GetParameters().GetFieldOfViewSize(), swarm.GetParameters().GetBlindSpotSize());
        return links;
    }

    /// <summary>
    /// From a list of agents and their parameters, get all unique pairs of agents based on their perception.
    /// </summary>
    /// <param name="agents">The list of agents</param>
    /// <param name="fovSize">The field of view size in meters</param>
    /// <param name="bsSize">The blind spot size in degree</param>
    /// <returns>The <see cref="List{T}"/> of pairs of agents.</returns>
    public static List<Tuple<AgentData, AgentData>> GetLinksList(List<AgentData> agents, float fovSize, float bsSize)
    {
        List<Tuple<AgentData, AgentData>> links = new List<Tuple<AgentData, AgentData>>();

        bool[,] adjacentMatrix = GetAdjacentMatrix(agents, fovSize, bsSize);

        for (int i = 0; i < agents.Count; i++)
        {
            for (int j = i; j < agents.Count; j++)
            {
                if (adjacentMatrix[i, j])
                {
                    Tuple<AgentData, AgentData> link = new Tuple<AgentData, AgentData>(agents[i], agents[j]);
                    links.Add(link);
                }
            }
        }
        return links;
    }
    #endregion

    #region Methods - Clusters (connected components)
    /// <summary>
    /// From a swarm, get the diff�rent groups based on agent perception and graph theory.
    /// An agent belong to only one cluster.
    /// </summary>
    /// <returns> A <see cref="List{T}"/> of clusters represented by a <see cref="List{T}"/> of <see cref="AgentData"/>.</returns>
    public static List<List<AgentData>> GetClusters(SwarmData swarm)
    {
        //Create the list that will store the different clusters
        List<List<AgentData>> clusters = new List<List<AgentData>>();

        //Create a clone of the log agent data list, to manipulate it
        List<AgentData> agentsClone = new List<AgentData>(swarm.GetAgentsData());

        while (agentsClone.Count > 0)
        {
            //Create the list representing the first cluster
            List<AgentData> newCluster = new List<AgentData>();
            //The first agent will be choose by default
            AgentData firstAgent = agentsClone[0];
            //Remove the first agent from the list containing all agents (it now belongs to a cluster)
            agentsClone.Remove(firstAgent);
            //Add first agent in the new cluster
            newCluster.Add(firstAgent);

            int i = 0;
            while (i < newCluster.Count)
            {
                List<AgentData> temp = GetNeighbours(newCluster[i], swarm.GetAgentsData(), swarm.GetParameters().GetFieldOfViewSize(), swarm.GetParameters().GetBlindSpotSize());
                foreach (AgentData g in temp)
                {
                    //Check if the neighbour does not already belong to the current cluster
                    if (!newCluster.Contains(g))
                    {
                        bool res = agentsClone.Remove(g);
                        if (res) newCluster.Add(g);
                    }
                }
                i++;
            }
            clusters.Add(newCluster);
        }
        return clusters;
    }

    /// <summary>
    /// From a swarm get the different groups based on agent perception and graph theory. Those groups are sorted by size.
    /// An agent belongs to only one cluster.
    /// </summary>
    /// <returns> 
    /// A <see cref="List{T}"/> of clusters represented by a <see cref="List{T}"/> of <see cref="AgentData"/>. 
    /// They are sorted by size, from the largest group to the smallest.
    /// </returns>
    public static List<List<AgentData>> GetOrderedClusters(SwarmData swarm)
    {
        List<List<AgentData>> orderedClusters = new List<List<AgentData>>();

        List<List<AgentData>> clusters = GetClusters(swarm);

        while (clusters.Count > 0)
        {
            int maxSize = -1;
            List<AgentData> biggerCluster = null;

            foreach (List<AgentData> c in clusters)
            {
                if (c.Count > maxSize)
                {
                    maxSize = c.Count;
                    biggerCluster = c;
                }
            }
            orderedClusters.Add(biggerCluster);
            clusters.Remove(biggerCluster);
        }

        return orderedClusters;
    }


    /// <summary>
    /// From a list of linked agents, compute the different cluster (non-connected graphs) and return it.
    /// </summary>
    /// <param name="linksList">The list of connected agents.</param>
    /// <param name="agents">The list of agents.</param>
    /// <returns>The list containing all clusters.</returns>
    private List<List<AgentData>> GetClustersFromLinks(List<Tuple<AgentData, AgentData>> linksList, List<AgentData> agents)
    {
        //Clone the links list to modify it
        List<Tuple<AgentData, AgentData>> clone = new List<Tuple<AgentData, AgentData>>(linksList);


        //Create the list that will store the different clusters
        List<List<AgentData>> clusters = new List<List<AgentData>>();


        while (clone.Count > 0)
        {
            //Take the first link to start a new cluster
            List<AgentData> newCluster = new List<AgentData>();

            //Add the two agents of the link
            newCluster.Add(clone[0].Item1);
            newCluster.Add(clone[0].Item2);

            //Remove the two agents from the list
            agents.Remove(clone[0].Item1);
            agents.Remove(clone[0].Item2);

            //Remove the link from the list
            clone.RemoveAt(0);

            int i = 0;
            while (i < newCluster.Count)
            {
                List<Tuple<AgentData, AgentData>> temp = new List<Tuple<AgentData, AgentData>>();
                foreach (Tuple<AgentData, AgentData> l in clone)
                {
                    if (System.Object.ReferenceEquals(l.Item1, newCluster[i]))
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

                foreach (Tuple<AgentData, AgentData> t in temp)
                {
                    clone.Remove(t);
                }
                i++;
            }
            clusters.Add(newCluster);
        }

        //Now, all the clusters are defined from the links. However, some agents are missing because they are isolated.
        //Complete the list of cluster by adding isolated agents
        foreach (AgentData a in agents)
        {
            List<AgentData> temp = new List<AgentData>();
            temp.Add(a);
            clusters.Add(temp);
        }

        return clusters;
    }


    #endregion

    #region Methods - Communities
    /// <summary>
    /// This method compute agents' communities from a list of agents (from a swarm) based on their links with other agents and a modularity score using the Louvain algorithm.
    /// </summary>
    /// <param name="swarm"> Le current swarm analysed.</param>
    /// <returns> A <see cref="List{T}"/> of communities (<see cref="List{}"/> of <see cref="AgentData"/>)</returns>
    public static List<List<AgentData>> GetCommunities(SwarmData swarm)
    {
        List<AgentData> agents = swarm.GetAgentsData();

        //Get the adjacent matrix from the agents' list
        bool[,] adjacentMatrix = GetAdjacentMatrix(swarm);
        //Get the total number of edges from the adjacent matrix
        int m = GetNumberOfEdge(adjacentMatrix);

        //In order to use the adjacent matrix whith integer indexes, replace Agent with their position in the list of agents
        List<List<int>> keyCommunities = new List<List<int>>();

        //In the Louvain algorithm, each agent is a community at the start
        for (int i = 0; i < agents.Count; i++)
        {
            List<int> temp = new List<int>();
            temp.Add(i);
            keyCommunities.Add(temp);
        }


        bool done = false;
        while (!done)
        {
            //Search for the best modularity score between two communities
            float bestModularityScore = float.MinValue;
            List<int> bestCom1 = null;
            List<int> bestCom2 = null;

            //Test for each pair of communities
            for (int i = 0; i < keyCommunities.Count - 1; i++)
            {
                for (int j = i + 1; j < keyCommunities.Count; j++)
                {
                    float score = GetModularityScore(keyCommunities[i], keyCommunities[j], adjacentMatrix, m);
                    if (score > bestModularityScore)
                    {
                        bestModularityScore = score;
                        bestCom1 = keyCommunities[i];
                        bestCom2 = keyCommunities[j];
                    }
                }
            }

            //If the modularity score is positive, merge the two corresponding communities and repeat. Else, stop.
            if (bestModularityScore <= 0.0f) done = true;
            else
            {
                bestCom1.AddRange(bestCom2);
                keyCommunities.Remove(bestCom2);
            }
        }

        //Last step, convert communities of key into communities of agents
        List<List<AgentData>> communities = new List<List<AgentData>>();
        foreach (List<int> c in keyCommunities)
        {
            List<AgentData> agentCom = new List<AgentData>();
            foreach (int i in c)
            {
                agentCom.Add(agents[i]);
            }
            communities.Add(agentCom);
        }

        return communities;
    }

    /// <summary>
    /// This method compute agents' communities from a list of agents (from a swarm) based on their links with other agents and a modularity score using the Louvain algorithm.
    /// </summary>
    /// <param name="swarm"> Le current swarm analysed.</param>
    /// <returns> A <see cref="List{T}"/> of communities (<see cref="List{}"/> of <see cref="AgentData"/>), ordered by community size.</returns>
    public static List<List<AgentData>> GetOrderedCommunities(SwarmData swarm)
    {
        List<List<AgentData>> communities = GetCommunities(swarm);
        for (int i = 1; i < communities.Count; i++)
        {
            for (int j = 0; j < communities.Count - i; j++)
            {
                if (communities[j].Count > communities[j + 1].Count)
                {
                    List<AgentData> temp = communities[j];
                    communities[j] = communities[j + 1];
                    communities[j + 1] = temp;
                }
            }
        }
        return communities;
    }


    /// <summary>
    /// From two agents communities, computes the modularity score as if they were merged; and return it.
    /// </summary>
    /// <param name="community1">The first community containing <see cref="int"/> keys referring to the indexes of the adjacent matrix (each key is an agent).</param>
    /// <param name="community2">The second community containing <see cref="int"/> keys referring to the indexes of the adjacent matrix (each key is an agent).</param>
    /// <param name="adjacentMatrix"> The adjacent matrix of the graph. Can be obtained using <see cref="GetAdjacentMatrix(List{Agent})"/>.</param>
    /// <param name="m"> The number of edge of the graph. Can be obtained using <see cref="GetNumberOfEdge(bool[,])"/> from the adjacent matrix.</param>
    /// <returns> The modularity score from the two merged communities.</returns>
    private static float GetModularityScore(List<int> community1, List<int> community2, bool[,] adjacentMatrix, int m)
    {
        float score = 0.0f; //https://www.youtube.com/watch?v=lG5hkAHo-zs

        //Compute for each pair of agents a score
        foreach (int i in community1)
        {
            foreach (int j in community2)
            {
                int ki = GetNumberOfEdgeFromNode(i, adjacentMatrix);
                int kj = GetNumberOfEdgeFromNode(j, adjacentMatrix);
                float val = (ki * kj) / (2.0f * (float)m);

                //Check if the two agents are direclty linked or not
                if (adjacentMatrix[i, j])
                {
                    val = 1 - val;
                }
                else
                {
                    val = 0 - val;
                }

                //Add the score to the total
                score += val;
            }
        }
        return score;
    }

    #endregion

    #region Methods - Leaves, branches and trunk

    /// <summary>
    /// From a graph of agents, separate agents into 3 categories.
    /// Leaves : agents that share no link with other agents
    /// Branches :  agents from branches of the graph (only one way to reach them, and the graph end at the end of the branch)
    /// Trunk : the lasting agents
    /// </summary>
    /// <param name="swarm"> The swarm analysed.</param>
    /// <returns>A <see cref="Tuple"/> containing leaves, branches and trunk agents.</returns>
    public static Tuple<List<AgentData>, List<AgentData>, List<AgentData>> SeparateLeavesBranchesAndTrunk(SwarmData swarm)
    {
        return SeparateLeavesBranchesAndTrunk(swarm.GetAgentsData(), GetAdjacentMatrix(swarm));
    }


    /// <summary>
    /// From a graph of agents, separate agents into 3 categories.
    /// Leaves : agents that share no link with other agents
    /// Branches :  agents from branches of the graph (only one way to reach them, and the graph end at the end of the branch)
    /// Trunk : the lasting agents
    /// </summary>
    /// <param name="agents"> The agents of the graph.</param>
    /// <param name="adjacentMatrix">The corresponding adjacent matrix of the graph.</param>
    /// <returns>A <see cref="Tuple"/> containing leaves, branches and trunk agents.</returns>
    public static Tuple<List<AgentData>, List<AgentData>, List<AgentData>> SeparateLeavesBranchesAndTrunk(List<AgentData> agents, bool[,] adjacentMatrix)
    {
        List<AgentData> leaves = new List<AgentData>();
        List<AgentData> branches = new List<AgentData>();
        List<AgentData> trunk = new List<AgentData>(agents);

        Tuple<List<AgentData>, List<AgentData>, List<AgentData>> res = new Tuple<List<AgentData>, List<AgentData>, List<AgentData>>(leaves, branches, trunk);

        bool finished = false;
        while (!finished)
        {
            finished = true;
            int[] degreeMatrix = GetDegreeMatrix(adjacentMatrix);
            for (int i = 0; i < degreeMatrix.GetLength(0); i++)
            {
                if (degreeMatrix[i] == 0)
                {
                    leaves.Add(trunk[i]);
                    trunk.RemoveAt(i);
                    adjacentMatrix = RemoveIndexInSquareMatrix(adjacentMatrix, i);
                    finished = false;
                    break;
                }
            }
        }


        finished = false;
        while (!finished)
        {
            finished = true;
            int[] degreeMatrix = GetDegreeMatrix(adjacentMatrix);
            for (int i = 0; i < degreeMatrix.GetLength(0); i++)
            {
                if (degreeMatrix[i] <= 1)
                {
                    branches.Add(trunk[i]);
                    trunk.RemoveAt(i);
                    finished = false;
                    adjacentMatrix = RemoveIndexInSquareMatrix(adjacentMatrix, i);
                    break;
                }
            }
        }

        int count = leaves.Count + branches.Count + trunk.Count;
        if (count != agents.Count) Debug.LogError("Cette m�thode duplique ou oublie des agents!");

        return res;

    }



    #endregion

    #region Methods - Graph matrix

    /// <summary>
    /// Compute the adjacent matrix from the agents of the swarm (undirected graph). 
    /// This method uses <see cref="GetAdjacentMatrix(List{AgentData}, float, float)"/> method.
    /// </summary>
    /// <param name="swarm">The analysed swarm.</param>
    /// <returns>The adjacent matrix as a 2D array.</returns>    
    public static bool[,] GetAdjacentMatrix(SwarmData swarm)
    {
        List<AgentData> agents = swarm.GetAgentsData();

        float fovSize = swarm.GetParameters().GetFieldOfViewSize();
        float bsSize = swarm.GetParameters().GetBlindSpotSize();

        bool[,] adjacentMatrix = GetAdjacentMatrix(agents, fovSize, bsSize);

        return adjacentMatrix;
    }

    /// <summary>
    /// Compute the adjacent matrix from the agents, based on the field of view size and the blind spot size set in parameters
    /// </summary>
    /// <param name="agents">The list of agents</param>
    /// <param name="fovSize">The field of view size in meters</param>
    /// <param name="bsSize">The blind spot size in degree</param>
    /// <returns>The adjacent matrix as a 2D array.</returns>
    public static bool[,] GetAdjacentMatrix(List<AgentData> agents, float fovSize, float bsSize)
    {
        bool[,] adjacentMatrix = new bool[agents.Count, agents.Count];

        //For each pair of agents
        for (int i = 0; i < agents.Count; i++)
        {
            for (int j = 0; j < agents.Count; j++)
            {
                //If both agents are linked
                if (i != j && Linked(agents[i], agents[j], fovSize, bsSize))
                {
                    adjacentMatrix[i, j] = true;
                }
                else
                {
                    adjacentMatrix[i, j] = false;
                }
            }
        }
        return adjacentMatrix;
    }

    /// <summary>
    /// Compute the degree matrix from the corresponding adjacent matrix.
    /// </summary>
    /// <param name="adjacentMatrix">The adjacent matrix of the graph.</param>
    /// <returns>The corresponding degree matrix.</returns>
    public static int[] GetDegreeMatrix(bool[,] adjacentMatrix)
    {
        int[] degreeMatrix = new int[adjacentMatrix.GetLength(0)];

        for (int i = 0; i < adjacentMatrix.GetLength(0); i++)
        {
            int count = 0; //Count the number of links of the current node
            for (int j = 0; j < adjacentMatrix.GetLength(1); j++)
            {
                if (adjacentMatrix[i, j])
                {
                    count++;
                }
            }
            degreeMatrix[i] = count;
        }
        return degreeMatrix;
    }

    /// <summary>
    /// Compute the Laplacian matrix from the adjacent matrix (undirected graph)
    /// </summary>
    /// <param name="adjacentMatrix">The adjacent matrix of the analysed graph.</param>
    /// <returns>The Laplacian matrix as a 2D array.</returns>    
    public static int[,] GetLaplacianMatrix(bool[,] adjacentMatrix)
    {

        int height = adjacentMatrix.GetLength(0);
        int width = adjacentMatrix.GetLength(1);
        int[,] laplacianMatrix = new int[height, width];

        for (int i = 0; i < height; i++)
        {
            int count = 0; //Count the number of links of the current node
            for (int j = 0; j < width; j++)
            {
                if (adjacentMatrix[i, j])
                {
                    count++;
                    laplacianMatrix[i, j] = -1;
                }
            }
            laplacianMatrix[i, i] = count;
        }
        return laplacianMatrix;
    }


    /// <summary>
    /// Calculate the total number of edges of a graph from the adjacent matrix.
    /// </summary>
    /// <param name="adjacentMatrix">The adjacent matrix of a graph.</param>
    /// <returns> The number of edges.</returns>
    private static int GetNumberOfEdge(bool[,] adjacentMatrix)
    {
        int count = 0;

        int width = adjacentMatrix.GetLength(0);
        int height = adjacentMatrix.GetLength(1);

        for (int i = 0; i < width - 1; i++)
        {
            for (int j = i + 1; j < height; j++)
            {
                if (adjacentMatrix[i, j]) count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Calculate the number of edges of one node from the adjacent matrix.
    /// </summary>
    /// <param name="node">The id of the node, referring to its line in the adjacent matrix.</param>
    /// <param name="adjacentMatrix">The adejacent matrix of a graph.</param>
    /// <returns>The number of edges of the node.</returns>
    private static int GetNumberOfEdgeFromNode(int node, bool[,] adjacentMatrix)
    {
        int count = 0;

        int height = adjacentMatrix.GetLength(1);

        for (int i = 0; i < height; i++)
        {
            if (adjacentMatrix[node, i]) count++;
        }

        return count;

    }

    /// <summary>
    /// Remove a specified entry from a square matrix. Consequently remove a line and a column.
    /// </summary>
    /// <param name="squareMatrix">The square matrix whose size must be reduced.</param>
    /// <param name="index">The index of the line/column that will be removed.</param>
    /// <returns>The reduced square matrix (of 1).</returns>
    private static bool[,] RemoveIndexInSquareMatrix(bool[,] squareMatrix, int index)
    {
        //Check possible issues
        if (squareMatrix.GetLength(0) != squareMatrix.GetLength(1)) throw new Exception("The specified matrix is not square");
        if (index >= squareMatrix.GetLength(0) || index < 0) throw new ArgumentOutOfRangeException("The specified index is out of the bound of the matrix.");
        if (squareMatrix.GetLength(0) == 1) return new bool[0, 0];

        //Compute the size of the resulting square matrix
        int newSize = squareMatrix.GetLength(0) - 1;

        //Create the new square matrix
        bool[,] newMatrix = new bool[newSize, newSize];

        //Fill the new square matrix, using the original matrix
        int i2 = 0;
        for (int i = 0; i < newSize; i++)
        {
            //Index line must be avoided
            if (i2 == index) i2++;
            int j2 = 0;
            for (int j = 0; j < newSize; j++)
            {
                //Index column must be avoided
                if (j2 == index) j2++;
                newMatrix[i, j] = squareMatrix[i2, j2];
                j2++;

            }
            i2++;
        }

        return newMatrix;
    }

    #endregion

    #region Methods - Center of mass
    /// <summary>
    /// Compute the center of mass of the swarm.
    /// </summary>
    /// <param name="swarmData"> The swarm data.</param>
    /// <returns>The center of mass of the swarm.</returns>
    public static Vector3 GetCenterOfMass(SwarmData swarmData)
    {
        List<AgentData> agents = swarmData.GetAgentsData();

        return GetCenterOfMass(agents);
    }

    public static Vector3 GetCenterOfMass(List<AgentData> agents)
    {
        Vector3 centerOfMass = Vector3.zero;
        foreach (AgentData a in agents)
        {
            centerOfMass += a.GetPosition();
        }
        centerOfMass /= agents.Count;
        return centerOfMass;
    }
    #endregion

    #region Methods - KNN
    /// <summary>
    /// Get the KNN of an agent. If more neighbours are asked than the number of agents, it return the agents.
    /// </summary>
    /// <param name="agent">Agent for which we are looking for the KNN</param>
    /// <param name="l">The list of agents</param>
    /// <param name="n">Number of nearest neighbours searched</param>
    /// <returns>Return the KNN of the specified agent</returns>
    public static List<AgentData> KNN(AgentData agent, List<AgentData> l, int n)
    {
        List<AgentData> agents = new List<AgentData>(l);
        List<AgentData> knn = new List<AgentData>();

        //Check if there are enough agents
        if (agents.Count < n) n = agents.Count;

        for (int i = 0; i < n; i++)
        {
            AgentData nearest = NearestAgent(agent, agents);
            knn.Add(nearest);
            agents.Remove(nearest);
        }

        return knn;
    }

    /// <summary>
    /// Get the nearest agent of an agent from a list.
    /// </summary>
    /// <param name="agent">Agent for which we are looking for the nearest agent</param>
    /// <param name="l">The list of agents</param>
    /// <returns>Return the nearest neighbour</returns>
    public static AgentData NearestAgent(AgentData agent, List<AgentData> l)
    {
        float min = float.MaxValue;
        AgentData minAgent = null;
        foreach (AgentData a in l)
        {
            if (System.Object.ReferenceEquals(a, agent)) continue;

            float dist = Vector3.Distance(a.GetPosition(), agent.GetPosition());

            if (dist < min)
            {
                min = dist;
                minAgent = a;
            }
        }
        return minAgent;
    }
    #endregion

    #region Methods - Convex hul

    /// <summary>
    /// Get the convex hul of each cluster. 
    /// </summary>
    /// <param name="swarmData">The analysed swarm</param>
    /// <returns>The list of each cluster's convex hul.</returns>
    public static List<List<Vector3>> GetConvexHul(SwarmData swarmData)
    {
        List<List<Vector3>> convexHuls = new List<List<Vector3>>();
        List<List<AgentData>> clusters = SwarmTools.GetOrderedClusters(swarmData);

        foreach (List<AgentData> c in clusters)
        {
            if (c.Count < 3) continue;
            List<Vector3> positions = new List<Vector3>();

            foreach (AgentData g in c)
            {
                positions.Add(g.GetPosition());
            }

            //Calcul du point pivot
            float ordinate = float.MaxValue;
            float abcissa = float.MaxValue;
            Vector3 pivot = Vector3.zero;
            foreach (Vector3 p in positions)
            {
                if (p.z < ordinate || (p.z == ordinate && p.x < abcissa))
                {
                    pivot = p;
                    ordinate = pivot.z;
                    abcissa = pivot.x;
                }
            }
            positions.Remove(pivot);

            //Calcul des angles pour tri
            List<float> angles = new List<float>();
            Vector3 abissaAxe = new Vector3(1, 0, 0);
            foreach (Vector3 p in positions)
            {
                Vector3 temp = p - pivot;
                angles.Add(Vector3.Angle(temp, abissaAxe));
            }

            //Tri des points
            for (int i = 1; i < positions.Count; i++)
            {
                for (int j = 0; j < positions.Count - i; j++)
                {
                    if (angles[j] > angles[j + 1])
                    {
                        float temp = angles[j + 1];
                        angles[j + 1] = angles[j];
                        angles[j] = temp;

                        Vector3 tempPos = positions[j + 1];
                        positions[j + 1] = positions[j];
                        positions[j] = tempPos;
                    }
                }
            }
            angles.Clear();
            positions.Insert(0, pivot);

            //It�rations
            List<Vector3> pile = new List<Vector3>();
            pile.Add(positions[0]);
            pile.Add(positions[1]);

            for (int i = 2; i < positions.Count; i++)
            {
                while ((pile.Count >= 2) && VectorialProduct(pile[pile.Count - 2], pile[pile.Count - 1], positions[i]) <= 0 || pile[pile.Count - 1] == positions[i])
                {
                    pile.RemoveAt(pile.Count - 1);
                }
                pile.Add(positions[i]);
            }

            convexHuls.Add(pile);
        }
        return convexHuls;
    }


    /// <summary>
    /// Compute the area of the convex hul of the biggest cluster of the swarm.
    /// </summary>
    /// <param name="swarmData">The analysed swarm</param>
    /// <returns>The area of the convex hul of the biggest cluster.</returns>
    public static float GetConvexHulArea(SwarmData swarm)
    {
        //Get convex huls of the swarm
        List<List<Vector3>> convexHuls = SwarmTools.GetConvexHul(swarm);

        //Keep only the convex hul of the biggest cluster
        List<Vector3> convexHul = convexHuls[0];

        //Compute the mean position of the vertices composing the convex hull
        Vector3 pointC = Vector3.zero;

        foreach (Vector3 v in convexHul)
        {
            pointC += v;
        }

        pointC /= convexHul.Count;


        float totalArea = 0.0f;

        //Split the convex hull into multiple triangles to compute triangles area independently using H�ron formula
        for (int i = 0; i < convexHul.Count; i++)
        {
            Vector3 pointA = convexHul[i];
            int j = (i + 1) % convexHul.Count;
            Vector3 pointB = convexHul[j];

            float a = Vector3.Distance(pointA, pointB);
            float b = Vector3.Distance(pointC, pointB);
            float c = Vector3.Distance(pointA, pointC);

            float d = (a + b + c) / 2.0f;

            float A = Mathf.Sqrt(d * (d - a) * (d - b) * (d - c));

            totalArea += A;
        }

        return totalArea;
    }



    public static float VectorialProduct(Vector3 a, Vector3 b, Vector3 c)
    {
        return ((b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z));
    }
    #endregion

    #region Methods - Distance intra cluster
    public static float MeanKNNDistanceBiggerCluster(SwarmData swarmData, int k)
    {
        List<List<AgentData>> clusters = SwarmTools.GetOrderedClusters(swarmData);

        float dist = MeanKNNDistance(clusters[0], k);

        return dist;
    }

    public static float MeanKNNDistance(List<AgentData> cluster, int k)
    {
        //Check for lone agent
        if(cluster.Count < 2) return 0.0f;

        float meanDist = 0.0f;
        List<float> distances = new List<float>();

        foreach (AgentData g in cluster)
        {
            distances.AddRange(GetKNNDistances(cluster, g, k));
        }

        foreach (float d in distances)
        {
            meanDist += d;
        }

        meanDist /= distances.Count;

        return meanDist;
    }

    /// <summary>
    /// Calculate the k nearest distances from the agent set in parameter, to the other agents from the list set in paramter.
    /// </summary>
    /// <param name="cluster"> The set of agents from which the k nearest distances of the agent set in parameter will be calculated. </param>
    /// <param name="agent"> The agent reference to calculate the distances.</param>
    /// <param name="k">The maximum number of distances returned. </param>
    /// <returns>The k nearest distances to other agents, possibly less if there is not enough other agents.</returns>
    private static List<float> GetKNNDistances(List<AgentData> cluster, AgentData agent, int k)
    {
        //Compute every distance from parameter agent to other agents
        List<float> distances = new List<float>();

        //Compare current agent with all agents
        foreach (AgentData g in cluster)
        {
            //Check if the current agent is compared with itself
            if (System.Object.ReferenceEquals(g, agent)) continue;

            //Compute distance
            float dist = Vector3.Distance(g.GetPosition(), agent.GetPosition());

            distances.Add(dist);
        }


        //Sort list
        distances.Sort(new ListTools.GFG());


        //Get the knn
        List<float> knnDistances = new List<float>();

        if (distances.Count < k) k = distances.Count;
        for (int i = 0; i < k; i++)
        {
            knnDistances.Add(distances[i]);
        }

        return knnDistances;
    }

    #endregion

    #region Methods - Distance inter cluster


    public static float GetSignificantDistanceBetweenClusters(SwarmData swarmData)
    {
        List<List<AgentData>> clusters = SwarmTools.GetOrderedClusters(swarmData);

        //If there is not enough cluster, exit
        if (clusters.Count < 2) return -1;

        float significantDistance = -1;

        List<List<List<AgentData>>> superList = new List<List<List<AgentData>>>();
        //Creer des superlist des cluster
        foreach (List<AgentData> c in clusters)
        {
            List<List<AgentData>> temp = new List<List<AgentData>>();
            temp.Add(c);
            superList.Add(temp);
        }

        //Tant que le nombre de superlist est sup�rieur � 2
        while (superList.Count > 2)
        {
            //Calculer la distance plus petite entre deux clusters
            float minDist = float.MaxValue;
            List<List<AgentData>> minCs1 = null;
            List<List<AgentData>> minCs2 = null;

            for (int i = 0; i < superList.Count; i++)
            {
                List<List<AgentData>> cs1 = superList[i];
                for (int j = i; j < superList.Count; j++)
                {
                    if (i == j) continue;

                    List<List<AgentData>> cs2 = superList[j];

                    float dist = MinDistanceBetweenTwoClusterSets(cs1, cs2);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        minCs1 = cs1;
                        minCs2 = cs2;
                    }
                }
            }

            //Fusionner les superlists des clusters concern�s
            minCs1.AddRange(minCs2);
            //Supprimer de la liste le cluster fusionn�
            superList.Remove(minCs2);
        }

        //Normalement ici, il ne reste que deux clusters
        //Calculer la min dist entre les deux
        significantDistance = MinDistanceBetweenTwoClusterSets(superList[0], superList[1]);
        //la retourner


        return significantDistance;
    }

    private static float MinDistanceBetweenTwoClusterSets(List<List<AgentData>> clusterSet1, List<List<AgentData>> clusterSet2)
    {
        float minDist = float.MaxValue;
        foreach (List<AgentData> c1 in clusterSet1)
        {
            foreach (List<AgentData> c2 in clusterSet2)
            {
                float dist = MinDistanceBetweenTwoClusters(c1, c2);
                if (dist < minDist)
                    minDist = dist;
            }
        }
        return minDist;
    }


    /// <summary>
    /// Compute the min distance between two cluster, and return it.
    /// </summary>
    /// <param name="cluster1">The first cluster</param>
    /// <param name="cluster2">The other cluster</param>
    /// <returns>The min distance between the two clusters set in parameter.</returns>
    private static float MinDistanceBetweenTwoClusters(List<AgentData> cluster1, List<AgentData> cluster2)
    {
        float minDist = float.MaxValue;

        foreach (AgentData l1 in cluster1)
        {
            foreach (AgentData l2 in cluster2)
            {
                float dist = Vector3.Distance(l1.GetPosition(), l2.GetPosition());
                if (dist < minDist)
                    minDist = dist;
            }
        }
        return minDist;
    }
    #endregion

    #region Methods - Densities Grid

    public static Tuple<int[,], float> GetDensitiesUsingKNNWithinConvexArea(SwarmData swarmData)
    {
        Tuple<int[,], float> t = GetDensitiesUsingKNN(swarmData);
        int[,] densities = t.Item1;
        float zoneSize = t.Item2;
        List<List<Vector3>> convexHuls = SwarmTools.GetConvexHul(swarmData);

        Vector2 outsidePoint = new Vector2(-1, -1);

        for (int i = 0; i < densities.GetLength(0); i++)
        {
            for (int j = 0; j < densities.GetLength(1); j++)
            {
                if (densities[i, j] > 0) continue;

                float x = i * zoneSize + zoneSize / 2.0f;
                float z = j * zoneSize + zoneSize / 2.0f;

                Vector2 center = new Vector2(x, z);

                bool inside = false;

                foreach (List<Vector3> hul in convexHuls)
                {
                    int nb = 0;

                    for (int k = 0; k < hul.Count; k++)
                    {
                        int k2 = (k + 1) % hul.Count;

                        Vector2 p1 = new Vector2(hul[k].x, hul[k].z);
                        Vector2 p2 = new Vector2(hul[k2].x, hul[k2].z);

                        if (GeometryTools.LineSegmentsIntersect(center, outsidePoint, p1, p2)) nb++;
                    }

                    if (nb % 2 == 1)
                    {
                        inside = true;
                        break;
                    }

                }

                if (!inside) densities[i, j] = -1;
            }
        }

        return new Tuple<int[,], float>(densities, zoneSize);
    }


    public static Tuple<int[,], float> GetDensitiesUsingKNN(SwarmData swarmData)
    {
        float dist = MeanKNNDistanceBiggerCluster(swarmData, 2);
        int[,] densities = GetDensities(swarmData, dist);

        return new Tuple<int[,], float>(densities, dist);
    }

    public static int[,] GetDensities(SwarmData swarmData, float zoneSize = 0.5f)
    {
        int nbX = (int)(swarmData.GetParameters().GetMapSizeX() / zoneSize) + 1;
        int nbZ = (int)(swarmData.GetParameters().GetMapSizeZ() / zoneSize) + 1;

        int[,] densities = new int[nbX, nbZ];

        for (int i = 0; i < nbX; i++)
        {
            for (int j = 0; j < nbZ; j++)
            {
                densities[i, j] = 0;
            }
        }

        List<AgentData> agents = swarmData.GetAgentsData();

        foreach (AgentData agent in agents)
        {
            int x = (int)(agent.GetPosition().x / zoneSize);
            int z = (int)(agent.GetPosition().z / zoneSize);

            if (x >= nbX) Debug.Log("X sup�rieur : " + x + " & " + nbX);
            if (z >= nbZ) Debug.Log("Z sup�rieur : " + z + " & " + nbZ);

            densities[x, z]++;
        }

        return densities;
    }

    #endregion


    #region - Delaunay Triangulation

    /// <summary>
    /// Bowyer–Watson algorithm : https://en.wikipedia.org/wiki/Bowyer%E2%80%93Watson_algorithm
    /// </summary>
    /// <param name="swarm"></param>
    /// <returns></returns>
    public static List<Tuple<AgentData, AgentData, AgentData>> GetDelaunayTriangulation(SwarmData swarm)
    {
        //Create the list that will contains all the triangles of the triangulation
        List<Tuple<AgentData, AgentData, AgentData>> triangles = new List<Tuple<AgentData, AgentData, AgentData>>();


        //------------------------------------------------------
        //Generate the super triangle ABC containing all agents
        //------------------------------------------------------

        float xmin = float.MaxValue;
        float zmin = float.MaxValue;
        float xmax = float.MinValue;
        float zmax = float.MinValue;

        //Get the min bounding box coordinates of the swarm
        foreach (AgentData a in swarm.GetAgentsData())
        {
            if (a.GetPosition().x < xmin) xmin = a.GetPosition().x;

            if (a.GetPosition().z < zmin) zmin = a.GetPosition().z;

            if (a.GetPosition().x > xmax) xmax = a.GetPosition().x;

            if (a.GetPosition().z > zmax) zmax = a.GetPosition().z;
        }

        //Create 2 points, respectively the corner top right and bottom left
        Vector2 max = new Vector2(xmax, zmax);
        Vector2 min = new Vector2(xmin, zmin);

        //Use the diagonal of the bounding box to define the padding
        float padding = (Vector2.Distance(min, max)+1)*4; //It may be a bit big :p

        //Compute the three vertex of the super triangle
        Vector3 A = new Vector3(xmin - padding, 0, zmin - padding);
        Vector3 B = new Vector3(xmax + padding, 0, zmin - padding);
        Vector3 C = new Vector3(((xmin + xmax) / 2), 0, zmax + padding);

        //Create 3 agents representing the vertex of the super triangle
        AgentData agentA = new AgentData(A, A);
        AgentData agentB = new AgentData(B, B);
        AgentData agentC = new AgentData(C, C);

        //Create the super triangle using the 3 newly created agents
        Tuple<AgentData, AgentData, AgentData> superTriangle = new Tuple<AgentData, AgentData, AgentData>(agentA, agentB, agentC);

        //Add the super triangle in the list of triangles
        triangles.Add(superTriangle);

        //---------------------
        //Add agents one by one
        //---------------------

        //For each agent of the swarm
        foreach (AgentData a in swarm.GetAgentsData())
        {
            bool ok = false;

            //Create a list that will contains the triangles whose circumscribed circles contains the agent
            List< Tuple < AgentData, AgentData, AgentData >> taggedTriangles = new List<Tuple<AgentData, AgentData, AgentData>>();

            //------------------------------------------------------
            //Check which triangles contain the agent and mark them.
            //------------------------------------------------------
            foreach (Tuple<AgentData, AgentData, AgentData> t in triangles)
            {
                //Check if the agent lies in the triangle's circumsribed circle
                if(GeometryTools.PointInTheCircumscribedCircle(a.GetPosition(), t.Item1.GetPosition(), t.Item2.GetPosition(), t.Item3.GetPosition()))
                {            
                    //Marked the triangle if it contains the agent
                    taggedTriangles.Add(t);

                    ok = true;
                }
            }

            //----------------------------------------------------
            //Remove marked triangles and add the new ones if they 
            //don't share an edge with another triangle
            //----------------------------------------------------
            foreach (Tuple<AgentData, AgentData, AgentData> t in taggedTriangles)
            {
                //When an agent is added into a triangle, 3 new triangles will be created
                //But a new triangle is created only if it don't share an edge with another marked triangle

                //---------------------------
                //Test for the first triangle
                //---------------------------

                Tuple<AgentData, AgentData> edge = new Tuple<AgentData, AgentData>(t.Item1, t.Item2);
                bool test = false;
                foreach (Tuple<AgentData, AgentData, AgentData> tr in taggedTriangles)
                {
                    //Test whether the 2 triangles are the same one
                    if (System.Object.ReferenceEquals(t, tr)) continue;

                    //Check if the triangle contains the edege
                    if (TriangleContainsEdge(tr, edge))
                    {
                        test = true;
                        break;
                    }
                }
                if (!test)
                {
                    Tuple<AgentData, AgentData, AgentData> t1 = new Tuple<AgentData, AgentData, AgentData>(a, t.Item1, t.Item2);
                    triangles.Add(t1);
                }

                //----------------------------
                //Test for the second triangle
                //----------------------------

                edge = new Tuple<AgentData, AgentData>(t.Item2, t.Item3);
                test = false;
                foreach (Tuple<AgentData, AgentData, AgentData> tr in taggedTriangles)
                {
                    //Test whether the 2 triangles are the same one
                    if (System.Object.ReferenceEquals(t, tr)) continue;

                    //Check if the triangle contains the edege
                    if (TriangleContainsEdge(tr, edge))
                    {
                        test = true;
                        break;
                    }
                }
                if (!test)
                {
                    Tuple<AgentData, AgentData, AgentData> t2 = new Tuple<AgentData, AgentData, AgentData>(a, t.Item2, t.Item3); 
                    triangles.Add(t2);
                }

                //---------------------------
                //Test for the third triangle
                //---------------------------

                edge = new Tuple<AgentData, AgentData>(t.Item3, t.Item1);
                test = false;
                foreach (Tuple<AgentData, AgentData, AgentData> tr in taggedTriangles)
                {
                    //Test whether the 2 triangles are the same one
                    if (System.Object.ReferenceEquals(t, tr)) continue;

                    //Check if the triangle contains the edege
                    if (TriangleContainsEdge(tr, edge))
                    {
                        test = true;
                        break;
                    }
                }
                if (!test)
                {
                    Tuple<AgentData, AgentData, AgentData> t3 = new Tuple<AgentData, AgentData, AgentData>(a, t.Item3, t.Item1);
                    triangles.Add(t3);
                }

                //Delete the triangle in any case
                triangles.Remove(t);
            }

            //All agents must have an enclosing triangle
            if (!ok) Debug.LogError("The agent has not found an enclosing triangle.");
        }

        //-------------------------------------------------------------
        //Remove the vertices of the super triangle and their triangles
        //-------------------------------------------------------------

        //This list contains the triangles that will be deleted
        List<Tuple<AgentData, AgentData, AgentData>> lastTriangles = new List<Tuple<AgentData, AgentData, AgentData>>();

        //Check for each triangle if they contain one of the vertices of the super triangle
        foreach (Tuple<AgentData, AgentData, AgentData> t in triangles)
        {
            if (t.Item1.GetPosition() == A || t.Item1.GetPosition() == B || t.Item1.GetPosition() == C) { lastTriangles.Add(t); }
            if (t.Item2.GetPosition() == A || t.Item2.GetPosition() == B || t.Item2.GetPosition() == C) { lastTriangles.Add(t); }
            if (t.Item3.GetPosition() == A || t.Item3.GetPosition() == B || t.Item3.GetPosition() == C) { lastTriangles.Add(t); }
        }

        //Remove the triangles marked
        foreach (Tuple<AgentData, AgentData, AgentData> t in lastTriangles)
        { 
            triangles.Remove(t);
        }
        

        //Return the triangles of the triangulation
        return triangles;
    }

    /// <summary>
    /// This method checks whether the triangle contains the proposed edge. 
    /// It does not take into account the order of the vertices.
    /// </summary>
    /// <param name="triangle"> The triangle that may contains the edge or not.</param>
    /// <param name="edge"> The tested edge.</param>
    /// <returns>True if the triangle contains the edge, false otherwise.</returns>
    public static bool TriangleContainsEdge(Tuple<AgentData, AgentData, AgentData> triangle, Tuple<AgentData, AgentData> edge)
    {
        //Test the first edge of the triangle
        if (System.Object.ReferenceEquals(edge.Item1, triangle.Item1) && System.Object.ReferenceEquals(edge.Item2, triangle.Item2)) return true;
        if (System.Object.ReferenceEquals(edge.Item1, triangle.Item2) && System.Object.ReferenceEquals(edge.Item2, triangle.Item1)) return true;

        //Test the second edge of the triangle
        if (System.Object.ReferenceEquals(edge.Item1, triangle.Item2) && System.Object.ReferenceEquals(edge.Item2, triangle.Item3)) return true;
        if (System.Object.ReferenceEquals(edge.Item1, triangle.Item3) && System.Object.ReferenceEquals(edge.Item2, triangle.Item2)) return true;

        //Test the third edge of the triangle
        if (System.Object.ReferenceEquals(edge.Item1, triangle.Item3) && System.Object.ReferenceEquals(edge.Item2, triangle.Item1)) return true;
        if (System.Object.ReferenceEquals(edge.Item1, triangle.Item1) && System.Object.ReferenceEquals(edge.Item2, triangle.Item3)) return true;


        return false;
    }



    

    #endregion

    #region Methods - Concave hul
    // Duckham, M., Kulik, L., Worboys, M.F., Galton, A. (2008)
    // Efficient generation of simple polygons for characterizing the shape of a set of points in the plane.
    // Pattern Recognition v41, 3224-3236

    //https://link.springer.com/chapter/10.1007/978-3-642-21593-3_19#preview


    // Sharvit, D., Chan, J., Tek, H., & Kimia, B. B. (1998). 
    //Symmetry-based indexing of image databases. 
    //Journal of Visual Communication and Image Representation, 9(4), 366-380.


    //https://en.wikipedia.org/wiki/Delaunay_triangulation

    //https://en.wikipedia.org/wiki/Delaunay_tessellation_field_estimator


    //https://journals.sagepub.com/doi/10.1177/2041669519897681
    //https://www.ncbi.nlm.nih.gov/pmc/articles/PMC4978768/

    //https://link.springer.com/article/10.1007/s11042-015-2605-6

    //https://www.hindawi.com/journals/cin/2015/708759/
    //https://jov.arvojournals.org/article.aspx?articleid=2778465

    #endregion
}
