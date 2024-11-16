import React, { useRef, useEffect, useState, useCallback } from "react";
import {
  HandLandmarker,
  FilesetResolver,
  NormalizedLandmark,
} from "@mediapipe/tasks-vision";

const App: React.FC = () => {
  const videoRef = useRef<HTMLVideoElement | null>(null);
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const [handLandmarker, setHandLandmarker] = useState<HandLandmarker | null>(
    null
  );

  // 予測結果の状態管理
  const [predictedSign, setPredictedSign] = useState<{
    sign: string;
    probability: number;
  } | null>(null);

  const [word, setWord] = useState<string>("");

  const lastPredictionTimeRef = useRef<number>(0);

  const requestsPerSecond = 2;
  const requestInterval = 1000 / requestsPerSecond;

  const lastSignRef = useRef<string | null>(null); // 前回の指文字を保持

  useEffect(() => {
    const initializeHandLandmarker = async () => {
      // Mediapipe用のWASMファイルのURLを指定
      const vision = await FilesetResolver.forVisionTasks(
        "https://cdn.jsdelivr.net/npm/@mediapipe/tasks-vision@latest/wasm"
      );

      // HandLandmarkerのインスタンスを作成
      const handLandmarkerInstance = await HandLandmarker.createFromOptions(
        vision,
        {
          baseOptions: {
            modelAssetPath: "/hand_landmarker.task", // publicフォルダにhand_landmarker.taskを配置
            delegate: "GPU",
          },
          numHands: 2,
        }
      );

      // 動画モードでHandLandmarkerを設定
      await handLandmarkerInstance.setOptions({ runningMode: "VIDEO" });
      setHandLandmarker(handLandmarkerInstance);
    };

    initializeHandLandmarker();
  }, []);

  useEffect(() => {
    const startCamera = async () => {
      const video = videoRef.current;
      if (video && navigator.mediaDevices) {
        try {
          const stream = await navigator.mediaDevices.getUserMedia({
            video: { facingMode: "environment" },
          });
          video.srcObject = stream;
          video.play();
        } catch (error) {
          console.error("Error accessing the camera:", error);
        }
      }
    };

    startCamera();
  }, []);

  // ランドマークをCanvasに描画する関数
  const drawLandmarks = (landmarks: NormalizedLandmark[]) => {
    const canvas = canvasRef.current;
    const video = videoRef.current;
    if (canvas && video) {
      const ctx = canvas.getContext("2d");
      if (ctx) {
        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;

        ctx.clearRect(0, 0, canvas.width, canvas.height);
        ctx.drawImage(video, 0, 0, canvas.width, canvas.height);

        ctx.fillStyle = "#FF0000";
        ctx.strokeStyle = "#00FF00";
        ctx.lineWidth = 2;

        landmarks.forEach((landmark) => {
          const x = landmark.x * canvas.width;
          const y = landmark.y * canvas.height;
          ctx.beginPath();
          ctx.arc(x, y, 5, 0, 2 * Math.PI);
          ctx.fill();
          ctx.stroke();
        });
      }
    }
  };

  const renderLoop = useCallback(async () => {
    const video = videoRef.current;
    if (video && handLandmarker) {
      const startTimeMs = performance.now();
      if (video.currentTime > 0) {
        const results = await handLandmarker.detectForVideo(video, startTimeMs);

        if (results.landmarks && results.landmarks.length > 0) {
          drawLandmarks(results.landmarks.flat());
          const normalizedData = normalizeData(results.landmarks.flat());

          // リクエスト間隔に基づいてリクエストを送信
          if (
            performance.now() - lastPredictionTimeRef.current >
            requestInterval
          ) {
            lastPredictionTimeRef.current = performance.now();
            postNormalizedData(normalizedData);
          }
        }
      }

      requestAnimationFrame(renderLoop);
    }
  }, [handLandmarker, requestInterval]);

  const normalizeData = (data: NormalizedLandmark[]): number[][] => {
    let x = 0;
    let y = 0;
    let z = 0;
    let max = -1;
    const coordinates: number[][] = [];

    data.forEach((d, i) => {
      if (i === 0) {
        x = d.x;
        y = d.y;
        z = d.z;
      } else {
        const t = (d.x - x) ** 2 + (d.y - y) ** 2 + (d.z - z) ** 2;
        max = Math.max(max, t);
        coordinates.push([d.x - x, d.y - y, d.z - z]);
      }
    });

    // maxが正の値であることを確認し、maxが負の場合はそのまま返す
    if (max <= 0) return coordinates;

    // maxで各座標を正規化
    const normalizedCoordinates = coordinates.map((d) =>
      d.map((v) => v / Math.sqrt(max))
    );

    return normalizedCoordinates;
  };

  const wordDict: Record<string, string> = {
    さき: "先",
    かき: "柿",
    かさ: "傘",
    さけ: "酒",
    あさ: "朝",
    くさ: "草",
    くせ: "癖",
    さお: "竿",
  };

  const speakSign = (sign: string) => {
    const utterance = new SpeechSynthesisUtterance(sign);
    window.speechSynthesis.speak(utterance);
  };

  const postNormalizedData = async (data: number[][]) => {
    try {
      const dataToSend = { landmark: data };
      const response = await fetch("https://tk-2423.onrender.com/predict", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(dataToSend),
      });

      if (!response.ok) {
        throw new Error("Network response was not ok");
      }

      const result = await response.json();

      // 予測結果を処理
      const predictions = result.prediction[0]; // 二重の配列になっている可能性があるため[0]を追加
      const maxProbability = Math.max(...predictions);
      const maxIndex = predictions.indexOf(maxProbability);

      // クラスと指文字のマッピング
      const signs = [
        "あ",
        "い",
        "う",
        "え",
        "お", // 0-4
        "か",
        "き",
        "く",
        "け",
        "こ", // 5-9
        "さ",
        "し",
        "す",
        "せ",
        "そ", // 10-14
        "た",
        "ち",
        "つ",
        "て",
        "と", // 15-19
        "な",
        "に",
        "ぬ",
        "ね", // 20-23
        "は",
        "は", //ははbeta版の二つある
        "ひ",
        "ふ",
        "へ",
        "ほ", // 25-29
        "ま",
        "み",
        "む",
        "め", // 30-33
        "や",
        "ゆ",
        "よ", // 35-37
        "ら",
        "る",
        "れ",
        "ろ", // 38, 40-42
        "わ", // 43
      ]; // クラス数に応じて追加

      // 確率が高い場合のみ表示
      if (maxProbability > 0.5) {
        const newSign = signs[maxIndex];
        setPredictedSign({
          sign: newSign,
          probability: maxProbability,
        });
        // 新しい指文字と前の指文字を結合
        const combinedSign =
          (lastSignRef.current ? lastSignRef.current : "") + newSign;

        // wordDictのキーと一致する場合、setWordを呼び出す
        if (wordDict[combinedSign]) {
          speakSign(wordDict[combinedSign]);
          setWord(wordDict[combinedSign]);
          console.log(word);
        }
        if (newSign !== lastSignRef.current) {
          lastSignRef.current = newSign;
        }
      } else {
        setPredictedSign(null); // 確率が低い場合は非表示
      }
    } catch (error) {
      console.error("Error posting normalized data:", error);
    }
  };

  // // 推論関数の型を定義
  // const infer = async (data: number[][]) => {
  //   try {
  //     // TFLiteモデルのロード
  //     const model = await tf.loadLayersModel("/model/model.json");

  //     console.log(model);

  //     // 入力データをTensorに変換
  //     const inputTensor = tf.tensor(data);

  //     // 推論を実行
  //     const output = model.predict(inputTensor);

  //     // 結果を返す
  //     return output;
  //   } catch (error) {
  //     console.error("推論中にエラーが発生しました:", error);
  //     return undefined;
  //   }
  // };

  useEffect(() => {
    if (handLandmarker) {
      renderLoop();
    }
  }, [handLandmarker, renderLoop]);

  return (
    <div
      style={{ display: "flex", flexDirection: "column", alignItems: "center" }}
    >
      <p
        style={{
          color: "red",
          fontSize: "40px",
          fontWeight: 700,
          position: "fixed",
          top: 0,
        }}
      >
        HearU
      </p>
      {/* カメラ映像を表示するためのvideoタグ */}
      <video
        ref={videoRef}
        autoPlay
        playsInline
        muted
        style={{ display: "none" }}
      />
      {/* ランドマークを描画するためのcanvasタグ */}
      <canvas ref={canvasRef} style={{ width: "100%", height: "auto" }} />

      {/* 予測結果の表示 */}
      {predictedSign && (
        <div>
          <h3>
            予測された指文字: {predictedSign.sign} (確率:{" "}
            {(predictedSign.probability * 100).toFixed(2)}%)
          </h3>
          {/* <p>確率: {(predictedSign.probability * 100).toFixed(2)}%</p> */}
          <h3>単語: {word}</h3>
        </div>
      )}
    </div>
  );
};

export default App;
