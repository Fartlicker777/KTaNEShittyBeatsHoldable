using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class MagicStopwatchScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;

    public KMSelectable left, right;
    public TextMesh screen;
    public Transform minuteHand, hourHand;

    private double currentSpeed = 1.0;

    void Awake () {
        left.OnInteract += DecreaseSpeed;
        right.OnInteract += IncreaseSpeed;
        Bomb.OnBombSolved += Reset;
        Bomb.OnBombExploded += Reset;
    }

    bool DecreaseSpeed()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, left.transform);
        left.AddInteractionPunch(0.5f);
        currentSpeed -= 0.1;
        if (currentSpeed < 0)
            currentSpeed = 0; //Insurance to make sure currentSpeed is not negative, in which everything fucking breaks.
        SetSpeed();
        return false;
    }
    bool IncreaseSpeed()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, right.transform);
        right.AddInteractionPunch(0.5f);
        currentSpeed += 0.1;
        if (currentSpeed > 9.9)
            currentSpeed = 9.9;
        SetSpeed();
        return false;
    }
    void SetSpeed()
    {
        currentSpeed = Math.Round(currentSpeed, 1);
        Time.timeScale = (float)currentSpeed;
        screen.text = String.Format("{0:0.0}%", currentSpeed);
    }
    void OnDestroy() //Sets the time slow back to normal when the holdable is despawned (detonation, solve, or exit to office)
    { Reset(); }

    void Reset()
    {
        currentSpeed = 1;
        Time.timeScale = 1;
    }
    void Update ()
    {
        minuteHand.localEulerAngles += 10 * Time.deltaTime * (float)currentSpeed * Vector3.forward;
        hourHand.localEulerAngles += 2.5f * Time.deltaTime * (float)currentSpeed * Vector3.forward;
    }
}
