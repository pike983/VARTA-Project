using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

//
// This script handles the background music for the game.
// The script is partially based on the user reply to the question found here:
// https://answers.unity.com/questions/1561486/how-to-get-a-list-of-audioclips-from-a-directory-a.html
//
public class AudioController : MonoBehaviour
{
    private string path;

    public AudioSource audioSource; // Audio Source component
    public AudioClip audioTrack; // Currently playing track

    private List<AudioClip> audioTracks = new List<AudioClip>(); // List of all tracks
    private List<string> audioFiles = new List<string>(); // List of all music files

    private int currentTrack = 0; // Current track index

    public bool pauseMusic = false; // Pause music flag


    // This function shuffles the audioTracks list.
    private void Shuffle()
    {
        List<AudioClip> shuffledList = new List<AudioClip>();

        // Shuffle the audioTracks list by adding to the shuffledList,
        // then reassign the shuffledList to the audioTracks list.
        while (audioTracks.Count > 0)
        {
            int randomIndex = Random.Range(0, audioTracks.Count);
            shuffledList.Add(audioTracks[randomIndex]);
            audioTracks.RemoveAt(randomIndex);
        }

        audioTracks = shuffledList;
    }

    // This function loads all the audio files from the path.
    private void LoadAudioFiles()
    {
        // Get all the files in the path.
        string[] files = Directory.GetFiles(path);

        // Loop through the files and add them to the audioFiles list.
        foreach (string file in files)
        {
            //Debug.Log(file);
            if (file.EndsWith(".mp3") || file.EndsWith(".wav") || file.EndsWith(".ogg"))
            {
                //Debug.Log("Adding file: " + file);
                audioFiles.Add(file);
                // Add the audio file to the audioTracks list.
                if (file.EndsWith(".wav"))
                {
                    audioTracks.Add(new WWW(file).GetAudioClip(false, true, AudioType.WAV));
                }
                else if (file.EndsWith(".mp3"))
                {
                    audioTracks.Add(new WWW(file).GetAudioClip(false, true, AudioType.MPEG));
                }
                else if (file.EndsWith(".ogg"))
                {
                    audioTracks.Add(new WWW(file).GetAudioClip(false, true, AudioType.OGGVORBIS));
                }
            }
        }
    }

    // Plays the next track in the audioTracks list.
    public void PlayNextTrack()
    {
        // If the current track is the last track in the list, reset the current track to 0.
        if (currentTrack == audioTracks.Count - 1)
        {
            currentTrack = 0;
        }
        else
        {
            currentTrack++;
        }

        // Play the next track.
        //Debug.Log("Current Number: " + currentTrack);
        //Debug.Log("Number of Tracks: " + audioTracks.Count);
        audioTrack = audioTracks[currentTrack];
        audioSource.clip = audioTrack;
        audioSource.Play();
    }

    // Checks if the current track is at the end.
    public bool IsTrackAtEnd()
    {
        if (audioSource.time >= audioSource.clip.length)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void PlayPauseMusic()
    {
        pauseMusic = !pauseMusic;
    }

    // Start is called before the first frame update
    void Start()
    {
        path = Application.dataPath + "/Sounds/BackgroundMusic";
        //Debug.Log(path);
        LoadAudioFiles();
        Shuffle();
        PlayNextTrack();
    }

    // Update is called once per frame
    void Update()
    {
        // If the current track is at the end, and not paused, play the next track.
        if (IsTrackAtEnd() && !pauseMusic)
        {
            PlayNextTrack();
        }
        else if (pauseMusic)
        {
            audioSource.Pause();
        }
        else
        {
            audioSource.UnPause();
        }
    }
}
