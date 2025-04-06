# Automator-Broser で使用可能なコマンド一覧

## web\_\_initChrome

### 概要

Chrome を立ち上げる。

### 引数

- headless(任意): true/false
  Chrome をバックグラウンドで起動するかを指定。

  `true`の場合はバックグラウンドで起動する。
  指定がない場合は、`true`が設定される。

### コマンド例

```
web__initChrome                     # Chromeをバックグラウンドで起動
web__initChrome[headless=false]     # Chromeを起動（ブラウザが表示される）
```

## web\_\_goto

### 概要

指定の URL に遷移する。

### 引数

- url(_必須_):
  遷移先の URL を入力。

### コマンド例

```
web__goto[url=http://www.yahoo.co.jp]   # Yahoo Japanに遷移
```

## web\_\_input

### 概要

input にテキストを入力する。

### 引数

- text(_必須_)

  入力するテキスト

- [要素](#要素について)

  入力するテキストボックスの id や name を指定

  詳細は「要素について」の章を参照してください

### コマンド例

```
# <input class="foo bar">で指定されているテキストボックスの1個目に Hello を入力
web__input[text=Hello&tag=input&class=foo.bar&num=1]

# <input id="foo" name="bar">で指定されているテキストボックスに World を入力
web__input[text=World&tag=input&id=foo&name=bar]
```

## web\_\_click

### 概要

特定の要素に対してマウスクリックを行なう。

### 引数

- [要素](#要素について)

  入力するテキストボックスの id や name を指定

  詳細は「要素について」の章を参照してください

### コマンド例

```
# <button id="submit">BUTTON</button>をクリック
web__click[tag=button&id=submit]
```

## web\_\_scrshot

### 概要

スクリーンショットを取得。

_Selenium の使用上、バックグラウンドで起動しないとページ全体のスクリーンショットが撮れません。_

### 引数

- name(任意)

  取得した画像ファイルのファイル名を指定。

  無い場合は現在日時がファイル名になります。

### コマンド例

```
web__scrshot                  # 現在日時をファイル名としてスクリーンショットを取得
web__scrshot[name=test.png]   # test.pngというファイル名でしてスクリーンショットを取得
```

## web\_\_reset

### 概要

ブラウザを閉じるなどの後処理を行う。

### 引数

なし

### コマンド例

```
web__reset
```

## 要素について

要素の指定に関しては以下の属性で絞り込むことができます。

- tag

  HTML タグ名。`input`や`button`などを指定できる。

- id
- class

  クラス名。複数のクラスで絞り込みたいときは、`.`で連結する。

- type
- name
- num

  複数の要素が見つかった場合に、どの要素を指定するか。

例：

```
# <input class="foo bar">で指定されているテキストボックスの1個目に Hello を入力
web__input[text=Hello&tag=input&class=foo.bar&num=1]

# <input id="foo" name="bar">で指定されているテキストボックスに World を入力
web__input[text=World&tag=input&id=foo&name=bar]
```
