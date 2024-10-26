using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;


public class AISpeechManager : MonoBehaviour
{
    SpeechRecognizer recognizer;
    // テキストを表示するUI
    public TMP_Text spokenText;
    
    // 処理が走ってるか走ってないかのフラグ
    private bool recognitionStarted = false;
    private string responseText;

    [SerializeField]
    private string subscriptionKey;

    public async void ToggleRecognition()
    {
        if (recognitionStarted)
        {
            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(true);
            recognitionStarted = false;
            spokenText.text = "Disconnected";
        }
        else
        {
            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
            recognitionStarted = true;
        }
    }

    void Start()
    {
        SpeechConfig config = SpeechConfig.FromSubscription(subscriptionKey, "eastasia");
	// 言語の日本語にするために必要
        config.SpeechRecognitionLanguage = "ja-JP";
        AudioConfig audioConfig = AudioConfig.FromDefaultMicrophoneInput();

        recognizer = new SpeechRecognizer(config, audioConfig);
    
        var message = "Speech recognition started";
        Debug.Log(message);

	// リアルタイムでテキストが返ってくるイベントハンドラー
        recognizer.Recognizing += (s, e) => {
            responseText = e.Result.Text;
            Debug.Log("Recognizing... : " + message);
        };
	
	// 話終わった区切りのタイミングでテキストを返してくれるイベントハンドラー
        recognizer.Recognized += (s, e) => {
            responseText = e.Result.Text;
            Debug.Log("Recognized: " + message);
        };
	
        recognizer.Canceled += (s, e) => {
            responseText = e.ErrorDetails.ToString();
            Debug.Log("Canceled: " + message);
        };
    }

    void Update()
    {
        if (recognitionStarted)
        {
	　　// 処理が走っている時はリアルタイムでテキストをUIに反映させる
            spokenText.text = responseText;
        }
    }
}
