using OpenAI.Completions;
using OpenAI;
using static DiaryUI.Config;

using static DiaryUI.Util;

namespace DiaryUI
{
    public class LLMClient
    {
        private OpenAIClient Client { get; set; }
        public LLMClient()
        {
            Client = new OpenAIClient(new OpenAIAuthentication(Config.OpenAIApiKey));
        }

        public string MakeRequest(string prompt)
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
                Model = OpenAI.Models.Model.Davinci
            };
            try
            {
                var result = Client.CompletionsEndpoint.CreateCompletionAsync(request, new CancellationToken()).Result;
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
