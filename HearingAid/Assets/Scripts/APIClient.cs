using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class APIClient : MonoBehaviour
{
    private string apiUrl = "163.43.142.229:8000";

    // GETリクエスト
    public IEnumerator GetItem(int itemId)
    {
        UnityWebRequest request = UnityWebRequest.Get($"http://{apiUrl}/items/{itemId}");
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

    public IEnumerator GetIsOn()
    {
        UnityWebRequest request = UnityWebRequest.Get($"http://{apiUrl}/isOn");
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

    // POSTリクエスト
    public IEnumerator CreateItem(int id, string name, string description)
    {
        ItemData itemData = new ItemData { id = id, name = name, description = description };
        string json = JsonUtility.ToJson(itemData);

        UnityWebRequest request = new UnityWebRequest($"http://{apiUrl}/items/", "POST");
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

[System.Serializable]
public class ItemData
{
    public int id;
    public string name;
    public string description;
}
