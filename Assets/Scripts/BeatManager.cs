using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

public class BeatManager : MonoBehaviour
{
    [SerializeField] public float _bpm;            // bullets per minute bangin game
    [SerializeField] public float _localOffset;    // In the event the song doesnt start exactly on time
    [SerializeField] public float _globalOffset;    // Used for when the user wants to adjust some timings
    [SerializeField] public float _buffer;          // plus or minus on beat time

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] public float _subdivision;
    [SerializeField] private UnityEvent _trigger;

    private float _lastBeat = 0;
    private float sampledTime;

    void Start()
    {
        return;
    }

    // Update is called once per frame
    void Update()
    {
        GetSampledTime();
        CheckForNextBeat(sampledTime);
        return;
    }

    private void GetSampledTime(){
        sampledTime = ((_audioSource.timeSamples + ConvertToSamples(_localOffset)) / ConvertToSamples(GetBeatLength(_bpm)));
    }

    private float GetBeatLength(float bpm){
        /*
        Gets the length of time between each actionable beat (i.e what subdivisions do we want actions to apply to)
        */
        return 60f / (bpm * _subdivision);
    }

    private float ConvertToSamples(float val){
        /*
        Returns a ratio into a number of samples that represents said ratio (1 is 1 full second)
        */
        return val*_audioSource.clip.frequency; 
    }

    public bool IsOnBeat(){
        /*
        Returns True if called on beat, otherwise False
        */
        String s = String.Format("{0}: Sampled Time: {1,5}, Off by: {2,5}",
                        (sampledTime < Mathf.Round(sampledTime)+_buffer & sampledTime > Mathf.Round(sampledTime)-_buffer ? "ON TIME":(sampledTime > Mathf.Round(sampledTime) ? "LATE":"EARLY")),
                         sampledTime,
                         Mathf.Round(sampledTime)-sampledTime);
        Debug.Log(s);

        return (sampledTime < Mathf.Round(sampledTime)+_buffer & sampledTime > Mathf.Round(sampledTime)-_buffer);

        
    }

    private void CheckForNextBeat(float c_beat){
        /*
        Checks for the next beat, and activates the trigger function when the next beat beats
        */
        if (Mathf.FloorToInt(c_beat) != _lastBeat){
            _lastBeat = Mathf.FloorToInt(c_beat);
            _trigger.Invoke();
        }
    }
}
