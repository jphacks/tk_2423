using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using Newtonsoft.Json;

public class APIClient : MonoBehaviour
{
    private string apiUrl = "163.43.242.53:443";
    public TMP_Text signText; // UI Text to display the predicted sign

    public TMP_Text OnOffText; // UI Text to display the status of the device
    public Image OnOffImage; // UI Image to display the status of the device

    private bool isOn = false;

    // 指文字とクラスのマッピング
    private readonly string[] signs = {
        "あ", "い", "う", "え", "お",
        "か", "き", "く", "け", "こ",
        "さ", "し", "す", "せ", "そ",
        "た", "ち", "つ", "て", "と",
        "な", "に", "ぬ", "ね",
        "は", "ひ", "ふ", "へ", "ほ",
        "ま", "み", "む", "め",
        "や", "ゆ", "よ",
        "ら", "る", "れ", "ろ",
        "わ"
    };

    // GETリクエスト
    public IEnumerator GetItem(int itemId)
    {
        UnityWebRequest request = UnityWebRequest.Get($"https://{apiUrl}/items/{itemId}");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }

    public IEnumerator GetLatestSign()
    {
        UnityWebRequest request = UnityWebRequest.Get($"https://{apiUrl}/now_prediction");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + request.downloadHandler.text);
            // レスポンスの処理
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("JSON Response: " + jsonResponse);
            // string jsonResponse = "{\"latest_prediction\": [[1.0,2.0,3.0,4.0,5.0]]}";
            PredictionResponse response = JsonConvert.DeserializeObject<PredictionResponse>(jsonResponse);
            Debug.Log("Latest Prediction: " + response.latest_prediction[0][2]);
            //最新の予測結果が存在するか確認
            if (response.latest_prediction != null && response.latest_prediction.Length > 0 && isOn)
            {
                float[] predictions = response.latest_prediction[0]; // 二重配列で[0]を取得
                Debug.Log("Predictions: " + predictions);
                // 最も高い確率の予測を取得
                float maxProbability = Mathf.Max(predictions);
                int maxIndex = System.Array.IndexOf(predictions, maxProbability);

                if (maxProbability > 0.5f)
                {
                    string predictedSign = signs[maxIndex];
                    signText.text = $"{predictedSign}";
                }
                else
                {
                    signText.text = "△";
                }
            }
            else
            {
                signText.text = "×";
            }
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }

    public IEnumerator GetIsOn()
    {
        UnityWebRequest request = UnityWebRequest.Get($"https://{apiUrl}/isOn");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + request.downloadHandler.text);
            if (request.downloadHandler.text == "true")
            {
                OnOffText.text = "ON";
                OnOffImage.color = Color.green;
                isOn = true;
            }
            else
            {
                OnOffText.text = "OFF";
                OnOffImage.color = Color.red;
                isOn = false;
            }
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }

    // POSTリクエスト
    public IEnumerator CreateItem(int id, string name, string description)
    {
        ItemData itemData = new ItemData { id = id, name = name, description = description };
        string json = JsonUtility.ToJson(itemData);

        UnityWebRequest request = new UnityWebRequest($"https://{apiUrl}/items/", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Item created successfully: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }
}

// APIのレスポンス形式に合わせたクラスを定義
[System.Serializable]
public class PredictionResponse
{
    public float[][] latest_prediction; // APIのJSONレスポンスで対応する構造
}

[System.Serializable]
public class PredictionResponse2
{
    public float [] latest_prediction; // APIのJSONレスポンスで対応する構造
}

[System.Serializable]
public class ItemData
{
    public int id;
    public string name;
    public string description;
}
