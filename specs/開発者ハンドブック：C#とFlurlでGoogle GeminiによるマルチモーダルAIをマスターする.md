# 開発者ハンドブック：C#とFlurlでGoogle GeminiによるマルチモーダルAIをマスターする

---

## 序論

### AIの新たなフロンティア

近年、人工知能（AI）の分野は、純粋にテキストベースの大規模言語モデル（LLM）から、テキスト、画像、音声、動画といった複数の入力を組み合わせて理解・処理できる強力なマルチモーダルモデルへと大きな変革を遂げています。この進化により、開発者はこれまで以上にリッチで文脈に応じた、人間のような対話が可能なアプリケーションを構築できるようになりました。

### 主要技術の紹介

本稿では、この最先端のAI機能を.NETアプリケーションに統合するための主要な技術を紹介します。

* **Google Gemini**: Googleが提供する最も高性能で汎用性の高いモデルファミリーの一つであり、当初からマルチモーダルであることを前提に構築されています。異なるデータタイプを横断して複雑な推論を実行する能力がその特徴です。
* **Flurl**: .NET向けのモダンで流暢（fluent）、かつ可読性の高いHTTPクライアントライブラリです。RESTful APIの利用プロセスを簡素化し、明確にすることで、Gemini APIのような洗練されたサービスとの連携に理想的な選択肢となります。

## 本稿の目的と構成

このドキュメントは、C#開発者向けに、Flurlを使用してGemini APIにテキストと画像のプロンプトを送信する、本番環境に対応可能なクライアントを構築するための決定的なステップバイステップガイドとして機能します。本稿は、基礎概念から始まり、コンポーネントの準備、完全な実装の構築、そして最後に高度なトピックと本番環境での考慮事項について議論するという構成になっています。

---

## パート1：基礎概念：Gemini APIとFlurl

### 1.1 Gemini APIエコシステムのナビゲーション：Google AI vs. Vertex AI

Gemini APIを利用しようとする開発者が最初に直面するのは、Google AIとVertex AIという2つの異なるドキュメントとセットアップ手順の存在です。この違いを最初に明確にしないと、開発者は本来シンプルなAPIキーで十分な場面で、複雑なGoogle Cloud Platform（GCP）のセットアップに着手してしまい、多大な時間と労力を浪費する可能性があります。この区別を理解することは、プロジェクトを成功に導くための最初の重要なステップです。

#### 推奨されるパス：Google AI Developer API

* **説明**: これは、アプリケーションへの迅速な統合を目的として設計された、直接的で軽量な、開発者中心のREST APIです。
* **認証**: シンプルなAPIキーを使用し、HTTPヘッダーの`x-goog-api-key`で渡されます。これは公式のREST APIの例で明確に示されています。
* **キーの取得**: キーはGoogle AI Studioで無料で生成でき、Googleアカウントをプロジェクトにリンクさせることで取得できます。この参入障壁の低さが、本ガイドのスコープに最適です。

#### エンタープライズパス：Vertex AI API

* **説明**: これはGoogleのフルマネージドでエンタープライズグレードのAIプラットフォームであり、他のGoogle Cloudエコシステムと深く統合されています。
* **認証**: Identity and Access Management（IAM）とApplication Default Credentials（ADC）に依存しており、これらはエンタープライズワークロードに対してより複雑で安全ですが、スタンドアロンアプリケーションにとっては大きなオーバーヘッドとなります。
* **セットアップ**: ドキュメントに記載されている必要な手順には、GCPプロジェクトの作成、課金の有効化、`gcloud` CLIのインストールと設定、そして`gcloud auth application-default login`による認証が含まれます。

> これらの詳細を理解することで、よりシンプルなGoogle AI APIに焦点を当てるという本ガイドの決定が正当化されます。

### 1.2 Geminiマルチモーダルリクエストの構造

Gemini APIのJSON構造は、単なるデータ形式ではなく、対話的で文脈を保持するための設計思想を反映しています。APIが`contents`配列内に`parts`配列を持つ構造 6 は、チャットログを模倣しています。単一の「ターン」（`content` オブジェクト）に複数の`part`を含めることで、ユーザーは1つのメッセージ内でテキストと画像をシームレスに混在させることができます（例：「この猫\[猫の画像]について説明し、こちらの犬\[犬の画像]と比較してください」）。`contents`配列自体が複数のターンを保持できるため、対話の履歴を構築することも可能です。

