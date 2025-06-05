using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Implementation de l'algorithme de flocking provenant de netlogo : http://www.netlogoweb.org/launch#http://www.netlogoweb.org/assets/modelslib/Sample%20Models/Biology/Flocking.nlogo

public class FlockingBehaviour : MonoBehaviour
{
    #region Serialized fields
    [SerializeField]
    private FlockingManager manager;
    #endregion

    #region Private fields
    public Material[] materials;



    private List<GameObject> flockmates;
    private GameObject nearest_neighbor;


    private Vector3 newRotation; //Permet de stocker la nouvelle rotation dans une variable temporaire, afin de mettre à jour les rotations des agents après que les agents aient calculé leur nouvelle rotation

    private float mapSize = 100.0f;
    #endregion

    #region MonoBehaviour Callbacks
    // Start is called before the first frame update
    void Start()
    {
        if (manager == null)
        {
            if (manager == null) Debug.LogError("Missing Flocking Manager.", this);
        }

        flockmates = new List<GameObject>();
        mapSize = manager.getMapSize();
    }

    // Update is called once per frame
    void Update()
    {
        mapSize = manager.getMapSize();

        FindFlockmatesAndNearestNeighbor();
        if(flockmates.Count>0)
        {
            //On stocke la valeur au début, pour l'utiliser tout au long de l'update
            //C'est nécessaire lorsque l'on appelle "Align" et "Cohere" car il y'a deux mise à jour sur la rotation, et l'on ne met à jour que plus tard la rotation de l'agent.
            //Il faut donc utiliser la rotation temporaire tout au long de l'algorithme. (Il n'est utile que dans la méthode "TurnAtMost".
            newRotation = this.transform.rotation.eulerAngles;

            if (isTooCloseToNearestNeighbor())
            {
                Separate();
            } else
            {
                Align();
                Cohere();
            }
        } 
    }

    private void LateUpdate()
    {
        //Mise à jour de la rotation
        this.transform.rotation = Quaternion.Euler(newRotation);

        //Mise à jour de la position
        MoveForward(10.0f);

        //Mise à jour des couleurs
        UpdateColor();
    }

    #endregion

    private void FindFlockmatesAndNearestNeighbor()
    {
        
        //Nettoyage des tableaux de l'étape précédente
        flockmates.Clear();
        nearest_neighbor = null;

        //Récupération des paramètres et de la liste des agents du manager
        float vision = manager.getVision();
        List<GameObject> agents = manager.getAgents();

        float minDist = float.MaxValue;
        foreach(GameObject g in agents)
        {
            if (GameObject.ReferenceEquals(g, this.gameObject)) continue;

            
            var dist= GetMinDistanceInInfiniteArea(this.transform.position, g.transform.position);
            
            if (dist < vision)
            {
                flockmates.Add(g);
                if (dist < minDist)
                {
                    nearest_neighbor = g;
                    minDist = dist;
                }
            }
        }
    }


    private bool isTooCloseToNearestNeighbor()
    {
        float minimum_separation = manager.getMinimunSeparation();
        if (Vector3.Distance(this.transform.position, nearest_neighbor.transform.position) < minimum_separation)
        {
            return true;
        } else
        {
            return false; ;
        }
        
    }

    private void Separate()
    {
        TurnAway(nearest_neighbor.transform.rotation.eulerAngles.y, manager.getMaxSeparateTurn());
    }

    private void Align()
    {
        TurnTowards(AverageFlockmateHeading(), manager.getMaxAlignTurn());
    }

    private float AverageFlockmateHeading()
    {
        float x_component = 0.0f;
        float z_component = 0.0f;

        foreach(GameObject g in flockmates)
        {
            x_component += g.transform.forward.x; 
            z_component += g.transform.forward.z; 
        }
        if(x_component==0 && z_component==0)
        {
            return newRotation.y;
        } else
        {
            return (Mathf.Atan2(x_component, z_component)*Mathf.Rad2Deg);
        }
    }


    private void Cohere()
    {
        TurnTowards(AverageHeadingTowardsFlockmates(), manager.getMaxCohereTurn());
    }


    private float AverageHeadingTowardsFlockmates()
    {
        float x_component = 0.0f;
        float z_component = 0.0f;

        foreach (GameObject g in flockmates)
        {
            Vector3 temp=GetHeadingTowardAgentInInfiniteArea(this.transform.position, g.transform.position);
            x_component += temp.x;
            z_component += temp.z;
        }


        if (x_component == 0 && z_component == 0)
        {
            return newRotation.y;
        }
        else
        {
            return (Mathf.Atan2(x_component, z_component) * Mathf.Rad2Deg);
        }
    }

