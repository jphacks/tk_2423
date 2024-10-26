using UnityEngine;
using System.Collections;

public class SoundDetector : MonoBehaviour
{
    private AudioSource audioSource;
    private float[] spectrumData = new float[8];  // FFTデータ用
    private int sampleRate;

    private float currentHertz = 0;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = Microphone.Start(null, true, 1, 44100); // マイクからの録音
        audioSource.loop = true;
        
        while (!(Microphone.GetPosition(null) > 0)) {} // マイクの準備を待つ
        audioSource.Play();
        
        sampleRate = AudioSettings.outputSampleRate;
        Debug.Log("Sample Rate: " + sampleRate);

        StartCoroutine(UpdateSpectrum());
    }

    // void Update()
    // {
    //     // 周波数スペクトルデータを取得
    //     audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.Hamming);

        // string spectrumDataString = string.Join(", ", spectrumData);
        //     Debug.Log("Spectrum Data: [" + spectrumDataString + "]");

    //     // 特定の周波数帯のデータを分析し、閾値を超えているか確認
    //     DetectSpecificSound();
        
    // }

    IEnumerator UpdateSpectrum()
{
    while(true){
        yield return new WaitForSeconds(1);
            // 第一引数に渡す配列に、周波数スペクトルのデータが格納される。
        // FFTのアルゴリズムの特性上、サイズは2のべき乗（2,4,8...1024...)である必要がある。
        float[] spectrum = new float[64];
        audioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

                string spectrumDataString = string.Join(", ", spectrum);
                Debug.Log("Spectrum Data: [" + spectrumDataString + "]");

        // spectrum = new Float[N]の各番地iに格納されるデータspectrum[i]に対応する周波数について
        // spectrum[0]:0Hz, spectrum[N-1]:ナイキスト周波数となっているので、データは、(ナイキスト周波数 / N)Hz 刻みに入っている
        
        // 次に、最も強度の強い周波数成分を求める。まずはデータの番地から
        var max = 0;
        for (int i = 0; i < spectrum.Length; i++)
        {
            var current = spectrum[i];
            if (current > spectrum[max])
            {
                max = i;
            }
        }

        // データのmax番地が最も強度の強い周波数成分である。これを周波数に変換する。
        var nyquistFreq = (float)sampleRate / 2f;
        currentHertz = nyquistFreq * (max / spectrum.Length);
        Debug.Log("Current Hertz: " + currentHertz);
    }
    
}



    private void DetectSpecificSound()
    {
        // クラクションの周波数帯域（例: 300Hz ~ 600Hz）を設定
        int minFrequencyIndex = Mathf.FloorToInt(300 * spectrumData.Length / (sampleRate / 2));
        int maxFrequencyIndex = Mathf.CeilToInt(600 * spectrumData.Length / (sampleRate / 2));

        // 設定した周波数帯域の平均強度を算出
        float averageAmplitude = 0f;
        for (int i = minFrequencyIndex; i <= maxFrequencyIndex; i++)
        {
            averageAmplitude += spectrumData[i];
            //Debug.Log(spectrumData[i]);
        }
        averageAmplitude /= (maxFrequencyIndex - minFrequencyIndex + 1);
        // Debug.Log("Average Amplitude: " + averageAmplitude);
        // 強度が一定以上である場合、特定音を検出したと判断
        if (averageAmplitude > 0.01f)
        {
            Debug.Log("クラクションの音が検出されました！");
        }
    }
}