#### エンドポイント

Google AI Developer APIのエンドポイントは明確に定義されています：

```
https://generativelanguage.googleapis.com/v1beta/models/{model-name}:generateContent
```

`{model-name}`は、`gemini-1.5-pro-latest`や`gemini-1.5-flash-latest`といった特定のモデルIDのプレースホルダーです。

#### リクエストボディ（JSONペイロード）

公式のREST APIの例を参考に、要求されるJSON構造を詳細に解説します。

* **contents**: 1つ以上の対話ターンを保持するトップレベルの配列。単一のプロンプトの場合、これは1つのオブジェクトを含みます。
* **parts**: 各`content`オブジェクト内の配列。これがマルチモダリティの中核であり、プロンプトの個々の部分を保持します。

  * **テキストパート**: 単一のキーを持つオブジェクト：`{ "text": "ここにプロンプトを記述します。" }`。
  * **画像パート**: 単一のキー`inlineData`を持つオブジェクト。その値は別のオブジェクトです：

    ```json
    {
      "inlineData": {
        "mimeType": "image/jpeg",
        "data": "BASE64_ENCODED_STRING"
      }
    }
    ```

    * `mimeType`: このフィールドは必須であり、モデルにバイナリデータをどのように解釈すべきかを伝えます。一般的な値には`image/jpeg`、`image/png`、`image/webp`などがあります。
    * `data`: Base64文字列としてエンコードされた、生の画像ファイルのバイトデータです。

#### レスポンスボディ（JSONペイロード）

成功時のレスポンス構造について簡単に説明します。モデルの出力を含む`candidates`配列に焦点を当てます。各候補（`candidate`）には`content`オブジェクトがあり、その中には生成された`text`を含む`parts`があることを説明し、開発者が結果を解析する準備を整えます。

### 1.3 Flurlの紹介：流暢なHTTPクライアント

* **中心哲学**: Flurlは、C#でのHTTP呼び出しをより読みやすく、保守しやすく、テストしやすくするために設計されています。これは、拡張メソッド上に構築された流暢で連鎖可能なインターフェースを通じて実現されます。
* **本プロジェクトにおける主要メソッド**:

  * `WithHeader("key", "value")`: HTTPヘッダーを追加するために使用するメソッドで、`x-goog-api-key`による認証に最適です。
  * `PostJsonAsync(object)`: 任意のC#オブジェクトを受け取り、それをJSON文字列にシリアライズし、`Content-Type: application/json`ヘッダーを設定して、HTTP POSTリクエストのボディで送信する強力なメソッドです。これが我々の実装の基盤となります。
  * `ReceiveJson<T>()`: `PostJsonAsync`の対となるメソッド。JSONレスポンスボディを読み取り、指定された型`T`の厳密に型付けされたC#オブジェクトにデシリアライズします。
* **エラーハンドリング**: Flurlがデフォルトで非成功（4xxまたは5xx）のHTTPステータスコードに対してスローする`FlurlHttpException`を紹介します。これにより、エラーハンドリングのロジックが大幅に簡素化されます。

---

## パート2：.NETコンポーネントの準備

### 2.1 C#レコードによるAPIコントラクトのモデリング

Flurlのオブジェクト指向的な性質は、Gemini APIの構造化されたJSONと非常に相性が良いです。APIは複雑でネストされたJSONペイロードを要求しますが、手作業での文字列結合や`JObject`の使用はエラーの温床となります。対照的に、Flurlの`PostJsonAsync(object)`メソッドは「モデルファースト」のアプローチを推奨します。JSONスキーマを正確に反映するC#のクラスやレコードを定義することで、コンパイル時の型安全性、IntelliSenseの恩恵、そしてコードの可読성と保守性の大幅な向上が得られます。これは単なるコーディングの好みではなく、バグを減らし、コードを自己文書化する戦略的な設計上の選択です。

#### C# レコードによる実装

**C#**

