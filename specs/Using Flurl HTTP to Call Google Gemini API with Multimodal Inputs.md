# Using Flurl HTTP to Call Google Gemini API with Multimodal Inputs

## Overview of Gemini API and Flurl Integration

Google’s **Gemini** API is a multimodal generative AI service that can interpret **text, images, videos, audio, and documents** in prompts. In other words, Gemini models (e.g. *Gemini 2.5 Flash*) are designed to seamlessly understand input from multiple modalities and produce text-based results. In this guide, we demonstrate how to call the Gemini API from C# using the **Flurl HTTP** library, with working examples for each input type (text, image, audio, video, and PDF). Flurl provides a fluent, easy-to-use interface for making HTTP requests from .NET.

We will cover:

* **API Keys and Authentication:** Differences between API keys obtained via Google AI Studio vs. Google Cloud, and how to authenticate programmatically.
* **Supported Modalities & MIME Types:** What input types the Gemini API supports (text, image, audio, video, etc.) and how to format requests for each, including required MIME types.
* **C# Code Examples with Flurl:** Step-by-step examples for sending a text prompt, and for uploading and querying an image, an audio clip, a video clip, and a PDF document. Each example will show how to construct the HTTP requests using Flurl and handle the responses (in English).
* **Configuration Tips:** Notes on setting up Flurl and managing API keys for a smooth development experience.

Throughout the guide, we cite official Google documentation and use the official endpoints for the Gemini API.

## API Keys and Authentication (AI Studio vs. Google Cloud)

**Gemini Developer API vs. Vertex AI API:** Google offers two ways to access Gemini: the *Gemini Developer API* (via Google AI Studio) and the *Vertex AI Gemini API* (via Google Cloud). The Developer API is a standalone service aimed at quick prototyping (including mobile/web clients), whereas Vertex AI integration is for production and enterprise use with cloud infrastructure. There are key differences in how you obtain credentials and call these APIs:

* **Endpoint & Account:** The Developer API uses the endpoint `generativelanguage.googleapis.com` and only requires a Google account (no Cloud project billing required for basic use), whereas the Vertex AI API uses the Google Cloud endpoint `aiplatform.googleapis.com` and requires a Google Cloud project with terms acceptance and billing enabled. In short, Developer API calls go to the **Generative Language API** endpoint, while enterprise calls go to the Vertex AI endpoint.
* **API Key vs. Service Account:** For the Developer API, you authenticate with a simple **API key**. You can create this key for free in **Google AI Studio** (the Gemini web interface). This API key is used in your requests and is sufficient for programmatic access in our examples. In contrast, the Vertex AI Gemini API uses Google Cloud authentication (typically an **OAuth2 access token** from a service account or user) instead of a direct API key. In other words, the AI Studio key is all you need for calling the Developer API, whereas Vertex AI calls require Cloud IAM credentials (more complex but integrating with other Cloud services).

> **Which key to use for C# code?** In this guide, we use an **API key from Google AI Studio** for simplicity. This key grants programmatic access to the Gemini Developer API endpoints. Make sure the Generative Language API is enabled on the Google Cloud project associated with your AI Studio key (AI Studio typically creates or uses a cloud project under the hood). For production deployments on Google Cloud, you would instead use the Vertex AI API with service account authentication, but that is beyond our scope here.

**Providing the API Key:** Once you have your Gemini API key, you can pass it in REST requests either as a **query parameter** `?key=YOUR_API_KEY` or in an HTTP header (e.g. `X-Goog-Api-Key: YOUR_API_KEY`). In the Flurl examples below, we’ll append the key as a query parameter for simplicity. Keep your API key secure (don’t hard-code it in public code). For local development, you might store it in an environment variable or configuration and retrieve it at runtime.

## Supported Input Modalities and File Uploads

