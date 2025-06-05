using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayerDominantForce : Displayer
{
    #region Serialized fields
    [SerializeField]
    private bool showDominantForceDirection = false;

    [SerializeField]
    private Color defaultColor = Color.white;

    [SerializeField]
    private List<BehaviourManager.ForceType> analysedForces;

    [SerializeField]
    private List<Color> correspondingColors;

    [SerializeField]
    private GameObject picto;

    [SerializeField]
    private float pictoHeight = 0.1f;
    #endregion

    #region Private fields
    private List<GameObject> pictos;

    private Gradient [] gradients;
    #endregion

    #region Methods - Monobehaviour callbacks
    // Start is called before the first frame update
    void Start()
    {
        pictos = new List<GameObject>();

        UpdateGradients();
    }

    private void Update()
    {
        UpdateGradients();
    }

    #endregion


    #region Methods - Displayer override
    public override void DisplayVisual(SwarmData swarmData)
    {
        ClearVisual();

        List<AgentData> agents = swarmData.GetAgentsData();
        BehaviourManager.AgentBehaviour agentBehaviour = swarmData.GetParameters().GetAgentBehaviour();
        foreach (AgentData a in agents)
        {

            //Preparation for obtaining the forces influencing the agent
            List<Tuple<BehaviourManager.ForceType, Vector3>> forces = BehaviourManager.GetForces(a, agentBehaviour);

            List<List<Vector3>> listForces = new List<List<Vector3>>();

            for (int i = 0; i < analysedForces.Count+1; i++)
            {
                listForces.Add(new List<Vector3>());
            }

            //Forces sorted according to the forces analysed
            foreach (Tuple<BehaviourManager.ForceType, Vector3> t in forces)
            {
                bool other = true;
                for(int i=0;i<analysedForces.Count;i++)
                {
                    if(analysedForces[i] == t.Item1)
                    {
                        other = false;
                        listForces[i].Add(t.Item2);
                    }
                }
                if (other) listForces[listForces.Count - 1].Add(t.Item2);
            }


            //Calculation of intensities and summed forces
            float[] forceIntensities = new float[listForces.Count];
            Vector3[] summedForces = new Vector3[listForces.Count];

            for(int i = 0;i<listForces.Count; i++)
            {
                summedForces[i] = Vector3.zero;
                foreach (Vector3 v in listForces[i])
                {
                    summedForces[i] += v;
                }
                forceIntensities[i] = summedForces[i].magnitude;
            }

            GameObject g;
            g = Instantiate(picto);

            g.GetComponentInChildren<Renderer>().material.color = defaultColor;

            Vector3 dir = a.GetAcceleration();

            //Find the dominant force and change the colour of the arrow accordingly
            for (int i = 0; i< analysedForces.Count; i++)
            {
                bool best = true;
                for(int j =0; j < forceIntensities.Length; j++)
                {
                    if (i == j) continue;
                    if (forceIntensities[i] < forceIntensities[j]) best = false;
                }

                if(best)
                {
                    float angle = Vector3.Angle(summedForces[i], a.GetAcceleration());

                    if (angle < 90)
                    {
                        g.GetComponentInChildren<Renderer>().material.color = gradients[i].Evaluate((90 - angle) / 90);
                        if (showDominantForceDirection) dir = summedForces[i];
                    }
                    else
                    {
                        g.GetComponentInChildren<Renderer>().material.color = defaultColor;
                    }

                    break;
                }
            }



            g.transform.parent = this.transform;

            g.transform.localPosition = a.GetPosition() + new Vector3(0.0f, pictoHeight, 0.0f);

            float agentDirection_YAxis = 180 - (Mathf.Acos(dir.normalized.x) * 180.0f / Mathf.PI);
            if (dir.z < 0.0f) agentDirection_YAxis = agentDirection_YAxis * -1;
            g.transform.localRotation = Quaternion.Euler(0.0f, agentDirection_YAxis, 0.0f);

            pictos.Add(g);

        }


    }
    public override void ClearVisual()
    {
        foreach (GameObject g in pictos)
        {
            Destroy(g);
        }
        pictos.Clear();
    }
    #endregion

    private void UpdateGradients()
    {
        if (correspondingColors.Count != analysedForces.Count) Debug.LogError("Il n'y a pas le m�me nombre de forces que de couleurs");

        // Blend alpha from opaque at 0% to transparent at 100%
        var alphas = new GradientAlphaKey[2];
        alphas[0] = new GradientAlphaKey(1.0f, 0.0f);
        alphas[1] = new GradientAlphaKey(1.0f, 1.0f);


        gradients = new Gradient[analysedForces.Count];
        for (int i = 0; i < analysedForces.Count; i++)
        {
            gradients[i] = new Gradient();

            var colors = new GradientColorKey[2];
            colors[0] = new GradientColorKey(defaultColor, 0.0f);
            colors[1] = new GradientColorKey(correspondingColors[i], 1.0f);
            gradients[i].SetKeys(colors, alphas);
        }
    }
}