```csharp
// --- Request Models ---
using System.Text.Json.Serialization;

public record GeminiRequest(
    [property: JsonPropertyName("contents")] IEnumerable<Content> Contents,
    [property: JsonPropertyName("generationConfig"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] GenerationConfig? GenerationConfig = null
);

public record Content(
    [property: JsonPropertyName("parts")] IEnumerable<Part> Parts
);

public record Part(
    [property: JsonPropertyName("text"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Text = null,
    InlineData? InlineData = null
);

public record InlineData(
    string MimeType,
    [property: JsonPropertyName("data")] string Data
);

public record GenerationConfig(
    [property: JsonPropertyName("temperature"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] float? Temperature,
    [property: JsonPropertyName("topP"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] float? TopP,
    [property: JsonPropertyName("topK"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? TopK,
    int? MaxOutputTokens,
    IEnumerable<string>? StopSequences
);

// --- Response Models ---
public record GeminiResponse(
    [property: JsonPropertyName("candidates")] IEnumerable<Candidate>? Candidates,
    [property: JsonPropertyName("promptFeedback")] PromptFeedback? PromptFeedback
);

public record Candidate(
    [property: JsonPropertyName("content")] Content? Content,
    string? FinishReason,
    [property: JsonPropertyName("index")] int Index,
    IEnumerable<SafetyRating>? SafetyRatings
);

public record SafetyRating(
    [property: JsonPropertyName("category")] string? Category,
    [property: JsonPropertyName("probability")] string? Probability
);

public record PromptFeedback(
    IEnumerable<SafetyRating>? SafetyRatings
);

// --- Error Response Model ---
public record GeminiErrorResponse(
    [property: JsonPropertyName("error")] ErrorDetails? Error
);

public record ErrorDetails(
    [property: JsonPropertyName("code")] int Code,
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("status")] string? Status
);
```

### 2.2 画像ユーティリティ：ファイルからBase64へ

単純な実装ではMIMEタイプを`image/jpeg`とハードコーディングしてしまうかもしれませんが、APIペイロードの`mimeType`フィールド 2 は、提供されたBase64データをどのようにデコードし解釈すべきかという、モデルへの直接的な指示です。JPEGのMIMEタイプでPNGを送信すると、モデル側でデコードエラーや誤解釈を引き起こす可能性があります。したがって、真に堅牢で再利用可能なユーティリティは、入力ファイルに基づいてMIMEタイプを動的に決定する必要があります。この細部への配慮が、一度きりのスクリプトを回復力のあるソフトウェアコンポーネントへと昇華させます。

#### ImageHelperクラスの作成

**C#**

```csharp
public static class ImageHelper
{
    public static async Task<string> ConvertImageFileToBase64Async(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
        {
            throw new FileNotFoundException("Image file not found at the specified path.", imagePath);
        }

        byte imageBytes = await File.ReadAllBytesAsync(imagePath);
        return Convert.ToBase64String(imageBytes);
    }

    public static string GetMimeTypeForFilePath(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            ".heic" => "image/heic",
            ".heif" => "image/heif",
            _ => throw new NotSupportedException($"File extension '{extension}' is not a supported image type.")
        };
    }
}
```

#### 表1：Gemini APIで一般的に使用される画像のMIMEタイプ

| ファイル拡張子      | MIMEタイプ文字列             | 注記                            |
| ------------ | ---------------------- | ----------------------------- |
| .jpg, .jpeg  | image/jpeg             | 写真画像に最適。                      |
| .png         | image/png              | 透明度をサポート。可逆圧縮。                |
| .webp        | image/webp             | 優れた圧縮率と透明度をサポートする現代的なフォーマット。  |
| .gif         | image/gif              | アニメーションをサポート（モデルは最初のフレームを処理）。 |
| .heic, .heif | image/heic, image/heif | Appleデバイスで一般的な高効率フォーマット。      |

---

## パート3：完全な実装：再利用可能なGeminiクライアント

### 3.1 GeminiClientサービスの設計

インターフェース`IGeminiClient`とクラス`GeminiClient : IGeminiClient`を定義します。コンストラクタは`string apiKey`とオプションの`HttpClient`を受け入れます。この設計が、直接的なインスタンス化と大規模アプリケーションにおける依存性注入の両方をサポートすることを説明します。APIのベースURL（`https://generativelanguage.googleapis.com`）と選択したモデルID（例：`gemini-1.5-pro-latest`）を定数またはプライベートフィールドとして保存します。

