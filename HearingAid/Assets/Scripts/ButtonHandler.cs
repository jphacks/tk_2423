using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
    public APIClient apiClient;  // APIClientへの参照を格納
    public int itemId = 3;       // 取得するアイテムのID

    private void Start()
    {
        // 必要であれば、ボタンのクリックイベントにメソッドを追加
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    // ボタンがクリックされたときに呼ばれるメソッド
    public void OnButtonClick()
    {
        StartCoroutine(apiClient.GetItem(itemId));
    }
}
