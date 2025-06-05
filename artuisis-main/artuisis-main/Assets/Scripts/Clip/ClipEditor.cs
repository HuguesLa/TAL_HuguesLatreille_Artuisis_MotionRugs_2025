using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SFB;
using System;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(ClipPlayer))]
public class ClipEditor : MonoBehaviour
{
    #region Serialized fields
    [SerializeField]
    private bool mirrorClipX = false;

    [SerializeField]
    private bool mirrorClipZ = false;

    [SerializeField]
    private ClipPlayer clipPlayer;

    [SerializeField]
    private GameObject buttonsIfClipLoaded;

    [SerializeField]
    private GameObject saveButton;

    [SerializeField]
    private Slider slider;

    [SerializeField]
    private Button playButton;

    [SerializeField]
    private TMP_Text clipDurationText;    
    
    [SerializeField]
    private TMP_Text timestampText;

    #endregion

    #region Private fields - clip parameters
    

    private SwarmClip clip;

    private string filePath = "";
    #endregion

    #region Private fields - clip editor parameters
    private bool loaded = false;
    private bool sliderValueChanged = false;
    private bool modifiedClip = false;

    #endregion

    #region Methods - MonoBehaviour callbacks
    // Start is called before the first frame update
    void Start()
    {
        if(clipPlayer == null)
        {
            Debug.LogError("There is no ClipPlayer in the scene.", this);
        }

        //Set clip player in loop mode
        clipPlayer.SetLoopMode(true);

        //Get value change (current frame number) of clip player event 
        clipPlayer.FrameChanged += UpdateSliderValue;
    }

    // Update is called once per frame
    void Update()
    {
        //Display or hide UI button
        buttonsIfClipLoaded.SetActive(loaded);
        saveButton.SetActive(modifiedClip);
        //Update button name based on his behavior if pressed
        playButton.GetComponentInChildren<TMP_Text>().text = clipPlayer.IsPlaying() ? "Pause" : "Play";
        //Debug.Log(clipPlayer.GetFrameNumber());
        if(loaded)
        {
            clipDurationText.text = (clip.GetFrames().Count * Time.fixedDeltaTime).ToString() + " s";
            timestampText.text = (clipPlayer.GetFrameNumber() * Time.fixedDeltaTime).ToString();
        }
    }
    #endregion

    #region Methods - Clip player methods

    /// <summary>
    /// This method udpates the visual state of the slider in the UI.
    /// In doing so, it prevent the call of the "on slider change" by setting a bool value to true.
    /// </summary>
    private void UpdateSliderValue(object sender, EventArgs e)
    {
        sliderValueChanged = true;
        slider.value = clipPlayer.GetClipAdvancement();
    }

    /// <summary>
    /// This methods allow to select a specific frame moment in the clip.
    /// This selection is based on the slider value  (set in parameter in the editor).
    /// Reload the display to show the actual frame of the clip.
    /// </summary>
    public void SelectFrame()
    {
        if (!sliderValueChanged)
        {
            clipPlayer.SetClipAtPercentage(slider.value);
        }
        sliderValueChanged = false;
    }

    /// <summary> 
    /// This method reverses the state of the clip player.
    /// If the clip player was playing, then this method changes it state to pause.
    /// Else, if the clip player was paused, this method changes it state to play
    /// </summary>
    public void PlayOrPauseClip()
    {
        if (loaded)
        {
            if (clipPlayer.IsPlaying()) 
            {
                clipPlayer.Pause();
            }
            else
            {
                clipPlayer.Play();
            }  
        }
    }
    #endregion

    #region Methods - Clip editor

    /// <summary>
    /// This method allows to remove a part of the clip, to reduce it size. The removed part is lost"/>
    /// </summary>
    /// <param name="firstPart">
    /// A <see cref="bool"/> value that decides whether the first or the last part of the clip which is deleted. 
    /// True, the first part is deleted. 
    /// False, the last part is deleted.
    /// </param>
    public void RemoveClipPart(bool firstPart)
    {
        //If there are enough frame to cut the clip
        if (clip.GetFrames().Count > 2)
        {
            if (firstPart) //Remove the first part of the clip
            {
                clip.GetFrames().RemoveRange(0,clipPlayer.GetFrameNumber());
                clipPlayer.SetClip(clip);
                clipPlayer.SetFrameNumber(0);
                
            }
            else //Remove the last part of the clip
            {
                clip.GetFrames().RemoveRange(clipPlayer.GetFrameNumber(), clip.GetFrames().Count - clipPlayer.GetFrameNumber());
                clipPlayer.SetClip(clip);
                clipPlayer.SetFrameNumber(clip.GetFrames().Count - 1);
            }
            modifiedClip = true;
        }
    }

    /// <summary>
    /// Save a modified clip, under the original filename adding "_mod", in the same folder as the original clip. 
    /// A clip can't be saved using this method if it wasn't modified.
    /// </summary>
    public void SaveUpdatedClip()
    {
        if (modifiedClip)
        {
            int pos = filePath.LastIndexOf('.');
            string newFilePath = filePath.Remove(pos);
            newFilePath += "_mod.dat";
            Debug.Log(newFilePath);
            ClipTools.SaveClip(clip, newFilePath);
            modifiedClip = false;
        }
    }





    /// <summary>
    /// Open a windows explorer to select a clip and load it.
    /// Allowed files have .dat extension.
    /// Once the clip is load, it's intialized.
    /// </summary>
    public void ShowExplorer()
    {
        var extensions = new[] {
        new ExtensionFilter("Data files", "dat" ),
        };
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, true);
        if(paths.Length > 0)
        {
            filePath = paths[0];

            if (filePath != string.Empty)
            {
                clip = ClipTools.LoadClip(filePath);
                loaded = (clip != null);

                if (loaded)
                {
                    Debug.Log("Clip loaded");
                    if (mirrorClipX || mirrorClipZ) clip = ClipTools.MirrorClip(clip, mirrorClipX, mirrorClipZ);
                    clipPlayer.SetClip(clip);
                }
            }
        }
    }

    public void ExitApp()
    {
        Application.Quit();
    }
    #endregion

    #region Methods - Simulation
    public void BackToSimulation()
    {
        SwarmData frame = clipPlayer.GetCurrentFrame();

        GameObject go = new GameObject();
        FrameTransmitter frameTransmitter = go.AddComponent<FrameTransmitter>();
        frameTransmitter.SetFrame(frame);
        SceneManager.LoadScene("Scenes/SimulationScene"); 
    }

    #endregion
}