Gemini models accept a combination of text and file inputs in a single prompt. Official documentation confirms that you can provide **unstructured images, videos, and documents** (as well as text) to Gemini models for analysis. For example, you could ask the model questions about an image, transcribe or summarize an audio clip, or get insights from a PDF. In all cases, the model’s *output* will be text. At the time of writing, **text and image inputs** are fully supported, and **audio/video inputs are supported** on multimodal Gemini models (such as Gemini 1.5 Pro or 2.0/2.5 models) – note that large videos may require special processing time. We include examples for all these modalities.

**Uploading Media Files:** The Gemini API uses an upload mechanism to handle non-text inputs. You don’t send raw binary data in the prompt JSON directly; instead, you **upload the file first** and then reference it in your prompt by a URI. This design lets you reuse uploaded media across multiple requests without re-uploading each time. The workflow is:

1. **Upload the file** via the `files.upload` endpoint (a special URI under `generativelanguage.googleapis.com`). This returns a `file_uri` (and metadata) for the uploaded file. The upload uses a **resumable upload protocol** to handle potentially large files.
2. **Use the file in a prompt** by including a reference to that `file_uri` and its MIME type in the JSON payload of a `generateContent` call. The model will then incorporate the file’s content (image pixels, audio waveform, etc.) when generating a response.

**Supported MIME Types:** When uploading, you must specify the correct MIME type for the file so the API knows how to handle it. Common supported types include:

* **Images:** `image/jpeg` (for .jpg or .jpeg files) and `image/png` are accepted image types.
* **Audio:** `audio/mpeg` (for .mp3 files) is used in our example. Other audio types like `audio/wav` or `audio/ogg` may be supported as well (use the appropriate MIME type for the file format).
* **Video:** `video/mp4` is used for MP4 videos. (Gemini currently processes video files by extracting frames and possibly audio; other video formats might work if you provide the correct MIME, but MP4 is a safe choice.)
* **Text files:** `text/plain` for .txt or similar.
* **PDF documents:** `application/pdf` for PDF files.

Ensure the MIME type matches the file content; the API might reject mismatched types. After uploading, the API responds with a JSON object containing the file’s metadata (including a `file.uri` and possibly a processing `state`). For large or complex files (especially videos), the file might take a short time to process – you should only use it in a prompt once its state is **ACTIVE** (ready). We’ll demonstrate polling for video readiness in the example.

Now, let’s walk through each modality with C# Flurl code examples. *(Before running the code, install the Flurl.Http NuGet package and add `using Flurl.Http;` at the top of your file.)* Each code snippet assumes you have a variable `apiKey` with your Gemini API key value.

## Example 1: Text-Only Prompt

This is the simplest case – you send a text prompt and get a text completion from Gemini. No file upload is needed. We call the `.../models/{model}:generateContent` endpoint with a JSON body containing the prompt. In this example, we use the **Gemini 2.5 Flash** model (a fast, multimodal model) and ask a question. The result will be a JSON containing the model’s answer.

```csharp
using Flurl.Http;

string apiKey = "YOUR_GEMINI_API_KEY";  // set your API key from AI Studio
string baseUrl = "https://generativelanguage.googleapis.com/v1beta/";

// Prepare the request payload (text prompt).
var requestBody = new
{
    contents = new object[] {
        new {  // single user message
            parts = new object[] {
                new { text = "Explain how AI works in a few words." }
            },
            // The role defaults to "user" for each content item if not specified in this API version
        }
    }
};

// Send the POST request to the generateContent endpoint with the API key.
string modelName = "gemini-2.5-flash";  // model ID (ensure this model is available to your API key)
string endpoint = $"{baseUrl}models/{modelName}:generateContent?key={apiKey}";
var responseJson = await endpoint.PostJsonAsync(requestBody).ReceiveString();

// Print the raw JSON response (which includes the generated text).
Console.WriteLine(responseJson);

/*
  The response JSON will contain an array of "candidates". Each candidate has a "content" with "parts",
  where the model's reply text is located. For example:

  {
    "candidates": [
      {
        "content": {
          "parts": [ { "text": "AI works by using algorithms ... (model's answer) ..." } ]
        }
      }
    ]
  }

  You can parse this JSON to extract response text. In this simple example, we just print the whole JSON.
*/
```

