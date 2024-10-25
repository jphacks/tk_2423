using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static WhisperSpeechToText; 
using static AISpeechManager;
public class SpeechManager : MonoBehaviour
{
    // PC の録音のターゲットになるマイクデバイス名
    // これはお使いのデバイスで変わります
    // 完全一致でないと受け取れないので注意
    string recordingTargetMicDeviceName = "MacBook Airのマイク";
 
    // 録音のターゲットになるマイクデバイス名 "Android audio input"
    string recordingTargetMicDeviceNameForVR = "Android audio input";

    // デバイス
    bool catchedVRMic = false;

    private AISpeechManager AISpeechManager;

    void Start()
    {
        // マイクデバイスを探す
        foreach (string device in Microphone.devices)
        {
 
            // PC 用のマイクデバイスを割り当て
            if (device == recordingTargetMicDeviceName)
            {
                Debug.Log($"{recordingTargetMicDeviceName} searched");
                catchedVRMic = false;
            }
 
            // XREAL Air 用のマイクデバイスを割り当て
            if (device == recordingTargetMicDeviceNameForVR)
            {
                Debug.Log($"{recordingTargetMicDeviceNameForVR} serched");
 
                catchedVRMic = true;
            }
 
        }
        AISpeechManager = GetComponent<AISpeechManager>();
    }
    void Update()
    {
        if (!catchedVRMic)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                Debug.Log("PC で Aボタンを押した");
                AISpeechManager.ToggleRecognition();
            }
        }
        else
        {
            if (OVRInput.GetDown(OVRInput.RawButton.A))
            {
                AISpeechManager.ToggleRecognition();
            }
        }
    }
	public void Record()
	{
	    var whisperSpeechToText = GetComponent<WhisperSpeechToText>();
	    // var recordingstate = GetComponentInChildren<Text>();
	    if (whisperSpeechToText.IsRecording())
	    {
		// recordingstate.text = "録音開始";
		whisperSpeechToText.StopRecording();
	    }
	    else
	    {
		// recordingstate.text = "録音停止";
		whisperSpeechToText.StartRecording();
	    }
	}
}