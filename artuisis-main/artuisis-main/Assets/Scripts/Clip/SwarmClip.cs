using System.Collections.Generic;

[System.Serializable]
public class SwarmClip
{
    #region Private fields
    private List<SwarmData> frames;
    #endregion

    #region Methods - Constructor
    public SwarmClip(List<SwarmData> frames)
    {
        this.frames = frames;
    }
    #endregion

    #region Methods - Getter
    public List<SwarmData> GetFrames()
    {
        return frames;
    }

    public SwarmClip Clone()
    {
        List<SwarmData> framesClone = new List<SwarmData>();

        foreach(SwarmData f in this.frames)
        {
            framesClone.Add(f.Clone());
        }

        return new SwarmClip(framesClone);
    }
    #endregion
}
