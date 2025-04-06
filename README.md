# Automator-Browser

このプログラムは自動化ツール(Automator)の web 自動化拡張プラグインです。

使用する場合は、dll をダウンロードし、`lib`ファイルは以下に配置してください。

## 概要

このライブラリでは以下の機能を提供します。

- Selenium 各種ライブラリ 及び ChromeDriver のダウンロード
- Chrome の自動操作
  - Chrome 立ち上げ
  - テキスト入力処理
  - クリック処理
  - スクリーンショット取得
  - 終了処理

## インストール方法

[リリースノート](https://github.com/HatoriIchigo/automator-browser/releases)から、dll をダウンロードし、Automator-UI の`lib`フォルダに配置してください。

また、`config/web.ini`を作成してください。
詳細は次節（設定値）を参照。

## 設定値

設定値を`config/web.ini`で保存してください。

設定内容及び設定値は[ドキュメント](./docs/config.md)を参照してください。

## 使用できるコマンド

[ドキュメント](./docs/command.md)を参照してください。
