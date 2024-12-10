using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class Narrator : MonoBehaviour
{
    TMP_Text textComponent;
    public string CurrentText { get; private set; } = "";
    float characterWrittingDelay = 0.05f;
    float spaceWrittingDelay = 0.1f;
    AudioSource audioSource;

    private void Awake()
    {
        textComponent = GetComponentInChildren<TMP_Text>();
        audioSource = GetComponent<AudioSource>();
    }

    private void FixedUpdate()
    {
        if (textComponent.text != CurrentText)
            textComponent.text = CurrentText;
    }

    public void Say(string text, float delay = 0)
    {
        StartCoroutine(WriteText(text, delay));
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