### 3.2 中核となるマルチモーダルメソッド：GenerateContentAsync

インターフェースとクラスで主要なメソッドシグネチャを定義します：

```csharp
task<string> GenerateContentAsync(string prompt, string imagePath, CancellationToken cancellationToken = default);
```

#### Flurl呼び出しの実行例

**C#**

```csharp
GeminiResponse response = await _apiBaseUrl
   .AppendPathSegment($"v1beta/models/{_modelName}:generateContent")
   .WithHeader("x-goog-api-key", _apiKey)
   .PostJsonAsync(requestBody, cancellationToken)
   .ReceiveJson<GeminiResponse>();
```

### 3.3 堅牢なエラーハンドリングの実装

ベストプラクティスとして、Flurlの呼び出しを`try...catch (FlurlHttpException ex)`ブロックでラップします。`catch`ブロック内で、APIレスポンスから有意義なエラー情報を抽出する方法を示します。

### 3.4 実行可能なコンソールアプリケーション（Program.cs）

すべてを統合する、コピー＆ペーストして実行可能な完全な`Program.cs`ファイルを提供します。

**C#**

```csharp
// --- IGeminiClient.cs ---
public interface IGeminiClient
{
    Task<string> GenerateContentAsync(string prompt, string imagePath, GenerationConfig? generationConfig = null, CancellationToken cancellationToken = default);
}

// --- GeminiClient.cs ---
using Flurl.Http;

public class GeminiApiException : Exception
{
    public GeminiApiException(string message, Exception innerException) : base(message, innerException) { }
}

public class GeminiClient : IGeminiClient
{
    private readonly string _apiKey;
    private readonly string _modelName;
    private const string ApiBaseUrl = "https://generativelanguage.googleapis.com";

    public GeminiClient(string apiKey, string modelName = "gemini-1.5-flash-latest")
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentNullException(nameof(apiKey));

        _apiKey = apiKey;
        _modelName = modelName;
    }

    public async Task<string> GenerateContentAsync(string prompt, string imagePath, GenerationConfig? generationConfig = null, CancellationToken cancellationToken = default)
    {
        try
        {
            string base64Image = await ImageHelper.ConvertImageFileToBase64Async(imagePath);
            string mimeType = ImageHelper.GetMimeTypeForFilePath(imagePath);

            var requestBody = new GeminiRequest(
                Contents: new
                {
                    new Content(new
                    {
                        new Part(Text: prompt),
                        new Part(InlineData: new InlineData(mimeType, base64Image))
                    })
                },
                GenerationConfig: generationConfig
            );

            GeminiResponse response = await ApiBaseUrl
               .AppendPathSegment($"v1beta/models/{_modelName}:generateContent")
               .WithHeader("x-goog-api-key", _apiKey)
               .PostJsonAsync(requestBody, cancellationToken)
               .ReceiveJson<GeminiResponse>();

            string? resultText = response?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

            return resultText ?? "No content generated.";
        }
        catch (FlurlHttpException ex)
        {
            string errorBody = await ex.GetResponseStringAsync();
            var errorResponse = System.Text.Json.JsonSerializer.Deserialize<GeminiErrorResponse>(errorBody);
            string errorMessage = errorResponse?.Error?.Message ?? "An unknown API error occurred.";

            throw new GeminiApiException($"API call failed with status {ex.StatusCode}: {errorMessage}", ex);
        }
        catch (Exception ex)
        {
            // Handle other exceptions like file not found, etc.
            throw new GeminiApiException($"An unexpected error occurred: {ex.Message}", ex);
        }
    }
}

// --- Program.cs ---
public class Program
{
    public static async Task Main(string[] args)
    {
        // IMPORTANT: Set this environment variable before running.
        string? apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("Error: GEMINI_API_KEY environment variable not set.");
            return;
        }

        // Replace with the actual path to your image file.
        string imagePath = "path/to/your/image.jpg";
        string prompt = "What is in this image? Describe it in detail.";

        try
        {
            var client = new GeminiClient(apiKey);
            Console.WriteLine("Sending request to Gemini API...");
            string response = await client.GenerateContentAsync(prompt, imagePath);

            Console.WriteLine("\n--- Gemini Response ---");
            Console.WriteLine(response);
            Console.WriteLine("-----------------------\n");
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Error: Image file not found. {ex.Message}");
        }
        catch (GeminiApiException ex)
        {
            Console.WriteLine($"Error calling Gemini API: {ex.Message}");
            if (ex.InnerException is FlurlHttpException fex)
            {
                 Console.WriteLine($"Underlying HTTP Status: {fex.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }
    }
}
```

