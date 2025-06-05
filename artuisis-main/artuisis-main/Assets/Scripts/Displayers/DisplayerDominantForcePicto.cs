using System;
using System.Collections.Generic;
using UnityEngine;

public class DisplayerDominantForcePicto : Displayer
{
    #region Serialized fields
    [SerializeField]
    private bool lowNeighboursOnly = true;

    [SerializeField]
    [Range(1,5)]
    private int neighboursCount = 2;

    [SerializeField]
    private GameObject pictoAttraction;

    [SerializeField]
    private GameObject pictoRepulsion;    
    
    [SerializeField]
    private GameObject pictoAlignment;

    [SerializeField]
    private GameObject pictoIsolated;

    [SerializeField]
    [Range(0.0f,0.5f)]
    private float toleranceThreshold = 0.0f;

    [SerializeField]
    [Range(0,180)]
    private int angleThreshold = 0;

    [SerializeField]
    private float pictoHeight = 0.1f;
    #endregion

    #region Private fields
    private List<GameObject> pictos = new List<GameObject>();
    #endregion

    #region Methods - Monobehaviour callbacks
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #endregion


    #region Methods - Displayer override
    public override void DisplayVisual(SwarmData swarmData)
    {
        ClearVisual();

        List<AgentData> agents =  swarmData.GetAgentsData();
        BehaviourManager.AgentBehaviour agentBehaviour = swarmData.GetParameters().GetAgentBehaviour();
        foreach (AgentData a in agents)
        {

            if (lowNeighboursOnly)
            {
                int count = SwarmTools.GetNeighbours(a, swarmData.GetAgentsData(), swarmData.GetParameters().GetFieldOfViewSize(), swarmData.GetParameters().GetBlindSpotSize()).Count;
                if (count > neighboursCount) continue;
            }


            List<Tuple<BehaviourManager.ForceType, Vector3>> forces = BehaviourManager.GetForces(a, agentBehaviour);

            List<Vector3> repulsion = new List<Vector3>();
            List<Vector3> attraction = new List<Vector3>();
            List<Vector3> alignment = new List<Vector3>();

            foreach (Tuple<BehaviourManager.ForceType, Vector3> t in forces)
            {
                switch (t.Item1)
                {
                    case BehaviourManager.ForceType.Attraction:
                        attraction.Add(t.Item2);
                        break;
                    case BehaviourManager.ForceType.Repulsion:
                        repulsion.Add(t.Item2);
                        break;
                    case BehaviourManager.ForceType.Alignment:
                        alignment.Add(t.Item2);
                        break;
                }
            }

            float repIntensity = 0.0f;
            Vector3 rep = Vector3.zero;
            float attIntensity = 0.0f;
            Vector3 att = Vector3.zero;
            float aliIntensity = 0.0f;
            Vector3 ali = Vector3.zero;

            Vector3 totalForce = Vector3.zero;
            foreach (Vector3 v in repulsion)
            {
                if (v.magnitude > repIntensity)
                {
                    repIntensity = v.magnitude;
                    rep = v;
                }
                totalForce += v;
            }

            foreach (Vector3 v in attraction)
            {
                if (v.magnitude > attIntensity)
                {
                    attIntensity = v.magnitude;
                    att = v;
                }
                totalForce += v;
            }

            foreach (Vector3 v in alignment)
            {
                if (v.magnitude > aliIntensity)
                {
                    aliIntensity = v.magnitude;
                    ali = v;
                }
                totalForce += v;
            }
            GameObject g;
            Vector3 dir;

            if (repIntensity == 0.0f && attIntensity == 0.0f && aliIntensity == 0.0f)
            {
                g = Instantiate(pictoIsolated);
                dir = a.GetSpeed();


                g.transform.parent = this.transform;

                g.transform.localPosition = a.GetPosition() + new Vector3(0.0f, pictoHeight, 0.0f);

                float agentDirection_YAxis = 180 - (Mathf.Acos(dir.normalized.x) * 180.0f / Mathf.PI);
                if (dir.z < 0.0f) agentDirection_YAxis = agentDirection_YAxis * -1;
                g.transform.localRotation = Quaternion.Euler(0.0f, agentDirection_YAxis, 0.0f);

                pictos.Add(g);
            }
            else
            {



                Vector3 potentialSpeed = a.GetSpeed() + totalForce * Time.fixedDeltaTime;

                float angle = Vector3.Angle(potentialSpeed, a.GetSpeed()) * 180.0f / Mathf.PI;


                if (angle > angleThreshold)
                {
                    float ratio = 1.0f;
                    List<float> temp = new List<float>();
                    temp.Add(repIntensity);
                    temp.Add(attIntensity);
                    temp.Add(aliIntensity);

                    temp.Sort(new ListTools.GFG());

                    ratio = temp[0] / temp[1];


                    if (ratio < 1.0f + toleranceThreshold && ratio > 1.0f - toleranceThreshold) continue;




                    //if (aliIntensity > repIntensity && aliIntensity > attIntensity)

                    if (aliIntensity > repIntensity && aliIntensity > attIntensity)
                    {
                        g = Instantiate(pictoAlignment);
                        dir = ali;
                    }
                    else
                    {
                        if (repIntensity > attIntensity)
                        {
                            g = Instantiate(pictoRepulsion);
                            dir = rep;
                        }
                        else
                        {
                            g = Instantiate(pictoAttraction);
                            dir = att;
                        }
                    }

                    //dir = a.GetAcceleration();




                    g.transform.parent = this.transform;

                    g.transform.localPosition = a.GetPosition() + new Vector3(0.0f, pictoHeight, 0.0f);

                    float agentDirection_YAxis = 180 - (Mathf.Acos(dir.normalized.x) * 180.0f / Mathf.PI);
                    if (dir.z < 0.0f) agentDirection_YAxis = agentDirection_YAxis * -1;
                    g.transform.localRotation = Quaternion.Euler(0.0f, agentDirection_YAxis, 0.0f);

                    pictos.Add(g);
                }
            }
        }


    }
    public override void ClearVisual()
    {
        foreach(GameObject g in pictos)
        {
            Destroy(g);
        }
        pictos.Clear();
    }
    #endregion
}
