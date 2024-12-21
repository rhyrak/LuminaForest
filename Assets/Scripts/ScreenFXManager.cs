using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenFXManager : MonoBehaviour
{
    [SerializeField] private Volume GlobalVolume;
    public static ScreenFXManager instance;
    private ChromaticAberration chromaticAberration;
    private bool runChromaticAberration = false;
    private float chromaDuration;
    private float chromaTimer;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        GlobalVolume.profile.TryGet(out chromaticAberration);
    }

    // Update is called once per frame
    public void Update()
    {
        if (runChromaticAberration)
        {
            chromaTimer -= Time.deltaTime;
            chromaticAberration.intensity.Override(Mathf.Lerp(
                chromaticAberration.intensity.value, 0f, Mathf.Abs(chromaDuration - chromaTimer)));
            if (chromaTimer <= 0.0f)
            {
                runChromaticAberration = false;
                chromaticAberration.intensity.Override(0.0f);
            }
        }
    }

    public void RunChromaticAberration(float duration)
    {
        chromaDuration = duration > 0.0f ? duration : 0.0f;
        chromaticAberration.intensity.Override(1.0f);
        runChromaticAberration = true;
        chromaTimer = duration;
    }

    public void EnablePlayerDiedEffects()
    {
        ColorAdjustments colorAdjustments;
        GlobalVolume.profile.TryGet(out colorAdjustments);
        colorAdjustments.active = true;
        DepthOfField depthOfField;
        GlobalVolume.profile.TryGet(out depthOfField);
        depthOfField.active = true;
    }
}
