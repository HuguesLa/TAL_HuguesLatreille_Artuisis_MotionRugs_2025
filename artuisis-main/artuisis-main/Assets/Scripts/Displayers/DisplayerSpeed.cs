using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayerSpeed : Displayer
{
    [SerializeField]
    private GameObject prefab;

    [SerializeField]
    private float arrowHeight = 0.1f;

    #region Private fields
    private List<GameObject> arrows;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        arrows = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Methods - Displayer override
    public override void DisplayVisual(SwarmData swarmData)
    {
        ClearVisual();

        List<AgentData> agents = swarmData.GetAgentsData();

        foreach (AgentData a in agents)
        {
            
            Vector3 dir = a.GetSpeed();

            GameObject g = Instantiate(prefab);

            g.transform.parent = this.transform;

            g.transform.localPosition = a.GetPosition() + new Vector3(0.0f, arrowHeight, 0.0f);

            float agentDirection_YAxis = 180 - (Mathf.Acos(dir.normalized.x) * 180.0f / Mathf.PI);
            if (dir.z < 0.0f) agentDirection_YAxis = agentDirection_YAxis * -1;
            g.transform.localRotation = Quaternion.Euler(0.0f, agentDirection_YAxis, 0.0f);

            arrows.Add(g);
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
