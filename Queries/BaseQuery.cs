using Newtonsoft.Json;

using System;
using System.Reflection.PortableExecutable;
using System.Text;

using static DiaryUI.Util;

namespace DiaryUI
{
    /// <summary>
    /// A query for interfacing with the LLM
    /// Constructs a pretty complicated query.
    /// Also includes parsing stuff for the response values, in case it's extended to a JsonQuery.
    /// TODO: later, obviously save those things like the list of people generated.
    /// </summary>
    public abstract class BaseQuery
    {
        public abstract string HumanReadablePrompt { get; }
        public abstract string Render(string rawResponse);
        public virtual bool ReturnsJson => false;
        public string AppendTranscriptToPrompt(Chunk chunk) => $"Here is the transcript of the audio recording:{Newline}'{chunk.Content}'{Newline}";
        public string GenericInstructions(string fn) => $"The following is a transcript of an audio recording, with filename: '{fn}'";
        public virtual List<string> PromptBuildingComponents { get; set; } = new List<string>();
        public virtual string GenerateFullPrompt(Chunk chunk, string filename)
        {
            PromptBuildingComponents = new List<string>() { GenericInstructions(filename), HumanReadablePrompt, AppendTranscriptToPrompt(chunk) };
            var res = string.Join(Newline, PromptBuildingComponents);
            return res;
        }
    }

    /// <summary>
    /// returns text, dumps text to output like normal.
    /// </summary>
    public abstract class NormalQuery : BaseQuery
    {
        public override string Render(string rawResponse) => rawResponse;
    }

    public abstract class JsonQuery : BaseQuery
    {
        public override bool ReturnsJson => true;
        public abstract string JsonTemplate { get; }
        public string JsonTemplateIntro => "Here is a JSON template for what you should output.";
        public string JsonOutputInstructions => $"Return a valid JSON file in the following format, and remember to properly escape all quotes and special characters using double quotes. If any single quotes appear in responses, escape them like this: \"What\\\'s your name?\"";
        public override string GenerateFullPrompt(Chunk chunk, string filename)
        {

            PromptBuildingComponents = new List<string>() { GenericInstructions(filename), HumanReadablePrompt, JsonTemplateIntro, JsonTemplate, JsonOutputInstructions, AppendTranscriptToPrompt(chunk) };
            var res = string.Join(Newline, PromptBuildingComponents);

            return res;
        }
    }

    public class SummarizerQuery : NormalQuery
    {
        public override string HumanReadablePrompt => "Summarize the main topics discussed in detail.  Give a summary of the circumstances, the speaker, the situations, and any items of interest to the general public.";
    }

    public class RewriteQuery : NormalQuery
    {
        public override string HumanReadablePrompt => $"The following is the transcript of an audio recording. Rewrite and condense the contents clearly while retaining details.";
    }

    public class PersonalityQuery : NormalQuery
    {
        public override string HumanReadablePrompt => $"Describe in detail the personality of everyone involved, including age, sex, origin, hometown, emotions, mind state, traits, and anything else you can surmise. Feel free to extrapolate and follow your intuition.";
    }

    public class QuestionsQuery : JsonQuery
    {
        public override string HumanReadablePrompt => $"Generate a list of follow-up questions to explore questions from the transcript, which would help understand more about what happened, the opinion and emotions of the speaker, their background, ideas, thoughts, or beliefs.";
        public override string JsonTemplate => "[\"<interesting, detailed, perceptive and relevant question like \'what\\'re the speaker\\'s feeling about <X>?\'\", <question 2>, etc]";
        public override string Render(string rawResponse)
        {
            try
            {
                var clean = rawResponse;
                var oo = JsonConvert.DeserializeObject<List<string>>(clean);
                var sb = new StringBuilder();
                sb.AppendLine("<ul>");
                foreach (var p in oo)
                {
                    sb.AppendLine($"<li>{p}");
                }
                sb.AppendLine("</ul>");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return rawResponse;
            }
        }
    }

