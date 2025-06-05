using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayerDensities : Displayer
{
    [SerializeField]
    private GameObject prefab;

    [SerializeField]
    private float zoneSize=0.5f;

    #region Private fields
    private List<GameObject> cubes;

    private Gradient gradient;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        cubes = new List<GameObject>();

        // Blend alpha from opaque at 0% to transparent at 100%
        var alphas = new GradientAlphaKey[2];
        alphas[0] = new GradientAlphaKey(1.0f, 0.0f);
        alphas[1] = new GradientAlphaKey(1.0f, 1.0f);



        gradient = new Gradient();

        var colors = new GradientColorKey[5];
        colors[0] = new GradientColorKey(Color.blue, 0.0f);
        colors[1] = new GradientColorKey(Color.cyan, 0.25f);
        colors[2] = new GradientColorKey(Color.green, 0.50f);
        colors[3] = new GradientColorKey(Color.yellow, 0.75f);
        colors[4] = new GradientColorKey(Color.red, 1.0f);
        gradient.SetKeys(colors, alphas);

    }

    // Update is called once per frame
    void Update()
    {

    }

    #region Methods - Displayer override
    public override void DisplayVisual(SwarmData swarmData)
    {
        ClearVisual();


        //int [,] densities = SwarmMetrics.GetDensities(swarmData,zoneSize);

        Tuple<int[,], float> t = SwarmTools.GetDensitiesUsingKNNWithinConvexArea(swarmData);

        int[,] densities = t.Item1;
        zoneSize = t.Item2;

        for (int i=0;i< densities.GetLength(0); i++)
        {
            for(int j=0;j< densities.GetLength(1);j++)
            {
                if (densities[i,j] > 0)
                {
                    GameObject g = Instantiate(prefab);

                    g.transform.parent = this.transform;

                    float ratio = (float) densities[i, j] / (float)swarmData.GetAgentsData().Count;
                    g.GetComponentInChildren<Renderer>().material.color = gradient.Evaluate(ratio);

                    //float height = densities[i, j] * 0.2f;

                    g.transform.localPosition = new Vector3(((i * zoneSize) + (zoneSize / 2)),0.01f, ((j * zoneSize) + (zoneSize / 2)));
                    g.transform.localScale = new Vector3(zoneSize, 0.02f , zoneSize);
                    cubes.Add(g);
                }

                if (densities[i, j] < 0)
                {
                    GameObject g = Instantiate(prefab);

                    g.transform.parent = this.transform;

                    float ratio = (float)densities[i, j] / (float)swarmData.GetAgentsData().Count;
                    g.GetComponentInChildren<Renderer>().material.color = Color.black;

                    //float height = densities[i, j] * 0.2f;

                    g.transform.localPosition = new Vector3(((i * zoneSize) + (zoneSize / 2)), 0.01f, ((j * zoneSize) + (zoneSize / 2)));
                    g.transform.localScale = new Vector3(zoneSize, 0.02f, zoneSize);
                    cubes.Add(g);
                }
            }
        }
    }
    public override void ClearVisual()
    {
        foreach (GameObject g in cubes)
        {
            Destroy(g);
        }
        cubes.Clear();
    }
    #endregion
}
