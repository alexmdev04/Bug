using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class uiDebugConsole : MonoBehaviour
{
    public static uiDebugConsole instance { get; private set; }
    TMP_InputField inputField;
    [SerializeField] bool outputCommandInputs;
    public int previousInputsIndex = 0;
    public List<string> previousInputs = new() { string.Empty };
    string command;
    string data1;
    VolumeProfile defaultProfile;
    struct commandInputs
    {
        public string command;
        public string data1;
        //public string data2;
        //public string data3;
    }
    void Awake()
    {
        instance = this;
        inputField = GetComponent<TMP_InputField>();
        inputField.onSubmit.AddListener((string playerInput) => Command(playerInput) );
        gameObject.SetActive(false);
        defaultProfile = LevelLoader.instance.globalVolume.profile;
    }
    void Update()
    {
        inputField.ActivateInputField();
        PreviousInput();
    }
    void OnDisable()
    {
        previousInputs[0] = string.Empty;
        inputField.text = string.Empty;
        previousInputsIndex = 0;
    }
    void Command(string input)
    {
        string outputMsg = string.Empty;
        previousInputs.Insert(1, input);
        commandInputs parsedInput = ParseInput(input);
        command = parsedInput.command.ToLower();
        data1 = parsedInput.data1;
        bool data1Present = data1 != string.Empty;
        if (outputCommandInputs)
        {
            Debug.Log("command = \"" + parsedInput.command + "\", " +
                      "data1 = \"" + parsedInput.data1 + "\", ");
                      //"input2 = \"" + commandInputs.input2 + "\", " +
                      //"input3 = \"" + commandInputs.input3 + "\"");
        }
        switch (parsedInput.command)
        {
            case "level":
                {
                    switch (parsedInput.data1)
                    {
                        case "reload":
                        case "restart":
                            {
                                LevelLoader.instance.LoadLevel(new LevelLoader.levelLoadData()
                                {
                                    levelAssetKey = LevelLoader.instance.levelCurrent.assetKey,
                                    useFade = true,
                                    levelDifficulty = Level.levelDifficultiesEnum.normal

                                });
                                break;
                            }
                        case "exit":
                        case "leave":
                        case "unload":
                            {
                                LevelLoader.instance.UnloadLevel(LevelLoader.instance.levelCurrent, true, true);
                                break;
                            }
                        default:
                            {
                                if (!data1Present)
                                {
                                    InvalidInput();
                                    break;
                                }
                                string output = parsedInput.data1;
                                if (int.TryParse(parsedInput.data1, out int levelNumber))
                                {
                                    output = "Level" + levelNumber.ToString();
                                }
                                LevelLoader.instance.LoadLevel(new LevelLoader.levelLoadData()
                                {
                                    levelAssetKey = output,
                                    useFade = true,
                                    levelDifficulty = Level.levelDifficultiesEnum.normal
                                });
                                break;
                            }
                    }

                    break;
                }
            case "section":
                {
                    switch (parsedInput.data1) 
                    {
                        case "skip":
                        case "next":
                        case "forward":
                            {
                                LevelLoader.instance.levelCurrent.SectionStart(LevelLoader.instance.levelCurrent.SectionIndex() + 1);
                                break;
                            }
                        case "previous":
                        case "back":
                        case "last":
                            {
                                LevelLoader.instance.levelCurrent.SectionStart(LevelLoader.instance.levelCurrent.SectionIndex() - 1);
                                break;
                            }
                        default:
                            {
                                if (parsedInput.data1.ToCharArray().AllCharsAreDigits())
                                {
                                    outputMsg = "Started " + LevelLoader.instance.levelCurrent.assetKey + " Section" + parsedInput.data1;
                                    LevelLoader.instance.levelCurrent.SectionStart(Convert.ToInt32(parsedInput.data1) - 1);
                                    break;
                                }
                                InvalidInput();
                                break;
                            }
                    }
                    break;
                }
            case "grapple":
                {
                    switch (parsedInput.data1)
                    {
                        case "enable":
                            {
                                Grapple.instance.Enable();
                                break;
                            }
                        case "disable":
                            {
                                Grapple.instance.Disable();
                                break;
                            }
                        case "testcube":
                            {
                                Grapple.instance.grappleDestinationMarker.SetActive(Grapple.instance.grappleDestinationMarker.activeSelf);
                                break;
                            }
                        default:
                            {
                                InvalidInput();
                                break;
                            }
                    }
                    break;
                }
            case "god":
            case "godmode":
                {
                    uiDebug.instance.ToggleGod();
                    break;
                }
            case "ufo":
            case "noclip":
                {
                    float noclipSpeed = uiDebug.instance.noclipSpeed;
                    switch (parsedInput.data1)
                    {
                        case "enable":
                            {
                                uiDebug.instance.ToggleNoclip();
                                break;
                            }
                        default:
                            {
                                if (data1Present && !float.TryParse(parsedInput.data1, out noclipSpeed))
                                {
                                    InvalidInput();
                                    break;
                                }
                                uiDebug.instance.noclipSpeed = noclipSpeed;
                                break;
                            }
                    }
                    break;
                }
            case "menu":
                {
                    LevelLoader.instance.UnloadLevel(LevelLoader.instance.levelCurrent, true, true);
                    break;
                }
            case "player":
                {
                    switch (parsedInput.data1)
                    {
                        case "position":
                            {
                                //Player.instance.debugSetPosition(parsedInput.data2);
                                outputMsg = "Not Implemented";
                                break;
                            }
                        case "position reset":
                            {
                                outputMsg = "Player position has been reset";
                                break;
                            }
                    }
                    break;
                }
            case "tutorial":
                {
                    LevelLoader.instance.UnloadLevel(LevelLoader.instance.levelCurrent, true, true);
                    LevelLoader.instance.menuLevel.GetComponent<MenuLevel>().ForceRestartTutorial();
                    break;
                }
            case "fps":
                {
                    int fps = Application.targetFrameRate;
                    switch (parsedInput.data1)
                    {
                        case "show":
                        case "enable":
                            {
                                uiDebug.instance.ToggleFPS();
                                break;
                            }
                        default:
                            {
                                if (data1Present && !int.TryParse(parsedInput.data1, out fps))
                                {
                                    InvalidInput();
                                    break;
                                }
                                Application.targetFrameRate = fps;
                                break;
                            }
                    }
                    break;
                }
            case "vsync":
                {
                    bool vSyncEnabled = QualitySettings.vSyncCount > 0;
                    QualitySettings.vSyncCount = vSyncEnabled ? 0 : 1;
                    outputMsg = "vSync " + (vSyncEnabled ? "Disabled" : "Enabled");
                    break;
                }
            case "fov":
                {
                    if (float.TryParse(parsedInput.data1, out float fov))
                    {
                        Camera.main.fieldOfView = Extensions.FOVHorizontalToVertical(fov, Camera.main, true);
                    }
                    else
                    {
                        InvalidInput();
                    }
                    break;
                }
            case "torch":
                {
                    Player.instance.ToggleTorch();
                    break;
                }
            case "close":
            case "exit":
            case "quit":
                {
                    Application.Quit();
                    break;
                }
            case "gfx":
            case "graphics":
            case "effect":
            case "effects":
                {
                    switch (parsedInput.data1.ToLower())
                    {
                        case "all":
                            {
                                LevelLoader.instance.globalVolume.enabled = !LevelLoader.instance.globalVolume.enabled;
                                outputMsg = "Effects " + (LevelLoader.instance.globalVolume.enabled ? "Enabled" : "Disabled");
                                break;
                            }
                        case "default":
                        case "reset":
                            {
                                LevelLoader.instance.globalVolume.profile = defaultProfile;
                                outputMsg = "Effects reset to default";
                                break;
                            }
                        case "motionblur":
                        case "blur":
                        case "mb":
                            {
                                if (LevelLoader.instance.globalVolume.profile.TryGet(out MotionBlur motionBlur)) { motionBlur.active = !motionBlur.active;
                                    outputMsg = "Motion Blur " + (motionBlur.active ? "Enabled" : "Disabled"); }
                                break;
                            }
                        case "vignette":
                            {
                                if (LevelLoader.instance.globalVolume.profile.TryGet(out Vignette vignette)) { vignette.active = !vignette.active;
                                    outputMsg = "Vignette " + (vignette.active ? "Enabled" : "Disabled"); }
                                break;
                            }
                        case "chromaticaberration":
                        case "ca":
                        case "aberration":
                            {
                                if (LevelLoader.instance.globalVolume.profile.TryGet(out ChromaticAberration chromaticAberration)) { chromaticAberration.active = !chromaticAberration.active;
                                    outputMsg = "Chromatic Aberration " + (chromaticAberration.active ? "Enabled" : "Disabled"); }
                                break;
                            }
                        case "bloom":
                        case "glow":
                            {
                                if (LevelLoader.instance.globalVolume.profile.TryGet(out Bloom bloom)) { bloom.active = !bloom.active; 
                                    outputMsg = "Bloom " + (bloom.active ? "Enabled" : "Disabled"); }
                                break;
                            }
                        case "paniniprojection":
                        case "warp":
                        case "warping":
                        case "stretch":
                            {
                                if (LevelLoader.instance.globalVolume.profile.TryGet(out PaniniProjection paniniProjection)) { paniniProjection.active = !paniniProjection.active; 
                                    outputMsg = "Panini Projection " + (paniniProjection.active ? "Enabled" : "Disabled"); }
                                break;
                            }
                        case "tonemapping":
                            {
                                if (LevelLoader.instance.globalVolume.profile.TryGet(out Tonemapping tonemapping)) { tonemapping.active = !tonemapping.active; 
                                    outputMsg = "Tonemapping " + (tonemapping.active ? "Enabled" : "Disabled"); }
                                break;
                            }
                        default:
                            {
                                InvalidInput();
                                break;
                            }
                    }
                    break;
                }
            case "statsRepeatRate":
            case "debugUpdateRate":
                {
                    if (float.TryParse(parsedInput.data1, out float updateRate))
                    {
                        updateRate = Mathf.Clamp(updateRate, 0.05f, 1f);
                        uiDebug.instance.statsRepeatRate = updateRate;
                        uiDebug.instance.RefreshRepeating();
                        outputMsg = "Set statsRepeatRate to " + updateRate;
                    }
                    else
                    {
                        InvalidInput();
                    }
                    break;
                }
            //case "dev":
            //    {
            //        //InternalCommandCall("effect all");
            //        InternalCommandCall("noclip 100");
            //        InternalCommandCall("noclip enable");
            //        InternalCommandCall("level Level3");
            //        InternalCommandCall("effect paniniprojection");
            //        InternalCommandCall("effect motionblur");
            //        break;
            //    }
            default:
                {
                    InvalidCommand();
                    break;
                }
        }
        inputField.text = "";
        if (outputMsg != string.Empty)
        {
            uiMessage.instance.New(outputMsg, uiDebug.str_uiDebugConsole);
            if (uiDebug.instance.debugMode) { Debug.Log(outputMsg); }
        }
        gameObject.SetActive(false);
    }
    commandInputs ParseInput(string input)
    {
        commandInputs returnInputs = new commandInputs();
        char[] chars = input.ToCharArray();
        List<int> spaceIndexes = new();

        // getting indexes of the spaces in the input text
        for (int i = 0; i < chars.Length; i++) { if (chars[i] == ' ') { spaceIndexes.Add(i); } }
        // if there are no spaces then return the input in full
        if (spaceIndexes.Count == 0)
        {
            if (outputCommandInputs) { Debug.Log("only command no inputs"); }
            returnInputs.command = input;
            return returnInputs;
        }
        // if the first letter is a space and there is more than 1 letter then remove the space (if there is only a space then it will just be an unrecognised command)
        if (spaceIndexes[0] == 0 && chars.Length >= 1) { chars = chars[1..]; }
        // if the first space index is the final character then ignore it and output the command
        if (chars[^1] == spaceIndexes[0])
        {
            if (outputCommandInputs) { Debug.Log("only command no inputs, space at the end ignored"); }
            returnInputs.command = chars[..^1].ArrayToString();
            return returnInputs;
        }
        // add all characters of the input to the command up until the first space
        returnInputs.command = chars[..spaceIndexes[0]].ArrayToString();
        returnInputs.data1 = chars[(spaceIndexes[0] + 1)..].ArrayToString();

        //if (chars[^1] == ' ') 
        //{ 
        //    chars = chars[..^1];
        //}

        //if (spaceIndexes.Count == 2)
        //{
        //    // if the second space index is the final character then ignore it and output the command
        //    if (chars[^1] == spaceIndexes[1])
        //    {
        //        returnInputs.input1 = chars[(spaceIndexes[0] + 1)..spaceIndexes[1]].ArrayToString();
        //        return returnInputs;
        //    }
        //    for (int i = spaceIndexes[0]; i < spaceIndexes[1]; i++)
        //    {
        //        returnInputs.input1 += chars[i];
        //    }
        //}
        //else
        //{
            
        //}

        return returnInputs;
    }
    public void InvalidCommand(string commandOverride = default)
    {
        string commandOutput = commandOverride == default ? command : commandOverride;
        uiMessage.instance.New("Invalid Command: " + commandOutput, uiDebug.str_uiDebugConsole);
    }
    public void InvalidInput(string dataOverride = default, string commandOverride = default)
    {
        string dataOutput = dataOverride == default ? data1 : dataOverride;
        string commandOutput = commandOverride == default ? command : commandOverride;
        uiMessage.instance.New("Invalid input \"" + dataOutput + "\" for \"" + commandOutput + "\" command", uiDebug.str_uiDebugConsole);
    }
    void PreviousInput()
    {
        if (previousInputsIndex == 0) { previousInputs[0] = inputField.text; }
        if (previousInputs.Count > 1)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) && previousInputsIndex < previousInputs.Count - 1)
            {
                previousInputsIndex++;
                inputField.text = previousInputs[previousInputsIndex];
                char[] inputFieldChars = inputField.text.ToCharArray();
                inputField.stringPosition = inputFieldChars.ToList().IndexOf(inputFieldChars[^1]) + 1;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) && previousInputsIndex > 0)
            {
                previousInputsIndex--;
                inputField.text = previousInputs[previousInputsIndex];
            }
        }
    }
    public void InternalCommandCall(string input)
    {
        Command(input);
    }
}