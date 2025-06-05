using System;
using UnityEngine;
using System.Collections.Generic;

public class ClipPlayer : MonoBehaviour
{
    #region Serialized fields
    [SerializeField]
    private bool automaticCameraPositioning = false;

    [SerializeField]
    private Transform displayers;

    [SerializeField]
    private List<Displayer> usedDisplayers;
    #endregion

    #region Private fields - clip parameters
    private SwarmClip clip;
    private int nbFrames;

    #endregion

    #region Private fields - player parameters
    private bool playing = false;

    private bool displaying = true;

    private bool loopClip = false;

    private int frameNumber
    {
        get { return _frameNumber; }
        set
        {
            this._frameNumber = value;
            OnFrameChanged(null);
        }
    }

    private int _frameNumber = 0;

    #endregion

    #region Private fields - others
    private Camera mainCamera;

    Displayer[] existingDisplayers;
    #endregion

    #region Event - Frame number changed
    public event EventHandler FrameChanged;

    protected virtual void OnFrameChanged(EventArgs e)
    {
        // Raise the event
        if (FrameChanged != null)
            FrameChanged(this, e);
    }
    #endregion

    #region Methods - MonoBehaviour callbacks
    private void Start()
    {
        mainCamera = Camera.main;
        if(mainCamera == null)
        {
            Debug.Log("Missing main camera in the scene.");
        }

        existingDisplayers = displayers.GetComponentsInChildren<Displayer>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (playing)
        {
            UpdateCameraPositioning();
            DisplayFrame();
            UpdateFrameNumber();
        } else
        {
            if(!displaying)
            {
                //--Clear display for unused displayer--//
                foreach (Displayer d in existingDisplayers)
                {
                        d.ClearVisual();
                }
            }
        }
    }
    #endregion

    #region Methods - Clip player core methods
    /// <summary>
    /// Passes the current frame number to the next frame number.
    /// If the clip must loop, restart the clip when the clip reached its end.
    /// Else, if the clip reached is end and does not loop, stop the player.
    /// </summary>
    private void UpdateFrameNumber()
    {
        if(loopClip)
        {
            frameNumber = (frameNumber + 1) % nbFrames;
        } 
        else
        {
            if(IsClipFinished())
            {
                Pause();
            }
            else
            {
                frameNumber++;
            }
        }
    }

 

    /// <summary>
    /// This method displays the current frame of the loaded <see cref="SwarmClip"/>.
    /// By displaying the position of saved agents in the clip using actors.
    /// </summary>
    private void DisplayFrame()
    {

        SwarmData frame = clip.GetFrames()[frameNumber];

        //--Clear display for unused displayer--//
        foreach (Displayer d in existingDisplayers)
        {
            if (!usedDisplayers.Contains(d))
                d.ClearVisual();
        }

        //--Refresh the display--//
        foreach (Displayer d in usedDisplayers)
        {
            d.DisplayVisual(frame);
        }
    }


    /// <summary>
    /// This method intialize a clip, allowing it to be play in the right conditions.
    /// It pause the player, get the clip parameter, set the camera position according to the clip parameters, and then display the first frame (and update the UI).
    /// </summary>
    private void InitializeClip()
    {
        //Pause clip player
        Pause();



        //Reset values for a new clip
        this.frameNumber = 0;
        this.nbFrames = clip.GetFrames().Count;


        //Udpate UI
        DisplayFrame();

    }

    /// <summary>
    /// Set the clip that will be played by this player. That method also initialise the clip.
    /// </summary>
    /// <param name="clip">
    /// The clip to play. A <see cref="NullReferenceException"/> will be raised if the value is null.
    /// </param>
    /// <exception cref="NullReferenceException"> Raised if the clip in parameter is null.</exception>
    public void SetClip(SwarmClip clip)
    {
        if (clip != null)
        {
            this.clip = clip;
            InitializeClip();
        }
        else
        {
            Debug.LogError("The clip set in parameter can't be null.", this);
            throw new NullReferenceException();
        }
    }
    #endregion 

