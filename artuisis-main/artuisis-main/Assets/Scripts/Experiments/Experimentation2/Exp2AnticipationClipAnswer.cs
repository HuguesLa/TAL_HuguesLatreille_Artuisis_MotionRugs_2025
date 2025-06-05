[System.Serializable]
public class Exp2AnticipationAnswer
{
    public string filename;
    public int visualisation;
    public int rotation;
    public bool fracture;
    public float height;
    public float answerTime;

    public Exp2AnticipationAnswer(string filename, int visualisation,int rotation, bool fracture, float height, float answerTime)
    {
        this.filename = filename;
        this.visualisation = visualisation;
        this.rotation = rotation;
        this.fracture = fracture;
        this.height = height;
        this.answerTime = answerTime;
    }
}
