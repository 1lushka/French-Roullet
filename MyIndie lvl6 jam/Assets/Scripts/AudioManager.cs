using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixer mainMixer;

    public void SetGroupVolume(string groupName, float value)
    {
        if (mainMixer == null)
        {
            Debug.LogWarning("AudioMixer не назначен в AudioManager!");
            return;
        }

        float dB = Mathf.Lerp(-80f, 0f, value);
        mainMixer.SetFloat(groupName, dB);
    }

    public void SetMasterVolume(float value) => SetGroupVolume("Master", value);
    public void SetMusicVolume(float value) => SetGroupVolume("Music", value);
    public void SetSFXVolume(float value) => SetGroupVolume("SFX", value);
}
