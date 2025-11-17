## CKWY_Server
CKWY_Serverは、.NET 8 + DotNetty + Orleans を基盤としたゲーム向けのActorモデルサーバーフレームワークです。
本プロジェクトは 3 つの独立したサーバープロセスを採用しており、ログイン認証、ネットワークゲートウェイ、ゲームロジックをそれぞれ担当することで、高い拡張性と安定性を実現しています。

### 使用技術
言語　.NET 8 / C#

ネットワーク層　DotNetty（TCP / WebSocket）

分散 Actor モデル　Microsoft Orleans

シリアライゼーション　protobuf

データベース　MongoDB

### プロジェクト構成
```
/ProjectName
 ├── CKWYServer/         # Visual Studio Project
 ├── Common/             # JWT、Luban
 ├── LoginServer/        # 認証・ログインサーバー
 ├── GateServer/         # ネットワークゲートウェイ
 ├── GameServer/         # ゲームロジックサーバー（Orleans）
 ├── Grains/             # Orleans Grain
 ├── IGrains/            # Orleans Grain Interface
 ├── Protobuf/           # プロトコル
 ├── Master/             # ゲームのマスターデータ
 └── README.md
```
### LoginServer
アカウント認証、プレイヤーアカウント作成/取得、トークン発行、リソースダウンロード、GateServer への接続ルーティング

### GateServer
TCP / WebSocket 対応、クライアント接続管理、メッセージのシリアライズと逆シリアライズ、GameServer の Orleans Grain へメッセージ転送

### GameServer
Orleans の Virtual Actor モデル を使用し、スレッド安全で高拡張なゲームロジックを実現、ゲーム内のエンティティ（モンスター、世界状態）を Actor 化、データ保存

### 開発環境

.NET 8

Orleans	3.7.2

DotNetty	0.7.6

### Demo
以下は実際にプレイ可能なゲームのリンクです。
https://www.aoisorastory.com/unity/index.html

クライアントに関する説明は以下のRepositoryを参照。
https://github.com/huangziyang852/CKWY_Client_Copy

現在実装している機能はまだ少ないため、具体的には以下の動画をご参照ください。 https://youtu.be/5S5HEo3sRck