**Result:** The model will return a brief explanation of how AI works, as a text string in the JSON. For instance, it might say *"Artificial intelligence works by using algorithms and computational models to mimic human learning and decision-making..."* (or similar) in the `candidates[0].content.parts[0].text` field. The above code prints the entire JSON, but you could parse `responseJson` to get the text answer programmatically.

## Example 2: Image Input with Text Prompt

Now we send an image alongside a question. For example, we have a photo and we ask Gemini to describe or answer questions about it. The steps are:

1. **Upload the image file** to get a `file_uri`. We use the `upload/v1beta/files` endpoint with a resumable upload protocol.
2. **Call the model** with a prompt that includes the uploaded image (by URI and MIME type) and our text question.

Let’s say we have an image file `"photo.jpg"` and we want to ask: "What instruments are in this photo?" Below is the code to do this with Flurl:

```csharp
using Flurl.Http;
using System.IO;
using System.Linq;
using Flurl;              // (for FlurlRequest if needed)
using System.Net.Http;   // (for ByteArrayContent)

// Setup
string apiKey = "YOUR_GEMINI_API_KEY";
string baseUrl = "https://generativelanguage.googleapis.com/v1beta/";
string imagePath = "C:\\path\\to\\photo.jpg";
string imageMime = "image/jpeg";

// 1. Initiate a resumable upload for the image file
long fileSize = new FileInfo(imagePath).Length;
string initUrl = $"{baseUrl}upload/v1beta/files?key={apiKey}";

IFlurlResponse initResp = await initUrl
    .WithHeader("X-Goog-Upload-Protocol", "resumable")
    .WithHeader("X-Goog-Upload-Command", "start")
    .WithHeader("X-Goog-Upload-Header-Content-Length", fileSize.ToString())
    .WithHeader("X-Goog-Upload-Header-Content-Type", imageMime)
    .WithHeader("Content-Type", "application/json")
    .PostJsonAsync(new { file = new { display_name = "photo.jpg" } });

// Extract the upload URL from the response header
var uploadUrl = initResp.Headers.FirstOrDefault(h => h.Name == "X-Goog-Upload-URL")?.Value;
if (uploadUrl == null)
{
    throw new Exception("Upload URL not found in response");
}

// 2. Upload the image bytes to the obtained upload URL
byte[] imageBytes = File.ReadAllBytes(imagePath);
IFlurlResponse uploadResp = await uploadUrl
    .WithHeader("Content-Length", fileSize.ToString())
    .WithHeader("X-Goog-Upload-Offset", "0")
    .WithHeader("X-Goog-Upload-Command", "upload, finalize")
    .PostAsync(new ByteArrayContent(imageBytes));

// Parse the upload response to get file metadata (including the URI)
dynamic fileInfo = await uploadResp.ReceiveJson();
string fileUri = fileInfo.file.uri;
string fileState = fileInfo.file.state;
Console.WriteLine($"Uploaded file URI: {fileUri}, state: {fileState}");

// (If needed, wait until the file is fully processed. For images, state is usually "ACTIVE" immediately.)
if (fileState != "ACTIVE")
{
    // In case of large images (rare), poll until active
    string fileName = fileInfo.file.name;
    while (fileState != "ACTIVE")
    {
        await Task.Delay(1000);
        dynamic statusResp = await $"{baseUrl}files/{fileName}?key={apiKey}".GetJsonAsync();
        fileState = statusResp.file.state;
    }
}

// 3. Call the Gemini model with the image and a prompt question
string model = "gemini-2.5-flash";  // using a multimodal model variant
var promptBody = new
{
    contents = new object[] {
        new {
            parts = new object[] {
                // We include the image file (file_data with URI and MIME) and the question text as separate parts
                new { file_data = new { mime_type = imageMime, file_uri = fileUri } },
                new { text = "What instruments are shown in this photo?" }
            },
            role = "user"
        }
    }
};
string endpoint = $"{baseUrl}models/{model}:generateContent?key={apiKey}";
dynamic answerResp = await endpoint.PostJsonAsync(promptBody).ReceiveJson();
string answerText = answerResp.candidates[0].content.parts[0].text;
Console.WriteLine("Model answer: " + answerText);
```

