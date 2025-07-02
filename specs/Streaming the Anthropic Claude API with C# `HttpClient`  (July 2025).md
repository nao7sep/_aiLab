# Streaming the Anthropic Claude API with C# `HttpClient`  (July 2025)

> **Scope.** A **stream‑first** cookbook for .NET 6+ teams integrating Anthropic’s Claude 4/3 models. Vision (image/PDF) blocks and the Files API are covered where they interact with streaming.

## 1  Why stream?

* **Sub‑second TTFB.** Streaming cuts *time‑to‑first‑byte* versus blocking calls by \~0.2‑0.6 s on **claude‑haiku‑4‑latest**.
* **Progressive UX.** Render Markdown, voice‑synth, or tool calls as soon as `content_block_delta` arrives.
* **Abort cost.** Billing stops at the last delivered token when users cancel.

## 2  Endpoints & auth

| Deployment         | URL pattern                                                                                                                                           |
| ------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Direct API**     | `https://api.anthropic.com/v1/messages`                                                                                                               |
| **Amazon Bedrock** | `https://bedrock.<region>.amazonaws.com/model/anthropic.claude‑opus‑4/invoke‑stream`                                                                  |
| **Vertex AI**      | `https://<region>-aiplatform.googleapis.com/v1/projects/{project}/locations/{region}/publishers/anthropic/models/claude‑opus‑4:streamGenerateContent` |

**Required headers (direct):**

```text
x-api-key: $ANTHROPIC_API_KEY
anthropic-version: 2025-06-01   # latest stable as of July 2025
content-type: application/json
```

