Great. I’ll create a detailed report on how to use the Anthropic Claude Sonnet 4 chat completions API, focusing on the latest version. The report will include:

* A breakdown of the JSON request payload (including optional fields)
* The expected structure of the JSON response
* Streaming format details (if supported)
* C# examples using HttpClient to make chat completions requests (with and without streaming)

I’ll get started and let you know once the report is ready.


# Anthropropic Claude API JSON Request-Response Guide

## Overview of the Claude API

Anthropic's API provides access to **Claude**, a large language model similar to ChatGPT. The API is used via HTTP requests with JSON payloads, and it returns JSON responses. The primary capability is **chat completions** (the **Messages API**), which allows you to send a conversation (user and assistant messages) and get Claude’s next response. Unlike some AI services, **Claude generates text only** – it does **not** create images or other media (so you don't have to worry about image generation; Claude only outputs textual content).

Anthropic has introduced new models such as *Claude 4*, with variants like **Claude Opus 4** and **Claude Sonnet 4** (these are model names). In this guide, we'll focus on using the **Claude Sonnet 4** model with the chat completions endpoint, including how to enable streaming responses. All examples will be in C#, using `HttpClient` with asynchronous calls.

## Endpoint and Authentication

To call Claude via the Anthropopic API, you will make an HTTP POST request to the chat completions endpoint. The base URL for chat (Messages API) is:

* **Endpoint:** `POST https://api.anthropic.com/v1/messages`

Each request must include the proper headers for authentication and versioning:

* **`x-api-key`** – Your Anthropic API key (provided in the header). This authenticates your request. For example: `"x-api-key: YOUR_API_KEY"`.
* **`anthropic-version`** – Specifies the API version. Use the latest stable version header. As of now the latest is `"2023-06-01"`. (Anthropic uses date-based versioning; using the latest version ensures you get the newest features and model support.)
* **`Content-Type`** – This should be `"application/json"` since we send JSON data.

**Example (cURL) of endpoint and headers:**

```bash
curl https://api.anthropic.com/v1/messages \
     --header "x-api-key: $ANTHROPIC_API_KEY" \
     --header "anthropic-version: 2023-06-01" \
     --header "content-type: application/json" \
     --data '{...}'
```



In C#, you will set these headers on an `HttpRequestMessage` or directly on `HttpClient` default headers before sending the request.

## Constructing the Request JSON (Chat Completions)

The body of your POST request is a JSON object defining the **completion request**. The key fields in the JSON payload are:

* **`model`** (string, **required**): The model name you want to use for completion. For example, to use Claude 4 (Sonnet variant), you might specify `"claude-sonnet-4-20250514"`. (Anthropic often appends a date to model names for versioning; the latest Claude 4 as of May 2025 is `claude-sonnet-4-20250514`. Aliases like `"claude-sonnet-4-0"` may also exist for the latest version.)
* **`messages`** (array of objects, **required**): The conversation history as a list of message objects. Each message has a **`role`** and **`content`**. Roles are typically `"user"` for user prompts and `"assistant"` for Claude’s responses. The model is trained on an alternating **user ↔ assistant** dialogue format. So usually, you will provide one or more user messages (and possibly prior assistant replies if continuing a conversation), and Claude will produce the next assistant message. For a simple query, you can just send a single user message. For example: `"messages": [ {"role": "user", "content": "Hello, world"} ]`.

  * **Content format:** The `content` of each message is typically a string containing the text. (Anthropic also supports image content in messages for certain vision features, but Claude will still respond with text. For standard usage, just provide text.)
  * If you include multiple messages, ensure they alternate roles (user then assistant then user, etc.). The last message in your list should usually be a `user` role (the prompt to which you want Claude to respond). If the last message is an `assistant` role with partial content, the API will treat it as a prefix Claude should continue from – an advanced technique to constrain Claude's reply.
