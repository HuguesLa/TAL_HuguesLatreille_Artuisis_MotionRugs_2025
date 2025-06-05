using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwarmManager : MonoBehaviour
{
    #region Serialized fields
    [SerializeField]
    private bool generateMap = true;
    [SerializeField]
    private GameObject mapPrefab;

    [SerializeField]
    private float numberOfAgents;

    [SerializeField]
    EditorParametersInterface parametersInterface;

    [SerializeField]
    [Tooltip("The transform containing all the \"Displayer\" components.")]
    private Transform displayers;

    [SerializeField]
    private List<Displayer> usedDisplayers;
    #endregion

    private SwarmData swarm;

    private GameObject map;

    private Displayer[] existingDisplayers;

    // Start is called before the first frame update
    void Start()
    {
        if(generateMap)
        {
            //Instantiation of the map
            map = Instantiate(mapPrefab);
            map.transform.parent = null;
        }


        existingDisplayers = displayers.GetComponentsInChildren<Displayer>();

        if (parametersInterface == null) {
            Debug.LogError("ParameterManager is missing in the scene", this);
        }


        FrameTransmitter frameTransmitter = FindAnyObjectByType<FrameTransmitter>();

        if (frameTransmitter != null)
        {
            SwarmData frame = frameTransmitter.GetFrameAndDestroy();
            swarm = frame.Clone();
            parametersInterface.SetParameters(swarm.GetParameters());
        }
        else
        {
            SwarmParameters parameters = parametersInterface.GetParameters();

            SerializableRandom random = new SerializableRandom();

            List<AgentData> agents = new List<AgentData>();
            for (int i = 0; i < numberOfAgents; i++)
            {

                Vector3 position = new Vector3((float)random.Rand(0, (double)parameters.GetMapSizeX()), 0.0f, (float)random.Rand(0, (double)parameters.GetMapSizeZ()));

                float angle = (float) random.Rand(-MathF.PI, MathF.PI);
                Vector3 direction = new Vector3(Mathf.Cos(angle), 0.0f, Mathf.Sin(angle)).normalized;


                AgentData agent = new AgentData(position, direction);
                agents.Add(agent);
            }
            swarm = new SwarmData(agents, parameters, random);
        } 
    }


    public void ResetSwarm()
    {
        //The garbage collector should handle the deletion (if not, add it)

        SwarmParameters parameters = parametersInterface.GetParameters();

        SerializableRandom random = new SerializableRandom();

        List<AgentData> agents = new List<AgentData>();
        for (int i = 0; i < numberOfAgents; i++)
        {

            Vector3 position = new Vector3((float)random.Rand(0, (double)parameters.GetMapSizeX()), 0.0f, (float)random.Rand(0, (double)parameters.GetMapSizeZ()));

            float angle = (float)random.Rand(-MathF.PI, MathF.PI);
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0.0f, Mathf.Sin(angle)).normalized;


            AgentData agent = new AgentData(position, direction);
            agents.Add(agent);
        }
        swarm = new SwarmData(agents, parameters, random);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Update swarm parameters
        SwarmParameters parameters = parametersInterface.GetParameters();
        swarm.SetParameters(parameters);

        List<AgentData> agents = swarm.GetAgentsData();

        //Update Position, direction
        foreach (AgentData a in agents)
        {
            Tuple<Vector3, Vector3> positionAndDirection = MovementManager.ApplyAgentMovement(a, parameters, Time.deltaTime);

            Vector3 position = CorrectPosition(positionAndDirection.Item1, 0.04f);
            //Vector3 position = positionAndDirection.Item1;

            a.SetPosition(position);
            a.SetDirection(positionAndDirection.Item2);
        }

        //Reset forces and apply agent's behaviour
        foreach (AgentData a in agents)
        {
            //Clear forces
            a.ClearForces();

            //Get new agent's forces
            List<Vector3> forces = BehaviourManager.ApplySocialBehaviour(parameters.GetAgentBehaviour(), a, swarm);
            a.SetForces(forces);
        }

        foreach (AgentData a in agents)
        {
            a.UdpateAcceleration();
            Vector3 speed = a.UpdateSpeed(Time.deltaTime);

            //Limit speed vector based on agent max speed
            float maxSpeed = parameters.GetMaxSpeed();
            float temp = speed.sqrMagnitude; //faster than Vector3.Magnitude(this.speed);
            if (temp > (maxSpeed * maxSpeed)) // Temp is squared, so it's necessary to compare whith "maxSpeed" squared too
            {
                speed.Normalize();
                speed *= maxSpeed;
                a.SetSpeed(speed);
            }
        }


        //--Display--//
        foreach (Displayer d in existingDisplayers)
        {
            if(!usedDisplayers.Contains(d))
                d.ClearVisual();
        }

        foreach (Displayer d in usedDisplayers)
        {
            if(d!=null) d.DisplayVisual(swarm);
        }

        if(map!=null) UpdateMap();

        //Update camera position
        float x=parameters.GetMapSizeX();
        float z=parameters.GetMapSizeZ();
        float max = Mathf.Max(x, z);
        Camera.main.transform.position = new Vector3(x / 2.0f, max, z / 2.0f);
    }


    /// <summary>
    /// Load another scene (the clip player one)
    /// </summary>
    public void LoadClipPlayer()
    {
        SceneManager.LoadScene("ClipEditorScene");
    }


    public void AddUsedDisplayer(Displayer disp)
    {
        if (!usedDisplayers.Contains(disp))
            usedDisplayers.Add(disp);
    }

    public void RemoveUsedDisplayer(Displayer disp)
    {
        if (usedDisplayers.Contains(disp))
            usedDisplayers.Remove(disp);
    }

    #region Methods - Map
    private void UpdateMap()
    {
        float x = swarm.GetParameters().GetMapSizeX();
        float z = swarm.GetParameters().GetMapSizeZ();
        map.transform.position = new Vector3(x / 2.0f, 0.0f, z / 2.0f);
        map.transform.localScale = new Vector3(x, 1.0f, z);
    }

    public Vector3 CorrectPosition(Vector3 position, float objectRadius)
    {
        float mapSizeX = swarm.GetParameters().GetMapSizeX();
        float mapSizeZ = swarm.GetParameters().GetMapSizeZ();
  

        float x = position.x;
        float z = position.z;

        if (position.x > mapSizeX - objectRadius)
        {
            x = mapSizeX - objectRadius;
        }
        if (position.x < objectRadius)
        {
            x = objectRadius;
        }

        if (position.z > mapSizeZ - objectRadius)
        {
            z = mapSizeZ - objectRadius;
        }
        if (position.z < objectRadius)
        {
            z = objectRadius;
        }
        Vector3 newPosition = new Vector3(x, 0.0f, z);

        return newPosition;
    }
    #endregion

    #region Methods - Getter
    public SwarmData CloneFrame()
    {
        return swarm.Clone();
    }

    public SwarmData GetSwarmData()
    {
        return swarm;
    }
    #endregion


    public void QuitApp()
    {
        Application.Quit();
    }
}
