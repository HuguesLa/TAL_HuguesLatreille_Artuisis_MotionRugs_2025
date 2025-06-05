using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ClipResult
{
    public string filename;
    public bool fracture;
    public int frameNumber;

    public ClipResult(string filename, bool fracture, int frameNumber)
    {
        this.filename = filename;
        this.fracture = fracture;
        this.frameNumber = frameNumber;
    }

}
