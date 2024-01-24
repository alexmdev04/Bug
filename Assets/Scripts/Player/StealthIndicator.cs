using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class StealthIndicator : MonoBehaviour
{
    [SerializeField] Volume globalVolume;
    [SerializeField] Color vignetteColor = Color.black;
    [SerializeField] Color vignetteColorTargetUp = Color.black;
    //[SerializeField] Color vignetteColorTargetDown = Color.black;
    [SerializeField] float vignetteIntensity = 0.35f;
    [SerializeField] float vignetteSmoothness = 0.5f;
    [SerializeField] Color hiddenColor = Color.green;
    [SerializeField] Color spottedColor = Color.yellow;
    [SerializeField] Color fullAlertColor = Color.red;
    Vignette vignette;
    void Awake()
    {
        globalVolume.profile.TryGet(out vignette);
    }

    // Update is called once per frame
    void Update()
    {
        float lerpUpMax = 1f;
        //float lerpDownMax = 1f;
        switch (StealthHandler.instance.stealthLevel)
        {
            case StealthHandler.stealthLevelEnum.hidden:
                {
                    vignetteColorTargetUp = spottedColor;
                    lerpUpMax = StealthHandler.instance.toLevelSpotted;
                    //lerpDownMax = 0;
                    break;
                }
            case StealthHandler.stealthLevelEnum.spotted:
                {
                    vignetteColorTargetUp = fullAlertColor;
                    lerpUpMax = StealthHandler.instance.toLevelFullAlert;
                    break;
                }
            case StealthHandler.stealthLevelEnum.fullAlert:
                {
                    //lerpDownMax = 0;
                    break;
                }
        }

        vignetteColor = Color.Lerp(Color.black, vignetteColorTargetUp, StealthHandler.instance.stealthLevelTimer / lerpUpMax);




        //lerpUpMax = StealthHandler.instance.toLevelSpotted + StealthHandler.instance.toLevelFullAlert;
        //lerpDownMax = StealthHandler.instance.fromLevelSpottedCooldown;
        
        vignette.color.value = vignetteColor;
        vignette.intensity.value = vignetteIntensity;
        vignette.smoothness.value = vignetteSmoothness;
    }
    public void VignetteSet(float intensity = -1f, float smoothness = -1f)
    {
        if (intensity >= 0f) { vignetteIntensity = intensity; }
        if (smoothness >= 0f) { vignetteSmoothness = smoothness; }
    }
    public void VignetteSet(Color color, float intensity = -1f, float smoothness = -1f)
    {
        vignetteColor = color;
        if (intensity >= 0f) { vignetteIntensity = intensity; }
        if (smoothness >= 0f) { vignetteSmoothness = smoothness; }
    }
    public void VignetteReset()
    {
        vignetteColor = Color.black;
        vignetteIntensity = 0.35f;
        vignetteSmoothness = 0.5f;
    }
}
