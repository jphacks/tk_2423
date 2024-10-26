using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class RealTimeSpectrumAnalyzer : MonoBehaviour
{
    public int spectrumSize = 1024;  // FFTのサイズ
    public float similarityThreshold = 0.8f;  // 類似性の閾値

    private float[] currentSpectrum;
    private float[] hornSpectrum;

    void Start()
    {
        // スペクトルデータの初期化
        currentSpectrum = new float[spectrumSize];
        
        TextAsset jsonFile = Resources.Load<TextAsset>("average_horn_spectrum");

        if (jsonFile != null)
        {
            try
            {
                // JSONを float[] に変換
                hornSpectrum = JsonUtility.FromJson<Wrapper>(jsonFile.text).array;
                Debug.Log("スペクトルデータが正常に読み込まれました");
            }
            catch (Exception ex)
            {
                Debug.LogError($"スペクトルデータの読み込み中にエラーが発生しました: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError("JSONファイルが見つかりません。Resources フォルダに 'average_horn_spectrum.json' を配置してください。");
        }
        // 事前にPythonで計算したクラクションの平均スペクトルを読み込む
        TextAsset hornSpectrumAsset = Resources.Load<TextAsset>("average_horn_spectrum");
        hornSpectrum = JsonUtility.FromJson<float[]>(hornSpectrumAsset.text);
    }
    [Serializable]
    private class Wrapper
    {
        public float[] array;
    }

    void Update()
    {
        // リアルタイムスペクトルを取得
        AudioListener.GetSpectrumData(currentSpectrum, 0, FFTWindow.BlackmanHarris);

        // クラクションと比較
        float similarity = CompareSpectrums(currentSpectrum, hornSpectrum);
        
        if (similarity > similarityThreshold)
        {
            Debug.Log("クラクションの音が検出されました");
            HandleException();
        }
    }

    // スペクトル同士の類似度を計算する関数
    private float CompareSpectrums(float[] spectrumA, float[] spectrumB)
    {
        float sum = 0f;
        for (int i = 0; i < spectrumSize; i++)
        {
            sum += Mathf.Abs(spectrumA[i] - spectrumB[i]);
        }

        // 値を正規化して類似度（0〜1）を返す
        return 1f - (sum / spectrumSize);
    }

    private void HandleException()
    {
        // 例外処理（必要に応じて実装）
        Debug.LogWarning("例外処理が実行されました！");
    }
}
