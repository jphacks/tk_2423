using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Text;
using TMPro;
 
 
public class ChatGPTPart : MonoBehaviour
{
    
    public GameObject LogText;
    public GameObject SpeechText;
    // TextChat
    private TextMeshProUGUI TextLog;
    private TextMeshProUGUI TextSpeech;
 
    // マイクの開始・終了管理
    bool flagMicRecordStart = false;
 
    // マイクデバイスがキャッチできたかどうか
    bool catchedMicDevice = false;
 
    // 現在録音するマイクデバイス名
    string currentRecordingMicDeviceName = "null";
 
    // PC の録音のターゲットになるマイクデバイス名
    // これはお使いのデバイスで変わります
    // 完全一致でないと受け取れないので注意
    string recordingTargetMicDeviceName = "MacBook Airのマイク";
 
    // 録音のターゲットになるマイクデバイス名 "Android audio input"
    string recordingTargetMicDeviceNameForVR = "Android audio input";
 
    // ヘッダーサイズ
    int HeaderByteSize = 44;
 
    // BitsPerSample
    int BitsPerSample = 16;
 
    // AudioFormat
    int AudioFormat = 1;
 
    // 録音する AudioClip
    AudioClip recordedAudioClip;
 
    // サンプリング周波数
    int samplingFrequency = 44100;
 
    // 最大録音時間[sec]
    int maxTimeSeconds = 10;
 
    // Wav データ
    byte[] dataWav;
 
    // OpenAIAPIKey
    string OpenAIAPIKey = "sk-proj-3pOponqfxtRDMMuQihCiT3BlbkFJZ4PE8g6iCjCMyTYMH8Yi";
 
    void Start()
    {
        catchedMicDevice = false;
 
        TextLog = LogText.GetComponent<TextMeshProUGUI>();
        TextSpeech = SpeechText.GetComponent<TextMeshProUGUI>();
 
        Launch();
    }
 
    void Launch()
    {
 
        // マイクデバイスを探す
        foreach (string device in Microphone.devices)
        {
            Debug.Log($"Mic device name : {device}");
 
            // PC 用のマイクデバイスを割り当て
            if (device == recordingTargetMicDeviceName)
            {
                Debug.Log($"{recordingTargetMicDeviceName} searched");
 
                currentRecordingMicDeviceName = device;
 
                catchedMicDevice = true;
            }
 
            // XREAL Air 用のマイクデバイスを割り当て
            if (device == recordingTargetMicDeviceNameForVR)
            {
                Debug.Log($"{recordingTargetMicDeviceNameForVR} serched");
 
                currentRecordingMicDeviceName = device;
 
                catchedMicDevice = true;
            }
 
        }
 
        if (catchedMicDevice)
        {
            Debug.Log($"マイク捜索成功");
            Debug.Log($"currentRecordingMicDeviceName : {currentRecordingMicDeviceName}");
 
 
            TextLog.text += $"マイク捜索成功\n";
 
 
        }
        else
        {
            Debug.Log($"マイク捜索失敗");
        }
 
    }
 
    void Update()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            Debug.Log("Meta Quest で A ボタンを押した");
 
