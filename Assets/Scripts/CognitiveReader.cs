//
// Based on quickstart code from Microsoft for Azure Cognitive Services Speech SDK
// Found here: https://github.com/Azure-Samples/cognitive-services-speech-sdk/blob/master/quickstart/csharp/unity/from-microphone/Assets/Scripts/HelloWorld.cs
// and here: https://github.com/Azure-Samples/cognitive-services-speech-sdk/blob/master/quickstart/csharp/unity/text-to-speech/Assets/Scripts/HelloWorld.cs
//

// For Text to Speech
using System;
using System.Threading;

// For Speech to Text and Text to Speech
using UnityEngine;
using UnityEngine.UI;
using Microsoft.CognitiveServices.Speech;

using TMPro;

// For Speech to Text
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif
#if PLATFORM_IOS
using UnityEngine.iOS;
using System.Collections;
#endif

public class CognitiveReader : MonoBehaviour
{
    // For Speech to Text
    // Hook up the two properties below with a Text and Button object in your UI.
    public TMPro.TextMeshProUGUI outputText;
    public TMPro.TextMeshProUGUI recognizedOutputText;
    public Button startRecoButton;

    private object listeningThreadLocker = new object();
    private bool waitingForReco;
    private string heardMessage;

    private bool micPermissionGranted = false;


    // For Speech to Text and Text to Speech
    private const string SpeechSubscriptionKey = "a81c747fbc4f426985562e1171fad456";
    private const string SpeechRegion = "westus";
    private const string RecognitionLanguage = "en-CA";

    // For Text to Speech
    //public Text textToSpeechOutputText;
    public AudioSource audioSource;

    private const int SampleRate = 24000;
    private object speakingThreadLocker = new object();
    private bool waitingForSpeak;
    private bool audioSourceNeedStop;
    private string messageForSpeak;
    private bool ableToSpeak = true;

    private SpeechConfig speakingConfig;
    private SpeechSynthesizer synthesizer;

#if PLATFORM_ANDROID || PLATFORM_IOS
    // Required to manifest microphone permission, cf.
    // https://docs.unity3d.com/Manual/android-manifest.html
    private Microphone mic;
#endif

    // A method to replace the heardMessage with a new message, generally a previously heard message
    public void ReplaceHeardMessage(string newMessage)
    {
        heardMessage = newMessage;
    }