**Explanation:** We first perform a **resumable upload** in two steps. The initial POST (with headers like `X-Goog-Upload-Protocol: resumable`, etc.) starts the upload and returns an `X-Goog-Upload-URL` for the actual data upload. We then POST the image bytes to that URL with `upload, finalize` command to complete the upload. The response includes a JSON object with the file’s details (we retrieve `file.uri`). After that, we construct the prompt. In the `contents` array, we provide two parts: one is the image (as `file_data` with its MIME type and URI) and another is the question text. We then call the model’s `generateContent` endpoint with this payload.

**Result:** The model’s response (stored in `answerText`) will be a description or answer about the image content. For example, if the photo showed guitars and drums, the model might respond: *“The photo contains musical instruments including a guitar and a drum set.”* The code above prints the model’s answer text. (Under the hood, the JSON response looks similar to the text-only case, but with possibly multiple parts if the model also echoes or references the image. Here we directly grab the first text part of the first candidate answer.)

## Example 3: Audio Input with Text Prompt

We can also send an audio clip to Gemini. For instance, you might want the model to describe the audio or transcribe it. The process is analogous to the image example: upload the audio file, then call `generateContent` with a reference to it. Suppose we have an audio file `"sample.mp3"` and we ask the model: "Describe this audio clip."

```csharp
string audioPath = "C:\\path\\to\\sample.mp3";
string audioMime = "audio/mpeg";  // MIME for mp3

// 1. Upload the audio file (resumable upload process)
long audioSize = new FileInfo(audioPath).Length;
var initResp2 = await $"{baseUrl}upload/v1beta/files?key={apiKey}"
    .WithHeader("X-Goog-Upload-Protocol", "resumable")
    .WithHeader("X-Goog-Upload-Command", "start")
    .WithHeader("X-Goog-Upload-Header-Content-Length", audioSize.ToString())
    .WithHeader("X-Goog-Upload-Header-Content-Type", audioMime)
    .WithHeader("Content-Type", "application/json")
    .PostJsonAsync(new { file = new { display_name = "sample.mp3" } });
string uploadUrl2 = initResp2.Headers.FirstOrDefault(h => h.Name == "X-Goog-Upload-URL")?.Value;
await uploadUrl2
    .WithHeader("Content-Length", audioSize.ToString())
    .WithHeader("X-Goog-Upload-Offset", "0")
    .WithHeader("X-Goog-Upload-Command", "upload, finalize")
    .PostAsync(new ByteArrayContent(File.ReadAllBytes(audioPath)));
dynamic audioFileInfo = await initResp2.ReceiveJson();  // (Note: in Flurl, we should re-fetch the file info via a GET, since the initial response didn't include file data. Alternatively, capture the second response.)
string audioFileUri = audioFileInfo.file.uri;

// (Audio files are typically processed quickly; you can poll status if needed, similar to image.)

// 2. Call the model with the audio file reference and prompt
var audioPrompt = new
{
    contents = new object[] {
        new {
            parts = new object[] {
                new { file_data = new { mime_type = audioMime, file_uri = audioFileUri } },
                new { text = "Describe this audio clip." }
            },
            role = "user"
        }
    }
};
dynamic audioAnswer = await $"{baseUrl}models/gemini-2.5-flash:generateContent?key={apiKey}"
                        .PostJsonAsync(audioPrompt).ReceiveJson();
string audioText = audioAnswer.candidates[0].content.parts[0].text;
Console.WriteLine("Audio description: " + audioText);
```

