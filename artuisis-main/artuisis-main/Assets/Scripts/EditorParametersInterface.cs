using UnityEngine;

/// <summary>
/// This script aims to provide an interface to control swarm's parameters in the unity editor
/// </summary>

public class EditorParametersInterface : MonoBehaviour
{
    [Header("Swarm behaviour")]
    [SerializeField]
    private BehaviourManager.AgentBehaviour agentBehaviour; //Define the current behaviour of the agents

    [Header("Swarm movement")]
    [SerializeField]
    private MovementManager.AgentMovement agentMovement; //Defines how the agent moves

    [Header("Map parameters")]
    [SerializeField]
    [Range(5, 20)]
    private float mapSizeX = 5.0f;

    [SerializeField]
    [Range(5, 20)]
    private float mapSizeZ = 5.0f;

    [Header("Field of view size")]
    [SerializeField]
    [Range(0.0f, 5.0f)]
    [Tooltip("This is the size of the radius.")]
    private float fieldOfViewSize = 1.0f;
    [SerializeField]
    [Range(0, 360)]
    [Tooltip("This is the size of blind spot of the agent (in degrees)")]
    private float blindSpotSize = 30;

    [Header("Intensity parameters")]
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float maxSpeed = 0.1f;
    [SerializeField]
    [Range(0.0f, 5.0f)]
    private float moveForwardIntensity = 0.0f;
    [SerializeField]
    [Range(0.0f, 5.0f)]
    private float randomMovementIntensity = 0.0f;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float frictionIntensity = 0.1f;
    [SerializeField]
    [Range(0.0f, 20.0f)]
    private float avoidCollisionWithNeighboursIntensity = 20.0f;


    [Header("Reynolds model parameters")]
    [SerializeField]
    [Range(0.0f, 5.0f)]
    private float cohesionIntensity = 1.0f;
    [SerializeField]
    [Range(0.0f, 5.0f)]
    private float alignmentIntensity = 1.0f;
    [SerializeField]
    [Range(0.0f, 5.0f)]
    private float separationIntensity = 1.0f;


    [Header("Couzin model parameters")]
    [SerializeField]
    [Range(0.0f, 1.0f)]
    [Tooltip("This is the size of the radius.")]
    private float attractionZoneSize = 0.3f;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    [Tooltip("This is the size of the radius.")]
    private float alignmentZoneSize = 0.3f;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    [Tooltip("This is the size of the radius.")]
    private float repulsionZoneSize = 0.3f;

    [Header("Preservation of connectivity parameters")]
    [SerializeField]
    [Range(0.01f, 2.0f)]
    private float distanceBetweenAgents = 0.6f;

    public SwarmParameters GetParameters()
    {
        SwarmParameters parameters = new SwarmParameters(agentBehaviour,
                                                        agentMovement,
                                                        mapSizeX,
                                                        mapSizeZ,
                                                        fieldOfViewSize,
                                                        blindSpotSize,
                                                        maxSpeed,
                                                        moveForwardIntensity,
                                                        randomMovementIntensity,
                                                        frictionIntensity,
                                                        avoidCollisionWithNeighboursIntensity,
                                                        cohesionIntensity,
                                                        alignmentIntensity,
                                                        separationIntensity,
                                                        attractionZoneSize,
                                                        alignmentZoneSize,
                                                        repulsionZoneSize,
                                                        distanceBetweenAgents);
        return parameters;
    }

    public void SetParameters(SwarmParameters parameters)
    {
        this.agentBehaviour = parameters.GetAgentBehaviour();
        this.agentMovement = parameters.GetAgentMovement();
        this.mapSizeX = parameters.GetMapSizeX();
        this.mapSizeZ = parameters.GetMapSizeZ();
        this.fieldOfViewSize = parameters.GetFieldOfViewSize();
        this.blindSpotSize = parameters.GetBlindSpotSize();
        this.maxSpeed = parameters.GetMaxSpeed();
        this.moveForwardIntensity = parameters.GetMoveForwardIntensity();
        this.randomMovementIntensity = parameters.GetRandomMovementIntensity();
        this.frictionIntensity = parameters.GetFrictionIntensity();
        this.avoidCollisionWithNeighboursIntensity = parameters.GetAvoidCollisionWithNeighboursIntensity();
        this.cohesionIntensity = parameters.GetCohesionIntensity();
        this.alignmentIntensity = parameters.GetAlignmentIntensity();
        this.separationIntensity = parameters.GetSeparationIntensity();
        this.attractionZoneSize = parameters.GetAttractionZoneSize();
        this.alignmentZoneSize = parameters.GetAlignmentZoneSize();
        this.repulsionZoneSize = parameters.GetRepulsionZoneSize();
        this.distanceBetweenAgents = parameters.GetDistanceBetweenAgents();
    }


    #region Methods - Setter
    public void SetCohesionIntensity(float intensity)
    {
        this.cohesionIntensity = intensity;
    }

    public void SetAlignmentIntensity(float intensity)
    {
        this.alignmentIntensity = intensity;
    }

    public void SetSeparatonIntensity(float intensity)
    {
        this.separationIntensity = intensity;
    }

    public void SetFieldOfViewSize(float intensity)
    {
        this.fieldOfViewSize = intensity;
    }

    public void SetBlindSpotSize(float intensity)
    {
        this.blindSpotSize = intensity;
    }

    public void SetRandomMovementIntensity(float intensity)
    {
        this.randomMovementIntensity = intensity;
    }

    public void SetMaxSpeed(float intensity)
    {
        this.maxSpeed = intensity;
    }


    public void SetFrictionIntensity(float intensity) { 
        this.frictionIntensity = intensity;
    }


    public void SetAgentMovement(MovementManager.AgentMovement type)
    {
        this.agentMovement = type;
    }
    #endregion

    #region Methods - Getter
    //Setter pour l'attribut CohesionIntensity
    public float GetCohesionIntensity() {  return this.cohesionIntensity; }

    public float GetAlignmentIntensity() { return this.alignmentIntensity; }

    public float GetSeparationIntensity() { return this.separationIntensity; }

    public float GetFieldOfViewSize() { return this.fieldOfViewSize; }

    public float GetBlindSpotSize() { return this.blindSpotSize; }

    public float GetRandomMovementIntensity() { return this.randomMovementIntensity; }

    public float GetMaxSpeed() { return this.maxSpeed; }

    public float GetFrictionIntensity() { return this.frictionIntensity; }

    public MovementManager.AgentMovement GetAgentMovenent() { return this.agentMovement; }
    #endregion
}
