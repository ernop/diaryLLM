namespace DiaryUI
{
    /// <summary>
    /// a query done to the database for a given prompt, of a given type, with certain results.
    /// 
    /// i.e. this might also have been named DbQuery - it's not the full inmemory query object, just the DAL
    /// </summary>
    public class Query : BaseEntity
    {
        public override string Slug => "Query";
        public string HumanPrompt { get; set; }
        public string Prompt { get; set; }

        /// <summary>
        /// String-enum for the "kind" of query it was. This guides interpretation
        /// For example, summary type => text summary
        /// But other types might output a json which would need to be deserialized and have something done with it.
        /// TODO fix this? ugh.
        /// </summary>
        public string Kind { get; set; }

        public string Response { get; set; }

        /// <summary>
        /// The related chunk it was made on
        /// </summary>
        public Chunk Chunk { get; set; }
        public override string Describe() => $"Query:chunkId{Chunk.Id}, kind:{Kind}, Text:{Prompt}";
        public ModelEnum Model { get; set; }
    }

    public enum ModelEnum
    {
        GPT3_5_Turbo = 1,
    }
}
