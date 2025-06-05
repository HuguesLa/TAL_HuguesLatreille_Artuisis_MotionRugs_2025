[System.Serializable]
public class SwarmParameters
{
    //--Swarm behaviour--//
    private BehaviourManager.AgentBehaviour agentBehaviour; //Define the current behaviour of the agents

    //--Swarm movement--//
    private MovementManager.AgentMovement agentMovement; //Defines how the agent moves

    //--Map parameters--//
    private float mapSizeX;
    private float mapSizeZ;

    //--Field of view size--//
    private float fieldOfViewSize; //This is the size of the radius
    private float blindSpotSize; //This is the size of blind spot of the agent (in degrees)

    //--Intensity parameters--//
    private float maxSpeed;
    private float moveForwardIntensity;
    private float randomMovementIntensity;
    private float frictionIntensity;
    private float avoidCollisionWithNeighboursIntensity;


    //--Reynolds model parameters--//
    private float cohesionIntensity;
    private float alignmentIntensity;
    private float separationIntensity;


    //--Couzin model parameters--//
    private float attractionZoneSize; //This is the size of the radius
    private float alignmentZoneSize; //This is the size of the radius
    private float repulsionZoneSize; //This is the size of the radius

    //--Preservation of connectivity parameters--//
    private float distanceBetweenAgents;

    #region Methods - Constructor
    public SwarmParameters(
        BehaviourManager.AgentBehaviour agentBehaviour,
        MovementManager.AgentMovement agentMovement, 
        float mapSizeX,
        float mapSizeZ,
        float fieldOfViewSize, 
        float blindSpotSize,
        float maxSpeed,
        float moveForwardIntensity,
        float randomMovementIntensity,
        float frictionIntensity,
        float avoidCollisionWithNeighboursIntensity,
        float cohesionIntensity,
        float alignmentIntensity,
        float separationIntensity,
        float attractionZoneSize,
        float alignmentZoneSize,
        float repulsionZoneSize,
        float distanceBetweenAgents)
    {
        this.agentBehaviour = agentBehaviour;
        this.agentMovement = agentMovement;
        this.mapSizeX = mapSizeX;
        this.mapSizeZ = mapSizeZ;
        this.fieldOfViewSize = fieldOfViewSize;
        this.blindSpotSize = blindSpotSize;
        this.maxSpeed = maxSpeed;
        this.moveForwardIntensity = moveForwardIntensity;
        this.randomMovementIntensity = randomMovementIntensity;
        this.frictionIntensity = frictionIntensity;
        this.avoidCollisionWithNeighboursIntensity = avoidCollisionWithNeighboursIntensity;
        this.cohesionIntensity = cohesionIntensity;
        this.alignmentIntensity = alignmentIntensity;
        this.separationIntensity = separationIntensity;
        this.attractionZoneSize = attractionZoneSize;
        this.alignmentZoneSize = alignmentZoneSize;
        this.repulsionZoneSize = repulsionZoneSize;
        this.distanceBetweenAgents = distanceBetweenAgents;
    }
    #endregion

    #region Methods - Getter
    public SwarmParameters Clone()
    {
        SwarmParameters parameters = (SwarmParameters)this.MemberwiseClone();

        return parameters;
    }

    //--Agent behaviour--//
    public BehaviourManager.AgentBehaviour GetAgentBehaviour()
    {
        return agentBehaviour;
    }

    //--Agent movement--//
    public MovementManager.AgentMovement GetAgentMovement()
    {
        return agentMovement;
    }

    //--Map parameters--//
    public float GetMapSizeX()
    {
        return mapSizeX;
    }

    public float GetMapSizeZ()
    {
        return mapSizeZ;
    }

    //--Field of view size--//
    public float GetFieldOfViewSize()
    {
        return fieldOfViewSize;
    }

    public float GetBlindSpotSize()
    {
        return blindSpotSize;
    }

    //--Intensity parameters--//
    public float GetMaxSpeed()
    {
        return maxSpeed;
    }

    public float GetMoveForwardIntensity()
    {
        return moveForwardIntensity;
    }

    public float GetRandomMovementIntensity()
    {
        return randomMovementIntensity;
    }

    public float GetFrictionIntensity()
    {
        return frictionIntensity;
    }

    public float GetAvoidCollisionWithNeighboursIntensity()
    {
        return avoidCollisionWithNeighboursIntensity;
    }

    //--Reynolds model parameters--//
    public float GetSeparationIntensity()
    {
        return separationIntensity;
    }

    public float GetAlignmentIntensity()
    {
        return alignmentIntensity;
    }

    public float GetCohesionIntensity()
    {
        return cohesionIntensity;
    }

    //--Couzin model parameters--//
    public float GetAttractionZoneSize()
    {
        return attractionZoneSize;
    }
    public float GetAlignmentZoneSize()
    {
        return alignmentZoneSize;
    }
    public float GetRepulsionZoneSize()
    {
        return repulsionZoneSize;
    }

    //--Preservation of connectivity parameters--//
    public float GetDistanceBetweenAgents()
    {
        return distanceBetweenAgents;
    }

    #endregion
}
