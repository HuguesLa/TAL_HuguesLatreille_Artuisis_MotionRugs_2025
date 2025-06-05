using System.Collections.Generic;
using UnityEngine;

public class DisplayerPerceptionArea : Displayer
{
    #region Serialized fields
    [SerializeField]
    [Range(3,100)]
    private int edge = 16; //Number of slices of the circle

    [SerializeField]
    private Material material;

    [SerializeField]
    private Color color;
    #endregion

    #region Private fields
    private Mesh mesh;
    #endregion

    #region Methods -  MonoBehaviour callbacks 
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
    }
    private void Update()
    {
        material.color = this.color;
    }

    #endregion

    #region Methods - Displayer override
    public override void ClearVisual()
    {
        mesh.Clear();
    }


    public override void DisplayVisual(SwarmData swarmData)
    {
        ClearVisual();

        SwarmParameters parameters = swarmData.GetParameters();
        float fovSize = parameters.GetFieldOfViewSize();
        float bsSize = parameters.GetBlindSpotSize();


        List<Vector3> vertices = new List<Vector3>(); ;

        List<int> triangles = new List<int>();

        foreach (AgentData a in swarmData.GetAgentsData())
        {
            Vector3 position = a.GetPosition();

            Vector3 direction = a.GetDirection();

            //Get the mesh for one agent
            List<Vector3> v = GetCircumferencePoints(edge, fovSize, bsSize, position, direction);
            List<int> t = MeshTools.DrawFilledTriangles(v.ToArray());

            //Prepare to merge the new mesh to the other meshes (in order to correct the index)
            for(int i=0; i<t.Count; i++)
            {
                t[i] += vertices.Count;
            }

            //Add the mesh
            vertices.AddRange(v);
            triangles.AddRange(t);
        }


        Vector2[] uv = new Vector2[vertices.Count];

        mesh.vertices = vertices.ToArray();
        mesh.uv = uv;
        mesh.triangles = triangles.ToArray();
    }
    #endregion


    #region Methods - Compute circle with empty slice

    /// <summary>
    /// This methods compute the different vertices allowing to draw a circle with an empty slice. Take into account the position and the orientation of the circle.
    /// </summary>
    /// <param name="sides"> Number of side of the circle (more side equal smoother circle)</param>
    /// <param name="radius"> Radius of the circle</param>
    /// <param name="emptyAngle"> Size of the empty spot of the circle (in degree)</param>
    /// <param name="position"> Position of the center of the circle</param>
    /// <param name="direction"> Direction of the circle (the empty spot will be at the opposite)</param>
    /// <returns> The list of the vertices of the circle. The first vertex is the origin.</returns>
    private List<Vector3> GetCircumferencePoints(int sides, float radius, float emptyAngle, Vector3 position, Vector3 direction)
    {
        List<Vector3> points = new List<Vector3>();
        float circumferenceProgressPerStep = (float)1 / sides;



        //Calculate the correct angle to start drawing the circle with an empty spot
        float angleDiff = 90.0f; //By default, the drawing of the circle start at (1,0) of the trigonometric circle. To start from (0,-1), we add a 90° angle diff
        angleDiff -= emptyAngle/2.0f; //Take into account the empty spot size
        
        float angle = Vector3.Angle(direction, new Vector3(0, 0, 1)); //Take into account the direction of the circle, to place correctly the empty spot at the rear
        if (direction.x < 0) angle = -angle;

        angleDiff += angle;

        angleDiff = angleDiff * Mathf.PI / 180.0f; //Conversion into radian


        //In order to add the empty spot in the circle, reduce the size of the step (there will be the same amount of edges, no matter what the step size).
        float emptyPercentage = emptyAngle / 360.0f;
        float TAU = (1.0f-emptyPercentage) * 2 * Mathf.PI;
        float radianProgressPerStep = circumferenceProgressPerStep * TAU;

        //Add the origin of the circle
        points.Add(position);

        //Compute each circumferencePoint of the circle
        for (int i = 0; i <= sides; i++)
        {
            float currentRadian = radianProgressPerStep * i - angleDiff; //Take into account the angle diff
            Vector3 newPoint = new Vector3(Mathf.Cos(currentRadian) * radius, 0, Mathf.Sin(currentRadian) * radius);

            newPoint += position;
            points.Add(newPoint);
        }
        return points;
    }

    #endregion
}