* **`max_tokens`** (integer, **required**): The maximum number of tokens to generate for the response. This is analogous to OpenAI's max\_tokens. It limits how long Claude's answer can be. For example, `1024` means Claude can output up to 1024 tokens in the reply. (Tokens are pieces of words; 1024 tokens is roughly 3-4 pages of text.)
* **`temperature`** (number, *optional*): Controls randomness of the output. Ranges 0.0 to 1.0 (default is 1.0). Lower values make output more focused/deterministic, higher values more creative. If unsure, you can omit it (defaults to 1.0) or set e.g. `0.7` for a slightly more stable response.
* **`top_p`** (number, *optional*): Enables nucleus sampling. If set (between 0 and 1), Claude will consider only the most probable tokens with cumulative probability `top_p` at each step. This is an alternative to using temperature (generally use either temperature or top\_p, not both). In most cases you can skip this and rely on `temperature` alone.
* **`stop_sequences`** (array of strings, *optional*): You can provide custom text sequences where the model should stop generation. If Claude's output includes any of those sequences, it will stop and return. The response will then have `stop_reason: "stop_sequence"` and `stop_sequence` field indicating which sequence was matched. Use this if you need to delimit the output (for example, you might set a stop sequence like `"\nHuman:"` to stop when a new user prompt starts).
* **`system`** (string or array of content blocks, *optional*): A **system prompt** to prime Claude with context or instructions. This is similar to a system role in OpenAI's API. For example, you might set a system message like *"You are an assistant that speaks in Shakespearean English."* This can guide Claude’s style or behavior. If not provided, Claude works with its default behavior/instructions.
* **`stream`** (boolean, *optional*): Set this to `true` if you want a **streaming response** (more on streaming below). When `stream: true`, the response will be sent as a sequence of server-sent events (SSE) that gradually deliver Claude’s answer. By default (or if `stream: false`), the API will return the full completion in one JSON response after processing.

At minimum, you **must** provide `model`, `messages`, and a `max_tokens` value. Here's an example of a simple request JSON payload:

```json
{
  "model": "claude-sonnet-4-20250514",
  "max_tokens": 1024,
  "messages": [
    { "role": "user", "content": "Hello, world" }
  ]
}
```

This JSON asks Claude (using the **Claude Sonnet 4** model) to respond to the user message "Hello, world". The `max_tokens` is 1024, meaning the reply can be up to that length. (In practice, you might use a smaller max\_tokens for short answers; here 1024 is just an example upper bound.)

> **Note:** *Claude's models do not generate images.* Even if your prompt is asking for an image or contains an `<image>` tag, the output will be a textual description or refusal. The API is only for text completions, so you will always receive text content.

## Understanding the Response JSON

Whether streaming or not, the **final result** from Claude will be a JSON object representing the assistant's message. If you call without streaming (the default), you get the full response in one JSON. If you use streaming, you'll get incremental pieces (events) that you assemble into the final JSON. Let's first look at the structure of the **completed response JSON** (non-streaming case).

Key fields in the response JSON include:

* **`content`**: This is an array of **content blocks** that make up Claude's answer. In most simple cases, you'll get a single content block of type `"text"` containing Claude’s reply text. For example: `"content": [ {"type": "text", "text": "Hi! My name is Claude."} ]`. If the model produced a longer answer, it might still be one block with a large text string. The array structure is mainly useful if the response is structured (e.g. it can include other types like `tool_use` or `thinking` in advanced scenarios). For basic usage, you can assume the content array will have at least one `"text"` block with the answer text.
* **`role`**: The role of this message – it will always be `"assistant"` for Claude’s generated response.
* **`id`**: A unique identifier for the message (string). Example: `"id": "msg_013Zva2CMHLNnXjNJJKqJ2EF"`. You typically don't need this ID unless you store or reference the message; it's mainly for logging or threading in Anthropic's system.
* **`model`**: The model that produced the response. It echoes back the model you used, e.g. `"model": "claude-sonnet-4-20250514"`.
* **`stop_reason`**: A string (or null) indicating **why the generation stopped**. Common values are:

  * `"end_turn"` – the model naturally finished its answer (no special stop condition; this is a normal completion).
  * `"max_tokens"` – it stopped because it hit the `max_tokens` limit you set.
  * `"stop_sequence"` – a provided stop sequence was encountered in output.
  * `"tool_use"` or `"pause_turn"` – these relate to advanced tool usage or long outputs (not typical in simple Q\&A).
  * `"refusal"` – the model stopped itself due to a policy (for instance, it refused to answer the prompt). This can happen if the content is disallowed.
    In non-streaming mode, `stop_reason` will always be set in the final JSON. (`null` is only seen mid-stream, which we’ll discuss.)