    #region Methods - Clip control - Player state
    /// <summary> 
    /// Set the player to play, if there is a clip
    /// </summary>
    /// <exception cref="Exception"> If no clip is loaded in this clip player.</exception>
    public void Play()
    {
        if (clip != null)
        {
            //If the clip is not finished, play it
            if (!IsClipFinished() || this.loopClip)
            {
                playing = true;
            }
        }
        else
        {
            Debug.LogError("There is no clip in the clip player, it's impossible to change its play state.", this);
            throw new Exception();
        }
    }

    /// <summary> 
    /// Pause the clip player, if there is a clip
    /// </summary>
    /// <exception cref="Exception"> If no clip is loaded in this clip player.</exception>
    public void Pause()
    {
        if (clip != null)
        {
            playing = false;
        }
        else
        {
            Debug.LogError("There is no clip in the clip player, it's impossible to change its play state.", this);
            throw new Exception();
        }
    }

    /// <summary>
    /// Set the loop mode of the player. 
    /// If <paramref name="looping"/> at true, the clip will be played on a loop. 
    /// At false, the clip will be played once.
    /// </summary>
    /// <param name="looping"> True : clip will be played on a loop. False : clip will be played once. </param>
    public void SetLoopMode(bool looping)
    {
        this.loopClip = looping;
    }
    #endregion

    #region Methods - Clip control - Display

    private void UpdateCameraPositioning()
    {
        if(automaticCameraPositioning)
        {
            //Camera positionning
            SwarmParameters parameters = clip.GetFrames()[frameNumber].GetParameters();
            mainCamera.transform.position = new Vector3(parameters.GetMapSizeX() / 2.0f, Mathf.Max(parameters.GetMapSizeZ(), parameters.GetMapSizeX()), parameters.GetMapSizeZ() / 2.0f);
            mainCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
        }
    }

    public void AllowDisplay(bool display)
    {
        displaying = display;
        if (!displaying) playing = false;
    }
    #endregion

    #region Methods - Clip control - Frame
    /// <summary>
    /// Change the current frame of the clip and display it. The clip will continue from this frame. 
    /// </summary>
    /// <param name="value"> 
    /// A <see cref="float"/> value representing a percentage of the clip. 
    /// This value must be between 0.0f and 1.0f. 1.0f correspond the end of the clip and 0.0f to its start.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException"> If <paramref name="value"/> doesn't respect the allowed range [0.0f, 1.0f]. </exception>
    public void SetClipAtPercentage(float value)
    {
        if (value >= 0.0f && value <= 1.0f)
        {
            this.frameNumber = (int)((this.nbFrames - 1) * value);
            DisplayFrame();
        }
        else
        {
            Debug.LogError("The value set in parameter is out of range (only [0.0f,1.0f] value allowed).", this);
            throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Change the current frame of the clip and display it. The clip will continue from this frame. 
    /// </summary>
    /// <param name="number"> 
    /// A <see cref="int"/> value corresponding to the wanted frame. 
    /// If the value is less than 0 or greater or equal to the number of frame, an <see cref="ArgumentOutOfRangeException"/> will be raised.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException"> If no clip is loaded in this clip player.</exception>
    public void SetFrameNumber(int number)
    {
        if (number >= 0 && number < this.nbFrames)
        {
            this.frameNumber = number;
            DisplayFrame();
        }
        else
        {
            Debug.LogError("The frame number set in parameter is out of range.", this);
            throw new ArgumentOutOfRangeException();
        }
    }
    #endregion

    #region Methods - Getter
    public int GetFrameNumber()
    {
        return this.frameNumber;
    }

    public bool IsPlaying()
    {
        return playing;
    }

    public bool IsClipFinished()
    {
        if (frameNumber >= (nbFrames - 1))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public float GetClipAdvancement()
    {
        float res = (float)this.frameNumber / (float)(this.nbFrames - 1);
        return res;
    }

    public SwarmData GetCurrentFrame()
    {
        return clip.GetFrames()[frameNumber];
    }

    #endregion

    #region Methods - Setter
    public void SetUsedDisplayers(List<Displayer> displayers)
    {
        usedDisplayers = displayers;
    }

    #endregion
}
