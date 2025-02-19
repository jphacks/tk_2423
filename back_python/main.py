from fastapi import FastAPI, HTTPException, BackgroundTasks
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
import numpy as np
import tensorflow as tf
from typing import List
import time

# TensorFlow Liteモデルの読み込みと準備
MODEL_PATH = "./model/a-nn-ikami-mikubo-fujikawa-model-xy-ver2.tflite"

# TFLiteモデルの読み込み
interpreter = tf.lite.Interpreter(model_path=MODEL_PATH)
interpreter.allocate_tensors()
input_details = interpreter.get_input_details()
output_details = interpreter.get_output_details()

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

# 最新の推論結果を保持するための変数
latest_output_data = None

# 起動状態を表す変数
appIsOn = False

# タイムアウト監視用のタイムスタンプ
last_request_time = None

# 入力データの構造を定義
class HandLandmarks(BaseModel):
    landmark: List[List[float]]  # リストのリスト形式で各ランドマークの[x, y, z]を表現

class SpectrumData(BaseModel):
    sampleRate: int
    spectrum: list[float]  # Python3.9+ の場合: list[float] / それ以前は List[float] など


# タイムアウトを監視し、10秒が経過するとappIsOnをFalseに設定
def monitor_timeout():
    global appIsOn, last_request_time
    while True:
        if appIsOn and (time.time() - last_request_time > 2):
            appIsOn = False
        time.sleep(1)  # 1秒ごとにチェック

# バックグラウンドでタイムアウト監視を開始
import threading
threading.Thread(target=monitor_timeout, daemon=True).start()

# POSTリクエストで推論を行い、最新の結果を保存
@app.post("/predict")
async def predict(hand_data: HandLandmarks, background_tasks: BackgroundTasks):
    global latest_output_data, appIsOn, last_request_time

    appIsOn = True
    last_request_time = time.time()  # リクエストのタイムスタンプを更新

    try:
        # 手のランドマークデータをnumpy配列に変換し、float32型にキャスト
        input_data = np.array(hand_data.landmark, dtype=np.float32)

        # モデルの入力形状に合わせてリシェイプ
        input_data = input_data.reshape(input_details[0]['shape'])

        # 入力データをモデルにセット
        interpreter.set_tensor(input_details[0]['index'], input_data)

        # 推論の実行
        interpreter.invoke()

        # 推論結果の取得
        latest_output_data = interpreter.get_tensor(output_details[0]['index'])

        # 結果をJSON形式で返す
        return {"prediction": latest_output_data.tolist()}
    except Exception as e:
        # エラーが発生した場合はHTTP例外を返す
        raise HTTPException(status_code=500, detail=str(e))

# GETリクエストで最新の推論結果を返すエンドポイント
@app.get("/now_prediction")
async def get_now_prediction():
    if latest_output_data is None:
        return {"message": "No prediction available yet."}
    return {"latest_prediction": latest_output_data.tolist()}

# GETリクエストで起動状態を返すエンドポイント
@app.get("/isOn")
async def read_isOn():
    return appIsOn


@app.post("/fft")
async def classify_fft(data: SpectrumData):
    """
    Unityから送られてきたスペクトラムデータを受け取り、
    TFLiteモデルで推論した結果を返すエンドポイント。
    """
    # 受け取ったスペクトラムをNumPy配列に変換
    input_data = np.array(data.spectrum, dtype=np.float32)

    # モデル入力の形状に合わせてリシェイプ（例: [1, 8192]など）
    # スペクトラム長に合わせて可変にする場合: reshape((1, -1))
    input_data = input_data.reshape((1, -1))

    # TFLiteモデルに入力データをセット
    # interpreter.set_tensor(input_details[0]['index'], input_data)

    # # 推論実行
    # interpreter.invoke()

    # # モデルの出力を取得
    # output_data = interpreter.get_tensor(output_details[0]['index'])  # shape例: [1, num_classes]

    # # 分類モデルの想定であれば argmax を取る
    # predicted_class = np.argmax(output_data, axis=1)[0]

    # 例: 返却データをJSON形式で返す（クラスインデックスを返却）
    return {"predicted_class": 1}

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
