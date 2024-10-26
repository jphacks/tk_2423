using UnityEngine;

class MicAudio : MonoBehaviour {
    [SerializeField] private string m_DeviceName;
    private const int SAMPLE_RATE = 48000;
    private const int RESOLUTION = 1024;  // FFTに使うデータサイズ
    private AudioSource m_MicAudioSource;

    [SerializeField] private LineRenderer m_LineRenderer;
    private readonly Vector3[] m_Positions = new Vector3[RESOLUTION];
    [SerializeField, Range(1, 30000)] private float m_AmpGain = 300;

    private float[] samples = new float[RESOLUTION]; // FFT用サンプルデータ

    private void Awake() {
        m_MicAudioSource = GetComponent<AudioSource>();
    }

    void Start() {
        string targetDevice = "";

        foreach (var device in Microphone.devices) {
            Debug.Log($"Device Name: {device}");
            if (device.Equals(m_DeviceName)) {
                targetDevice = device;
            }
        }

        Debug.Log($"=== Device Set: {targetDevice} ===");
        MicStart(targetDevice);

        // LineRenderer初期化
        for (int i = 0; i < RESOLUTION; i++) {
            var x = 10 * (i / (float)(RESOLUTION / 2) - 1);
            m_Positions[i] = new Vector3(x, 0, 0);
        }

        m_LineRenderer.SetPositions(m_Positions);
    }

    void Update() {
        DrawSpectrum();
    }

    private void DrawSpectrum() {
        if (!m_MicAudioSource.isPlaying) return;

        // マイクから現在の音声データを取得
        m_MicAudioSource.GetOutputData(samples, 0);

        // FFTでスペクトラムデータを取得
        float[] spectrum = new float[RESOLUTION];
        m_MicAudioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

        // LineRendererにスペクトラムデータを描画
        for (int i = 0; i < RESOLUTION / 2; i++) {
            m_Positions[i].y = spectrum[i] * m_AmpGain;
        }

        m_LineRenderer.SetPositions(m_Positions);
    }

    private void MicStart(string device) {
        if (device.Equals("")) return;

        // 1秒以上の録音クリップを作成し、ループ再生で連続データを取得
        m_MicAudioSource.clip = Microphone.Start(device, true, 1, SAMPLE_RATE);

        // マイクデバイスの準備ができるまで待つ
        while (Microphone.GetPosition(device) <= 0) {}

        m_MicAudioSource.Play();
    }
}
