using System.IO;
using Microsoft.Extensions.AI;

namespace Eppie.AI
{
    public class Service
    {
        private readonly int defaultTopK = 50;
        private readonly float defaultTopP = 0.9f;
        private readonly float defaultTemperature = 1;
        private readonly int defaultMaxLength = 1024;

        private ChatOptions? _modelOptions;
        private IChatClient _model;

        public async Task LoadModelAsync(string modelPath)
        {
            UnloadModel();

            var name = GetModelName(modelPath);

            switch (name)
            {
                case string p when p.Contains(phi):
                    {
                        _model = await GenAIModel.CreateAsync(modelPath, new LlmPromptTemplate
                        {
                            System = "<|system|>\n{{CONTENT}}<|end|>\n",
                            User = "<|user|>\n{{CONTENT}}<|end|>\n",
                            Assistant = "<|assistant|>\n{{CONTENT}}<|end|>\n",
                            Stop = new[] { "<|system|>", "<|user|>", "<|assistant|>", "<|end|>" }
                        }).ConfigureAwait(false);
                    }
                    break;
                case string d when d.Contains(deepseek):
                    {
                        _model = await GenAIModel.CreateAsync(modelPath, new LlmPromptTemplate
                        {
                            System = "<｜begin▁of▁sentence｜>{{CONTENT}}",
                            User = "<｜User｜>{{CONTENT}}",
                            Assistant = "<｜Assistant｜>{{CONTENT}}<｜end▁of▁sentence｜>",
                            Stop = new[] { "<｜User｜>", "<｜Assistant｜>", "<｜end▁of▁sentence｜>", "<｜begin▁of▁sentence｜>" }
                        }).ConfigureAwait(false);
                    }
                    break;
                case string l when l.Contains(llama):
                    {
                        _model = await GenAIModel.CreateAsync(modelPath, new LlmPromptTemplate
                        {
                            System = "<|start_header_id|>system<|end_header_id|>\n{{CONTENT}}<|eot_id|>",
                            User = "<|start_header_id|>user<|end_header_id|>\n{{CONTENT}}<|eot_id|>",
                            Assistant = "<|start_header_id|>assistant<|end_header_id|>\n{{CONTENT}}<|eot_id|>",
                            Stop = new[] { "<|start_header_id|>", "<|end_header_id|>", "<|eot_id|>" }
                        }).ConfigureAwait(false);
                    }
                    break;
                case string m when m.Contains(mistral):
                    {
                        _model = await GenAIModel.CreateAsync(modelPath, new LlmPromptTemplate
                        {
                            User = "[INST]{{CONTENT}}[/INST] ",
                            Stop = new[] { "[INST]", "[/INST]" }
                        }).ConfigureAwait(false);
                    }
                    break;
                case string q when q.Contains(qwen):
                    {
                        _model = await GenAIModel.CreateAsync(modelPath, new LlmPromptTemplate
                        {
                            System = "<|im_start|>system\n{{CONTENT}}<|im_end|>\n",
                            User = "<|im_start|>user\n{{CONTENT}}<|im_end|>\n",
                            Assistant = "<|im_start|>assistant\n{{CONTENT}}<|im_end|>",
                            Stop = new[] { "<|im_start|>", "<|im_end|>" }
                        }).ConfigureAwait(false);
                    }
                    break;
                case string g when g.Contains(gemma):
                    {
                        _model = await GenAIModel.CreateAsync(modelPath, new LlmPromptTemplate
                        {
                            System = "<start_of_turn>user\n{{CONTENT}}<end_of_turn>\n",
                            User = "<start_of_turn>model\n{{CONTENT}}<end_of_turn>\n",
                            Stop = new[] { "<start_of_turn>", "<end_of_turn>" }
                        }).ConfigureAwait(false);
                    }
                    break;
            }

            _modelOptions = GetDefaultChatOptions(_model);
        }

        public void UnloadModel()
        {
            _model?.Dispose();
            _model = null;
        }

        public async Task<string> ProcessTextAsync(string systemPrompt, string text, ChatOptions options, CancellationToken cts, Action<string> onTextUpdate = null)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var result = string.Empty;
            text = text.Trim();

            var modelOptions = _modelOptions;

            if (options != null)
            {
                modelOptions.TopP = options.TopP;
                modelOptions.TopK = options.TopK;
                modelOptions.Temperature = options.Temperature;
                modelOptions.AdditionalProperties = options.AdditionalProperties;
            }

            await Task.Run(
                async () =>
                {
                    await foreach (var messagePart in _model.CompleteStreamingAsync(
                        new List<ChatMessage>
                        {
                            new ChatMessage(ChatRole.System, systemPrompt),
                            new ChatMessage(ChatRole.User, text)
                        },
                        modelOptions,
                        cts).ConfigureAwait(false))
                    {
                        result += messagePart.Text;
                        onTextUpdate?.Invoke(messagePart.Text);
                    }
                }, cts).ConfigureAwait(false);

            return RemoveThinkBlockIfExists(result);
        }

        private static string RemoveThinkBlockIfExists(string text)
        {
            // Deletion of <think> block if it exists
            const string BeginThinkBlock = "<think>";
            const string EndThinkBlock = "</think>";
            if (text.StartsWith(BeginThinkBlock, StringComparison.InvariantCultureIgnoreCase))
            {
                int startIndex = text.IndexOf(BeginThinkBlock, StringComparison.InvariantCultureIgnoreCase);
                int endIndex = text.IndexOf(EndThinkBlock, StringComparison.InvariantCultureIgnoreCase) + EndThinkBlock.Length;
                if (startIndex != -1 && endIndex != -1)
                {
                    text = text.Remove(startIndex, endIndex - startIndex);
                }
            }

            return text.Trim();
        }

        public ChatOptions GetDefaultChatOptions(IChatClient? chatClient)
        {
            var chatOptions = chatClient?.GetService<ChatOptions>();
            return chatOptions ?? new ChatOptions
            {
                MaxOutputTokens = defaultMaxLength,
                Temperature = defaultTemperature,
                TopP = defaultTopP,
                TopK = defaultTopK,
            };
        }

        const string phi = "phi";
        const string deepseek = "deepseek";
        const string llama = "llama";
        const string mistral = "mistral";
        const string qwen = "qwen";
        const string gemma = "gemma";

        static public string GetModelName(string path)
        {
            switch (path.ToLowerInvariant())
            {
                case string p when p.Contains(phi):
                    return phi;
                case string d when d.Contains(deepseek):
                    return deepseek;
                case string l when l.Contains(llama) || l.Contains("nemotron"):
                    return llama;
                case string m when m.Contains(mistral):
                    return mistral;
                case string q when q.Contains(qwen):
                    return qwen;
                case string g when g.Contains(gemma):
                    return gemma;
                default:
                    return null;
            }
        }
    }
}
