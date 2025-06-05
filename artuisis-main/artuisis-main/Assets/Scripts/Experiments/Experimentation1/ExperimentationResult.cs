using System.Collections.Generic;


[System.Serializable]
public class ExperimentationResult
{
    public List<ClipResult> results;

    public ExperimentationResult()
    {
        this.results = new List<ClipResult>();
    }

    public void AddClipResult(ClipResult res)
    {
        results.Add(res);
    }
}
