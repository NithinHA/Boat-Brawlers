using System;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
	public Sound[] sounds;
	public bool IsMuted;

	protected override void Awake()
	{
		base.Awake();

		foreach(Sound sound in sounds)			// for each sounds that can be played in game, add it to sounds array and set desired properties in the inspector
		{
			sound.source = gameObject.AddComponent<AudioSource>();		// add a new AudioSource component for each sound that can be played in-game
			sound.source.clip = sound.clip;

			sound.source.loop = sound.is_looping;
			sound.source.volume = sound.volume;
			sound.source.pitch = sound.pitch;
		}
	}
	
    public void PlaySound(string name)
	{
		Sound s = Array.Find(sounds, sound => sound.name == name);		// find the sound with the name that matches to the name of sound to be played
		if (s == null)
		{
			Debug.LogWarning("Sound " + s.name + " not found");
			return;
		}
		s.source.Play();			// play the sound
	}

	public void StopSound(string name)
	{
		Sound s = Array.Find(sounds, sound => sound.name == name);      // find the sound with the name that matches to the name of sound to be stopped playing
		if (s == null)
		{
			Debug.LogWarning("Sound " + s.name + " not found");
			return;
		}
		if (!s.source.isPlaying)			// if the sound is not being played, display warning message that sound that is not being played cannot be stopped
		{
			Debug.LogWarning("Sound " + s.name + " is not being played");
			return;
		}
		s.source.Stop();			// stop the sound
	}

	public Sound GetSound(string name)
    {
		return Array.Find(sounds, sound => sound.name == name);
    }

	public void ReplaceSoundClip(string name, AudioClip newClip)
    {
		Sound s = Array.Find(sounds, sound => sound.name == name);
		if(s != null)
        {
			s.clip = newClip;
        }
	}

	public void ToggleSound(bool active)
	{
		IsMuted = !active;
		foreach (Sound sound in sounds)
		{
			sound.source.mute = IsMuted;
		}
	}
}
