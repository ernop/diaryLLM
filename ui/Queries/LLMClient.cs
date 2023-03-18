using OpenAI.Completions;
using OpenAI;
using static DiaryUI.Config;

using static DiaryUI.Util;
using OpenAI.Chat;

namespace DiaryUI
{
    public class LLMClient
    {
        private OpenAIClient Client { get; set; }
        
        public LLMClient()
        {
            Client = new OpenAIClient(new OpenAIAuthentication(Config.OpenAIApiKey));
        }

        public string MakeRequest(string prompt, string mode)
        {
            var promptTokens = Tokenize(prompt);

            var maxTokenLength = 4096;

            //TODO fix this for big profit.
            //okay, my tokenizer is inaccurate.  For example I had 3007 according to open ai but 2847 according to me.  That meant I requested too many

            //so, i'm increasing the buffer.
            var remaining = maxTokenLength - promptTokens;
            Console.WriteLine($"requesting completion of length:{remaining}");
            var messageList = new List<ChatPrompt>() { new ChatPrompt(mode, prompt) };
            var request = new OpenAI.Chat.ChatRequest(messageList);

            try
            {
                var result = Client.ChatEndpoint.GetCompletionAsync(request);
                //"message": "This is a chat model and not supported in the v1/completions endpoint. Did you mean to use v1/chat/completions?",
                var res = result.Result;
                return res.FirstChoice.Message;
                //return result.Completions[0].Text;
            }
            catch (Exception ex)
            {
                // my tokenizer doesn't match theirs, which causes problems.
                if (ex.InnerException.Message.StartsWith("Message = \"CreateCompletionAsync Failed! HTTP status code: BadRequest"))
                {

                }
                Console.WriteLine($"Failed to interact with openai.  Ex: {ex}");
                return null;
            }

        }

        public string MakeRequestCompletions(string prompt)
        {
            var promptTokens = Tokenize(prompt);

            var maxTokenLength = 4097;
            
            //TODO fix this for big profit.
            //okay, my tokenizer is inaccurate.  For example I had 3007 according to open ai but 2847 according to me.  That meant I requested too many

            //so, i'm increasing the buffer.
            var remaining = maxTokenLength - promptTokens - 400;
            Console.WriteLine($"requesting completion of length:{remaining}");
            
            var request = new CompletionRequest
            {
                Prompt = prompt,
                MaxTokens = remaining,
                Temperature = 0.7,
                TopP = 1,
                FrequencyPenalty = 0,
                PresencePenalty = 0,
                Model = OpenAI.Models.Model.GPT3_5_Turbo
            };
            try
            {
                var result = Client.CompletionsEndpoint.CreateCompletionAsync(request, new CancellationToken()).Result;
                //"message": "This is a chat model and not supported in the v1/completions endpoint. Did you mean to use v1/chat/completions?",
                return result.Completions[0].Text;
            }
            catch (Exception ex)
            {
                // my tokenizer doesn't match theirs, which causes problems.
                if (ex.InnerException.Message.StartsWith("Message = \"CreateCompletionAsync Failed! HTTP status code: BadRequest"))
                {

                }
                return null;
            }
            
        }
    }
}
