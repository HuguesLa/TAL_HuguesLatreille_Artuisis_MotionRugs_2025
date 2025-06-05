using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    #region Serialized fields - Perception parameters
    [Header("Field of view size")]
    [SerializeField]
    [Range(0.0f, 5.0f)]
    [Tooltip("This is the size of the agent perception radius (in meters).")]
    protected float fieldOfViewSize = 1.0f; //The range of perception of this agent in meters
    [SerializeField]
    [Range(0, 360)]
    [Tooltip("This is the size of blind spot of the agent (starting from the back) (in degrees).")]
    protected float blindSpotSize = 30; //The size of the blind spot of this agent (starting from the back) in Degrees
    #endregion

    #region Serialized fields - Intensity parameters
    [Header("Intensity parameters")]
    [SerializeField]
    [Range(0.0f, 30.0f)]
    protected float moveForwardIntensity = 1.0f;
    [SerializeField]
    [Range(0.0f, 30.0f)]
    protected float randomMovementIntensity = 20.0f;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    protected float frictionIntensity = 0.1f;
    [SerializeField]
    [Range(0.0f, 2.0f)]
    protected float maxSpeed = 1.0f;
    #endregion

    #region Private fields
    //Field of view
    //private GameObject fieldOfView;
    //private SphereCollider fieldOfViewCollider;

    //Feeler
    //private GameObject feeler;
    //private SphereCollider feelerCollider;

    //private List<GameObject> detectedObstacles;

    protected Vector3 acceleration = Vector3.zero; //the current acceleration of this agent, in m/s^2
    protected Vector3 speed = Vector3.zero; //The current speed of this agent, in m/s
    protected List<Vector3> forces = new List<Vector3>(); 

    protected List<GameObject> detectedAgents;


    protected float mapSizeX = 5.0f;
    protected float mapSizeZ = 5.0f;

    protected Vector3 savedPosition;

    protected PatternCreator forceField;



    #endregion

    #region Methods - Agent's primary methods

    /// <summary>
    /// This method update the current position and the current speed of an agent.
    /// The new position is based on the current speed and the  current position (position(t+1) = position(t) + speed(t) * DT).
    /// The new speed is based on the current acceleration and the current speed (speed(t+1) = speed(t) + acceleration(t+1) * DT).
    /// To avoid having speed reaching unlimited intensity, there is a speed max value lowering the speed intensity when it exceeds speed max value.
    /// The acceleration is then reset to zero.
    /// Moreover, this methods update the direction of the visual robot to match its current speed.
    /// </summary>
    protected void updateAgent()
    {
        //Update agent position based on speed and time passed since last change
        this.transform.position += this.speed * Time.deltaTime;
        this.transform.position = new Vector3(this.transform.position.x, 0.001f, this.transform.position.z);

        //Update agent visual direction to match the current movement
       // float agentDirection_YAxis = 180-(Mathf.Acos(this.speed.normalized.x) * 180.0f / Mathf.PI);
       // if (this.speed.z < 0.0f) agentDirection_YAxis = agentDirection_YAxis * -1;
       // this.transform.rotation = Quaternion.Euler(0.0f, agentDirection_YAxis, 0.0f);

        //Loop position if it go further than map size limit
        //StayInInfiniteArea();

        //Stay in the finite area if it go further than map size limit
        StayInFiniteArea(); //Called before updating speed and reseting acceleration

        //Update agent speed based on acceleration and time passed since last change
        this.speed += this.acceleration * Time.deltaTime;

        //Reset acceleration
        this.acceleration = Vector3.zero;

        //Reset forces list
        this.forces.Clear();

        //Limit speed vector based on agent max speed
        float temp = this.speed.sqrMagnitude; //faster than Vector3.Magnitude(this.speed);
        if (temp > (maxSpeed * maxSpeed)) // Temp is squared, so it's necessary to compare whith "maxSpeed" squared too
        {
            this.speed.Normalize();
            this.speed *= this.maxSpeed;
        }
    }

    /// <summary>
    /// This method add a force to the agent acceleration.
    /// </summary>
    protected void addForce(Vector3 force)
    {
        this.acceleration += force; //  *(1.0f/this.mass);
    }

    /// <summary>
    /// This method get the other agents perceived by the current agent, and add them to the "detectedAgents" list.
    /// The perceived agent depend of the field of view size and the blind spot angle.
    /// Agents in the blind spot are not perceived.
    /// Agents further the field of view size are not perceived.
    /// </summary>
    protected virtual void getAgentsInFieldOfView()
    {

        List<GameObject> agents = new List<GameObject>(); //FALSE
        detectedAgents = new List<GameObject>();

        foreach (GameObject g in agents)
        {
            if (GameObject.ReferenceEquals(g, this.gameObject)) continue;
            if (Vector3.Distance(g.transform.position, this.transform.position) <= fieldOfViewSize)
            {
                Vector3 dir = g.transform.position - this.transform.position;
                float angle = Vector3.Angle(this.speed, dir);


                if (angle <= (180 - (blindSpotSize / 2)))
                {
                    detectedAgents.Add(g);
                }
            }
        }
    }
    #endregion

    #region Methods - Agent rules

    /// <summary> 
    /// This method create a random force, added to the acceleration of the agent.
    /// It aim to allow an agent to move randomly.
    /// The greater the intensity (<see cref="Agent.randomMovementIntensity"/>), the greater the force.
    /// </summary>
    protected Vector3 RandomMovement()
    {
        float alea = 0.1f;
        if (Random.value < alea)
        {
            float x = Random.value - 0.5f;
            float z = Random.value - 0.5f;
            Vector3 force = new Vector3(x, 0.0f, z);
            force.Normalize();

            //Modification de la puissance de cette force
            force *= randomMovementIntensity;

            addForce(force);
            return force;
        } else
        {
            return Vector3.zero;
        }
    }

    /// <summary> 
    /// This method create a force leading forward, added to the acceleration of the agent.
    /// It aim to allow an agent to move forward, depending of the move forward intensity.
    /// The greater the intensity (<see cref="Agent.moveForwardIntensity"/>), the greater the force.
    /// </summary>
    protected Vector3 MoveForward()
    {
        Vector3 force = speed.normalized;

        //Modification de la puissance de cette force
        force *= moveForwardIntensity;

        addForce(force);

        return force;
    }

    /// <summary>
    /// This method create a force opposite to the current speed, added to the acceleration of the agent.
    /// It aim to reduce the agent speed, depending of the friction intensity (<see cref="Agent.frictionIntensity"/>).
    /// </summary> 
    protected Vector3 Friction()
    {
        float k = frictionIntensity;
        Vector3 force = this.speed;
        force *= -k;
        addForce(force);
        return force;
    }

    /// <summary>
    /// This method create a force opposite to the neighbours that are too close, added to the acceleration of the agent.
    /// It aim to avoid collision between agents.
    /// </summary>
    protected Vector3 AvoidCollisionWithNeighbors()
    {
        Vector3 totalForce = Vector3.zero;
        float safetyDistance = 0.09f;
        foreach(GameObject g in detectedAgents)
        {
            if(Vector3.Distance(g.transform.position, this.transform.position)<= safetyDistance) {
                Vector3 force = this.transform.position - g.transform.position;
                force.Normalize();
                force *= 20*this.maxSpeed;
                totalForce += force;
            }
        }
        addForce(totalForce);
        return totalForce;
    }

    /// <summary> 
    /// This method create a force generated from the environmental forces of the map, added to the acceleration of the agent.
    /// It aim to influence the agent movement depending of the environmental constraints.
    /// </summary>
    protected void EnvironmentalForce()
    {
        Vector2 temp = new Vector2(transform.position.x, transform.position.z);
        temp = forceField.GetEnvironmentalForce(temp);
        Vector3 force = new Vector3(temp.x, 0.0f, temp.y);
        force *= 100;
        addForce(force);
    }
    #endregion

    #region Methods - Finite area
    /// <summary>
    /// This method prevents the agent to leave the borders of the map.
    /// And add a force to push back the agent in the area.
    /// </summary>
    protected void StayInFiniteArea()
    {
        float safetyDistance = 0.3f;
        float x = 0.0f;
        float z = 0.0f;
        Vector3 temp = this.transform.position;
        if (this.transform.position.x > mapSizeX-safetyDistance)
        {
            float dist = Mathf.Abs(mapSizeX - this.transform.position.x);
            //temp.x = mapSizeX;
            x = -1 * safetyDistance / dist;
        }
        if (this.transform.position.x < safetyDistance )
        {
            float dist = Mathf.Abs(this.transform.position.x);
            //temp.x = 0.0f;
            x = 1 * safetyDistance / dist;
        }

        if (this.transform.position.z > mapSizeZ - safetyDistance)
        {
            float dist = Mathf.Abs(mapSizeZ - this.transform.position.z);
            //temp.z = mapSizeZ;
            z = -1 * safetyDistance/dist;
        }
        if (this.transform.position.z < safetyDistance)
        {
            float dist = Mathf.Abs(this.transform.position.z);
            //temp.z = 0.0f;
            z = 1 * safetyDistance / dist;
        }
        Vector3 rebond = new Vector3(x, 0.0f, z);
        //rebond *= 50;
        addForce(rebond);
        this.transform.position = temp;
    }
    #endregion

    #region Methods - Infinite area

    /// <summary>
    /// This method return the nearest position of an agent if we considere the environment as infinite.
    /// The area is infinite in height and width.
    /// </summary>
    /// <returns> A <see cref="Vector3"/> value representing the nearest position in infinite area </returns>
    //A mettre à jour pour obtenir la position du "to", et ne pas normaliser
    protected Vector3 NearestPositionInInfiniteArea(Vector3 to)
    {
        Vector3 from = this.transform.position;

        float minX = Mathf.Abs(from.x - to.x);
        float minZ = Mathf.Abs(from.z - to.z);

        if (from.x > to.x)
        {
            float xTemp = to.x + mapSizeX;
            if (Mathf.Abs(from.x - xTemp) < minX) to.x += mapSizeX;
        }
        else
        {
            float xTemp = from.x + mapSizeX;
            if (Mathf.Abs(xTemp - from.x) < minX) to.x -= mapSizeX;
        }

        if (from.z > to.z)
        {
            float zTemp = to.z + mapSizeZ;
            if (Mathf.Abs(from.z - zTemp) < minZ) to.z += mapSizeZ;
        }
        else
        {
            float zTemp = from.z + mapSizeZ;
            if (Mathf.Abs(zTemp - from.z) < minZ) to.z -= mapSizeZ;
        }

        return to;
    }

    /// <summary>
    /// This method allow the agent to navigate in an infinite environment.
    /// When the agent goes beyond the borders of the map (in x or z), loops its position to the other border.
    /// </summary>
    protected void StayInInfiniteArea()
    {
        Vector3 temp = this.transform.position;
        if (this.transform.position.x > mapSizeX) temp.x -= mapSizeX;
        if (this.transform.position.x < 0.0f) temp.x += mapSizeX;

        if (this.transform.position.z > mapSizeZ) temp.z -= mapSizeZ;
        if (this.transform.position.z < 0.0f) temp.z += mapSizeZ;

        //same as
        //temp.x = (temp.x + mapSizeX) % mapSizeX
        //temp.z = (temp.z + mapSizeZ) % mapSizeZ

        this.transform.position = temp;
    }
    #endregion

    #region Methods - Getter

    public Vector3 GetSpeed()
    {
        return speed;
    }

    public Vector3 GetAcceleration()
    {
        return acceleration;
    }

    public List<Vector3> GetForces()
    {
        return this.forces;
    }

    public virtual float GetFieldOfViewSize()
    {
        return fieldOfViewSize;
    }

    public virtual float GetBlindSpotSize()
    {
        return blindSpotSize;
    }

    public List<GameObject> GetNeighbors()
    {
        if (detectedAgents == null) return new List<GameObject>();
        else return new List<GameObject>(detectedAgents);
    }

    public float GetMaxSpeed()
    {
        return maxSpeed;
    }

    public Vector3 GetSavedPosition()
    {
        return savedPosition;
    }

    public void SavePosition()
    {
        savedPosition = this.transform.position;
    }
    #endregion

    #region Methods - Setter
    public void SetSpeed(Vector3 newSpeed)
    {
        this.speed = newSpeed;
    }
    #endregion
}
