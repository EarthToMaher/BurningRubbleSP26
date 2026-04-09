using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    // Here in case I wanna test AudioMixers
    public AudioMixerGroup[] _mixerGroup;

    public Sound[] _sounds;

    public Music[] _BGM;

    public AudioClip[] _randomEngine;

    public Sound _updatePitch;

    // On Awake, foreach sound in _sounds, it will add an AudioSource for each clip it finds with the correct parameters
    // Set within the AudioManager
    void Awake()
    {
        Debug.Log("This array is built");
        foreach (Sound s in _sounds)
        {
            s._source = gameObject.AddComponent<AudioSource>();
            s._source.clip = s._clip;

            s._source.volume = s._volume;
            s._source.pitch = s._pitch;

            s._source.loop = s._loop;
            s._source.playOnAwake = s._playOnAwake;
        }

        foreach (Music m in _BGM)
        {
            m._source = gameObject.AddComponent<AudioSource>();
            m._source.clip = m._clip;

            m._source.volume = m._volume;
            m._source.pitch = m._pitch;

            m._source.loop = m._loop;
            m._source.playOnAwake = m._playOnAwake;
        }
    }

    // Simple SFX script to play SFX's, this does not account for clips currently playing
    public void PlaySFX (string name)
    {
        Sound s = Array.Find(_sounds, sound => sound._name == name);
        if (s == null)
        {
            Debug.Log("Couldn't find Sound: " + name);
            return;
        }
        s._source.Play();
    }

    // Simple SFX script to play SFX's, this *does* account for clips of the **Same Type** currently playing
    public void PlaySFXOnce(string name)
    {
        Sound s = Array.Find(_sounds, sound => sound._name == name);

        if (s == null)
        {
            Debug.Log("Couldn't find Single Sound: " + name);
            return;
        }
        else if (!s._source.isPlaying)
        { 
            s._source.Play();
        }
    }

    // Plays a sustained sound with a min/max pitch. 'Anchor' referrs to the float that is connected by the min/max pitch
    // (i.e., the min/max pitch of the audio is 1 to 3, and the anchor is the speed of a kart.)
    public void PlayLoopSFX(string name, float minPitch, float maxPitch, float anchor)
    {
        Sound s = Array.Find(_sounds, sound => sound._name == name);

        if (s == null) {
            Debug.Log("Couldn't find Sustained Sound: " + name);
        } else
        {
            s._source.pitch = Mathf.Lerp(minPitch, maxPitch, anchor);
            s._source.Play();
        }
    }

    public void UpdatePitch(string name, float minPitch, float maxPitch, float anchor)
    {
        Sound s = Array.Find(_sounds, sound => sound._name == name);
        if (s == null || s._source == null) return;

        anchor = Mathf.Clamp01(anchor);
        float targetPitch = Mathf.Lerp(minPitch, maxPitch, anchor);

        // Smoothly interpolate toward target pitch
        s._source.pitch = Mathf.Lerp(s._source.pitch, targetPitch, Time.deltaTime * 5f);
    }

    public void PlayRandomizedCategory(string categoryName)
    {
        Sound[] matchingSounds = _sounds
            .Where(s => s._name.StartsWith(categoryName, System.StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (matchingSounds.Length == 0)
        {
            Debug.Log("No Sounds located for category '{categoryName}'");
            return;
        }

        int numToPlay = UnityEngine.Random.Range(1, Mathf.Min(4, matchingSounds.Length + 1));

        matchingSounds = matchingSounds.OrderBy(x => UnityEngine.Random.value).Take(numToPlay).ToArray();

        foreach (var sound in matchingSounds)
        {
            if (sound._source == null)
            {
                sound._source = gameObject.AddComponent<AudioSource>();
                sound._source.clip = sound._clip;
                sound._source.loop = true;
                sound._source.volume = sound._volume;
            }
            sound._source.Play();
        }
    }

    public void PlayCategoryOnce(string categoryName)
    {
        Sound[] matchingSounds = _sounds
            .Where(s => s._name.StartsWith(categoryName, System.StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (matchingSounds.Length == 0)
        {
            Debug.Log("No Sounds Located for category '{categoryName}'");
            return;
        }

        Sound randomChosen = matchingSounds[UnityEngine.Random.Range(0, matchingSounds.Length)];

        _updatePitch = randomChosen;
        Debug.Log(randomChosen._name);
        Debug.Log("Random Chosen is: " + randomChosen._source);
        randomChosen._source.Play();
    }

    // Stop functions for audio, should work universally
    public void StopSound(string name)
    {
        Sound s = Array.Find(_sounds, sound => sound._name == name);
        s._source.Stop();
    }

    // Only works with groups of audio (such as PlayRandomizedCategory())
    public void StopCategory(string categoryName)
    {
        foreach (var sound in _sounds.Where(s => s._name.StartsWith(categoryName)))
        {
            sound._source?.Stop();
        }
    }
}