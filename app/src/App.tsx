import React, { useRef, useEffect, useState, useCallback } from "react";
import "./App.css";
import {
  HandLandmarker,
  FilesetResolver,
  NormalizedLandmark,
} from "@mediapipe/tasks-vision";

import axios from "axios";

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

  //文章
  const [finalSentence, setFinalSentence] = useState<string>("");

  // 累積された文字列を内部で保持する
  const accumulatedTextRef = useRef<string>("");

  const lastPredictionTimeRef = useRef<number>(0);
  const lastSignTimeRef = useRef<number>(0);

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
    const canvas = canvasRef.current;
  
    if (video && canvas && handLandmarker) {
      const startTimeMs = performance.now();
      const ctx = canvas.getContext("2d");
  
      if (video.currentTime > 0) {

        video.style.transform = 'scaleX(-1)';
        video.style.transformOrigin = 'center'; // 中心を基準に反転

        const results = await handLandmarker.detectForVideo(video, startTimeMs);
  
        // キャンバスのサイズを動画に合わせて設定
        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;
  
        if (ctx) {
          // キャンバスをクリア
          ctx.clearRect(0, 0, canvas.width, canvas.height);
  
          // 動画フレームを描画
          ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
  
          if (results.landmarks && results.landmarks.length > 0) {
            // 手が検知された場合、ランドマークを描画
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
      }
  
      // 次のフレームをリクエスト
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

  const speakText = (text: string) => {
    const utterance = new SpeechSynthesisUtterance(text);
    window.speechSynthesis.speak(utterance);
  };

  const sameSignCountRef = useRef<number>(0); // 同じ指文字が連続した回数
  const requiredConsecutiveCount = 3; // 確定に必要な連続回数
  
  // 新しいRefを追加
  const lastConfirmedSignRef = useRef<string | null>(null);

  // 累計文字を状態で管理
  const [accumulatedText, setAccumulatedText] = useState<string>("");

  // postNormalizedData関数の中で累計文字を更新
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
      const predictions = result.prediction[0];
      const maxProbability = Math.max(...predictions);
      const maxIndex = predictions.indexOf(maxProbability);

      // クラスと指文字のマッピング
      const signs = [
        "あ",
        "い",
        "う",
        "え",
        "お",
        "か",
        "き",
        "く",
        "け",
        "こ",
        "さ",
        "し",
        "す",
        "せ",
        "そ",
        "た",
        "ち",
        "つ",
        "て",
        "と",
        "な",
        "に",
        "ぬ",
        "ね",
        "は",
        "ひ",
        "ふ",
        "へ",
        "ほ",
        "ま",
        "み",
        "む",
        "め",
        "や",
        "ゆ",
        "よ",
        "ら",
        "る",
        "れ",
        "ろ",
        "わ",
      ];

      if (maxProbability > 0.5) {
        const newSign = signs[maxIndex];

        setPredictedSign({
          sign: newSign,
          probability: maxProbability,
        });

        // 現在の時刻を取得
        const now = performance.now();

        if (newSign === lastSignRef.current) {
          // 同じ指文字が連続して検出された場合、カウントを増加
          sameSignCountRef.current += 1;
        } else {
          // 異なる指文字が検出された場合、カウントをリセット
          sameSignCountRef.current = 1;
          lastSignRef.current = newSign;
        }

        // カウントが指定回数に達した場合、前回確定した指文字と異なる場合のみ累積
        if (
          sameSignCountRef.current >= requiredConsecutiveCount &&
          newSign !== lastConfirmedSignRef.current
        ) {
          accumulatedTextRef.current += newSign;
          console.log(`累積された指文字: ${accumulatedTextRef.current}`);

          // 累計文字を状態に反映
          setAccumulatedText(accumulatedTextRef.current);

          // 最後に確定した指文字を更新
          lastConfirmedSignRef.current = newSign;

          // カウントをリセット
          sameSignCountRef.current = 0;
        }

        // 最終検出時間を更新
        lastSignTimeRef.current = now;
      } else {
        // 指文字が検出されなかった場合、カウントをリセット
        sameSignCountRef.current = 0;
        lastSignRef.current = null;
        lastSignTimeRef.current = performance.now();

        setPredictedSign(null);
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
  
  // 新しいRefを追加
const lastActiveTimeRef = useRef<number>(performance.now());

useEffect(() => {
  const checkInactivity = () => {
    const now = performance.now();
    const timeSinceLastSign = now - lastSignTimeRef.current;
    const timeSinceLastActive = now - lastActiveTimeRef.current;

    if (timeSinceLastSign < 1000) {
      // 入力が続いている場合、最終アクティブ時間を更新
      lastActiveTimeRef.current = now;
    } else if (
      accumulatedTextRef.current.length > 0 &&
      timeSinceLastActive > 1000
    ) {
      // 入力が停止してから一定時間経過した場合のみ送信
      fetchChatGPTResponse(accumulatedTextRef.current);
      accumulatedTextRef.current = "";
    }
  };

  const intervalId = setInterval(checkInactivity, 1000);

  return () => clearInterval(intervalId);
}, []);


  // ChatGPT APIにリクエストを送信する関数
  const fetchChatGPTResponse = async (text: string) => {
    console.log("Sending text to ChatGPT:", text);
    try {
      // 環境変数やサーバーサイドからAPIキーを取得
      const apiKey = import.meta.env.VITE_OPENAI_API_KEY;
      if (!apiKey) {
        throw new Error("OpenAI APIキーが設定されていません。");
      }

      const response = await axios.post(
        "https://api.openai.com/v1/chat/completions",
        {
          model: "gpt-3.5-turbo",
          messages: [
            {
              role: "system",
              content: "あなたは、入力されたひらがなを自然な日本語に変換するアシスタントです。単独の文字の場合、そのままの文字を返します。意味が通じる必要はありません。変換結果以外の情報は一切返さないでください。余計な説明や記号を含めないでください。",
            },
            {
              role: "user",
              content: `${text}`,
            },
          ],
          max_tokens: 20,
          temperature: 0.1,
        },
        {
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${apiKey}`,
          },
        }
      );
      

      const assistantMessage = response.data.choices[0].message.content.trim();
      console.log("ChatGPT response:", assistantMessage);

      // 結果を表示
      setFinalSentence(assistantMessage);

      // 音声で発話
      speakText(assistantMessage);
    } catch (error) {
      console.error("Error fetching ChatGPT response:", error);
    }
  };

  useEffect(() => {
    if (handLandmarker) {
      renderLoop();
    }
  }, [handLandmarker, renderLoop]);

  return (
    <div className="container">
      <h1 className="header">HearU</h1>
  
      <div className="video-canvas-container">
        <video
          ref={videoRef}
          autoPlay
          playsInline
          muted
          style={{ display: "none" }}
        />
        <canvas ref={canvasRef} className="canvas" />
      </div>
  
      <div className="prediction-accumulated-container">
      {/* 予測結果ボックス */}
      <div className="prediction-box">
        <span>予測結果</span>
        <p>
          指文字: {predictedSign ? predictedSign.sign : "認識中..."} <br />
          確率: {predictedSign ? (predictedSign.probability * 100).toFixed(2) + "%" : "0%"}
        </p>
      </div>
  
      {/* 累計文字ボックス */}
      <div className="accumulated-box">
        <span>累計文字</span>
        <p>{accumulatedText || "入力待ち..."}</p>
      </div>
      </div>
  
      {/* 文章ボックス */}
      <div className="final-box">
        <span>単語/文章</span>
        <p>{finalSentence || "生成待ち..."}</p>
      </div>
    </div>
  );
  
  
};

export default App;
