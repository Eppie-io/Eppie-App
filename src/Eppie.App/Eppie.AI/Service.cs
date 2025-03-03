#if AI_ENABLED
using Microsoft.Extensions.AI;
#endif
using System;
using System.Collections.Generic;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Eppie.AI
{
    public class Service
    {
#if AI_ENABLED
        private IChatClient _model;
#endif
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
        }

        public void UnloadModel()
        {
            _model?.Dispose();
            _model = null;
        }

#if AI_ENABLED
        public async Task<string> ProcessTextAsync(string systemPrompt, string text, CancellationToken cts, Action<string> onTextUpdate = null)
        {
            var result = string.Empty;

            await Task.Run(
                async () =>
                {
                    await foreach (var messagePart in _model.CompleteStreamingAsync(
                        new List<ChatMessage>
                        {
                                        new ChatMessage(ChatRole.System, systemPrompt),
                                        new ChatMessage(ChatRole.User, text)
                        },
                        null,
                        cts).ConfigureAwait(false))
                    {
                        result += messagePart.Text;
                        onTextUpdate?.Invoke(messagePart.Text);
                    }
                }, cts).ConfigureAwait(false);

            return RemoveThinkBlockIfExists(result);
        }
#else
        public Task<string> ProcessTextAsync(string systemPrompt, string text, CancellationToken cts, Action<string> onTextUpdate = null)
        {
            return Task.FromResult(string.Empty);
        }
#endif
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
    }
}