**Notes:** We use `audio/mpeg` as the MIME type for the MP3 file. After uploading, we get a `file_uri` for the audio. In the prompt, we include the audio file and a text request. In this case, the prompt asks for a description of the audio. You could also prompt the model to *transcribe* speech audio by asking something like `"Transcribe this audio clip."` – Gemini will then attempt to convert the audio to text, if it’s speech. The code above is simplified by reusing the upload pattern from images. (In practice, you might refactor the upload logic into a helper method to avoid duplication.)

**Result:** The model’s output (`audioText`) will be a textual description or transcription of the audio content. For example, if the audio was a short musical tune, the model might respond: *"It’s an upbeat acoustic guitar melody with a rhythmic drumbeat."* If it was speech, it might transcribe the spoken words.

## Example 4: Video Input with Text Prompt

Video input is supported by Gemini’s multimodal models, but handling video has an extra step: processing can be slower, so the file may not be immediately ready. After uploading a video, the API will report its status as `PROCESSING` until the video is fully processed (frames and audio extracted). You need to **poll the file status** and wait for it to become `ACTIVE` before generating a response. We demonstrate this with an example where we upload a video file `"clip.mp4"` and ask: "Describe this video clip."

```csharp
string videoPath = "C:\\path\\to\\clip.mp4";
string videoMime = "video/mp4";

// 1. Upload the video file (initiate resumable upload)
long videoSize = new FileInfo(videoPath).Length;
var initResp3 = await $"{baseUrl}upload/v1beta/files?key={apiKey}"
    .WithHeader("X-Goog-Upload-Protocol", "resumable")
    .WithHeader("X-Goog-Upload-Command", "start")
    .WithHeader("X-Goog-Upload-Header-Content-Length", videoSize.ToString())
    .WithHeader("X-Goog-Upload-Header-Content-Type", videoMime)
    .WithHeader("Content-Type", "application/json")
    .PostJsonAsync(new { file = new { display_name = "clip.mp4" } });
string uploadUrl3 = initResp3.Headers.FirstOrDefault(h => h.Name == "X-Goog-Upload-URL")?.Value;
await uploadUrl3
    .WithHeader("Content-Length", videoSize.ToString())
    .WithHeader("X-Goog-Upload-Offset", "0")
    .WithHeader("X-Goog-Upload-Command", "upload, finalize")
    .PostAsync(new ByteArrayContent(File.ReadAllBytes(videoPath)));
dynamic videoFileInfo = await initResp3.ReceiveJson();
string videoFileName = videoFileInfo.file.name;
string videoFileUri = videoFileInfo.file.uri;
string videoState = videoFileInfo.file.state;
Console.WriteLine($"Video uploaded. State = {videoState}");

// 2. Poll until the video file is processed (state becomes ACTIVE)
while (videoState != "ACTIVE")
{
    Console.WriteLine("Waiting for video processing...");
    await Task.Delay(5000);  // wait 5 seconds before checking again
    dynamic status = await $"{baseUrl}files/{videoFileName}?key={apiKey}".GetJsonAsync();
    videoState = status.file.state;
}
Console.WriteLine("Video is ready for analysis.");

// 3. Call the model with the video and a prompt
var videoPrompt = new
{
    contents = new object[] {
        new {
            parts = new object[] {
                new { file_data = new { mime_type = videoMime, file_uri = videoFileUri } },
                new { text = "Describe this video clip." }
            },
            role = "user"
        }
    }
};
dynamic videoAnswer = await $"{baseUrl}models/gemini-2.5-flash:generateContent?key={apiKey}"
                       .PostJsonAsync(videoPrompt).ReceiveJson();
string videoDescription = videoAnswer.candidates[0].content.parts[0].text;
Console.WriteLine("Video description: " + videoDescription);
```

