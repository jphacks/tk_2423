using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshPro))]
public class TextMeshProBackground : MonoBehaviour
{
    public float PaddingTop;
    public float PaddingBottom;
    public float PaddingLeft;
    public float PaddingRight;
    public Material material;

    private GameObject Background;
    private TextMeshPro textMeshPro;
    private Vector2 lastTextSize;

    void Start()
    {
        this.textMeshPro = GetComponent<TextMeshPro>();

        // 背景の初期設定
        this.Background = GameObject.CreatePrimitive(PrimitiveType.Plane);
        this.Background.name = "background";
        this.Background.transform.Rotate(-90, 0, 0);
        this.Background.transform.SetParent(this.transform, false); // 親のTransformに基づいて配置
        if(material != null)
            this.Background.GetComponent<MeshRenderer>().material = material;

        StartCoroutine(UpdateBackgroundSize());
    }

    IEnumerator UpdateBackgroundSize()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f); // パフォーマンス向上のため少し間隔を開ける

            // テキストが空の場合は背景を非表示にする
            if (string.IsNullOrEmpty(textMeshPro.text))
            {
                Background.SetActive(false);
                continue; // 次のループへ
            }
            else
            {
                Background.SetActive(true);
            }

            // 描画サイズを取得
            Vector2 currentSize = textMeshPro.GetRenderedValues(false);

            // サイズが変わった場合のみ更新
            if (currentSize != lastTextSize)
            {
                lastTextSize = currentSize;
                UpdateBackgroundTransform(currentSize);
            }
        }
    }

    void UpdateBackgroundTransform(Vector2 textSize)
    {
        // 描画位置の計算
        var pos = textMeshPro.textBounds.center;
        //Debug.Log(pos);
        var hoseiX = -(PaddingLeft / 2) + (PaddingRight / 2);
        var hoseiY = -(PaddingBottom / 2) + (PaddingTop / 2);
        var hoseiZ = 0.01f;
        this.Background.transform.localPosition = new Vector3(pos.x + hoseiX, pos.y + hoseiY, pos.z + hoseiZ);

        // 描画サイズの計算
        var scaleX = (textSize.x / 10) + (PaddingLeft + PaddingRight) / 10;
        var scaleZ = (textSize.y / 10) + (PaddingTop + PaddingBottom) / 10;
        this.Background.transform.localScale = new Vector3(scaleX, 1, scaleZ);
    }
}