            OnAppButtonClick();
        }
 
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("PC で Aボタンを押した");
 
            OnAppButtonClick();
        }
    }
 
    void RecordStart()
    {
        // マイクの録音を開始して AudioClip を割り当て
        recordedAudioClip = Microphone.Start(currentRecordingMicDeviceName, false, maxTimeSeconds, samplingFrequency);
    }
 
    void RecordStop()
    {
        // マイクの停止
        Microphone.End(currentRecordingMicDeviceName);
 
        Debug.Log($"WAV データ作成開始");
 
        // using を使ってメモリ開放を自動で行う
        using (MemoryStream currentMemoryStream = new MemoryStream())
        {
            // ChunkID RIFF
            byte[] bufRIFF = Encoding.ASCII.GetBytes("RIFF");
            currentMemoryStream.Write(bufRIFF, 0, bufRIFF.Length);
 
            // ChunkSize
            byte[] bufChunkSize = BitConverter.GetBytes((UInt32)(HeaderByteSize + recordedAudioClip.samples * recordedAudioClip.channels * BitsPerSample / 8));
            currentMemoryStream.Write(bufChunkSize, 0, bufChunkSize.Length);
 
            // Format WAVE
            byte[] bufFormatWAVE = Encoding.ASCII.GetBytes("WAVE");
            currentMemoryStream.Write(bufFormatWAVE, 0, bufFormatWAVE.Length);
 
            // Subchunk1ID fmt
            byte[] bufSubchunk1ID = Encoding.ASCII.GetBytes("fmt ");
            currentMemoryStream.Write(bufSubchunk1ID, 0, bufSubchunk1ID.Length);
 
            // Subchunk1Size (16 for PCM)
            byte[] bufSubchunk1Size = BitConverter.GetBytes((UInt32)16);
            currentMemoryStream.Write(bufSubchunk1Size, 0, bufSubchunk1Size.Length);
 
            // AudioFormat (PCM=1)
            byte[] bufAudioFormat = BitConverter.GetBytes((UInt16)AudioFormat);
            currentMemoryStream.Write(bufAudioFormat, 0, bufAudioFormat.Length);
 
            // NumChannels
            byte[] bufNumChannels = BitConverter.GetBytes((UInt16)recordedAudioClip.channels);
            currentMemoryStream.Write(bufNumChannels, 0, bufNumChannels.Length);
 
            // SampleRate
            byte[] bufSampleRate = BitConverter.GetBytes((UInt32)recordedAudioClip.frequency);
            currentMemoryStream.Write(bufSampleRate, 0, bufSampleRate.Length);
 
            // ByteRate (=SampleRate * NumChannels * BitsPerSample/8)
            byte[] bufByteRate = BitConverter.GetBytes((UInt32)(recordedAudioClip.samples * recordedAudioClip.channels * BitsPerSample / 8));
            currentMemoryStream.Write(bufByteRate, 0, bufByteRate.Length);
 
            // BlockAlign (=NumChannels * BitsPerSample/8)
            byte[] bufBlockAlign = BitConverter.GetBytes((UInt16)(recordedAudioClip.channels * BitsPerSample / 8));
            currentMemoryStream.Write(bufBlockAlign, 0, bufBlockAlign.Length);
 
            // BitsPerSample
            byte[] bufBitsPerSample = BitConverter.GetBytes((UInt16)BitsPerSample);
            currentMemoryStream.Write(bufBitsPerSample, 0, bufBitsPerSample.Length);
 
            // Subchunk2ID data
            byte[] bufSubchunk2ID = Encoding.ASCII.GetBytes("data");
            currentMemoryStream.Write(bufSubchunk2ID, 0, bufSubchunk2ID.Length);
 
            // Subchuk2Size
            byte[] bufSubchuk2Size = BitConverter.GetBytes((UInt32)(recordedAudioClip.samples * recordedAudioClip.channels * BitsPerSample / 8));
            currentMemoryStream.Write(bufSubchuk2Size, 0, bufSubchuk2Size.Length);
 
            // Data
            float[] floatData = new float[recordedAudioClip.samples * recordedAudioClip.channels];
            recordedAudioClip.GetData(floatData, 0);
 
            foreach (float f in floatData)
            {
                byte[] bufData = BitConverter.GetBytes((short)(f * short.MaxValue));
                currentMemoryStream.Write(bufData, 0, bufData.Length);
            }
 
            Debug.Log($"WAV データ作成完了");
 
            dataWav = currentMemoryStream.ToArray();
 
            Debug.Log($"dataWav.Length {dataWav.Length}");
 
            StartCoroutine(PostAPI());
        }
 
    }
 
    public void OnAppButtonClick()
    {
        if (catchedMicDevice)
        {
            if (flagMicRecordStart)
            {
                // Stop
                flagMicRecordStart = false;
                Debug.Log($"Mic Record Stop");
 
 
                RecordStop();
 
            }
            else
            {
                // Start
                flagMicRecordStart = true;
                Debug.Log($"Mic Record Start");
 
                TextLog.text = $"録音スタート...\n";
 
                RecordStart();
            }
        }
 
    }
 
    IEnumerator PostAPI()
    {
        // IMultipartFormSection で multipart/form-data のデータとして送れます
        // https://docs.unity3d.com/ja/2018.4/Manual/UnityWebRequest-SendingForm.html
        // https://docs.unity3d.com/ja/2019.4/ScriptReference/Networking.IMultipartFormSection.html
        // https://docs.unity3d.com/ja/2020.3/ScriptReference/Networking.MultipartFormDataSection.html
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
 
        // https://platform.openai.com/docs/api-reference/audio/createTranscription
        // Whisper モデルを使う
        formData.Add(new MultipartFormDataSection("model", "whisper-1"));
        // 日本語で返答
        formData.Add(new MultipartFormDataSection("language", "ja"));
        // WAV データを入れる
        formData.Add(new MultipartFormFileSection("file", dataWav, "whisper01.wav", "multipart/form-data"));
 
        // HTTP リクエストする(POST メソッド) UnityWebRequest を呼び出し
        // 第 2 引数で上記のフォームデータを割り当てて multipart/form-data のデータとして送ります
        string urlWhisperAPI = "https://api.openai.com/v1/audio/transcriptions";
        UnityWebRequest request = UnityWebRequest.Post(urlWhisperAPI, formData);
 
        // OpenAI 認証は Authorization ヘッダーで Bearer のあとに API トークンを入れる
        request.SetRequestHeader("Authorization", $"Bearer {OpenAIAPIKey}");
 
        // ダウンロード（サーバ→Unity）のハンドラを作成
        request.downloadHandler = new DownloadHandlerBuffer();
 
        Debug.Log("リクエスト開始");
        TextLog.text += $"リクエスト開始...\n";
 
        // リクエスト開始
        yield return request.SendWebRequest();
 
 
        // 結果によって分岐
        switch (request.result)
        {
            case UnityWebRequest.Result.InProgress:
                Debug.Log("リクエスト中");
                break;
 
            case UnityWebRequest.Result.ProtocolError:
                Debug.Log("ProtocolError");
                Debug.Log(request.responseCode);
                Debug.Log(request.error);
                break;
 
            case UnityWebRequest.Result.ConnectionError:
                Debug.Log("ConnectionError");
                break;
 
            case UnityWebRequest.Result.Success:
                Debug.Log("リクエスト成功");
 
                // コンソールに表示
                Debug.Log($"responseData: {request.downloadHandler.text}");
 
                TextSpeech.text += $"{request.downloadHandler.text}";
 
                break;
        }
 
 
    }
}