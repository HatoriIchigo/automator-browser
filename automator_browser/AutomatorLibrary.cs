using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using automator_baselib;
using OpenQA.Selenium;
using OpenQA.Selenium.BiDi.Modules.BrowsingContext;
using OpenQA.Selenium.Chrome;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace automator_browser
{
    public class AutomatorLibrary : PluginBase
    {
        private string webDriverFilePath = Path.Combine(Directory.GetCurrentDirectory(), "opt", "selenium-webdriver", "lib", "net8.0", "WebDriver.dll");
        private string supportFilePath = Path.Combine(Directory.GetCurrentDirectory(), "opt", "selenium-support", "lib", "netstandard2.0", "WebDriver.Support.dll");

        private IWebDriver? driver;


        public override IEnumerable<string> Init()
        {
            string err;

            // 識別子の設定，初期化
            identifier = "web";
            foreach (string e in base.Init())
            {
                if (e != "")
                {
                    yield return e;
                    yield break;
                }
            }
            yield return new Output("D-web-0004", "親クラスの初期化に成功", "", "").toString();

            // Seleniumドライバーのインストール
            err = DownloadWebDriver();
            if (err != "")
            {
                yield return err;
                yield break;
            }
            yield return new Output("D-web-0005", "Selenium WebDriverダウンロード処理完了", "", "").toString();
            err = DownloadSupport();
            if (err != "")
            {
                yield return err; yield break;
            }
            yield return new Output("D-web-0006", "Selenium Supportダウンロード処理完了", "", "").toString();

            // seleniumドライバ動的読み込み
            err = "";
            try
            {
                Assembly.LoadFrom(webDriverFilePath);
                Assembly.LoadFrom(supportFilePath);
            }
            catch (Exception e) {
                err = new Output("E-web-0001", "Seleniumドライバの読み込みに失敗しました。[" + e.Message + "]", "", "").ToString();
            }
            if (err != "") { yield return err; yield break; }
            yield return new Output("D-web-0007", "Selenium dll読み込み完了", "", "").toString();

            // 各種webドライバーのインストール
            err = SettingChromeDriver();
            if (err != "") { yield return err; yield break; }
            yield return new Output("D-web-0008", "ChromeDriver設定完了", "", "").toString();


            // アクション定義
            this.actionItems.Clear();

            this.actionItems.Add("initChrome", new ActionItem("initChrome", "initChrome",
                (pattern) => { return true; },
                web__initChrome, ""));
            this.actionItems.Add("goto", new ActionItem("goto", "",
                (pattern) => { return true; },
                web__goto, ""));
            this.actionItems.Add("input", new ActionItem("input", "",
                (pattern) => { return true; },
                web__input, ""));
            this.actionItems.Add("click", new ActionItem("click", "",
                (pattern) => { return true; },
                web__click, ""));
            this.actionItems.Add("scrshot", new ActionItem("scrshot", "",
                (pattern) => { return true; },
                web__scrshot, ""));
            this.actionItems.Add("getText", new ActionItem("getText", "",
                (pattern) => { return true; },
                web__getText, ""));
            this.actionItems.Add("reset", new ActionItem("reset", "",
                (pattern) => { return true; },
                web__reset, ""));

            yield return new Output("I-web-0009", "automator_browserの初期設定が完了しました。", "", "").toString();
        }

        public override IEnumerable<string> CheckCmd(string cmd) { yield return "";  }

        public override IEnumerable<string> Action(string input, string cmd)
        {
            foreach (string log in base.Action(input, cmd))
            {
                yield return log;
            }
        }

        public override IEnumerable<string> Reset()
        {
            foreach (String log in web__reset("", ""))
            {
                yield return log;
            }
        }

        /*
         * 
         * 初期化の段階で必要になる関数
         * 
         */

        private string SettingChromeDriver()
        {
            // Chrome Driverのダウンロード用関数
            void DownloadAndExtractChromeDriver(string version)
            {
                // ディレクトリ削除
                try { Directory.Delete(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "opt", "chromedriver"), true); } catch { }

                // url生成
                string url = properties["CHROME_DRIVER_URL"].Replace("<VERSION>", version);
                string zipPath = Path.Combine(Directory.GetCurrentDirectory(), "opt", "chromedriver.zip");

                // ダウンロード
                using WebClient client = new WebClient();
                client.DownloadFile(url, zipPath);

                // zip展開
                ZipFile.ExtractToDirectory(zipPath, Path.Combine(Directory.GetCurrentDirectory(), "opt"), true);
                File.Delete(zipPath);

                // フォルダ名chromedriverに修正
                string[] directories = Directory.GetDirectories(Path.Combine(Directory.GetCurrentDirectory(), "opt"));

                foreach (string directoryPath in directories)
                {
                    string directoryName = Path.GetFileName(directoryPath);
                    if (directoryName.StartsWith("chromedriver"))
                    {
                        string newDirectoryName = "chromedriver";
                        string newDirectoryPath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "opt", directoryName), Path.Combine(Directory.GetCurrentDirectory(), "opt", newDirectoryName));
                        try
                        {
                            Directory.Move(directoryPath, newDirectoryPath);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"ディレクトリ名の変更に失敗しました: {ex.Message}");
                        }
                    }
                }
            }

            // メイン
            if (!properties.ContainsKey("ENABLE_CHROME") || properties["ENABLE_CHROME"] == "0")
            {
                return "";
            }

            // 必要に応じてChrome Driverのインストール
            if (properties.ContainsKey("CHROME_DIR") && properties.ContainsKey("CHROME_DRIVER_URL"))
            {
                // chromeのバージョンを取得
                string chromePath = Path.Combine(properties["CHROME_DIR"], "chrome.exe");
                string? chromeVersion = FileVersionInfo.GetVersionInfo(chromePath).ProductVersion;
                if (chromeVersion == null || chromeVersion == "")
                {
                    return new Output("E-web-0033", "Chromeのバージョンが取得できませんでした。", "", "").toString();
                }

                // chrome driverのバージョン確認
                string chromeDriverPath = Path.Combine(Directory.GetCurrentDirectory(), "opt", "chromedriver", "chromedriver.exe");
                if (File.Exists(chromeDriverPath))
                {
                    string? chromeDriverVersion = FileVersionInfo.GetVersionInfo(chromeDriverPath).ProductVersion;
                    if (chromeDriverVersion == null || chromeDriverVersion == "")
                    {
                        return new Output("E-web-0034", "ChromeDriverのバージョンが取得できませんでした。", "", "").toString();
                    }
                    if (chromeVersion != chromeDriverVersion)
                    {
                        // chrome driver ダウンロード
                        DownloadAndExtractChromeDriver(chromeVersion);
                    }
                }
                else
                {
                    // chrome driver ダウンロード
                    DownloadAndExtractChromeDriver(chromeVersion);
                }
            }
            else
            { return new Output("E-web-0034", "設定値[CHROME_DIR]が設定されていません。", "", "").toString(); }
            return "";
        }

        private string DownloadWebDriver()
        {
            // キーチェック
            if (!properties.ContainsKey("SELENIUM_WEBDRIVER_DOWNLOAD_URL"))
            {
                return new Output("E-web-0035", "設定値[SELENIUM_WEBDRIVER_DOWNLOAD_URL]が設定されていません。", "", "").toString();
            }

            // driverチェック
            if (!File.Exists(this.webDriverFilePath))
            {
                try
                {
                    // url生成
                    string url = properties["SELENIUM_WEBDRIVER_DOWNLOAD_URL"];
                    Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "opt", "selenium-webdriver"));
                    string zipPath = Path.Combine(Directory.GetCurrentDirectory(), "opt", "selenium-webdriver", "webdriver.zip");

                    // ダウンロード
                    using WebClient client = new WebClient();
                    client.DownloadFile(url, zipPath);

                    // zip展開
                    ZipFile.ExtractToDirectory(zipPath, Path.Combine(Directory.GetCurrentDirectory(), "opt", "selenium-webdriver"), true);
                    File.Delete(zipPath);
                }
                catch (Exception e)
                {
                    return new Output("E-web-0036", "Selenium WebDriverのダウンロードに失敗しました。", "", "").toString();
                }
            }

            return "";
        }

        private string DownloadSupport()
        {
            // キーチェック
            if (!properties.ContainsKey("SELENIUM_SUPPORT_DOWNLOAD_URL"))
            {
                return new Output("E-web-0037", "設定値[SELENIUM_SUPPORT_DOWNLOAD_URL]が設定されていません。", "", "").toString();
            }

            // driverチェック
            if (!File.Exists(this.supportFilePath))
            {
                try
                {
                    // url生成
                    string url = properties["SELENIUM_SUPPORT_DOWNLOAD_URL"];
                    Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "opt", "selenium-support"));
                    string zipPath = Path.Combine(Directory.GetCurrentDirectory(), "opt", "selenium-support", "support.zip");

                    // ダウンロード
                    using WebClient client = new WebClient();
                    client.DownloadFile(url, zipPath);

                    // zip展開
                    ZipFile.ExtractToDirectory(zipPath, Path.Combine(Directory.GetCurrentDirectory(), "opt", "selenium-support"), true);
                    File.Delete(zipPath);
                }
                catch (Exception e)
                {
                    return new Output("E-web-0038", "Selenium Supportのダウンロードに失敗しました。", "", "").toString();
                }
            }

            return "";
        }

        /*
         * 
         * Main
         * 
         */

        private IEnumerable<string> web__initChrome(string input, string cmd)
        {
            // 設定値確認
            if (!properties.ContainsKey("ENABLE_CHROME") && properties["ENABLE_CHROME"] != "1")
            {
                yield return new Output("E-web-0010", "Chromeが使用できない設定になっています。", "", "").toString();
                yield break;
            }
            if (!properties.ContainsKey("CHROME_DRIVER_APP"))
            {
                yield return new Output("E-web-0011", "設定値が未設定です。: CHROME_DRIVER_APP", "", "").toString();
                yield break;
            }

            // Chrome Driverの確認
            string chromedriverPath = Path.Combine(Directory.GetCurrentDirectory(), "opt", "chromedriver", properties["CHROME_DRIVER_APP"]);
            if (!Path.Exists(chromedriverPath))
            {
                yield return new Output("E-web-0012", "chromedriverが見つかりませんでした。: " + chromedriverPath, "", "").toString();
                yield break;
            }

            // 引数取得，チェック
            Dictionary<string, string>? args = new Dictionary<string, string>();
            string err = ParseCmd(args, cmd, "initChrome");
            if (err != "")
            {
                yield return new Output("E-web-0014", "コマンド分解中にエラーが発生しました。[initChrome]", "", "").toString();
                yield break;
            }
            if (args.ContainsKey("headless"))
            {
                if (args["headless"] == "true" || args["headless"] == "false")
                {
                    yield return new Output("D-web-0030", "headlessを" + args["headless"] + "で起動します。", "", "").toString();
                }
                else
                {
                    yield return new Output("E-web-0029", "headlessの値がtrueかfalseであることを確認してください。[headless=" + args["headless"] + "]", "", "").toString();
                    yield break;
                }
            }
            else
            {
                args["headless"] = "true";
                yield return new Output("D-web-0030", "headlessをtrueで起動します。", "", "").toString();
            }

            // 起動
            err = "";
            try
            {
                ChromeOptions options = new ChromeOptions();
                if (args["headless"] == "true") { options.AddArgument("--headless"); }
                this.driver = new ChromeDriver(chromedriverPath, options);
                this.driver.Manage().Window.Maximize();
            }
            catch (Exception e)
            {
                err = new Output("E-web-0028", "Chromeの立ち上げに失敗しました。", "Bool", "false").toString();
            }

            if (err != "") { yield return err; }
            else { yield return new Output("D-web-0013", "Chromeの立ち上げに成功", "Bool", "true").toString(); }

        }

        private IEnumerable<string> web__goto(string input, string cmd)
        {
            // 引数取得，チェック
            Dictionary<string, string>? args = new Dictionary<string, string>();
            string err = ParseCmd(args, cmd, "goto");
            if (err != "")
            {
                yield return new Output("E-web-0014", "コマンド分解中にエラーが発生しました。[goto]", "", "").toString();
                yield break;
            }
            if (!args.ContainsKey("url"))
            {
                yield return new Output("E-web-0015", "引数urlが見つかりませんでした。[goto]", "", "").toString();
                yield break;
            }
            if (this.driver == null)
            {
                yield return new Output("E-web-0027", "Driverが設定されていません。[goto]", "", "").toString();
                yield break;
            }

            // 遷移
            err = "";
            try
            {
                this.driver.Navigate().GoToUrl(args["url"]);
            }
            catch (Exception e)
            {
                err = new Output("E-web-0016", "遷移に失敗しました。: " + e.Message, "", "").toString();
            }

            if (err == "")
            {
                yield return new Output("I-web-0017", "遷移に成功しました。", "", "").toString();
            }
            else
            {
                yield return err;
            }
        }

        private IEnumerable<string> web__input(string input, string cmd)
        {
            // 引数取得，チェック
            Dictionary<string, string>? args = new Dictionary<string, string>();
            string err = ParseCmd(args, cmd, "input");

            if (err != "")
            {
                yield return new Output("E-web-0014", "コマンド分解中にエラーが発生しました。[input]", "", "").toString();
                yield break;
            }
            if (!args.ContainsKey("text"))
            {
                yield return new Output("E-web-0015", "引数textが見つかりませんでした。[input]", "", "").toString();
                yield break;
            }
            if (this.driver == null)
            {
                yield return new Output("E-web-0027", "Driverが設定されていません。[input]", "", "").toString();
                yield break;
            }

            // 入力
            err = "";
            try
            {
                IWebElement element = getElement(args);
                element.SendKeys(args["text"]);
            }
            catch (Exception e)
            {
                err = new Output("E-web-0018", "入力操作に失敗しました。" + e.Message, "", "").toString();
            }
            if (err == "")
            {
                yield return new Output("I-web-0019", "入力操作に成功しました。", "", "").toString();
            }
            else
            {
                yield return err;
            }
        }

        private IEnumerable<string> web__click(string input, string cmd)
        {
            // 引数取得，チェック
            Dictionary<string, string>? args = new Dictionary<string, string>();
            string err = ParseCmd(args, cmd, "click");

            if (err != "")
            {
                yield return new Output("E-web-0014", "コマンド分解中にエラーが発生しました。[click]", "", "").toString();
                yield break;
            }
            if (this.driver == null)
            {
                yield return new Output("E-web-0027", "Driverが設定されていません。[click]", "", "").toString();
                yield break;
            }

            // クリック
            err = "";
            try
            {
                IWebElement element = getElement(args);
                element.Click();
            }
            catch (Exception e)
            {
                err = new Output("E-web-0020", "クリック操作に失敗しました。" + e.Message, "", "").toString();
            }

            if (err == "")
            {
                yield return new Output("I-web-0021", "クリック操作に成功しました。", "", "").toString();
            }
            else
            {
                yield return err;
            }
        }

        private IEnumerable<string> web__scrshot(string input, string cmd)
        {
            // 引数取得，チェック
            Dictionary<string, string>? args = new Dictionary<string, string>();
            string err = ParseCmd(args, cmd, "scrshot");
            if (err != "")
            {
                yield return new Output("E-web-0014", "コマンド分解中にエラーが発生しました。[scrshot]", "", "").toString();
                yield break;
            }
            if (this.driver == null)
            {
                yield return new Output("E-web-0027", "Driverが設定されていません。[scrshot]", "", "").toString();
                yield break;
            }

            // ファイル名
            if (args.ContainsKey("name"))
            {
                args["name"] = Path.Combine(Directory.GetCurrentDirectory(), "tmp", args["name"]);
            }
            else
            {
                args["name"] = Path.Combine(Directory.GetCurrentDirectory(), "tmp", DateTime.Now.ToString("yyyyMMdd_HHmmss_ffffff") + ".png");
            }
            yield return new Output("D-web-0031", "画像を[" + args["name"] + "に出力します。", "", "").toString();


            err = "";
            try
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

                // ページの全体幅と高さを取得
                long totalWidth = (long)js.ExecuteScript("return document.documentElement.scrollWidth");
                if (args.ContainsKey("width"))
                {
                    totalWidth = int.Parse(args["width"]);
                }
                long totalHeight = (long)js.ExecuteScript("return document.body.scrollHeight");
                driver.Manage().Window.Size = new System.Drawing.Size((int)totalWidth, (int)totalHeight);


                Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                screenshot.SaveAsFile(args["name"]);

            }
            catch (Exception e)
            {
                err = new Output("E-web-0022", "スクリーンショットの取得に失敗しました。" + e.Message, "", "").toString();
            }

            if (err == "") { yield return new Output("I-web-0023", "スクリーンショットの取得に成功しました。", "", "").toString(); }
            else { yield return err; }
        }

        private IEnumerable<string> web__getText(string input, string cmd)
        {
            // 引数取得，チェック
            Dictionary<string, string>? args = new Dictionary<string, string>();
            string err = ParseCmd(args, cmd, "getText");
            if (err != "")
            {
                yield return new Output("E-web-0014", "コマンド分解中にエラーが発生しました。[getText]", "", "").toString();
                yield break;
            }
            if (this.driver == null)
            {
                yield return new Output("E-web-0027", "Driverが設定されていません。[getText]", "", "").toString();
                yield break;
            }

            // クリック
            err = "";
            string text = "";
            try
            {
                IWebElement element = getElement(args);
                text = element.Text;
            }
            catch (Exception e)
            {
                err = new Output("E-web-0024", "テキストの取得に失敗しました。" + e.Message, "", "").toString();
            }

            if (err == "")
            {
                yield return new Output("I-web-0025", "テキストの取得に成功しました。", "String", text).toString();
            }
            else
            {
                yield return err;
            }

        }

        private IEnumerable<string> web__reset(string input, string cmd)
        {
            if (this.driver == null)
            {
                yield return new Output("E-web-0027", "Driverが設定されていません。[reset]", "", "").toString();
                yield break;
            }
            this.driver.Quit();
            yield return new Output("I-web-0026", "Chromeの終了処理に成功しました。", "Bool", "true").toString();

        }

        /*
         * 
         * Action実行時に必要になる関数
         * 
         */
        private IWebElement getElement(Dictionary<string, string> args)
        {
            // セレクタ構築
            string target = "";
            int num = 0;
            if (args.ContainsKey("tag")) { target += args["tag"]; }
            if (args.ContainsKey("id")) { target += "#" +  args["id"]; }
            if (args.ContainsKey("class")) { target += "." + string.Join(".", args["class"].Split(':')); }
            if (args.ContainsKey("type")) { target += "[type='" + args["type"] + "'"; }
            if (args.ContainsKey("name")) { target += "[name='" + args["name"] + "'"; }

            // 検索
            IReadOnlyCollection<IWebElement> elements = this.driver.FindElements(By.CssSelector(target));
            if (elements.Count == 0)
            {
                throw new Exception("Element does not exist");
            }
            else if (elements.Count == 1)
            {
                return elements.ElementAt(0);
            }
            else
            {
                if (args.ContainsKey("num"))
                {
                    try
                    {
                        return elements.ElementAt(Int32.Parse(args["num"]));
                    }
                    catch { throw new Exception("conversion error"); }
                }
                throw new Exception();
            }

        }

    }
}
