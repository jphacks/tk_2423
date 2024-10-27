#signClassification
手話の指の位置認識を行い、その位置を学習対象にしてTensorflowのkerasで数値学習しました。Tensorflow Liteとしてモデルを保存しています。
##実行方法
###フォルダ構成
必要なものだけ抜き出しています
.
├──medipipe.ipynb(medipipeで手の点群の教師データを作るコード)
├──RenameReorderImageFiles.ipynb(学習画像のフォルダ名をrenameするコード)
├──Tensorflow_keras.ipynb(Tensorflowでkerasモデルを作るためのコード)
├── AnnotateImagesFlip(手の点群の位置を画像に出力したもの)
├── AnnotatedPoints(手の点群の位置の推定)
├── AnnotatedPointsFlip(左右逆のデータで学習をすることで、フロントでカメラから得たデータを左右反転する手間を省き高速化)
|   (通常は左右逆転させてから学習をする)
├── ImageClassification(yolov5)(今回のモデルにはしなかった)
│   └── yolov5
│       ├── runs
│       │   └── train(できたモデル)
│       │       ├── a-so-epoch10
│       │       │   └── weights
│       │       ├── a-so-epoch50
│       │           └── weights
│       ├── shuwa-a-so-.v2i.yolov5pytorch(学習データ)
│       │   ├── test
│       │   │   ├── images
│       │   │   └── labels
│       │   ├── train
│       │   │   ├── images
│       │   │   └── labels
│       │   └── valid
│       │       ├── images
│       │       └── labels
├── Models(今回作成したモデル)
├── 単語手話(「あ」から「そ」までの手話データ)
│   ├── 三窪
│   └── 伊神
└── 英文字手話(英語の手話データとしてtaと同じ形のものがあったため試験的に使用)