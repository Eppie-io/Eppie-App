using Microsoft.Extensions.AI;

namespace Eppie.AI
{
    public class Service
    {
        private readonly int defaultTopK = 50;
        private readonly float defaultTopP = 0.9f;
        private readonly float defaultTemperature = 0.5f;
        private readonly int defaultMaxLength = 1024;

        private ChatOptions? _modelOptions;
        private IChatClient _model;

        public async Task LoadModelAsync(string modelPath)
        {
            UnloadModel();

            // Phi3
            _model = await GenAIModel.CreateAsync(modelPath, new LlmPromptTemplate
            {
                System = "<|system|>\n{{CONTENT}}<|end|>\n",
                User = "<|user|>\n{{CONTENT}}<|end|>\n",
                Assistant = "<|assistant|>\n{{CONTENT}}<|end|>\n",
                Stop = new[] { "<|system|>", "<|user|>", "<|assistant|>", "<|end|>" }
            }).ConfigureAwait(false);

            // Mistral
            //_model = await GenAIModel.CreateAsync(modelPath, new LlmPromptTemplate
            //{
            //    User = "[INST]{{CONTENT}}[/INST] ",
            //    Stop = new[] { "[INST]", "[/INST]" }
            //}).ConfigureAwait(false);

            // DeepSeekR1
            //_model = await GenAIModel.CreateAsync(modelPath, new LlmPromptTemplate
            //{
            //    System = "<｜begin▁of▁sentence｜>{{CONTENT}}",
            //    User = "<｜User｜>{{CONTENT}}",
            //    Assistant = "<｜Assistant｜>{{CONTENT}}<｜end▁of▁sentence｜>",
            //    Stop = new[] { "<｜User｜>", "<｜Assistant｜>", "<｜end▁of▁sentence｜>", "<｜begin▁of▁sentence｜>" }
            //}).ConfigureAwait(false);

            _modelOptions = GetDefaultChatOptions(_model);
        }

        public void UnloadModel()
        {
            _model?.Dispose();
            _model = null;
        }

        public async Task<string> ProcessTextAsync(string systemPrompt, string text, CancellationToken cts, Action<string> onTextUpdate = null)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var result = string.Empty;
            text = text.Trim();

            await Task.Run(
                async () =>
                {
                    await foreach (var messagePart in _model.CompleteStreamingAsync(
                        new List<ChatMessage>
                        {
                            new ChatMessage(ChatRole.System, systemPrompt),
                            new ChatMessage(ChatRole.User, text)
                        },
                        _modelOptions,
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

            return text;
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
    }
}
