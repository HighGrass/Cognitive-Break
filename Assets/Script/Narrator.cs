using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Narrator : MonoBehaviour
{
    TMP_Text textComponent;
    public string CurrentText { get; private set; } = "";
    float characterWrittingDelay = 0.05f;
    float spaceWrittingDelay = 0.1f;
    AudioSource audioSource;

    public enum FraseType
    {
        None,
        Wait,
    }

    Dictionary<Coroutine, FraseType> Queue = new Dictionary<Coroutine, FraseType>();

    private void Awake()
    {
        textComponent = GetComponentInChildren<TMP_Text>();
        audioSource = GetComponent<AudioSource>();

        StartQueue();
    }

    private void FixedUpdate()
    {
        if (textComponent.text != CurrentText)
            textComponent.text = CurrentText;
    }

    void StartQueue() => StartCoroutine(ManageQueue());

    IEnumerator ManageQueue()
    {
        if (Queue.Count <= 0) // every frame
            yield return null;
    }

    IEnumerator SkipQueue()
    {
        yield break;
    }

    IEnumerator StopQueue()
    {
        yield break;
    }

    public void Say(string text, FraseType type, float delay = 0)
    {
        Coroutine coroutine = StartCoroutine(WriteText(text, delay));
        Queue.Add(coroutine, type);
    }

    public IEnumerator WriteText(string text, float waitTime = 0)
    {
        yield return new WaitForSeconds(waitTime);

        ClearText();
        WaitForSeconds timer = new WaitForSeconds(characterWrittingDelay);

        bool waitingForEndColoring = false;
        string colorString = "";
        bool finishingColoring = false;

        int index = -1;
        foreach (char ch in text)
        {
            index++;

            if (finishingColoring && ch != '>')
                continue;
            else if (ch == '>' && finishingColoring) // finished coloring
            {
                finishingColoring = false;
                CurrentText += "</color>";
                colorString = "";
                continue;
            }

            if (colorString.Length > 0)
            {
                colorString += ch;
                if (ch == '>')
                {
                    CurrentText += colorString;
                    colorString = "";
                    waitingForEndColoring = true;
                }

                continue;
            }

            if (ch == '<')
            {
                if (waitingForEndColoring)
                {
                    finishingColoring = true;
                    continue;
                }
                else
                {
                    colorString += "<";
                }

                continue;
            }

            if (ch == ' ')
                yield return new WaitForSeconds(spaceWrittingDelay);
            else if (ch == ',')
                yield return new WaitForSeconds(spaceWrittingDelay * 2);
            else
                yield return timer;
            CurrentText += ch;
            audioSource.Play();
        }

        yield return new WaitForSeconds(text.Length * 5 / 24);
        if (CurrentText == text)
            ClearText();
        yield break;
    }

    public float SpeechTime(string text)
    {
        float time = 0;
        foreach (char letter in text)
        {
            if (letter == ' ')
                time += spaceWrittingDelay;
            else if (letter == ',')
                time += spaceWrittingDelay * 2;
            else
                time += characterWrittingDelay;
        }
        return time;
    }

    public void ClearText()
    {
        CurrentText = "";
    }
}
