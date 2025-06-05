using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayerDelaunayTriangulation : Displayer
{
    #region Serialized fields
    [SerializeField]
    private Material material;

    [SerializeField]
    private GameObject prefab;

    #endregion

    #region Private fields
    private List<LineRenderer> linksRenderer;

    private List<GameObject> cubes;

    private Gradient gradient;
    #endregion

    #region Methods - MonoBehaviour callbacks
    private void Start()
    {
        linksRenderer = new List<LineRenderer>();

        cubes = new List<GameObject>();

        // Blend alpha from opaque at 0% to transparent at 100%
        var alphas = new GradientAlphaKey[2];
        alphas[0] = new GradientAlphaKey(1.0f, 0.0f);
        alphas[1] = new GradientAlphaKey(1.0f, 1.0f);



        gradient = new Gradient();

        var colors = new GradientColorKey[3];
        colors[0] = new GradientColorKey(Color.blue, 0.0f);
        colors[1] = new GradientColorKey(Color.white, 0.50f);
        colors[2] = new GradientColorKey(Color.red, 1.0f);
        gradient.SetKeys(colors, alphas);

    }
    #endregion

    #region Methods - Displayer override
    public override void DisplayVisual(SwarmData swarmData)
    {
        ClearVisual();

        List<Tuple<AgentData, AgentData, AgentData>> triangles = SwarmTools.GetDelaunayTriangulation(swarmData);

        List<float> areas = new List<float>();
        foreach (Tuple<AgentData, AgentData, AgentData> t in triangles)
        {
            areas.Add(GeometryTools.GetTriangleArea(t.Item1.GetPosition(), t.Item2.GetPosition(), t.Item3.GetPosition()));
        }

        float medianArea = ListTools.Median(areas);

        foreach (Tuple<AgentData, AgentData, AgentData> t in triangles)
        {

            Color lineColor = Color.black;


            //For creating line renderer object
            LineRenderer lineRenderer = new GameObject("Line").AddComponent<LineRenderer>();
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;

            lineRenderer.startWidth = 0.02f; //If you need to change the width of line depending on the distance between both agents :  0.03f*(1-distOnMaxDistance) + 0.005f;
            lineRenderer.endWidth = 0.02f;
            lineRenderer.positionCount = 4;
            lineRenderer.useWorldSpace = true;
            lineRenderer.material = material;
            //lineRenderer.material.SetFloat("_Mode", 2);
            lineRenderer.material.color = lineColor;

            //For drawing line in the world space, provide the x,y,z values
            lineRenderer.SetPosition(0, t.Item1.GetPosition()); //x,y and z position of the starting point of the line
            lineRenderer.SetPosition(1, t.Item2.GetPosition()); 
            lineRenderer.SetPosition(2, t.Item3.GetPosition()); 
            lineRenderer.SetPosition(3, t.Item1.GetPosition()); //x,y and z position of the end point of the line


            lineRenderer.receiveShadows = false;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            lineRenderer.transform.parent = this.transform;

            linksRenderer.Add(lineRenderer);


            GameObject g = Instantiate(prefab);
            g.transform.parent = this.transform;

            float area = GeometryTools.GetTriangleArea(t.Item1.GetPosition(), t.Item2.GetPosition(), t.Item3.GetPosition());

            float ratio = (((area / medianArea) -1.0f) / 2.0f) + 0.5f;

            if(ratio > 1.0f) ratio = 1.0f;
            if(ratio < 0.0f) ratio = 0.0f;

            g.GetComponentInChildren<Renderer>().material.color = gradient.Evaluate(ratio);
            Vector3 pos = GeometryTools.GetTriangleGravityCenter(t.Item1.GetPosition(), t.Item2.GetPosition(), t.Item3.GetPosition());
            g.transform.localPosition = pos;
            g.transform.localScale = new Vector3(0.2f, 0.1f, 0.2f);
            cubes.Add(g);
        }
        
    }

    public override void ClearVisual()
    {
        ClearLinksRenderer();
    }
    #endregion

    #region Methods - Other methods
    private void ClearLinksRenderer()
    {
        foreach (LineRenderer l in linksRenderer)
        {
            GameObject.Destroy(l.gameObject);
        }
        linksRenderer.Clear();

        foreach (GameObject g in cubes)
        {
            Destroy(g);
        }
        cubes.Clear();
    }
    #endregion
}