* **`stop_sequence`**: If `stop_reason` is `"stop_sequence"`, this field will contain the actual sequence string that caused the stop. Otherwise it is `null`.
* **`type`**: The object type – for message objects this is `"message"`. (The API explicitly returns `"type": "message"` in the response JSON.)
* **`usage`**: This field provides token usage statistics for the request. It typically includes:

  * **`input_tokens`** – how many tokens were in your input (all messages and prompts sent).
  * **`output_tokens`** – how many tokens Claude generated in the response.
    These are useful for billing and monitoring rate limits, since Anthropic (like OpenAI) charges based on token usage. For example, `"usage": { "input_tokens": 2095, "output_tokens": 503 }` indicates the prompt was 2095 tokens and the answer 503 tokens. (The numbers in the docs example are high because they likely included a long prompt or system content. For a simple "Hello" prompt, the token count would be much smaller.)

**Example Response JSON:** If we sent the "Hello, world" example earlier, a possible response could be:

```json
{
  "content": [
    {
      "text": "Hi! My name is Claude.",
      "type": "text"
    }
  ],
  "id": "msg_013Zva2CMHLNnXjNJJKqJ2EF",
  "model": "claude-sonnet-4-20250514",
  "role": "assistant",
  "stop_reason": "end_turn",
  "stop_sequence": null,
  "type": "message",
  "usage": {
    "input_tokens": 2095,
    "output_tokens": 503
  }
}
```

This JSON structure is taken from Anthropic's documentation (the values are an example). In practice, the `input_tokens` count for a simple greeting would be much smaller than 2095 – that high count likely reflects a longer system prompt context used in their example. The key part is the **`content`** array containing Claude's text reply ("Hi! My name is Claude.") and the `stop_reason: "end_turn"` indicating a normal completion.

**Text-Only Output:** As a reminder, Claude's response will always be textual. Even if you enabled some vision features (providing an image in the input), Claude would respond with text (e.g., describing the image). There is no image generation; the `content` blocks may include `"text"`, or in advanced cases `"tool_use"`, `"thinking"`, etc., but not any binary/image data. For most uses, you'll just handle the text content.

## Streaming Responses via SSE

Anthropic's API supports **streaming** the completion, similar to OpenAI's streaming mode, but it uses **Server-Sent Events (SSE)**. Streaming is useful for getting Claude's answer piece by piece (for instance, to start displaying it to a user as it's generated, without waiting for the full completion).

To use streaming, include `"stream": true` in your request JSON. When streaming is enabled, the **HTTP response will not be a single JSON**; instead, the server will keep the connection open and send a sequence of events. Each event is sent as a line (or few lines) prefixed by `event:` and `data:` as per SSE format. You will receive multiple events that gradually build the final answer.

Key points about Anthropic SSE streaming format:

* The response will have the HTTP header `Content-Type: text/event-stream` and it will send data in chunks (events). Each event has an **event name** and a JSON payload in the **data** field.

* **Event sequence:** According to Anthropic's docs, a streaming response for the Messages API follows a particular sequence of event types:

  1. **`message_start`** – indicates the start of the response. This event contains a JSON object representing the message being generated, but initially with empty content.
  2. **`content_block` events** – The model's output may be split into one or more **content blocks**. For each block, you get a `content_block_start` event, followed by one or more `content_block_delta` events, then a `content_block_stop`. These deliver the text content incrementally. Essentially, Claude’s answer is broken into blocks (e.g., paragraphs or segments), and you receive each block token-by-token (or small chunks) as `content_block_delta` events.
  3. **`message_delta` events** – These can update top-level fields of the message (like the usage token counts or stop\_reason) as the stream progresses. For example, as more tokens are generated, you might get a delta event updating the `usage`.
  4. **`message_stop`** – indicates the completion of the streaming response. This is the final event telling you the message is done (and typically includes the final `stop_reason` if not already provided).