    public async void SpeechToTextButtonClick()
    {
        // Creates an instance of a speech config with specified subscription key and service region.
        // Replace with your own subscription key and service region (e.g., "westus").
        var config = SpeechConfig.FromSubscription(SpeechSubscriptionKey, SpeechRegion);
        config.SpeechRecognitionLanguage = RecognitionLanguage;

        // Make sure to dispose the recognizer after use!
        using (var recognizer = new SpeechRecognizer(config))
        {
            lock (listeningThreadLocker)
            {
                waitingForReco = true;
            }

            // Starts speech recognition, and returns after a single utterance is recognized. The end of a
            // single utterance is determined by listening for silence at the end or until a maximum of 15
            // seconds of audio is processed.  The task returns the recognition text as result.
            // Note: Since RecognizeOnceAsync() returns only a single utterance, it is suitable only for single
            // shot recognition like command or query.
            // For long-running multi-utterance recognition, use StartContinuousRecognitionAsync() instead.
            var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);

            // Checks result.
            string newMessage = string.Empty;
            if (result.Reason == ResultReason.RecognizedSpeech) // Speech was recognized by the SpeechRecognizer.
            {
                newMessage = result.Text;
            }
            else if (result.Reason == ResultReason.NoMatch) // Speech was not recognized.
            {
                newMessage = "NOMATCH: Speech could not be recognized.";
            }
            else if (result.Reason == ResultReason.Canceled) // The speech recognition was cancelled by the user or the recognizer.
            {
                var cancellation = CancellationDetails.FromResult(result);
                newMessage = $"CANCELED: Reason={cancellation.Reason} ErrorDetails={cancellation.ErrorDetails}";
            }

            // Updates the heardMessage, which is what is displayed to the user.
            lock (listeningThreadLocker)
            {
                heardMessage = newMessage;
                waitingForReco = false;
            }
        }
    }

    // Calls TextToSpeechActivate on a string that has been converted into each individual letter and the word itself.
    public void ReadWordInParts(TMP_Dropdown optionMenu)
    {
        string text = optionMenu.options[optionMenu.value].text;
        string word = text + ", ";
        foreach (char letter in text)
        {
            word = word + " " + letter;
        }
        word = word + " " + text;
        TextToSpeechActivate(word);
    }

    // Reads out an option from a drop down menu.
    public void ReadOption(TMP_Dropdown optionMenu)
    {
        string text = optionMenu.options[optionMenu.value].text;
        TextToSpeechActivate(text);
    }

    // Reads out a text field.
    public void SpeakGivenText(Text text)
    {
        TextToSpeechActivate(text.text);
    }

    // Reads out a TMPro text field.
    public void SpeakGivenTMPText(TMPro.TextMeshProUGUI text)
    {
        TextToSpeechActivate(text.text);
    }

    // Reads out a string.
    public void TextToSpeechActivate(string textToSpeak)
    {
        if (!ableToSpeak)
        {
            return;
        }
        
        lock (speakingThreadLocker)
        {
            waitingForSpeak = true;
        }

        string newMessage = null;
        var startTime = DateTime.Now;

        // Starts speech synthesis, and returns once the synthesis is started.
        using (var result = synthesizer.StartSpeakingTextAsync(textToSpeak).Result)
        {
            // Native playback is not supported on Unity yet (currently only supported on Windows/Linux Desktop).
            // Use the Unity API to play audio here as a short term solution.
            // Native playback support will be added in the future release.
            var audioDataStream = AudioDataStream.FromResult(result);
            var isFirstAudioChunk = true;
            // Creates a buffer to hold the audio data.
            var audioClip = AudioClip.Create(
                "Speech",
                SampleRate * 600, // Can speak 10mins audio as maximum
                1,
                SampleRate,
                true,
                (float[] audioChunk) =>
                {
                    var chunkSize = audioChunk.Length;
                    var audioChunkBytes = new byte[chunkSize * 2];
                    var readBytes = audioDataStream.ReadData(audioChunkBytes);
                    if (isFirstAudioChunk && readBytes > 0)
                    {
                        var endTime = DateTime.Now;
                        var latency = endTime.Subtract(startTime).TotalMilliseconds;
                        newMessage = $"Speech synthesis succeeded!\nLatency: {latency} ms.";
                        isFirstAudioChunk = false;
                    }

                    for (int i = 0; i < chunkSize; ++i)
                    {
                        if (i < readBytes / 2)
                        {
                            audioChunk[i] = (short)(audioChunkBytes[i * 2 + 1] << 8 | audioChunkBytes[i * 2]) / 32768.0F;
                        }
                        else
                        {
                            audioChunk[i] = 0.0f;
                        }
                    }

                    if (readBytes == 0)
                    {
                        Thread.Sleep(200); // Leave some time for the audioSource to finish playback
                        audioSourceNeedStop = true;
                    }
                });

            // Play the audio source created from the given text.
            audioSource.clip = audioClip;
            audioSource.Play();
        }

        lock (speakingThreadLocker)
        {
            if (newMessage != null)
            {
                messageForSpeak = newMessage;
            }

            waitingForSpeak = false;
        }
    }

    void Start()
    {
        SpeechToTextStart();
        TextToSpeechStart();
    }

    void Update()
    {
        SpeechToTextUpdate();
        TextToSpeechUpdate();
    }

    // Sets up the speech to text service.
    private void SpeechToTextStart()
    {
        if (recognizedOutputText == null)
        {
            UnityEngine.Debug.LogError("recognizedOutputText property is null! Assign a UI Text element to it.");
        }
        else if (startRecoButton == null)
        {
            heardMessage = "startRecoButton property is null! Assign a UI Button to it.";
            UnityEngine.Debug.LogError(heardMessage);
        }
        else
        {
            // Continue with normal initialization, Text and Button objects are present.
            // PLATFORM_ANDROID is used to check if the application is running on Android and requests microphone access.
            // PLATFORM_IOS is used to check if the application is running on iOS and requests microphone access.
            // This is used to allow the application to run on different platforms without having to change the code.
#if PLATFORM_ANDROID
            // Request to use the microphone, cf.
            // https://docs.unity3d.com/Manual/android-RequestingPermissions.html
            message = "Waiting for mic permission";
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
            }
#elif PLATFORM_IOS
            if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                Application.RequestUserAuthorization(UserAuthorization.Microphone);
            }