    private void TurnTowards(float new_heading, float max_turn)
    {
        float temp = SubtractHeading(new_heading, newRotation.y);
        TurnAtMost(temp, max_turn);
    }

    private void TurnAway(float new_heading, float max_turn)
    {
        float temp=SubtractHeading(newRotation.y, new_heading);
        TurnAtMost(temp,max_turn);
    }

    
    private void TurnAtMost(float turn, float max_turn)
    {
        if (Mathf.Abs(turn)>max_turn)
        {
            if(turn > 0)
            {
                newRotation.y += max_turn;
            } else
            {
                newRotation.y -= max_turn;
            }
        } else
        {
            newRotation.y += Mathf.Abs(turn);
        }
    }


    public float SubtractHeading(float val1, float val2)
    {
        float res = val1 - val2;
        if(res>180)
        {
            if(res<0)
            {
                res += 360;
            } else
            {
                res -= 360;
            }
        }
        return res;
    }



    public void SetManager(FlockingManager f)
    {
        manager = f;
    }

    public void MoveForward(float speed)
    {
        this.transform.position += this.transform.forward * speed * Time.deltaTime;
        StayInInfiniteArea();
    }

    private void StayInInfiniteArea()
    {
        Vector3 temp = this.transform.position;
        if (this.transform.position.x > mapSize) temp.x -= mapSize;
        if (this.transform.position.x < 0.0f) temp.x += mapSize;

        if (this.transform.position.z > mapSize) temp.z -= mapSize;
        if (this.transform.position.z < 0.0f) temp.z += mapSize;

        this.transform.position = temp;
    }

    private float GetMinDistanceInInfiniteArea(Vector3 t1, Vector3 t2)
    {
        float minX = Mathf.Abs(t1.x - t2.x);
        float minZ = Mathf.Abs(t1.z - t2.z);

        if(t1.x>t2.x)
        {
            float xTemp = t2.x + mapSize;
            if (Mathf.Abs(t1.x - xTemp) < minX) t2.x += mapSize;
        } else
        {
            float xTemp = t1.x + mapSize;
            if (Mathf.Abs(xTemp - t1.x) < minX) t1.x += mapSize;
        }

        if (t1.z > t2.z)
        {
            float zTemp = t2.z + mapSize;
            if (Mathf.Abs(t1.z - zTemp) < minZ) t2.z += mapSize;
        }
        else
        {
            float zTemp = t1.z + mapSize;
            if (Mathf.Abs(zTemp - t1.z) < minZ) t1.z += mapSize;
        }
        return Vector3.Distance(t1, t2);
    }


    private Vector3 GetHeadingTowardAgentInInfiniteArea(Vector3 from, Vector3 to)
    {
        float minX = Mathf.Abs(from.x - to.x);
        float minZ = Mathf.Abs(from.z - to.z);

        if (from.x > to.x)
        {
            float xTemp = to.x + mapSize;
            if (Mathf.Abs(from.x - xTemp) < minX) to.x += mapSize;
        }
        else
        {
            float xTemp = from.x + mapSize;
            if (Mathf.Abs(xTemp - from.x) < minX) from.x += mapSize;
        }

        if (from.z > to.z)
        {
            float zTemp = to.z + mapSize;
            if (Mathf.Abs(from.z - zTemp) < minZ) to.z += mapSize;
        }
        else
        {
            float zTemp = from.z + mapSize;
            if (Mathf.Abs(zTemp - from.z) < minZ) from.z += mapSize;
        }

        Vector3 res = to - from;
        res.Normalize();

        return res;
    }

    private void UpdateColor()
    {
        if (flockmates.Count == 0)
        {
            gameObject.GetComponent<MeshRenderer>().material = materials[0];
        }
        else
        {
            float average = AverageFlockmateHeading();
            float diff = Mathf.Abs(SubtractHeading(this.transform.rotation.eulerAngles.y, average));

            if(manager.getUseGradient())
            {
                float percentage = diff / 180;
                Color.Lerp(Color.green, Color.red, percentage);
                gameObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(Color.green, Color.red, percentage);
            } else
            {
                if (diff < 90) gameObject.GetComponent<MeshRenderer>().material = materials[1];
                if (diff < 50) gameObject.GetComponent<MeshRenderer>().material = materials[2];
                if (diff < 30) gameObject.GetComponent<MeshRenderer>().material = materials[3];
                if (diff < 15) gameObject.GetComponent<MeshRenderer>().material = materials[4];
            }
        }
    }


}