* Throughout the stream, you might also receive **`ping`** events (keep-alive heartbeats with no content), and in rare cases **`error`** events if something goes wrong mid-stream (e.g., an `overloaded_error` if the server is under heavy load). Your code should handle or ignore these gracefully.

* During streaming, the `stop_reason` in the data starts as `null` (since the model hasn't stopped yet). It will be filled once the stop condition is reached (end or other reason).

**Example of SSE Data:** The SSE events are text lines. For example, in the older completion API (legacy) a streaming response might look like this:

```
event: completion
data: {"type": "completion", "completion": " Hello", "stop_reason": null, "model": "claude-2.0"}

event: completion
data: {"type": "completion", "completion": "!", "stop_reason": null, "model": "claude-2.0"}

event: ping
data: {"type": "ping"}

event: completion
data: {"type": "completion", "completion": " My", "stop_reason": null, "model": "claude-2.0"}

... [more events] ...
```

Each `completion` event here delivered a chunk of text (`" Hello"`, then `"!"`, then `" My"`, etc.) which you would concatenate to get "Hello! My". This continues until the full sentence "Hello! My name is Claude." is streamed. In the final event, `stop_reason` becomes `"stop_sequence"` (or `"end_turn"` in the new Messages API) to indicate completion.

In the **Messages API (Claude 4)**, the principle is similar, though the event names differ (e.g., `content_block_delta` instead of just `completion`). The *data JSON* in events will include pieces of the `content.text`. You will accumulate these pieces to reconstruct the full reply. Also, once streaming is done, you could combine all the data into a single message object that looks like the final JSON described earlier.

**Important:** SSE is a continuous stream; your HTTP client needs to read from the response stream as data arrives. The connection stays open until the server has sent the final `message_stop` (or an error). If you are using an HTTP client with timeouts, be sure to adjust them as needed, because generating a long response might take some time and you don't want the client to time out prematurely.

## Example Usage in C# (HttpClient)

Finally, let's put this together with a C# example using `HttpClient`. We'll show two scenarios: (1) a simple non-streaming request that gets the full response at once, and (2) a streaming request that reads partial responses.

**Prerequisites:** Ensure you have your API key ready and perhaps store it in a secure manner (e.g., environment variable or config). We'll just use a placeholder in code. We also assume you're using .NET's `System.Net.Http.HttpClient` and `System.Text.Json` for simplicity.

### 1. Non-Streaming Chat Completion Example (C#)

In this example, we send a single user prompt to Claude and get back the full completion in one go.

```csharp
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class AnthropicChatExample
{
    private static readonly HttpClient httpClient = new HttpClient();
    private const string ApiKey = "<YOUR_API_KEY>";

    public static async Task Main()
    {
        // 1. Prepare the HTTP request
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        request.Headers.Add("x-api-key", ApiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");  // latest API version as of now
        request.Content = new StringContent(JsonSerializer.Serialize(new
        {
            model = "claude-sonnet-4-20250514",    // Claude 4 (Sonnet) model
            max_tokens = 300,                     // max tokens for the response
            messages = new[]
            {
                new { role = "user", content = "Write a short greeting poem." }
            }
            // Note: stream is not set (defaults to false) for full response
        }), Encoding.UTF8, "application/json");

        // 2. Send the request
        HttpResponseMessage response = await httpClient.SendAsync(request);

        // 3. Read and parse the response JSON
        string jsonResponse = await response.Content.ReadAsStringAsync();
        Console.WriteLine("Raw JSON response:\n" + jsonResponse);

        // (Optional) Deserialize to an object or dynamic to inspect fields:
        using var doc = JsonDocument.Parse(jsonResponse);
        JsonElement root = doc.RootElement;
        string assistantReply = root.GetProperty("content")[0].GetProperty("text").GetString();
        Console.WriteLine("\nAssistant reply: " + assistantReply);
    }
}
```

**Explanation:** We construct an `HttpRequestMessage` to the `v1/messages` endpoint, attach the required headers (`x-api-key` and `anthropic-version`), and set the JSON body. In the JSON, we specify the model and provide `messages` with one user message. We use `HttpClient.SendAsync` to perform the POST. The response is read as a string (which will be a JSON string in the format discussed above). We then print the raw JSON and also show how to extract the assistant's text from the JSON (by parsing and accessing the `"content"[0]["text"]` field).

Running this code (with a valid API key and if you have access to the Claude model) would print something like:

```
Raw JSON response:
{"content":[{"text":"Hello! It's a pleasure to meet you.","type":"text"}],"id":"msg_...","model":"claude-sonnet-4-20250514","role":"assistant","stop_reason":"end_turn","stop_sequence":null,"type":"message","usage":{"input_tokens":...,"output_tokens":...}}

Assistant reply: Hello! It's a pleasure to meet you.
```

*(The exact content will vary, but it should be a polite greeting poem or sentence from Claude.)*

### 2. Streaming Chat Completion Example (C#)

Next, let's enable streaming. We will send a similar request but with `"stream": true` and then read from the response stream as data arrives.

```csharp
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class AnthropicChatStreamExample
{
    private static readonly HttpClient httpClient = new HttpClient();
    private const string ApiKey = "<YOUR_API_KEY>";

    public static async Task Main()
    {
        // 1. Prepare the HTTP request with stream=true
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        request.Headers.Add("x-api-key", ApiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = new StringContent(JsonSerializer.Serialize(new
        {
            model = "claude-sonnet-4-20250514",
            max_tokens = 300,
            stream = true,  // enable streaming
            messages = new[]
            {
                new { role = "user", content = "Tell me a fun fact about space." }
            }
        }), Encoding.UTF8, "application/json");

        // 2. Send the request, using ResponseHeadersRead to start reading events immediately
        HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        // 3. Get the response content stream and read server-sent events
        using Stream stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);
        string? line;
        string accumulatedText = "";
        Console.WriteLine("Streaming response (SSE events):");
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (line.StartsWith("data: "))
            {
                string dataPayload = line.Substring("data: ".Length);
                if (string.IsNullOrWhiteSpace(dataPayload))
                {
                    // Skip empty data lines (keep-alives)
                    continue;
                }
                // Anthropic might send a termination event differently; check for special cases if any:
                if (dataPayload == "[DONE]") // (Older protocol used "[DONE]" marker)
                {
                    break; // end of stream
                }
                // Parse the JSON fragment in this event:
                JsonDocument doc = JsonDocument.Parse(dataPayload);
                JsonElement dataObj = doc.RootElement;
                // You can inspect the event type:
                if (dataObj.TryGetProperty("type", out JsonElement typeEl))
                {
                    string eventType = typeEl.GetString();
                    if (eventType == "ping" || eventType == "error")
                    {
                        // handle ping or error events if needed
                        continue;
                    }
                }
                // If this event contains a piece of text, extract it
                if (dataObj.TryGetProperty("text", out JsonElement textEl))
                {
                    string textPiece = textEl.GetString();
                    accumulatedText += textPiece;
                    Console.Write(textPiece); // print incremental piece
                }
            }
        }
        Console.WriteLine("\n\nFinal accumulated answer: " + accumulatedText);
    }
}
```

**Explanation:** This code is similar in structure to the first example, but we add `"stream": true` in the JSON. We use `SendAsync` with `HttpCompletionOption.ResponseHeadersRead` – this is crucial to allow us to start processing the response *before* the entire response is finished (since in streaming mode the response potentially "never" fully finishes until all chunks are sent).

We then obtain the response stream via `response.Content.ReadAsStreamAsync()` and read it line by line with a `StreamReader`. SSE sends events separated by newline delimiters. Each event line begins with a prefix (like `event:` or `data:`). We are primarily interested in lines starting with `data: `, which contain JSON payloads.

As we read lines:

* We ignore any line that isn't starting with `data:` (for example, lines starting with `event:` or empty lines between events can be skipped in this simple approach).
* We strip the `"data: "` prefix to get the raw JSON string of the event.
* If the data payload is empty or just a keep-alive, we skip it. In older versions of the API, a literal `data: [DONE]` was used to indicate the end of stream, which we check for (the 2023-06 API version removed the explicit `[DONE]`, using named events instead, but including a check doesn't hurt in case of older versions or compatibility).
* We parse the JSON payload (using `System.Text.Json` here) to inspect it. Each event JSON will have a `"type"` field that indicates what kind of event it is (for example, `"content_block_delta"`, `"message_delta"`, etc., in the Claude 4 Messages API, or `"completion"` in the older API).
* For simplicity, we check if there's a `"text"` field in the JSON. **Content delta events** carry a piece of text in a field usually named `"text"` (since the content blocks are of type text). If present, we append it to our `accumulatedText` string and also print it immediately to the console. This way, as the loop runs, you'll see Claude's answer appear gradually.
* We continue until the stream is finished (the `ReadLineAsync` returns null, which means the server closed the connection after sending all events, or we break out if we detect a `[DONE]` marker or equivalent final event).

During this streaming process, you'll likely see partial words or sentences printed out in real-time. Once the loop ends, `accumulatedText` contains the full answer. We print that as the final accumulated answer.

**Note:** Real SSE parsing can get more complex (handling multi-line events, event types, etc.), but the above is a straightforward approach. Anthropic’s SSE events often send one JSON per `data:` line, which makes parsing easier. Just be mindful of potential `ping` events or other event types and handle/ignore them as needed.

### Things to Remember

* **Async usage:** Both examples use `await` for asynchronous calls (`SendAsync`, `ReadAsStringAsync`, reading lines, etc.), which is recommended for I/O operations to avoid blocking threads.
* **HTTP client reuse:** In these snippets, we used a static `HttpClient`. In a real application, you should reuse `HttpClient` instances or use an `HttpClientFactory`. Also consider setting `httpClient.Timeout = TimeSpan.FromSeconds(X)` appropriately or to `Timeout.InfiniteTimeSpan` if you expect long-running requests (especially for streaming) to avoid the client timing out.
* **No image generation:** Claude's API only returns text. If you send an image (in the new vision feature) as part of the input, the response will be a text description or analysis of the image, not an actual image. And Claude cannot create new images from scratch.
* **Latest model and version:** Using the `claude-sonnet-4-20250514` (or its alias `claude-sonnet-4-0`) ensures you're using the latest Claude v4 model. Always specify the `anthropic-version` header (currently `2023-06-01`) to match the API's expected format. Anthropic may release newer API versions in the future, so check their documentation for updates.

By following the above structure for requests and handling the responses as shown, you can integrate Claude's chat completions into your C# application. The main task is correctly formatting the JSON request with your conversation and parameters, and then parsing the JSON (or SSE events) in the response. Once that is set up, using a library or writing your own wrapper becomes straightforward since you know **what to send** and **what you'll receive** at a detailed level. With this knowledge, you can expand the class library with additional features (like handling errors, using system prompts, tool use, etc.) as needed, building on the solid foundation of request/response handling demonstrated here.

**Sources:**

* Anthropic Official API Documentation – *Messages (Chat) API reference*
* Anthropic Official API Documentation – *Streaming responses (server-sent events)*
* Anthropic Official API Documentation – *Parameter descriptions (temperature, top\_p, stop sequences, etc.)*
* Anthropic Official Release Notes / Versioning (changes in streaming format for 2023-06-01)
* *Anthropic Client SDK examples (Python)* – demonstrating usage of Claude 4 models (used for model naming conventions).
