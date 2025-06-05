using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayerSprings : Displayer
{

    public float width=1.0f;

    public int nbBoucles = 5;

    public int nbVertices = 16;

    public float wireWidth = 0.02f;

    [SerializeField]
    private Material material;

    private Mesh mesh;

    #region Methods - Monobehaviour callbacks
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

        //GetLinks within the swarm
        List<Tuple<AgentData, AgentData>> links = SwarmTools.GetLinksList(swarmData);

        //Get the size of the field of view of the swarm's agent
        float fovSize = swarmData.GetParameters().GetFieldOfViewSize();

        //Initialise lists of vertices and triangles for the mesh
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();


        foreach (Tuple<AgentData, AgentData> l in links)
        {
            //Get the vertices of the line
            Tuple<List<Vector3>, List<int>> m = MeshTools.GetSpringMesh(l.Item1.GetPosition(), l.Item2.GetPosition(), nbBoucles, nbVertices, width, wireWidth);

            List<Vector3> v = m.Item1;
            List<int> t = m.Item2;

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
