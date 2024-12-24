using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenFXManager : MonoBehaviour
{
    [SerializeField] private Volume globalVolume;
    public static ScreenFXManager Instance;
    private ChromaticAberration _chromaticAberration;
    private bool _runChromaticAberration = false;
    private float _chromaDuration;
    private float _chromaTimer;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        globalVolume.profile.TryGet(out _chromaticAberration);
    }

    // Update is called once per frame
    public void Update()
    {
        if (!_runChromaticAberration) return;
        _chromaTimer -= Time.deltaTime;
        _chromaticAberration.intensity.Override(Mathf.Lerp(
            _chromaticAberration.intensity.value, 0f, Mathf.Abs(_chromaDuration - _chromaTimer)));
        if (!(_chromaTimer <= 0.0f)) return;
        _runChromaticAberration = false;
        _chromaticAberration.intensity.Override(0.0f);
    }

    public void RunChromaticAberration(float duration)
    {
        _chromaDuration = duration > 0.0f ? duration : 0.0f;
        _chromaticAberration.intensity.Override(1.0f);
        _runChromaticAberration = true;
        _chromaTimer = duration;
    }

    public void EnablePlayerDiedEffects()
    {
        globalVolume.profile.TryGet(out ColorAdjustments colorAdjustments);
        colorAdjustments.active = true;
        globalVolume.profile.TryGet(out DepthOfField depthOfField);
        depthOfField.active = true;
    }

    public void DisablePlayerDiedEffects()
    {
        globalVolume.profile.TryGet(out ColorAdjustments colorAdjustments);
        colorAdjustments.active = false;
        globalVolume.profile.TryGet(out DepthOfField depthOfField);
        depthOfField.active = false;
    }
}
