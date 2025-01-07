using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Narrator : MonoBehaviour
{
    TMP_Text textComponent;
    public string CurrentText { get; private set; } = "";
    float characterWrittingDelay = 0.05f;
    float spaceWrittingDelay = 0.1f;
    AudioSource audioSource;
    private bool CanProceedSpeech { get; set; } = true;

    public enum FraseType
    {
        None,
        Wait,
    }

    List<QueueType> Queue = new List<QueueType>();
    Coroutine QueueCoroutine;
    Coroutine SkipQueueCoroutine;
    float Timer = 0;

    class QueueType
    {
        public string Text { get; private set; }
        public FraseType Type { get; }
        public float Delay { get; }

        public QueueType(string text, FraseType type = FraseType.None, float delay = 0f)
        {
            this.Text = text;
            this.Type = type;
            this.Delay = delay;
        }
    }

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

    IEnumerator ManageQueue() // every frame
    {
        while (true)
        {
            if (Timer > 0)
            {
                if (CanProceedSpeech)
                {
                    Timer = 0; // skip timeout
                    Debug.Log("NARRATOR - Speech interrupted");
                    try
                    {
                        if (QueueCoroutine != null)
                            StopCoroutine(QueueCoroutine);
                        if (SkipQueueCoroutine != null)
                            StopCoroutine(SkipQueueCoroutine);
                    }
                    catch (System.Exception)
                    {
                        throw;
                    }
                }

                Timer = Mathf.Clamp(Timer - Time.deltaTime, 0, Mathf.Infinity);
            }
            else
            {
                if (Queue.Count > 0 && CanProceedSpeech)
                {
                    // Queue started
                    StartSpeech(Queue[0]);

                    float waitTime = Queue[0].Text.Length / 24 + 3f;

                    Timer = SpeechTime(Queue[0].Text) + waitTime;

                    if (Queue[0].Type == FraseType.None)
                        SkipQueue(Timer);

                    Queue.RemoveAt(0);
                }
            }
            yield return null;
        }
    }

    void StartSpeech(QueueType queueType)
    {
        StopQueue();
        QueueCoroutine = StartCoroutine(WriteText(queueType.Text, queueType.Delay));
    }

    public void SkipQueue(float delay = 0)
    {
        if (delay <= 0)
        {
            SkipQueueCoroutine = null;
            CanProceedSpeech = true;
        }
        else
        {
            IEnumerator proceedSpeech()
            {
                yield return new WaitForSeconds(delay);
                CanProceedSpeech = true;
                yield break;
            }

            SkipQueueCoroutine = StartCoroutine(proceedSpeech());
        }
        return;
    }

    void StopQueue()
    {
        CanProceedSpeech = false;
        SkipQueueCoroutine = null;
        return;
    }

    public void Say(string text, FraseType type, float delay = 0)
    {
        QueueType speech = new QueueType(text, type, delay);
        Queue.Add(speech);
    }

    public IEnumerator WriteText(string text, float waitTime = 0)
    {
        yield return new WaitForSeconds(waitTime);

        ClearText();
        WaitForSeconds timer = new WaitForSeconds(characterWrittingDelay);

        string infoString = "";

        int index = -1;
        foreach (char ch in text)
        {
            index++;

            if (infoString.Length > 0)
            {
                infoString += ch;
                if (ch == '>')
                {
                    CurrentText += infoString;
                    infoString = "";
                }

                continue;
            }

            if (ch == '<')
            {
                infoString += "<";
                continue;
            }

            if (ch == ' ')
                yield return new WaitForSeconds(spaceWrittingDelay);
            else if (ch == ',')
                yield return new WaitForSeconds(spaceWrittingDelay * 10);
            else
                yield return timer;
            CurrentText += ch;
            audioSource.Play();
        }

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
                time += spaceWrittingDelay * 10;
            else
                time += characterWrittingDelay;
        }
        return time;
    }

    public void ClearText()
    {
        CurrentText = "";
    }

    public void ClearQueue()
    {
        if (QueueCoroutine != null)
            StopCoroutine(QueueCoroutine);
        if (SkipQueueCoroutine != null)
            StopCoroutine(SkipQueueCoroutine);
        ClearText();
        Queue.Clear();
        Timer = 0;
        CanProceedSpeech = true;
    }
}