Setting body field `"stream": true` switches the transport to **Server‑Sent Events (SSE)**. ([docs.anthropic.com](https://docs.anthropic.com/en/docs/build-with-claude/streaming))

## 3  Request anatomy (multimodal‑capable)

```jsonc
POST /v1/messages
{
  "model": "claude-opus-4-20250514",      // or "-latest"
  "stream": true,
  "max_tokens": 1024,
  "messages": [
    {
      "role": "user",
      "content": [
        { "type": "image", "source": {
            "type": "base64",
            "media_type": "image/jpeg",
            "data": "<base‑64>"
        }},
        { "type": "text", "text": "Describe the photo" }
      ]
    }
  ]
}
```

Vision blocks accept `image/jpeg|png|gif|webp` and up to **100 images** per request. Resize >8 MP images to avoid latency. ([docs.anthropic.com](https://docs.anthropic.com/en/docs/build-with-claude/vision))

## 4  Idiomatic C# streaming helper

```csharp
public static async IAsyncEnumerable<string> StreamClaudeAsync(
    string apiKey,
    object payload,                    // anonymous object matching JSON above
    [EnumeratorCancellation] CancellationToken ct = default)
{
    const string Url = "https://api.anthropic.com/v1/messages";

    var req = new HttpRequestMessage(HttpMethod.Post, Url)
    {
        Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
    };
    req.Headers.Add("x-api-key", apiKey);
    req.Headers.Add("anthropic-version", "2025-06-01");
    req.Headers.Accept.Add(new("text/event-stream"));

    using var resp = await Http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
    resp.EnsureSuccessStatusCode();

    await foreach (var chunk in ParseSse(resp, ct))
        yield return chunk;
}

private static async IAsyncEnumerable<string> ParseSse(HttpResponseMessage resp,
    [EnumeratorCancellation] CancellationToken ct)
{
    using var stream = await resp.Content.ReadAsStreamAsync(ct);
    using var reader = new StreamReader(stream);
    while (!reader.EndOfStream && !ct.IsCancellationRequested)
    {
        var line = await reader.ReadLineAsync(ct);
        if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data:")) continue;
        var json = line.AsSpan(5).Trim();
        using var doc = JsonDocument.Parse(json);
        var type = doc.RootElement.GetProperty("type").GetString();
        if (type == "content_block_delta")
            yield return doc.RootElement
                             .GetProperty("delta")
                             .GetProperty("text")
                             .GetString() ?? string.Empty;
    }
}
```

* Filter on `content_block_delta` events whose `delta.type == "text_delta"`. Other event types include thinking deltas and tool streams. ([docs.anthropic.com](https://docs.anthropic.com/en/docs/build-with-claude/streaming))
* Use a single static `HttpClient` with HTTP/2 enabled.

## 5  Building the payload in C# (with image)

```csharp
var payload = new
{
    model = "claude-sonnet-4-latest",
    stream = true,
    max_tokens = 800,
    messages = new[]
    {
        new
        {
            role = "user",
            content = new object[]
            {
                new { type = "image", source = new {
                        type = "base64",
                        media_type = "image/jpeg",
                        data = Convert.ToBase64String(File.ReadAllBytes(imgPath))
                    }},
                new { type = "text", text = prompt }
            }
        }
    }
};
```

For reusable media or PDFs, upload once via **Files API** (beta header `anthropic-beta: files-api-2025-04-14`) and embed `{"type":"file","file_id":"file_…"}` sources. ([docs.anthropic.com](https://docs.anthropic.com/en/docs/build-with-claude/files?utm_source=chatgpt.com))

## 6  SSE event flow

| Step | Event                 | Notes                                            |
| ---- | --------------------- | ------------------------------------------------ |
| 1    | `message_start`       | Metadata, empty `content`                        |
| 2    | `content_block_start` | Opens block *i*                                  |
| 3    | `content_block_delta` | Incremental `text_delta`, `thinking_delta`, etc. |
| 4    | `content_block_stop`  | Block *i* done                                   |
| 5    | `message_delta`       | Aggregate usage + stop\_reason                   |
| 6    | `message_stop`        | Final frame                                      |
| —    | `ping`                | Keep‑alive every 5 s                             |

Your parser should ignore unknown future events per Anthropic’s versioning policy. ([docs.anthropic.com](https://docs.anthropic.com/en/docs/build-with-claude/streaming))

## 7  Concurrency & cancellation tips

| Pattern                                                     | Benefit                                 |
| ----------------------------------------------------------- | --------------------------------------- |
| Pass `CancellationToken` to `SendAsync` **and** parser loop | abort instantly when UI clicks *Stop*   |
| Buffer into `Channel<string>`/`IObservable<string>`         | decouple network from rendering         |
| Use `HttpCompletionOption.ResponseHeadersRead`              | begin reading before full body buffered |
| Increase `SocketsHttpHandler.PooledConnectionIdleTimeout`   | avoid reconnect cost for chat apps      |

## 8  Model & quota notes (July 2025)

* **claude‑opus‑4‑latest** – best reasoning (\~7 tok/s); **‑sonnet‑4** balance; **‑haiku‑4** fastest (\~20 tok/s).
* Context: 200 k tokens for Claude 4 family.
* Token billing: input + output at model‑specific rates.
* Per‑minute rate limit: 100 requests / 300k total tokens (starter tier). ([docs.anthropic.com](https://docs.anthropic.com/en/api/messages))

## 9  Files API for large / repeated media

```csharp
static async Task<string> UploadFileAsync(string apiKey, string path)
{
    using var client = new HttpClient();
    using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/files")
    {
        Content = new StreamContent(File.OpenRead(path))
    };
    req.Headers.Add("x-api-key", apiKey);
    req.Headers.Add("anthropic-version", "2025-06-01");
    req.Headers.Add("anthropic-beta", "files-api-2025-04-14");
    req.Content.Headers.ContentType = new("application/pdf");

    var resp = await client.SendAsync(req);
    resp.EnsureSuccessStatusCode();
    using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
    return doc.RootElement.GetProperty("id").GetString()!;  // file_id
}
```

Uploaded files persist **72 h** and count toward a 10 GB workspace quota. ([docs.anthropic.com](https://docs.anthropic.com/en/api/files-create?utm_source=chatgpt.com))

## 10  Common failure modes

| Symptom                 | Cause                                           | Mitigation                                |
| ----------------------- | ----------------------------------------------- | ----------------------------------------- |
| **HTTP 400** right away | Missing `anthropic-version` or misspelled field | Check header + JSON casing                |
| **HTTP 429** mid‑stream | Rate/throughput exceeded                        | Exponential back‑off; reduce token counts |
| Stream closes silently  | Idle >30 s before next chunk                    | Send smaller images, enable keep‑alive    |

## 11  Further reading

* **Streaming Messages doc** ([docs.anthropic.com](https://docs.anthropic.com/en/docs/build-with-claude/streaming))
* **Vision guide** ([docs.anthropic.com](https://docs.anthropic.com/en/docs/build-with-claude/vision))
* **Files API beta** ([docs.anthropic.com](https://docs.anthropic.com/en/docs/build-with-claude/files?utm_source=chatgpt.com))
* **Event flow reference** ([docs.anthropic.com](https://docs.anthropic.com/en/docs/build-with-claude/streaming))

*© 2025 – Feel free to copy & adapt. Happy streaming!*
