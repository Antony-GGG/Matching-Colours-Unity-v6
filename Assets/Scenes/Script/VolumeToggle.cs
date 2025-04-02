using UnityEngine;
using Image = UnityEngine.UI.Image;

public class VolumeToggle : MonoBehaviour
{
    [SerializeField]private Sprite soundMute;
    [SerializeField]private Sprite soundUnmute;
    [SerializeField]private GameObject volumeToggle;
    private bool isOn = true;

    void Start()
    {
        if (!PlayerPrefs.HasKey("VolumeOn"))
        {
            if (isOn)
            {
                PlayerPrefs.SetInt("VolumeOn", 1);
            }
            else
            {
                PlayerPrefs.SetInt("VolumeOn", 0);
            }
        }

        if (PlayerPrefs.GetInt("VolumeOn") == 1)
        {
            foreach (Sound sound in AudioManager.instance.sounds)
            {
                sound.audioSource.mute = false;
            }
            isOn = true;
            volumeToggle.GetComponent<Image>().sprite = soundUnmute;
        }
        else if(PlayerPrefs.GetInt("VolumeOn") == 0)
        {
            foreach (Sound sound in AudioManager.instance.sounds)
            {
                sound.audioSource.mute = true;
            }
            isOn = false;
            volumeToggle.GetComponent<Image>().sprite = soundMute;
        }
    }

    public void ToggleAudio()
    {
        if (isOn)
        {
            isOn = false;
            PlayerPrefs.SetInt("VolumeOn", 0);

            foreach (Sound sound in AudioManager.instance.sounds)
            {
                sound.audioSource.mute = true;
            }

            volumeToggle.GetComponent<Image>().sprite = soundMute;
        }
        else
        {
            isOn = true;
            PlayerPrefs.SetInt("VolumeOn", 1);

            foreach (Sound sound in AudioManager.instance.sounds)
            {
                sound.audioSource.mute = false;
            }

            volumeToggle.GetComponent<Image>().sprite = soundUnmute;
        }
    }
}