---

## パート4：高度なトピックと本番環境への対応

### 4.1 安全なAPIキー管理

ソースコードにAPIキーをハードコーディングすることは重大なセキュリティリスクです。.NETの設定フレームワークを使用した具体的な例を提供します。

* **appsettings.json**: 機密性の低い設定に使用します。
* **ユーザーシークレット**: ローカル開発で推奨されるアプローチです（`dotnet user-secrets set "Gemini:ApiKey" "YOUR_KEY"`）。`IConfiguration`を介してこれを読み取る方法を示します。
* **環境変数**: Docker、Azure App Serviceなどの本番環境での標準です。

### 4.2 より複雑なプロンプトの処理

* **複数画像のプロンプト**: クライアントメソッドを変更して画像のパスのリストを受け入れ、複数の`inlineData`オブジェクトを持つ`parts`配列を構築する方法のコード例を提供します。
* **インターリーブされたプロンプト**: \`\` のようなシーケンスに従う`parts`配列を構築する方法のコード例を示します。

### 4.3 モデルの挙動を設定する（generationConfig）

開発者は、リクエストペイロードの`generationConfig`オブジェクトを使用してモデルの出力を微調整できます。

#### 表2：主要な生成設定パラメータ

| パラメータ           | 型       | 説明                             | 値の例            |
| --------------- | ------- | ------------------------------ | -------------- |
| temperature     | number  | ランダム性を制御。高いほど創造的、低いほど決定的。      | 0.9            |
| topP            | number  | 核サンプリング。累積確率が`topP`以下のトークンを考慮。 | 1.0            |
| topK            | integer | 各ステップで最も可能性の高い上位`K`個のトークンを考慮。  | 32             |
| maxOutputTokens | integer | レスポンスで生成するトークンの最大数。            | 2048           |
| stopSequences   | string  | 生成を停止させる文字列のシーケンスのセット。         | \["\n", "###"] |

### 4.4 依存性注入と単体テスト

ASP.NET Coreの`Program.cs`やワーカサービスで、`IGeminiClient`と`GeminiClient`を.NETの依存性注入コンテナに登録するコードを簡潔に示します。Flurlの強力なテストライブラリである`Flurl.Http.Testing`を紹介します。その利点は、実際のネットワークトラフィックなしでHTTP呼び出しを傍受し、モック化できることであり、これにより高速で信頼性の高い単体テストが可能になります。`MSTest`や`xUnit`のようなテストフレームワークを使用した、簡潔だが完全な単体テストの例を提供します。テストでは`HttpTest`を使用してGemini APIからの偽のレスポンスを設定し、`GeminiClient`がそれを正しく解析することをアサートします。

---

## 結論

### 達成事項の要約

本ガイドでは、Gemini APIのニュアンスの理解から、Flurlを使用した堅牢で再利用可能、かつテスト可能なC#クライアントの構築まで、主要な達成事項を簡潔にまとめました。

### 中心原則の再確認

明確な関心の分離、C#レコードによる厳密な型付け、流暢で読みやすいAPI呼び出し、包括的なエラーハンドリング、安全な設定といった、従うべきベストプラクティスを再確認しました。

### 今後の探求

この基盤の上にさらに構築していくことを推奨します。動画や音声入力の処理、関数呼び出しの使用、あるいはGeminiファミリーで利用可能なさまざまなモデルの実験など、ドキュメントで言及されている他のGemini APIの機能を探求することを示唆します。これにより、継続的な学習と開発のための明確な道筋が提供されます。
