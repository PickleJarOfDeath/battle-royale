using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Rendering.PostProcessing;

public class PlayerCondition : MonoBehaviour
{
    PostProcessVolume grainVolume;
    PostProcessVolume vignetteVolume;
    PostProcessVolume chromVolume;
    Grain grainIntensity;
    Vignette vignetteIntensity;
    ChromaticAberration chromIntensity;
    private PlayerController player;
    public float playerState;

    void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        grainIntensity = ScriptableObject.CreateInstance<Grain>();
        grainIntensity.enabled.Override(true);
        grainIntensity.colored.value = false;
        grainIntensity.intensity.Override(0f);
        grainIntensity.size.value = 3f;
        grainIntensity.lumContrib.Override(1f);
        grainVolume = PostProcessManager.instance.QuickVolume(gameObject.layer, 100f, grainIntensity);

        vignetteIntensity = ScriptableObject.CreateInstance<Vignette>();
        vignetteIntensity.enabled.Override(true);
        vignetteIntensity.mode.Override(VignetteMode.Classic);
        vignetteIntensity.color.Override(Color.red);
        vignetteIntensity.intensity.Override(0f);
        vignetteIntensity.smoothness.Override(1f);
        vignetteIntensity.roundness.Override(1f);
        vignetteVolume = PostProcessManager.instance.QuickVolume(gameObject.layer, 100f, vignetteIntensity);

        chromIntensity = ScriptableObject.CreateInstance<ChromaticAberration>();
        chromIntensity.enabled.Override(true);
        chromIntensity.intensity.Override(0f);
        chromVolume = PostProcessManager.instance.QuickVolume(gameObject.layer, 100f, chromIntensity);
    }

    // Update is called once per frame
    void Update()
    {
        playerState = 1f - (player.curHP * 0.01f);

        if (player.flashingDamage == true)
        {
            grainIntensity.intensity.value = 1f;
            grainIntensity.lumContrib.value = 0;
            vignetteIntensity.intensity.value = 1f;
            chromIntensity.intensity.value = 1f;
        }
        else
        {
            if (playerState != 0f)
            {
                grainIntensity.intensity.value = playerState;
                grainIntensity.lumContrib.value = 1f;
                vignetteIntensity.intensity.value = playerState * 0.625f;
                chromIntensity.intensity.value = playerState;
            }
            else
            {
                grainIntensity.intensity.value = 0f;
                grainIntensity.lumContrib.value = 1f;
                vignetteIntensity.intensity.value = 0f;
                chromIntensity.intensity.value = 0f;
            }
        }
    }
}
