using UnityEngine;


public abstract class Displayer : MonoBehaviour
{
    public abstract void DisplayVisual(SwarmData swarmData);
    public abstract void ClearVisual();
}