#else
            micPermissionGranted = true;
            heardMessage = "Click button to recognize speech";
#endif
            startRecoButton.onClick.AddListener(SpeechToTextButtonClick);
        }
    }

    // Handles the code that needs to be run every update for the speech to text service.
    private void SpeechToTextUpdate()
    {
#if PLATFORM_ANDROID
        if (!micPermissionGranted && Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            micPermissionGranted = true;
            // message = "Click button to recognize speech";
        }
#elif PLATFORM_IOS
        if (!micPermissionGranted && Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            micPermissionGranted = true;
            // message = "Click button to recognize speech";
        }
#endif

        lock (listeningThreadLocker)
        {
            if (startRecoButton != null)
            {
                startRecoButton.interactable = !waitingForReco && micPermissionGranted;
            }
            if (recognizedOutputText != null)
            {
                recognizedOutputText.text = heardMessage;
            }
        }
    }

    // Sets up the text to speech service.
    private void TextToSpeechStart()
    {
        if (outputText == null)
        {
            UnityEngine.Debug.LogError("outputText property is null! Assign a UI Text element to it.");
        }
        else
        {
            // Continue with normal initialization, Text, InputField and Button objects are present.
            messageForSpeak = "Click button to synthesize speech";

            // Creates an instance of a speech config with specified subscription key and service region.
            speakingConfig = SpeechConfig.FromSubscription(SpeechSubscriptionKey, SpeechRegion);

            // The default format is RIFF, which has a riff header.
            // We are playing the audio in memory as audio clip, which doesn't require riff header.
            // So we need to set the format to raw (24KHz for better quality).
            speakingConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Raw24Khz16BitMonoPcm);

            // Creates a speech synthesizer.
            // Make sure to dispose the synthesizer after use!
            synthesizer = new SpeechSynthesizer(speakingConfig, null);

            synthesizer.SynthesisCanceled += (s, e) =>
            {
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(e.Result);
                messageForSpeak = $"CANCELED:\nReason=[{cancellation.Reason}]\nErrorDetails=[{cancellation.ErrorDetails}]\nDid you update the subscription info?";
            };
        }
    }

    // Handles the code that needs to be run every update for the text to speech service.
    // Locks the thread to prevent multiple threads from accessing the same resource.
    // Tells the service if it is waiting for a previous message to be spoken or not.
    // And updates the message that is displayed on a TMPro text object with the result of the speech synthesis.
    // This result is the error message or the latency of the speech synthesis.
    // The latency is the time it takes for the speech synthesis to complete.
    // It also stops a previously started speech synthesis if a new one is started.
    private void TextToSpeechUpdate()
    {
        lock (speakingThreadLocker)
        {
            ableToSpeak = !waitingForSpeak;
            if (outputText != null)
            {
                outputText.text = messageForSpeak;
            }

            if (audioSourceNeedStop)
            {
                audioSource.Stop();
                audioSourceNeedStop = false;
            }
        }
    }
}
