# Automator-Browser の設定値に関するドキュメント

## 設定値の配置場所

設定ファイルは Automator-UI 配下の`config/web.ini`というファイル名で配置してください。

設定ファイルは「設定値名=設定値」の形式で記述します。

## 推奨設定ファイル

以下は推奨設定ファイルです。

```
# selenium関連の設定値
SELENIUM_WEBDRIVER_DOWNLOAD_URL=https://www.nuget.org/api/v2/package/Selenium.WebDriver/4.30.0
SELENIUM_SUPPORT_DOWNLOAD_URL=https://www.nuget.org/api/v2/package/Selenium.Support/4.30.0

# chrome関連の設定値
ENABLE_CHROME=1
CHROME_DIR=C:\Program Files\Google\Chrome\Application
CHROME_DRIVER_URL=https://storage.googleapis.com/chrome-for-testing-public/<VERSION>/win64/chromedriver-win64.zip
CHROME_DRIVER_APP=chromedriver.exe
```

## 各種設定値について

### SELENIUM_WEBDRIVER_DOWNLOAD_URL

この設定値は Web 自動化の際に使用するツール、Selenium の WebDriver のダウンロード先 URL です。

v4.30.0 以外での動作は確認していないため、基本は以下で固定になります。

```
SELENIUM_WEBDRIVER_DOWNLOAD_URL=https://www.nuget.org/api/v2/package/Selenium.WebDriver/4.30.0
```

### SELENIUM_SUPPORT_DOWNLOAD_URL

この設定値は Web 自動化の際に使用するツール、Selenium の Support プログラムのダウンロード先 URL です。

v4.30.0 以外での動作は確認していないため、基本は以下で固定になります。

```
SELENIUM_SUPPORT_DOWNLOAD_URL=https://www.nuget.org/api/v2/package/Selenium.Support/4.30.0
```

### ENABLE_CHROME

この設定値は Web 自動化の際に Chrome を使用できるようにするか、を設定する設定値です。

0 の場合は Chrome を選択できなくなります。（ChromeDriver のインストールをスキップ）

### CHROME_DIR

この設定値は Chrome.exe がダウンロードされているフォルダを指定します。

Chrome のバージョンを取得する(ChromeDriver のバージョンと合わせないといけない)ために使用します。

基本的には`C:\Program Files\Google\Chrome\Application`になりますが、環境によっては別のパスになる可能性があります。

### CHROME_DRIVER_URL

この設定値は ChromeDriver 取得先 URL を指定します。

`<VERSION>`はプログラム内部で置き換えられます。

基本的には以下で固定です。

```
CHROME_DRIVER_URL=https://storage.googleapis.com/chrome-for-testing-public/<VERSION>/win64/chromedriver-win64.zip
```

### CHROME_DRIVER_APP

この設定値は ChromeDriver のファイル名を指定します。

Windows の場合は`chrome.exe`になります。（なお、他の OS では不明です）
