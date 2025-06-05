using System;
using System.Collections.Generic;
using UnityEngine;

public class DisplayerLinks : Displayer
{
    #region Serialized fields
    [SerializeField]
    private bool visu3D = false;
    [SerializeField]
    private float minWidth = 0.001f;
    [SerializeField]
    private float maxWidth = 0.05f;

    [SerializeField]
    private float minHeight = 0.001f;
    [SerializeField]
    private float maxHeight = 0.025f;

    [SerializeField]
    private Material material;
    #endregion

    #region Private fields
    private Mesh mesh;
    #endregion

    #region Methods - MonoBehaviour callbacks
    private void Start()
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

    private void Update()
    {
        if (minWidth > maxWidth) minWidth = maxWidth;
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


        foreach (Tuple<AgentData,AgentData> l in links)
        {
            if (!visu3D)
            {
                //Calculate the rapport between the current link size and the field of view size
                float distOnMaxDistance = Vector3.Distance(l.Item1.GetPosition(), l.Item2.GetPosition()) / fovSize;

                //Calculate the width of the displayed link based on the rapport between the link size and fov
                float width = maxWidth - (maxWidth - minWidth) * distOnMaxDistance;
                
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
            else 
            {
                //Calculate the rapport between the current link size and the field of view size
                float distOnMaxDistance = Vector3.Distance(l.Item1.GetPosition(), l.Item2.GetPosition()) / fovSize;

                //Calculate the width of the displayed link based on the rapport between the link size and fov
                float width = maxWidth - (maxWidth - minWidth) * distOnMaxDistance;

                //Calculate the height of the displayed link based on the rapport between the link size and fov
                float height = maxHeight - (maxHeight - minHeight) * distOnMaxDistance;


                //Get the vertices of the line
                Tuple<List<Vector3>,List<int>> m = MeshTools.TranformLineToCuboidPoints(l.Item1.GetPosition(), l.Item2.GetPosition(), width,height);
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
