using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockingManager : MonoBehaviour
{

    #region Serialized fields
    [SerializeField]
    private GameObject agentPrefab;

    [SerializeField]
    private int population = 30;


    [Header("Paramètres pouvant être modifiés durant la simulation")]
    [SerializeField]
    [Range(20, 100)]
    private int map_size = 100;

    [SerializeField]
    private bool use_gradient = false;



    [Range(0.0f, 10.0f)]
    public float vision = 5.0f;
    [Range(0.0f, 5.0f)]
    public float minimum_separation = 1.0f;


    [SerializeField]
    [Range(0.0f, 20.0f)]
    private float max_align_turn = 5.0f;
    [SerializeField]
    [Range(0.0f, 20.0f)]
    private float max_cohere_turn = 3.0f;
    [SerializeField]
    [Range(0.0f, 20.0f)]
    private float max_separate_turn = 1.5f;
    #endregion


    #region Private fields
    private List<GameObject> agents;
    #endregion



    #region Monobehaviour Callbacks
    // Start is called before the first frame update
    void Start()
    {
        agents = new List<GameObject>();
        for(int i=0;i<population;i++)
        {
            GameObject newAgent=GameObject.Instantiate(agentPrefab);
            newAgent.transform.position = new Vector3(Random.Range(0.0f, 100.0f), 0.5f, Random.Range(00.0f, 100.0f));
            newAgent.transform.rotation = Quaternion.Euler(new Vector3(0.0f, Random.Range(0.0f, 359.0f), 0.0f));
            newAgent.GetComponent<FlockingBehaviour>().SetManager(this);
            agents.Add(newAgent);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #endregion


    #region Getter/Setter

    public float getVision()
    {
        return vision;
    }

    public float getMinimunSeparation()
    {
        return minimum_separation;
    }

    public float getMaxAlignTurn()
    {
        return max_align_turn;
    }

    public float getMaxCohereTurn()
    {
        return max_cohere_turn;
    }

    public float getMaxSeparateTurn()
    {
        return max_separate_turn;
    }

    public List<GameObject> getAgents()
    {
        return agents;
    }

    public int getMapSize()
    {
        return map_size;
    }


    public bool getUseGradient()
    {
        return use_gradient;
    }
    #endregion
}
