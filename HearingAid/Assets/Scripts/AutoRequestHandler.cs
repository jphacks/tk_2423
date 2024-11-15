using System.Collections;
using UnityEngine;

public class AutoRequestHandler : MonoBehaviour
{
    public APIClient apiClient;  // APIClientへの参照を格納
    public float interval = 1f;  // リクエストの間隔（秒）

    private void Start()
    {
        // 一定間隔でGetItemメソッドを呼び出すコルーチンを開始
        StartCoroutine(AutoGetRequest());
    }

    private IEnumerator AutoGetRequest()
    {
        while (true)
        {
            // APIClientのGetItemメソッドを呼び出す
            yield return StartCoroutine(apiClient.GetLatestSign());
            yield return StartCoroutine(apiClient.GetIsOn());
            
            // 指定された間隔だけ待機
            yield return new WaitForSeconds(interval);
        }
    }
}
