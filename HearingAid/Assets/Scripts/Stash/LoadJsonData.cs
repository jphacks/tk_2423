using UnityEngine;
using System.Collections.Generic;

public class LoadJsonData : MonoBehaviour
{
    private List<int> dataList;

    void Start()
    {
        // JSONファイルをResourcesから読み込む
        Debug.Log("JSONファイルを読み込みます。");
        TextAsset jsonTextFile = Resources.Load<TextAsset>("average_horn_spectrum.json");
        if (jsonTextFile != null)
        {
            // JSONをリストに変換
            dataList = JsonUtility.FromJson<ListWrapper>(jsonTextFile.text).list;
            Debug.Log("読み込んだリスト: " + string.Join(", ", dataList));
        }
        else
        {
            Debug.LogError("data_list.jsonが見つかりません。");
        }
    }

    [System.Serializable]
    public class ListWrapper
    {
        public List<int> list;
    }
}
