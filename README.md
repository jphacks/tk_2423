# HearU

[![HearU〜みんなの声が届く、伝わる〜のイメージイラスト](https://github.com/user-attachments/assets/162c27f1-9ac2-4245-b49a-8b81de2a99a2)](https://youtu.be/vqLtwue3FlE)

## 製品概要

### 背景(製品開発のきっかけ、課題等）

現在、ろう者を筆頭とした聴覚に障がいを持つ方のコミュニケーションの方法としては、音声を伴わない手話や筆談が主流である。しかし一般に手話を理解し使用することができる人はあまりに少なく、また筆談は紙などの媒体を必要とするため日常生活ではそぐわない場面も多い。

そんな中、近年ソフトバンクが開発を行なっているサービスが [SureTalk](https://www.suretalk.mb.softbank.jp/)である。これはちょうど Zoom 会議のようにユーザーを前面から撮影し、ユーザーが手話を行うと映像認識によってその手話が意味する文章を構築して相手にテキストとして送信するといった形の Web サービスである。このプロダクトでは逆に健常者が音声として発した言葉はテキストに変換されてろう者にも伝わるようにすることで双方向のインタラクションを促進させる狙いがあり、現在は一部の自治体などで利用されているということであった。

しかしこのサービスは手話を自動でテキストに変換するという革新性こそあれど、「前方から上半身を撮影しなければならない」という点で日常生活で利用できない場面が多い。例えば街中を歩いている最中に道を聞きたくなったりした時、いちいち適切な位置にカメラを置いて...というのは現実的でない。すなわち聴覚障がいを持った方々が健常者と全く同じ目線で安心して日常生活を過ごすためには、まだ足りない部分が多いと言える。

### 製品説明（具体的な製品の説明）

前項で述べたような課題を解決するため、私たちは「日常的に使用できるろう者支援システム」の構築を行なった。このシステムは具体的には以下の二つのプロダクトで構成される。

1. **"ユーザー側からの"手話認識** と音声で出力（ろう者 → 健常者）
2. 音声を認識し AR グラスに表示(健常者 → ろう者)

SureTalk にあった問題点は、特に 1 によって解決される。前方からの映像ではなくユーザーが首から下げたスマートフォンの広角カメラから手話を認識することでカメラの配置などは不要になり、いつでもどこでも手話認識をして発話を行うことができる。これは健常者同士の会話における発話と同様に捉えることができ、ろう者は健常者に何事も強制することなくコミュニケーションを成立させられるということである。まさにお互いが「同じ目線で」暮らすことを可能にする。

また健常者からろう者に向けた矢印のコミュニケーションについては、AR グラス（今回は VR HMD で実装）に SpeechtoText でテキスト化した音声をリアルタイム表示させることで円滑な理解を可能にする。またそれに加えて、AR グラスには周囲の音量を表示するメーターや大音量が検知された際の警告、そして車のクラクションを認識して警告を出すようなシステムを追加で実装することにより、ろう者の「安心安全な生活」をサポートするプロダクトとなっている。

### 特長

#### 1. 手前からの手話認識

[![手前から手話をリアルタイムで撮影して認識を行っているアプリの画面](https://github.com/user-attachments/assets/823b33f2-7d05-478b-98eb-faed539e2d55)](https://youtu.be/vqLtwue3FlE)

映像から手話を認識してテキスト形式に変換する研究やプロダクトは数あれど、それらは総じて少し離れた場所から手話話者を撮影することが必須であった。これは手話特有の性質によるものもあるが、「手話は外部から観測するもの」という固定観念に縛られている結果とも言える。本プロダクトでは手話話者の体の側から認識を可能にする独自モデルによって、手話 → テキストの変換を聴覚障がい者が内的に行うことができるという新規性がある。

#### 2. 双方向のコミュニケーションツール

[![聴覚障がい者と健常者双方向の情報伝達の概略図](https://github.com/user-attachments/assets/d12f7f90-6096-436a-884a-88a7f97dfb10)](https://youtu.be/vqLtwue3FlE)

現在聴覚障がい者と手話を知らない一般健常者とのやり取りは、往々にして簡単なジェスチャーか筆談に限られる。それは健常者が伝達ツールとして用いたい「音声」が聴覚障がい者に伝わらず、その一方で聴覚障がい者が用いたい「手話」が一般健常者には伝わらないことが原因であるが、これは円滑なコミュニケーションを行う上で大きな障壁である。私たちの開発したシステムでは二つのプロダクトがそれぞれの方向の情報伝達をサポートすることで、これら二つの問題を同時に解決している。

#### 3. 安全サポート機能

例えば街中で異常に大きな音がした時や車にクラクションを鳴らされた時、健常者は音に反応してそちらを向き状況を確認することができるが、ろう者はそれらの音情報に反応ができない。これは安心安全な日常生活を送る上で大きな問題である。そこで今回主に音声の文章化を行うために用いている HMD のサブ機能として、周囲の音環境を反映したさまざまな UI を視覚的に提供する機能を実装した。音量メーターやスペクトル解析を常時表示することに加え、車のクラクションに対してはスペクトル分析の特徴量から自動的に判定して警告を出すようなシステムを構成している。

### 解決出来ること

ろう者が日常生活を安心して過ごし、誰とでも気兼ねなくコミュニケーションを取ることができるようになる。

### 今後の展望

開発の展望としては、現時点では手話認識に関して一部の指文字のみが実装済みであるため、リッチなコミュニケーションのためにはその幅を広げる作業が必要となる。また都合上 AR グラスが手に入らず VR HMD のパススルー機能で代用しているため、より日常に適合した AR グラスへのデプロイが求められる。

プロダクトの展望としては、手前から手話を認識するアプリはストアにデプロイすることで主に聴覚障害を持った方々から確実に大きな反響を得ることができると考えている。また AR グラスのアプリは HMD の浸透度がまだ低いことからすぐに大きなインパクトを与える事はできないかもしれないが、日常生活支援ツールとして高い完成度を誇っているため、一部の顧客のバーニングニーズにアプローチし支持を得られると考えている。

### 注力したこと（こだわり等）

- 手前からの手話画像データや手話認識モデルが存在しなかったため、自分たちで数百枚の写真を撮影し独自モデルを構築した。
- 首からかけて使用するアプリケーションとメガネ型デバイスの併用という斬新なアイデア。

## 開発技術

### 活用した技術

#### API・データ

- Azure 音声サービス SpeechToText
- 点群データ取得 API MediaPipe
- 数百枚の学習データ (自分たちで撮影)

#### フレームワーク・ライブラリ・モジュール

- Unity
- React(TypeScript)
- FastAPI(Python)
- TensorFlow Lite

#### デバイス

- Meta Quest3S

### 独自技術

#### ハッカソンで開発した独自機能・技術

- 自分たちで撮影した数百枚の手話の写真をMedia Pipeを用いて、骨格を推定した。その骨格座標を用いて手話を分類する学習モデルを独自に開発した。
- クラクション検知 音声をフーリエ変換し、車のクラクションを検出できるようにした。
