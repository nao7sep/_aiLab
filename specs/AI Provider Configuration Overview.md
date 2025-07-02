# AI Provider Configuration Overview

This document describes the current configuration of the AI providers integrated into your system. Each section outlines the selected models, their purpose, and how they support different modalities (text, image, audio, video).

## OpenAI Configuration

```json
"OpenAiConfig": {
  "ApiHost": "api.openai.com",
  "ChatModel": "gpt-4.1",
  "ImageModel": "gpt-image-1",
  "AudioModel": "gpt-4o",
  "TimeoutSeconds": 30
}
```

**Explanation:**

| Key                    | Value         | Description                                                                |
| ---------------------- | ------------- | -------------------------------------------------------------------------- |
| `ChatModel`            | `gpt-4.1`     | Used for text-centric tasks requiring high reasoning and language quality. |
| `ImageModel`           | `gpt-image-1` | Used for generating high-quality images from text prompts.                 |
| `AudioModel`           | `gpt-4o`      | Used specifically for audio input/output (e.g., transcription, voice).     |
| `TimeoutSeconds`       | `30`          | Maximum duration to wait for a response from the OpenAI API.               |

**Rationale:**
The default chat model prioritizes text performance (`gpt-4.1`). Audio support is delegated to `gpt-4o` to maintain reasoning quality without compromising text output.

## Anthropic Configuration

```json
"AnthropicConfig": {
  "ApiHost": "api.anthropic.com",
  "ChatModel": "claude-sonnet-4",
  "TimeoutSeconds": 30
}
```

**Explanation:**

| Key              | Value             | Description                                                            |
| ---------------- | ----------------- | ---------------------------------------------------------------------- |
| `ChatModel`      | `claude-sonnet-4` | Optimized for balanced text reasoning and conversational capabilities. |
| `TimeoutSeconds` | `30`              | Request timeout for Claude API interactions.                           |

**Note:**
Claude currently supports image input but not image generation. This config uses Sonnet 4 for a good balance of cost and performance.

## Google (Vertex AI) Configuration

```json
"GoogleConfig": {
  "ApiHost": "asia-northeast1-aiplatform.googleapis.com",
  "ChatModel": "gemini-2.5-pro",
  "ImageModel": "imagen-4",
  "VideoModel": "veo-3",
  "TimeoutSeconds": 30
}
```

**Explanation:**

| Key                    | Value                                       | Description                                                                   |
| ---------------------- | ------------------------------------------- | ----------------------------------------------------------------------------- |
| `ApiHost`              | `asia-northeast1-aiplatform.googleapis.com` | Optimized for Japanese users with lowest latency via Tokyo region.            |
| `ChatModel`            | `gemini-2.5-pro`                            | High-performance multimodal model for advanced chat, reasoning, and planning. |
| `ImageModel`           | `imagen-4`                                  | Used for generating images based on detailed prompts.                         |
| `VideoModel`           | `veo-3`                                     | Used to generate short video content from descriptions.                       |
| `TimeoutSeconds`       | `30`                                        | Request timeout for Vertex AI endpoints.                                      |

**Note:**
Using the `asia-northeast1` (Tokyo) region ensures optimal latency and avoids cross-region overhead for Japanese users.

## xAI Configuration

```json
"XaiConfig": {
  "ApiHost": "api.x.ai",
  "ChatModel": "grok-3",
  "ImageModel": "grok-2-image",
  "TimeoutSeconds": 30
}
```

**Explanation:**

| Key                    | Value          | Description                                       |
| ---------------------- | -------------- | ------------------------------------------------- |
| `ChatModel`            | `grok-3`       | Used for text generation tasks. Not multimodal.   |
| `ImageModel`           | `grok-2-image` | Used for generating images based on text prompts. |
| `TimeoutSeconds`       | `30`           | Request timeout for xAI API interactions.         |

**Note:**
Grok is currently not multimodal. A separate model is explicitly defined for image generation.

## Summary

This configuration is designed for:

* High-quality chat and reasoning models per provider
* Clear separation of modality-specific models (image, audio, video)
* Regional optimization for low latency (especially for Japanese users)
* Consistent timeouts and flexible upgrade paths
