using AI.Dev.OpenAI.GPT;

using BERTTokenizers;

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using AI.Dev.OpenAI.GPT;

using Newtonsoft.Json;

namespace DiaryUI
{
    public static class Util
    {
        /// <summary>
        /// newline for use in prompt construction
        /// </summary>
        public static string Newline = "\r\n\r\n";

        /// <summary>
        /// divide into as few sections each with max Tokens <= maxTokens as possible
        /// first try to split 
        /// </summary>
        private static List<string> Subdivide(string text, int maxTokens)
        {
            var all = Tokenize(text);
            if (all <= maxTokens)
            {
                return new List<string>() { text };
            }

            //there were too many tokens, so we guess we have to split it up into this many  parts.
            //for example if limit is 3000 and you have 4000, split it into ceil(4/3) == 2 should be fine.
            var currentTry = Math.Ceiling((double)all / maxTokens);
            var words = text.Split(' ');
            var wordCount = words.Count();

            while (true)
            {
                var tryIsOkay = true;
                var wordsPerPart = (int)Math.Ceiling(wordCount / currentTry);
                var candidateRes = new List<string>() { };
                for (var ii = 0; ii < currentTry; ii++)
                {
                    var thePartWords = words.Skip(ii * wordsPerPart).Take(wordsPerPart);
                    var thePart = string.Join(' ', thePartWords);
                    var partTokens = Tokenize(thePart);
                    if (partTokens > maxTokens)
                    {
                        tryIsOkay = false;
                        break;
                    }
                    candidateRes.Add(thePart);
                }

                if (!tryIsOkay)
                {
                    //fall into trying to break it up more.
                    currentTry++;
                    continue;
                }

                //I'll just confirm joined matches.
                var candidateJoined = string.Join(' ', candidateRes);
                if (candidateJoined != text)
                {
                    throw new Exception("error splitting up text.");
                }

                return candidateRes;
            }
        }

        /// <summary>
        /// Chunkify into sections ~3000 tokens long
        /// </summary>
        public static void Chunkify(Transcript transcript, DiaryDbContext db)
        {
            var parts = Subdivide(transcript.Content, 3400);
            foreach (var part in parts)
            {
                var chu = new Chunk(part);
                chu.Transcript = transcript;
                db.Chunks.Add(chu);
            }
            db.SaveChanges();
        }

        /// <summary>
        /// 3/2023 fixed via https://github.com/dluc/openai-tools
        /// </summary>
        public static int Tokenize(string text)
        {
            var tokens = GPT3Tokenizer.Encode(text);
            return tokens.Count;
        }

        /// <summary>
        /// I need better structure for this, right now this is just a map from string to more code-like elements hooked into the specific class.
        /// </summary>
        public static BaseQuery GetQuery(string kind)
        {
            BaseQuery q;
            if (kind == "summarize")
            {
                q = new SummarizerQuery();
            }
            else if (kind == "emotions")
            {
                q = new EmotionsQuery();
            }
            else if (kind == "people")
            {
                q = new PeopleQuery();
            }
            else if (kind == "rewrite")
            {
                q = new RewriteQuery();
            }
            else if (kind == "places")
            {
                q = new PlacesQuery();
            }
            else if (kind == "stories")
            {
                q = new StoriesQuery();
            }
            else if (kind == "objects")
            {
                q = new ObjectsQuery();
            }
            else if (kind == "personality")
            {
                q = new PersonalityQuery();
            }
            else if (kind == "meta")
            {
                q = new MetaQuery();
            }
            else if (kind == "questions")
            {
                q = new QuestionsQuery();
            }
            else
            {
                throw new Exception("Invalid query kind.");
            }
            return q;
        }

        /// <summary>
        /// Replace things that look like sentence ends with newlines and return html. Doesn't actually modify content much/at all.
        /// </summary>
        public static string Newlineify(string input)
        {
            var res = input.Replace(".\r\n", ".\r\n<br>");
            return res;
        }

        /// <summary>
        /// Deserialize the string to a T, and then run that through the formatter.
        /// </summary>
        public static string DeserializeToAndFormat<T>(Func<T,string> func, string response)
        {
            try
            {
                var raw = JsonConvert.DeserializeObject<T>(response);
                var res = func(raw);
                return res;
            }
            catch (Exception ex)
            {
                //TODO annoying
                //TODO magic: just ask the LLM again to fix the json :)
                //note: if this happens a lot check for typos in your examples in the prompt - it's very responsive
                return response+$"<br><br>:{ex.Message}";
            }
        }

        public static int CountQueriesInTranscript(Transcript transcript)
        {
            var res = transcript.Chunks.SelectMany(c => c.Queries).Count();
            return res;
        }
    }
}