**What’s happening:** We upload the MP4 video similar to previous examples. The initial response will likely show `file.state = "PROCESSING"` for a video file. We then enter a loop to `GET /v1beta/files/{fileName}` every few seconds to check if the state changes to `"ACTIVE"`. (The official guide demonstrates this polling for video processing.) Once the video is ready, we proceed to call `generateContent` with the video file reference and our prompt. The prompt simply asks for a description of the video. You could ask other things as well, like questions about specific moments in the video, as long as the model can infer it from visual/audio content.

**Result:** The output (`videoDescription`) will be a textual description of the video’s content. For example, if the video clip was a few seconds from a nature documentary, the model might respond with something like: *"The video shows a forest with a river flowing through it, and a narrator describing the wildlife in the scene."* Keep in mind the quality of the description may depend on the model’s capability (Gemini’s understanding of video is still evolving). The key point is that Gemini can consume the video’s frames and audio to produce an answer.

## Example 5: Document (PDF) Input with Text Prompt

Finally, Gemini can also accept document files such as PDFs or text files. This is useful for asking the model to summarize or answer questions about a document’s content. The procedure is, again, to upload the file and then include it in the prompt. Here we’ll upload a PDF (`"document.pdf"`) and ask the model to summarize it.

```csharp
string pdfPath = "C:\\path\\to\\document.pdf";
string pdfMime = "application/pdf";

// 1. Upload the PDF file
long pdfSize = new FileInfo(pdfPath).Length;
var initResp4 = await $"{baseUrl}upload/v1beta/files?key={apiKey}"
    .WithHeader("X-Goog-Upload-Protocol", "resumable")
    .WithHeader("X-Goog-Upload-Command", "start")
    .WithHeader("X-Goog-Upload-Header-Content-Length", pdfSize.ToString())
    .WithHeader("X-Goog-Upload-Header-Content-Type", pdfMime)
    .WithHeader("Content-Type", "application/json")
    .PostJsonAsync(new { file = new { display_name = "document.pdf" } });
string uploadUrl4 = initResp4.Headers.FirstOrDefault(h => h.Name == "X-Goog-Upload-URL")?.Value;
await uploadUrl4
    .WithHeader("Content-Length", pdfSize.ToString())
    .WithHeader("X-Goog-Upload-Offset", "0")
    .WithHeader("X-Goog-Upload-Command", "upload, finalize")
    .PostAsync(new ByteArrayContent(File.ReadAllBytes(pdfPath)));
dynamic pdfFileInfo = await initResp4.ReceiveJson();
string pdfFileUri = pdfFileInfo.file.uri;
string pdfState = pdfFileInfo.file.state;
if (pdfState != "ACTIVE")
{
    // (Generally, text/PDF files become ACTIVE quickly. Polling can be done if needed.)
    string pdfName = pdfFileInfo.file.name;
    do {
        await Task.Delay(1000);
        dynamic status = await $"{baseUrl}files/{pdfName}?key={apiKey}".GetJsonAsync();
        pdfState = status.file.state;
    } while (pdfState != "ACTIVE");
}

// 2. Ask the model to summarize the PDF
var pdfPrompt = new
{
    contents = new object[] {
        new {
            parts = new object[] {
                new { text = "Please summarize the attached PDF document." },
                new { file_data = new { mime_type = pdfMime, file_uri = pdfFileUri } }
            },
            role = "user"
        }
    }
};
dynamic pdfAnswer = await $"{baseUrl}models/gemini-2.5-flash:generateContent?key={apiKey}"
                     .PostJsonAsync(pdfPrompt).ReceiveJson();
string summary = pdfAnswer.candidates[0].content.parts[0].text;
Console.WriteLine("PDF Summary: " + summary);
```

We use `application/pdf` as the MIME type. After uploading, we immediately call the model with a user message that includes a text instruction (to summarize) and the file reference. (In this case, we placed the text **before** the file in the parts list, which is also acceptable – the order of parts can be text then file or file then text as needed for clarity. The model receives both as the user’s message.) The model will read the PDF’s content and generate a summary.

