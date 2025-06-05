using System.Collections.Generic;
using UnityEngine;

public class DisplayerRelativeMovementAlt : Displayer
{
    #region Serialized fields
    [SerializeField]
    private GameObject prefab;

    [SerializeField]
    private float arrowHeight = 0.1f;

    [SerializeField]
    [Range(0.0f,180.0f)]
    private float sensitivity = 60.0f;
    #endregion

    #region Private fields
    private List<GameObject> arrows;

    private Gradient gradient;
    #endregion

    #region Methods - MonoBehaviour callbacks
    // Start is called before the first frame update
    void Start()
    {
        arrows = new List<GameObject>();

        // Blend alpha from opaque at 0% to transparent at 100%
        var alphas = new GradientAlphaKey[2];
        alphas[0] = new GradientAlphaKey(1.0f, 0.0f);
        alphas[1] = new GradientAlphaKey(1.0f, 1.0f);



        gradient = new Gradient();

        var colors = new GradientColorKey[2];
        colors[0] = new GradientColorKey(Color.white, 0.0f);
        colors[1] = new GradientColorKey(Color.magenta, 1.0f);
        gradient.SetKeys(colors, alphas);

    }
    #endregion

    #region Methods - Displayer Override
    public override void DisplayVisual(SwarmData swarmData)
    {
        ClearVisual();

        List<List<AgentData>> clusters =  SwarmTools.GetClusters(swarmData);

        foreach (List<AgentData> c in clusters)
        {
            Vector3 meanSwarmSpeed = Vector3.zero;
            foreach (AgentData a in c)
            {
                meanSwarmSpeed += a.GetSpeed();
            }
            meanSwarmSpeed = meanSwarmSpeed / c.Count;

            foreach (AgentData a in c)
            {
                Vector3 dir = a.GetSpeed() - meanSwarmSpeed;

                float angle = Vector3.Angle(a.GetSpeed(), meanSwarmSpeed);

                float ratio = (angle) / sensitivity;
                if (ratio > 1.0f) ratio = 1.0f; 

                GameObject g = Instantiate(prefab);

                g.GetComponentInChildren<Renderer>().material.color = gradient.Evaluate(ratio);

                g.transform.parent = this.transform;

                g.transform.localPosition = a.GetPosition() + new Vector3(0.0f, arrowHeight, 0.0f);

                float agentDirection_YAxis = 180 - (Mathf.Acos(dir.normalized.x) * 180.0f / Mathf.PI);
                if (dir.z < 0.0f) agentDirection_YAxis = agentDirection_YAxis * -1;
                g.transform.localRotation = Quaternion.Euler(0.0f, agentDirection_YAxis, 0.0f);

                arrows.Add(g);
            }
        }
    }

    public override void ClearVisual()
    {
        foreach (GameObject g in arrows)
        {
            Destroy(g);
        }
        arrows.Clear();
    }
    #endregion
}
