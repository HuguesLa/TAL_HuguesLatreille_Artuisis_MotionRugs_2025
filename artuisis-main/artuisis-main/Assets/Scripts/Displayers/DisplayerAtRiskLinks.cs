using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DisplayerAtRiskLinks : Displayer
{
    #region Serialized fields

    [SerializeField]
    [Range(1, 39)]
    private int nbLinks = 3; //Number of links expected

    [SerializeField]
    [Range(0.01f, 0.08f)]
    private float width = 0.03f;

    [SerializeField]
    private Material material;
    #endregion

    #region Private fields
    private Mesh mesh;
    #endregion

    #region Methods - MonoBehaviour callbacks
    // Start is called before the first frame update
    void Start()
    {
        //Create a mesh filter (need an upgrade in case of already axisting meshfilter)
        MeshFilter meshFilter = this.gameObject.AddComponent<MeshFilter>();
        this.mesh = new Mesh();
        meshFilter.mesh = mesh;

        //Create a mesh renderer (need an upgrade in case of already axisting meshrenderer)
        MeshRenderer meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
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

        List<Tuple<AgentData, AgentData>> res = new List<Tuple<AgentData, AgentData>>();

        //Get the clusters in the swarm
        List<List<AgentData>> clusters = SwarmTools.GetOrderedClusters(swarmData);

        foreach (List<AgentData> cluster in clusters)
        {
            if (cluster.Count < 2) continue;

            List<Tuple<AgentData, AgentData>> links = SwarmTools.GetLinksList(cluster, swarmData.GetParameters().GetFieldOfViewSize(), swarmData.GetParameters().GetBlindSpotSize());

            List<List<AgentData>> groups = new List<List<AgentData>>();

            foreach (AgentData a in cluster)
            {
                List<AgentData> group = new List<AgentData>();
                group.Add(a);
                groups.Add(group);
            }



            while (groups.Count > 1)
            {
                //Get the closest duo in the links list
                Tuple<AgentData, AgentData> closestDuo = null;
                float minDist = float.MaxValue;
                foreach (Tuple<AgentData, AgentData> t in links)
                {
                    float dist = Vector3.Distance(t.Item1.GetPosition(), t.Item2.GetPosition());
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestDuo = t;
                    }
                }

                if (groups.Count <= (nbLinks + 1)) res.Add(closestDuo);

                //Merge the closest duo
                List<AgentData> group1 = null;
                List<AgentData> group2 = null;
                foreach (List<AgentData> g in groups)
                {
                    if (g.Contains(closestDuo.Item1)) group1 = g;
                    if (g.Contains(closestDuo.Item2)) group2 = g;
                }

                if (!System.Object.ReferenceEquals(group1, group2))
                {
                    group1.AddRange(group2);
                    groups.Remove(group2);
                }

                //Remove all the links inter group
                List<Tuple<AgentData, AgentData>> linkToRemove = new List<Tuple<AgentData, AgentData>>();
                foreach (Tuple<AgentData, AgentData> t in links)
                {
                    foreach (List<AgentData> g in groups)
                    {
                        if (g.Contains(t.Item1) && g.Contains(t.Item2))
                        {
                            linkToRemove.Add(t);
                            break;
                        }
                    }
                }

                foreach (Tuple<AgentData, AgentData> t in linkToRemove)
                {
                    links.Remove(t);
                }
            }
        }
        //Display the remaining links
        //Initialise lists of vertices and triangles for the mesh
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        foreach (Tuple<AgentData, AgentData> l in res)
        {
            //Get the vertices of the line
            List<Vector3> v = MeshTools.TranformLineToRectanglePoints(l.Item1.GetPosition(), l.Item2.GetPosition(), width);
            //Get the triangles from the vertices of the line
            List<int> t = MeshTools.DrawFilledTriangles(v.ToArray());

            //Update triangles indexes before adding them to the triangles list
            for (int k = 0; k < t.Count; k++)
            {
                t[k] += vertices.Count;
            }

            //Updathe vertices and triangles list with the new line
            vertices.AddRange(v);
            triangles.AddRange(t);
        }

        Vector2[] uv = new Vector2[vertices.Count];

        mesh.vertices = vertices.ToArray();
        mesh.uv = uv;
        mesh.triangles = triangles.ToArray();


    }

    public override void ClearVisual()
    {
        mesh.Clear();
    }
    #endregion
}
