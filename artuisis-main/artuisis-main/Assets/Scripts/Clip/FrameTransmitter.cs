using UnityEngine;

public class FrameTransmitter : MonoBehaviour
{
    private SwarmData frame;
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        this.gameObject.name = "FrameTransmitter";
    }

    public void SetFrame(SwarmData frame)
    {
        this.frame = frame;
    }

    public SwarmData GetFrame()
    {
        return this.frame;
    }

    public SwarmData GetFrameAndDestroy()
    {
        Destroy(this.gameObject);
        return GetFrame();
    }
}