    public class EmotionsQuery : JsonQuery
    {
        public override string HumanReadablePrompt => $"Generate a list of emotions and attitudes that appear in the transcript. Listen to their manner and word choice and feel free to extrapolate.";
        public override string JsonTemplate => "[{'name':<Emotion Name>, 'intensity': <n from 1-10>}, ...]";
        public override string Render(string rawResponse)
        {
            var formatter = (List<Emotion> oo) =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("<ul>");
                foreach (var p in oo.OrderByDescending(el => el.intensity))
                {
                    sb.AppendLine($"<li>{p.name} ({p.intensity})</li>");
                }
                sb.AppendLine("</ul>");


                return sb.ToString();
            };
            return DeserializeToAndFormat<List<Emotion>>(formatter, rawResponse);
        }

        private class Emotion
        {
            public string name { get; set; }
            public int intensity { get; set; }
        }
    }

    public class PlacesQuery : JsonQuery
    {
        public override string HumanReadablePrompt => $"List all places mentioned.";
        public override string JsonTemplate => "[{\"name\":\"<Place Name>\", \"familiarity\":\"<Familiary>\", \"description\": \"<20 word description of the place as conveyed by the transcript>}\"]";
        private class InnerPlace
        {
            public string name { get; set; }
            public string familiarity { get; set; }
            public string description { get; set; }
        }

        public override string Render(string rawResponse)
        {
            var formatter = (List<InnerPlace> oo) =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("<ul>");
                foreach (var p in oo)
                {
                    var fam = string.IsNullOrEmpty(p.familiarity) ? "" : $"({p.familiarity}) ";
                    sb.AppendLine($"<li>{p.name} {fam}{p.description}</li>");
                }
                sb.AppendLine("</ul>");


                return sb.ToString();
            };
            return DeserializeToAndFormat<List<InnerPlace>>(formatter, rawResponse);
        }
    }



    public class ObjectsQuery : JsonQuery
    {
        public override string HumanReadablePrompt => $"List all physical objects mentioned.  Make sure they are real, physical objects, not abstract concepts.";
        public override string JsonTemplate => "[<object name>, ...].";
        public override string Render(string rawResponse)
        {
            var formatter = (List<string> oo) =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("<ul>");
                foreach (var p in oo)
                {
                    sb.AppendLine($"<li>{p}</li>");
                }
                sb.AppendLine("</ul>");
                return sb.ToString();
            };
            return DeserializeToAndFormat<List<string>>(formatter, rawResponse);
        }
    }

    public class PeopleQuery : JsonQuery
    {
        public override string HumanReadablePrompt => "List all the people mentioned in order of importance.";
        public override string JsonTemplate => "[{\"name\":<Person Name>, \"description\": <description>, \"likely_relationship\":\"<appears in the tape, family, stranger, famous person, known person but not present, etc.  Feel free to extrapolate>\", \"characteristics\": [\"<Good Trait 1>\", \"<Other Trait 2>\", ...]}, ...]";

        public override string Render(string rawResponse)
        {
            var formatter = (List<InnerPerson> oo) =>
            {
                var sb = new StringBuilder();
                foreach (var p in oo)
                {
                    var likely = string.IsNullOrEmpty(p.likely_relationship) ? "" : $"({p.likely_relationship})";
                    sb.AppendLine($"<p>{p.name}: {p.description}, {likely}<ul><li>{string.Join("<li>", p.characteristics)}</ul>");
                }

                return sb.ToString();
            };
            return DeserializeToAndFormat<List<InnerPerson>>(formatter, rawResponse);
        }

        private class InnerPerson
        {
            public string name { get; set; }
            public string description { get; set; }
            public string likely_relationship { get; set; }
            public List<string> characteristics { get; set; } = new List<string>();
        }
    }

    public class StoriesQuery : JsonQuery
    {
        public override string HumanReadablePrompt => "Pick out all the stories from this transcript and re-tell them in a concise, clear, relevant, and emotionally meaningful way.";
        public override string JsonTemplate => "[{\"story_name\": \"<creative name>\", \"story_text\": \"<text>\", \"catchphrase\": \"<amusing catchphrase describing the story>\", },... ]";
        public override string Render(string rawResponse)
        {
            var formatter = (List<Story> oo) =>
            {
                var sb = new StringBuilder();
                foreach (var p in oo)
                {
                    sb.AppendLine($"<p>{p.story_name}: {p.catchphrase}<br />{p.story_text}</p>");
                }

                return sb.ToString();
            };
            return DeserializeToAndFormat<List<Story>>(formatter, rawResponse);
        }

        private class Story
        {
            public string story_name { get; set; }
            public string story_text { get; set; }
            public string catchphrase { get; set; }
        }

    }


    public class MetaQuery : JsonQuery
    {
        public override string HumanReadablePrompt => "Your job is to evaluate the transcript, and fill in the following template with your best guesses.";
        public override string JsonTemplate => "{\"likely_date_of_recording\": \"<Date Guess>\", " +
            "\"likely_location\": \"<best guess>\", " +
            "\"short_summary\": \"<5 word summary>\", " +
            "\"long_summary\": \"<100 word summary>\", " +
            "\"todos\": [\"<todo items mentioned with date and time, if any>\", " +
            "\"notes_to_self\":[\"<any notes to self like Todo: call john tomorrow, if any>\"], " +
            "\"corrections_on_past_information\": [\"<Correction1, if any>`\"], " +
            "\"activities_described\": [\"<Activity1, if any>`\", \"<Further activities, if present>\"], " +
            "\"people_involved\": [\"<person1, if any>\", \"<person2>\", ...]," +
            "\"most_interesting_quotes\": [\"<quote1, if any>\", \"<quote2>\", ...]}"
            ;
        public override string Render(string rawResponse)
        {
            var formatter = (Meta oo) =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("<ul>");

                sb.AppendLine($"<li>Date: {oo.likely_date_of_recording}</li><li>Time: {oo.likely_time_of_recording}</li><li>Location: {oo.likely_location}</li>");
                sb.AppendLine($"<li>Mood: {oo.mood}</li><li>Summary: {oo.short_summary}</li><li>Long Summary: {oo.long_summary}</li>");
                var uu = 0;
                var uumap = new Dictionary<int, string>();
                uumap[0] = "Todos";
                uumap[1] = "Notes to self";
                uumap[2] = "Corrections";
                uumap[3] = "Activities";
                uumap[4] = "People";
                uumap[5] = "Quotes";
                foreach (var l in new List<List<string>>() { oo.todos, oo.notes_to_self, oo.corrections_on_past_information, oo.activities_described, oo.people_involved, oo.most_interesting_quotes })
                {
                    if (l.Count() > 0)
                    {
                        sb.AppendLine($"<li>{uumap[uu]}");
                        sb.AppendLine($"<ul>");
                        foreach (var t in l)
                        {
                            sb.AppendLine($"<li>{t}</li>");
                        }
                        sb.AppendLine("</ul>");
                    }
                    sb.AppendLine("</li>");
                    uu++;
                }

                sb.AppendLine("</ul>");
                return sb.ToString();
            };
            return DeserializeToAndFormat<Meta>(formatter, rawResponse);
        }

        private class Meta
        {
            public string likely_date_of_recording { get; set; } = "";
            public string likely_time_of_recording { get; set; } = "";
            public string likely_location { get; set; } = "";
            public string mood { get; set; } = "";
            public string short_summary { get; set; } = "";
            public string long_summary { get; set; } = "";
            public string weather { get; set; } = "";
            public List<string> todos { get; set; } = new List<string>();
            public List<string> notes_to_self { get; set; } = new List<string>();
            public List<string> corrections_on_past_information { get; set; } = new List<string>();
            public List<string> activities_described { get; set; } = new List<string>();
            public List<string> people_involved { get; set; } = new List<string>();
            public List<string> most_interesting_quotes { get; set; } = new List<string>();
        }
    }
}