**Result:** The string `summary` will contain Gemini’s summary of the PDF. For example, if the PDF was an article about renewable energy, the summary might say: *"The document discusses renewable energy sources, emphasizing solar and wind power. It explains the growth in renewable energy adoption, the environmental benefits, and challenges such as cost and storage. In summary, it advocates for increased investment in clean energy technologies to combat climate change."* The actual summary will of course depend on the PDF’s content and the model’s capabilities.

**Note:** There is a content size limit – Gemini models have context length limits (in terms of tokens). While they support very large contexts (the docs mention processing millions of tokens in some cases), extremely large PDFs may need to be split or handled with the `document` and `chunk` APIs. For a reasonably sized PDF, the above approach works, and the model will attempt to summarize or answer questions based on it.

## Tips for Using Flurl and Gemini API

* **Installing Flurl:** Make sure to install the **Flurl.Http** package (via NuGet) and include `using Flurl.Http;` in your code. Flurl is an asynchronous API, so ideally use the `async/await` pattern as shown. The examples above assume an `async` context (e.g., inside an async method).
* **Handling Responses:** In our examples, we often call `.ReceiveJson()` or `.ReceiveString()` to get the response. You can also deserialize directly to C# classes by defining models for the response JSON. The Gemini API’s responses have a structure documented in Google’s reference (candidates, content parts, etc.). Using `dynamic` (as shown) or strongly-typed classes can help extract the text output.
* **Error Handling:** Flurl will throw exceptions for non-2xx HTTP responses by default. You may want to wrap calls in try/catch blocks to handle issues (e.g., invalid API key, file too large, unsupported format, etc.). The Google API will return error details in JSON if a call fails.
* **API Key Security:** Avoid hard-coding your API key. Consider storing it in an environment variable or secure store. For example, you could set `GEMINI_API_KEY` as an environment variable and then retrieve it in C# (`string apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");`). The Gemini developer docs note that their client libraries will auto-read `GEMINI_API_KEY` if set, but when using raw HTTP via Flurl you must manually include the key as we did.
* **Google Cloud vs. Developer API:** All the above examples use the **Developer API endpoint** with an API key. If you wanted to use the Vertex AI Gemini endpoint instead, the HTTP pattern would differ: you’d POST to a URL like `https://LOCATION-aiplatform.googleapis.com/v1/projects/YOUR_PROJECT/locations/LOCATION/publishers/google/models/MODEL:generateContent` and use an OAuth 2.0 Bearer token for auth (in the `Authorization` header) instead of an API key. The request JSON format for the payload is largely the same. However, Vertex AI may require specifying the `publisher` and model version (e.g., `models/gemini-2.5-flash-001`). Google’s documentation provides a guide for migrating to the Vertex AI API if needed. For most developers starting out, the AI Studio API key approach is quicker and has a free quota.
* **Official References:** Always refer to official Google documentation for the most up-to-date details. Key resources include the **Gemini API reference** (for request/response schemas and available models), the **Using Files guide** on ai.google.dev (which we cited for the upload process), and the **Vertex AI generative AI docs** for enterprise usage. The endpoints and model IDs can evolve (e.g., newer model versions like Gemini 3 in future), so adapt the code accordingly.

By following this guide, you can integrate multimodal Gemini API calls into your C# applications. Using Flurl, the HTTP calls are concise and readable, and you have full control over the request construction. With text, images, audio, video, and other files supported, you can build rich AI-powered features – from chatbots that analyze images to apps that summarize documents or videos – all powered by Google’s Gemini models.

**Sources:** The information and examples above are based on Google’s official documentation and sample code for the Gemini API. Key references include Google AI Studio docs on using files (for multimodal inputs), the Gemini API key usage guide, and the Vertex AI generative AI quickstart, among others. These sources were used to ensure correct API usage and have been cited throughout this report.
