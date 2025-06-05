using System.Collections.Generic;

[System.Serializable]
public class SwarmData
{
    #region Private fields
    private List<AgentData> agentsData;
    private SwarmParameters parameters;
    private SerializableRandom random;
    #endregion

    #region Methods - Constructor
    public SwarmData(List<AgentData> agentsData, SwarmParameters parameters)
    {
        this.agentsData = agentsData;
        this.parameters = parameters;
        this.random = new SerializableRandom();
    }

    public SwarmData(List<AgentData> agentsData, SwarmParameters parameters, SerializableRandom random)
    {
        this.agentsData = agentsData;
        this.parameters = parameters;
        this.random =  random;
    }
    #endregion

    #region Methods - Getter
    public List<AgentData> GetAgentsData()
    {
        return new List<AgentData>(this.agentsData);
    }

    public SwarmParameters GetParameters()
    {
        return this.parameters;
    }

    public SerializableRandom GetRandomGenerator()
    {
        return this.random;
    }

    public SwarmData Clone()
    {
        List<AgentData> agentsClone = new List<AgentData>();
        foreach(AgentData a in this.agentsData)
        {
            agentsClone.Add(a.Clone());
        }
        SwarmData clone = new SwarmData(agentsClone, parameters.Clone(),random.Clone());

        return clone;
    }
    #endregion

    #region Methods - Setter
    public void SetParameters(SwarmParameters parameters)
    {
        this.parameters = parameters;
    }
    #endregion
}
