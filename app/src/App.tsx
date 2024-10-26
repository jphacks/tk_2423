import React, { useRef, useEffect, useState, useCallback } from "react";
import {
  HandLandmarker,
  FilesetResolver,
  NormalizedLandmark,
} from "@mediapipe/tasks-vision";
import * as tf from "@tensorflow/tfjs";

const App: React.FC = () => {
  const videoRef = useRef<HTMLVideoElement | null>(null);
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const [handLandmarker, setHandLandmarker] = useState<HandLandmarker | null>(
    null
  );

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
            video: true,
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
          // ランドマークデータをCanvasに描画
          drawLandmarks(results.landmarks.flat());
          //consoleに表示
          //   console.log(results.landmarks.flat())
          const normalizedData = normalizeData(results.landmarks.flat());
          postNormalizedData(normalizedData);
        }
      }

      requestAnimationFrame(renderLoop);
    }
  }, [handLandmarker]);

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

  const postNormalizedData = async (data: number[][]) => {
    try {
      const response = await fetch("YOUR_BACKEND_URL", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        throw new Error("Network response was not ok");
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
    <div>
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
      <h1>Hand Landmark Detection</h1>
    </div>
  );
};

export default App;
