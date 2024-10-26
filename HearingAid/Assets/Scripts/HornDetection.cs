using System.Collections;
using UnityEngine;
using TensorFlowLite;  // TFLiteプラグインが必要
using System.IO;
public class HornDetection : MonoBehaviour
{
    private Interpreter interpreter;

    [SerializeField]
    private SoundDetector soundDetector;

    private int max_past = 10;
    private int r = 0;
    void Start()
    {
        // TFLiteモデルの読み込み
        var modelPath = Application.streamingAssetsPath + "/car_horn_model.tflite";
        var model = File.ReadAllBytes(modelPath);
        // Optionを設定する
        var options = new InterpreterOptions()
        {
            threads = 2,
            useNNAPI = false,
        };
        if (model == null)
        {
            Debug.LogError("Model file is not loaded");
            return;
        }else{
            Debug.Log("Model file is loaded");
        }

        interpreter = new Interpreter(model,options);
        interpreter.ResizeInputTensor(0, new int[] { 1, 13, 44, 1 });
        interpreter.AllocateTensors();
    }

    public bool Inference(float[] fftData)
    {
        // var inputInfo = interpreter.GetInputTensorInfo(0);
        // Debug.Log("Input Shape: " + string.Join(",", inputInfo.shape));
        // Debug.Log("Input Type: " + inputInfo.type);
        // var melBins = inputInfo.shape[1];
        // var frameLength = inputInfo.shape[2];
        // float[,,,] inputTensor = new float[1, melBins, frameLength, 1];

        // // `mfccData`を `inputTensor` にコピー
        // for (int i = 0; i < melBins; i++)
        // {
        //     for (int j = 0; j < frameLength; j++)
        //     {
        //         inputTensor[0, i, j, 0] = fftData[i * frameLength + j];
        //     }
        // }
        // Debug.Log("Input Tensor: " + string.Join(",", inputTensor));

        // // 入力データのセット
        // interpreter.SetInputTensorData(0, inputTensor);


        // // モデルに入力するデータを設定
        // interpreter.SetInputTensorData(0, fftData);

        // // 推論を実行
        // interpreter.Invoke();

        // // 推論結果を取得
        // float[] output = new float[1];
        // interpreter.GetOutputTensorData(0, output);
        // float prediction = output[0];

        var car_horn = 500;
        var car_horn_index = (car_horn / 44100) * 2048;
        var prediction = 0.0f;
        var fftMean = 0.0f;
        var max = 0;

        for (int i = 0; i < fftData.Length;i++){
            var current = fftData[i];
            if (current > fftData[max])
            {
                max = i;
            }
            fftMean += fftData[i];
        }
        if (max == max_past){
            r++;
        }
        max_past = max;
        if (r > 10 && max > 140 && (fftData[max] / fftMean) > 0.00){
            r = 0;
            return true;
        }
        //Debug.Log("Max: " + max);
        for (int i = 140; i < 265;i++){
            prediction += fftData[(int)car_horn_index * i] /fftMean;
        }
        Debug.Log("Prediction: " + prediction);
        Debug.Log("FFT Mean: " + fftMean);
        // 判定結果の出力
        if (prediction > 1.7 && fftMean > 0.2)
        {
            Debug.Log("車のクラクションが検出されました");
            return true;
        }
        else
        {
            return false;
        }
    }


    // private void OnDestroy()
    // {
    //     interpreter.Dispose();
    // }
}
