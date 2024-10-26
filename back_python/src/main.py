from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
import numpy as np
import tensorflow as tf
from typing import List

# TensorFlow Liteモデルの読み込みと準備
# モデルのパスを適宜変更してください
MODEL_PATH = "./model/a-so-ikami-mikubo-model.tflite"

# TFLiteモデルの読み込み
interpreter = tf.lite.Interpreter(model_path=MODEL_PATH)
# メモリの確保
interpreter.allocate_tensors()
# 入力層・出力層の情報を取得
input_details = interpreter.get_input_details()
output_details = interpreter.get_output_details()
print(input_details[0]['shape'])
# FastAPIのインスタンスを作成
app = FastAPI()

# CORSの設定を追加
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # 必要に応じて許可するオリジンを指定
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# 入力データの構造を定義
class HandLandmarks(BaseModel):
    landmark: List[List[float]]  # リストのリスト形式で各ランドマークの[x, y, z]を表現


# ルートエンドポイントの追加
@app.get("/")
def read_root():
    return {"message": "Welcome to the Hand Recognition API"}


@app.post("/predict")
def predict(hand_data: HandLandmarks):
    try:
        # OK
        # 手のランドマークデータをnumpy配列に変換し、float32型にキャスト
        input_data = np.array(hand_data.landmark, dtype=np.float32)

        # numpy配列が期待する形状であることを確認
        if input_data.shape != tuple(input_details[0]['shape'][1:]):
            raise ValueError(f"入力データの形状が期待される形状 {tuple(input_details[0]['shape'][1:])} と一致しません。")

        # モデルの入力形状に合わせてリシェイプ
        input_data = input_data.reshape(input_details[0]['shape'])

        print(input_data)

        # 入力データをモデルにセット
        interpreter.set_tensor(input_details[0]['index'], input_data)

        # 推論の実行
        interpreter.invoke()

        # 推論結果の取得
        output_data = interpreter.get_tensor(output_details[0]['index'])

        print

        # 結果をJSON形式で返す
        return {"prediction": output_data.tolist()}
    except Exception as e:
        # エラーが発生した場合はHTTP例外を返す
        raise HTTPException(status_code=500, detail=str(e))
