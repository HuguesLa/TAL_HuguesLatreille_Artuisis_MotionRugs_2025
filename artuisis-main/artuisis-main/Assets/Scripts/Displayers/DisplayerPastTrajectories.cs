using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayerPastTrajectories : Displayer
{
    #region Serialized fields
    [SerializeField]
    private int nbPositionsSaved = 20;

    [SerializeField]
    private int saveAvoid = 10;

    [SerializeField]
    [Range(0.005f,0.08f)]
    private float startLineWidth = 0.03f;

    [SerializeField]
    [Range(0.005f, 0.08f)]
    private float endLineWidth = 0.005f;

    [SerializeField]
    private Material material;

    [SerializeField]
    private Color color;
    #endregion

    #region Private fields
    private Mesh mesh;
    List<List<Vector3>> pastPositions;

    private int count = 0;
    #endregion

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


        pastPositions = new List<List<Vector3>>();

        count = saveAvoid + 1;
    }

    // Update is called once per frame
    void Update()
    {
        material.color = this.color;
    }

    #endregion

    #region Methods - Displayer override
    public override void DisplayVisual(SwarmData swarmData)
    {
        ClearVisual();

        int nbAgents = swarmData.GetAgentsData().Count;

        if (pastPositions.Count!= nbAgents)
        {
            for(int i=0; i< nbAgents; i++)
            {
                pastPositions.Add(new List<Vector3>());
            }
        }

        float step = (float) (startLineWidth - endLineWidth) / (float) nbPositionsSaved;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        List<AgentData> agents = swarmData.GetAgentsData();

        for (int i = 0; i < nbAgents; i++)
        {
            if(count >= saveAvoid)
            {
                pastPositions[i].Add(agents[i].GetPosition());
                if (pastPositions[i].Count > nbPositionsSaved) pastPositions[i].RemoveAt(0);
                count = 0;
            } else
            {
                count++;
            }

            pastPositions[i].Add(agents[i].GetPosition());
            for (int j=0;j<pastPositions[i].Count-1; j++)
            {
                float width = startLineWidth - ((float)(pastPositions[i].Count - j) * step);
                List<Vector3> v = MeshTools.TranformLineToRectanglePoints(pastPositions[i][j], pastPositions[i][j+1], width);
                List<int> t = MeshTools.DrawFilledTriangles(v.ToArray());

                //Prepare to merge the new mesh to the other meshes (in order to correct the index)
                for (int k = 0; k < t.Count; k++)
                {
                    t[k] += vertices.Count;
                }

                //Add the mesh
                vertices.AddRange(v);
                triangles.AddRange(t);
            }
            pastPositions[i].RemoveAt(pastPositions[i].Count - 1);
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
