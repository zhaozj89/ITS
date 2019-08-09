using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using UnityEngine;

public class RhythmInput : MonoBehaviour
{
    public SpeechRecognizer speechRecognizer;

    public static DateTime StartTime { get; set; }

    public static bool firstFlag = true;
    void Start()
    {
        StartCoroutine("BindspeechRecognizer");

        // Listen speed
  
    }

    IEnumerator ActiveListenSpeed()
    {
        if (firstFlag)
        {
            firstFlag = false;
            yield return new WaitForSeconds(1);
        }
        while (true)
        {
            var text = speechRecognizer.GetRealTimeSpeechText();
            if (text != null)
            {
                this.EvaluateSpeed(text);
            }
            else
            {
                gameObject.SendMessage("NoSpeechPresentationUnitError", new PresentationUnitError("nospeech", Config.VerbalPriorityMapping["nospeech"]));
            }
            yield return new WaitForSeconds(Config.DURATION_SPEED);
        }
    }

    // when speech recoginition is ready
    IEnumerator BindspeechRecognizer()
    {
        while (true)
        {
            speechRecognizer = gameObject.GetComponent<SpeechRecognizer>();
            if (speechRecognizer != null && speechRecognizer.py != null)
            {
                StartTime = DateTime.Now;  // begin record
                StartCoroutine("ActiveListenSpeed");
                yield break;
            }
            yield return new WaitForSeconds(Config.DURATION_BINDER);
        }
    }

    // evaluation
    private void EvaluateSpeed(string text)
    {
        if (StartTime == null)
            throw new InvalidOperationException("Start time must be initialized!");

        if (text != null || text.Trim() != "")
        {
            var wordCount = text.Split(' ').Length;
            var duration = (DateTime.Now - StartTime).TotalSeconds;
            var wordPerSecond = wordCount / duration;

            //print(wordPerSecond);
            if (wordPerSecond > Config.THRESHOLD_RHYTHM_MAX_SPEED)
                gameObject.SendMessage("TooFastPresentationUnitError", new PresentationUnitError("toofast", Config.VerbalPriorityMapping["toofast"]));
            else if (wordPerSecond < Config.THRESHOLD_RHYTHM_MIN_SPEED)
                gameObject.SendMessage("TooSlowPresentationUnitError", new PresentationUnitError("tooslow", Config.VerbalPriorityMapping["tooslow"]));
            // speed properly, do nothing
        }
    }
}
