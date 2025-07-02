# Streaming the Gemini API with C# `HttpClient`  (July 2025)

> **Scope.** This guide is a **stream‑first** walk‑through for .NET 6+ teams who want real‑time token delivery from Google’s Gemini family.  Multimodal prompts (text + image/audio/video/PDF) are covered where they affect streaming.

## 1  Why stream?

* **Low latency UI.** First tokens arrive 0.15‑0.4 s faster on *gemini‑2.5‑flash* than blocking calls.
* **Progressive rendering.** Show partial Markdown, update React components, drive speech‑synth as tokens land.
* **Lower abort cost.** Users who stop early pay for consumed tokens only.

## 2  Streaming endpoints

| Deployment               | URL pattern                                                                                                                                                                                                                                                                                                                |
| ------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Google AI Studio key** | `https://generativelanguage.googleapis.com/v1beta/models/{model}:streamGenerateContent?alt=sse&key=API_KEY` ([ai.google.dev](https://ai.google.dev/api/generate-content))                                                                                                                                                  |
| **Vertex AI (OAuth)**    | `https://REGION-aiplatform.googleapis.com/v1/projects/{project}/locations/{region}/publishers/google/models/{model}:streamGenerateContent` ([cloud.google.com](https://cloud.google.com/vertex-ai/generative-ai/docs/reference/rest/v1/projects.locations.publishers.models/streamGenerateContent?utm_source=chatgpt.com)) |

`alt=sse` switches the HTTP body to **Server‑Sent Events**. The response is a text/event‑stream whose `data:` lines each contain a full `GenerateContentResponse` chunk.  No other streaming transport (gRPC‑JSON) is currently public.

## 3  Request anatomy (multimodal‑capable)

```jsonc
POST /v1beta/models/gemini‑2.5‑flash:streamGenerateContent?alt=sse&key=API_KEY
Content‑Type: application/json

{
  "contents": [{
    "role": "user",
    "parts": [
      { "text": "Describe the photo" },
      { "inline_data": { "mime_type": "image/jpeg", "data": "<base‑64>" }}
    ]
  }],
  "generationConfig": { "maxOutputTokens": 1024 }
}
```

The **same JSON** works for audio / video / PDF; change `mime_type` or use `file_data` URIs for > 20 MB assets.

## 4  Idiomatic C# streaming helper

```csharp
public static async IAsyncEnumerable<string> StreamGeminiAsync(
    string apiKey,
    string model,
    object payload,                // anonymous object matching JSON above
    [EnumeratorCancellation] CancellationToken ct = default)
{
    var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:streamGenerateContent?alt=sse&key={apiKey}";

    var req = new HttpRequestMessage(HttpMethod.Post, url)
    {
        Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
    };
    req.Headers.Accept.Add(new("text/event-stream"));

    using var resp = await Http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
    resp.EnsureSuccessStatusCode();

    using var stream = await resp.Content.ReadAsStreamAsync(ct);
    using var reader = new StreamReader(stream);

    while (!reader.EndOfStream && !ct.IsCancellationRequested)
    {
        var line = await reader.ReadLineAsync();
        if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data:")) continue;

        var json = line.AsSpan()[5..].Trim();
        var doc = JsonDocument.Parse(json);
        yield return doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text").GetString()!;
    }
}
```

* Keep **one static** `HttpClient` (`Http2 = true`) in your app.
* Because Gemini sends **full JSON objects**, you can parse tool calls or safety reasons before emitting the chunk.

## 5  Building the payload

```csharp
var payload = new
{
    contents = new[]
    {
        new
        {
            role = "user",
            parts = new object[]
            {
                new { text = prompt },                      // or array of Parts
                new
                {
                    inline_data = new
                    {
                        mime_type = "image/jpeg",
                        data = Convert.ToBase64String(File.ReadAllBytes(imgPath))
                    }
                }
            }
        }
    },
    generationConfig = new { temperature = 0.7, topP = 0.9 }
};
```

For **large or reused media** call the **File API** once, then embed `{ file_data: { mime_type, file_uri } }`.

## 6  Chunk structure (SSE frame)

```
id: 8af…
data: { "candidates": [ { "content": { "parts": [ { "text": "Once upon" } ] }, "finishReason": "NOT_FINISHED" } ] }

id: 8af…1
data: { "candidates": [ { "content": { "parts": [ { "text": " a backpack…" } ] }, "finishReason": "NOT_FINISHED" } ] }

…
```

* `finishReason` becomes `FINISH_REASON_STOP` on the final chunk.
* Errors are sent as separate `event: error` frames with JSON describing the HTTP status.

## 7  Concurrency & cancellation tips

| Pattern                                                       | Benefit                                        |
| ------------------------------------------------------------- | ---------------------------------------------- |
| `CancellationToken` passed to `SendAsync` **and** reader loop | lets UI “stop generating” instantly            |
| Wrap reader in **Task.Run** to isolate blocking I/O           | avoids UI thread stalls in WinUI / Blazor WASM |
| Buffer into `Channel<string>` or `IObservable<string>`        | decouples parsing from rendering               |
| Back‑pressure long responses by awaiting the UI side          | keeps memory bounded                           |

## 8  Model & quota notes (June 2025‑stable)

* **gemini‑2.5‑flash-latest** → fastest (≈ 15 tok/s); \*\*‑pro \*\* → better reasoning, ≈ 7 tok/s.
* Max output 4096 tokens / request. Context 8 M tokens on 2.5. ([ai.google.dev](https://ai.google.dev/gemini-api/docs/changelog?utm_source=chatgpt.com))
* File uploads persist 48 h, project‑wide cap 20 GB.

## 9  Vertex AI variant

1. Exchange service‑account credentials for an **OAuth 2 Bearer** token (`https://www.googleapis.com/auth/cloud-platform`).
2. Hit the region URL in §2 without `?key=`.  SSE frames are identical.

## 10  Common failure modes

| Symptom                         | Cause                                      | Fix                                                |
| ------------------------------- | ------------------------------------------ | -------------------------------------------------- |
| **HTTP 400** instantly          | malformed `parts[]` or wrong `mime_type`   | validate JSON against docs                         |
| **HTTP 429** after a few chunks | rate or context quota exhaustion           | exponential back‑off, shorten prompt               |
| SSE closes mid‑story            | client’s read idle timeout < model latency | extend `HttpClient.Timeout` or use sockets handler |

## 11  Further reading

* **API doc – streamGenerateContent** (all modalities) ([ai.google.dev](https://ai.google.dev/api/generate-content))
* **Shell example with `--no-buffer`** ([ai.google.dev](https://ai.google.dev/api/generate-content))
* **Vertex AI streaming reference** ([cloud.google.com](https://cloud.google.com/vertex-ai/generative-ai/docs/reference/rest/v1/projects.locations.publishers.models/streamGenerateContent?utm_source=chatgpt.com))

*© 2025 – Free to copy & adapt. Happy streaming!*
