using System.Collections.Generic;


[System.Serializable]
public class Exp2AnticipationResult
{
    public List<Exp2AnticipationAnswer> results;

    public Exp2AnticipationResult()
    {
        this.results = new List<Exp2AnticipationAnswer>();
    }

    public void AddClipResult(Exp2AnticipationAnswer res)
    {
        results.Add(res);
    }
}
