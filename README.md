# SuperHiyokoOnline    

[ 概要 ]  
---  
~ スーパーヒヨコ。それは足が生えた可愛らしいヒヨコのことである ~  
~ スーパーヒヨコたちは、運動会の競技を経て１位を争わなければならない ~  
~ ちなみに、攻撃や妨害はOK! ~    

本作品は、主にMagicOnionフレームワークを使用して開発したリアルタイム通信のオンラインゲームです。  
リアルではできない運動会をコンセプトに開発しました。  
このゲームは農場を舞台に、スーパーヒヨコたちが運動会を繰り広げる内容となっています。  
プレイヤーはスーパーヒヨコを操作して、競技でポイントを獲得し、最終的にポイントの高さで1位を目指します！  

[ 開発人数 ]  
---  
1人  

[ 開発期間 ]  
---
2024年11月～2025年1月    

[ 開発環境 ]  
---
〇使用技術  
  ・C#  
  ・Unity  
  ・MySQL  
  ・MagicOnion  
  ・EntityFramework  
  ・Sourcetree  
  ・Docker / Docker Desktop  
  ・Azure  
    <使用サービス>  
    ・リソース グループ  
    ・ストレージ  
    ・仮想ネットワーク   
    ・ネットワークセキュリティグループ  
    ・サブネット  
    ・Virtual Machines  
    ・Azure Database for MySQL  
    ・コンテナー レジストリ  
〇使用エディター  
  ・PhpStorm2024.1  

[ サーバー構成 ]
---  
![Image](https://github.com/user-attachments/assets/294d295d-99bf-434d-a2da-0d0f72c21ad6)  

[ フォルダ構成 ]
---  
├ Admin\               # DB環境  
│├ app\                # 管理ツールのソース(未着手)  
│└ db\                 # Laravel用のdocker構成ファイル  
│  
├ Client\              # Unityを使用したクライアントのプロジェクト  
│  
├ Document\            # 通信処理の流れをまとめた、シーケンス図の格納先  
│  
├ Server\              # MagicOnionフレームワークを使用したサーバーのプロジェクト  
│  
├ Shared\              # クライアント・サーバーでインターフェイスを共有するプロジェクト  
│  
└ SharedOutput\        # Sharedプロジェクトの出力先  









