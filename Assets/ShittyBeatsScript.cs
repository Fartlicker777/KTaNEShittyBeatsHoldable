using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class ShittyBeatsScript : MonoBehaviour {
    enum LoopOptions
    {
        NoLoop,
        Loop,
        Shuffle
    }
    enum Status
    {
        Stopped,
        Paused,
        Playing
    }
    public KMAudio Audio;

    public KMSelectable left, right, play, pause, stop, loopOption, volUp, volDown;
    public SpriteRenderer loopDisp;
    public Sprite[] loopOptions;
    public TextMesh number, songTitle, marquee;
    public MeshRenderer[] leds;
    
    public AudioSource audioPlayer;
    public AudioClip[] tracks;
    public Transform record;
    
    private Status currentState = Status.Stopped;
    private LoopOptions currentLoop = LoopOptions.NoLoop;
    private float volume = 5;
    private int[] shuffleOrder;
    private int shufflePointer;
    private const string marqueeMessage = "                      --LISTEN TO SHITTY BEATS TO RELAX / EFM TO TODAY!--   bit.ly/3a1eaeK                       ";
    private const int MARQUEE_LIMIT = 20;
    private const int TITLE_LIMIT = 18;

    private int currentSongIx;

    void Awake () 
    {
        currentSongIx = Rnd.Range(0, tracks.Length);
        audioPlayer.volume = volume / 10;
        Debug.Log("[Shitty Beats Holdable] ACTIVATED");
        left.OnInteract = () => Left();
        right.OnInteract = () => Right();
        play.OnInteract = () => Play();
        pause.OnInteract = () => Pause();
        stop.OnInteract = () => Stop();
        loopOption.OnInteract = () => Loop();
        volUp.OnInteract = () => VolUp();
        volDown.OnInteract = () => VolDown();
    }
    void Start()
    {
        StartCoroutine(ScrollMarquee());
        UpdateVolume();
        UpdateSelected();
    }

    IEnumerator ScrollMarquee()
    {
        while (true)
        {
            for (int i = 0; i < marqueeMessage.Length - MARQUEE_LIMIT; i++)
            {
                marquee.text = marqueeMessage.Substring(i, MARQUEE_LIMIT);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    private void GenericButtonPress(KMSelectable btn)
    {
        btn.AddInteractionPunch(0.3f);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, btn.transform);
    }
    private bool Left()
    {
        GenericButtonPress(left);
        currentSongIx += tracks.Length - 1;
        currentSongIx %= tracks.Length;
        UpdateSelected();
        return false;
    }
    private bool Right()
    {
        GenericButtonPress(right);
        currentSongIx++;
        currentSongIx %= tracks.Length;
        UpdateSelected();
        return false;
    }
    private bool Play()
    {
        GenericButtonPress(play);
        if (currentState == Status.Playing)
            return false;
        if (currentState == Status.Paused)
            audioPlayer.UnPause();
        else audioPlayer.Play();
        currentState = Status.Playing;
        return false;
    }
    private bool Pause()
    {
        GenericButtonPress(pause);
        if (currentState == Status.Paused)
        {
            audioPlayer.UnPause();
            currentState = Status.Playing;
        }
        else if (currentState == Status.Playing)
        {
            audioPlayer.Pause();
            currentState = Status.Paused;
        }
        return false;
    }
    private bool Stop()
    {
        GenericButtonPress(stop);
        if (audioPlayer.isPlaying)
            audioPlayer.Stop();
        currentState = Status.Stopped;
        return false;
    }
    private bool Loop()
    {
        GenericButtonPress(loopOption);
        currentLoop = (LoopOptions)(((int)currentLoop + 1) % 3);
        loopDisp.sprite = loopOptions[(int)currentLoop];
        if (currentLoop == LoopOptions.Shuffle)
            ShuffleQueue();
        return false;
    }
    private void ShuffleQueue()
    {
        currentLoop = LoopOptions.Shuffle;
        shufflePointer = 0;
        shuffleOrder = Enumerable.Range(0, tracks.Length).ToArray().Shuffle();
    }
    private bool VolUp()
    {
        GenericButtonPress(volUp);
        volume++;
        if (volume > 10)
            volume = 10;
        UpdateVolume();
        return false;
    }
    private bool VolDown()
    {
        GenericButtonPress(volDown);
        volume--;
        if (volume < 0)
            volume = 0;
        UpdateVolume();
        return false;
    }

    void UpdateVolume()
    {
        audioPlayer.volume = volume / 10;
        for (int i = 0; i < volume; i++)
            leds[i].material.color = Color.Lerp(new Color32(0x00, 0xEB, 0x08, 0xFF), Color.red, (float)i / 10);
        for (int i = (int)volume; i < 10; i++)
            leds[i].material.color = Color.black;
    }
    void UpdateSelected()
    {
        number.text = (currentSongIx + 1).ToString().PadLeft(2, '0');
        audioPlayer.clip = tracks[currentSongIx];
        string title = audioPlayer.clip.name;
        songTitle.text = FormatPara(title.Split(' '), TITLE_LIMIT);

        bool wasPlaying = false;
        if (currentState == Status.Playing)
        {
            audioPlayer.Stop();
            wasPlaying = true;
        }
        audioPlayer.clip = tracks[currentSongIx];
        if (wasPlaying)
            audioPlayer.Play();
        Debug.LogFormat("[Shitty Beats Holdable] Playing song {0}.", audioPlayer.clip.name);
    }
    void Update()
    {
        if (audioPlayer.isPlaying) 
            record.localRotation *= Quaternion.Euler(0, 100 * Time.deltaTime, 0);
        else if (currentState == Status.Playing)
        {
            if (currentLoop == LoopOptions.Loop)
                audioPlayer.Play();
            else if (currentLoop == LoopOptions.Shuffle)
            {
                currentSongIx = shuffleOrder[shufflePointer++];
                UpdateSelected();
                audioPlayer.Play();
                if (shufflePointer == tracks.Length)
                    ShuffleQueue();
            }
        }
    }

    string FormatPara(string[] input, int letterCount)
    {
        string displayString = string.Empty;
        int ctr = 0;
        foreach (string word in input)
        {
            if (word == "\n" || word == "\n\n")
                ctr = 0;
            else
            {
                ctr += word.Length;
                if (ctr > letterCount)
                {
                    displayString += " \n";
                    ctr = word.Length;
                    displayString += word;
                }
                else
                    displayString += " " + word;
            }
        }
        return displayString.Substring(1, displayString.Length - 1);
    }
}
