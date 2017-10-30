using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class Intro_PFXController : MonoBehaviour {
    [Header("Profile")]
    public PostProcessingProfile profile;
    [Header("Settings")]
    public AmbientOcclusionModel.Settings ambientOcclusion;
    public DepthOfFieldModel.Settings depthOfField;
    public BloomModel.Settings bloom;
    public ColorGradingModel.Settings colorGrading;
    public ChromaticAberrationModel.Settings chromaticAberration;
    public VignetteModel.Settings vignette;
    [Header("Enable/Disable")]
    public bool ambientOcclusion_e;
    public bool depthOfField_e;
    public bool bloom_e;
    public bool colorGrading_e;
    public bool chromaticAberration_e;
    public bool vignette_e;

    private void Start()
    {
        ambientOcclusion_e = profile.ambientOcclusion.enabled;
        depthOfField_e = profile.depthOfField.enabled;
        bloom_e = profile.bloom.enabled;
        colorGrading_e = profile.colorGrading.enabled;
        chromaticAberration_e = profile.chromaticAberration.enabled;
        vignette_e = profile.vignette.enabled;
    }

    void Update() {
        SetAnim();
	}

    private void SetAnim()
    {
        profile.ambientOcclusion.settings = ambientOcclusion;
        profile.depthOfField.settings = depthOfField;
        profile.bloom.settings = bloom;
        profile.colorGrading.settings = colorGrading;
        profile.chromaticAberration.settings = chromaticAberration;
        profile.vignette.settings = vignette;
        profile.ambientOcclusion.enabled = ambientOcclusion_e;
        profile.bloom.enabled = bloom_e;
        profile.colorGrading.enabled = colorGrading_e;
        profile.chromaticAberration.enabled = chromaticAberration_e;
        profile.vignette.enabled = vignette_e;
    }
}