namespace DiaryUI
{
    /// <summary>
    /// A piece of the transcript, chunked down to small enough to fit in the LLM context window.
    /// </summary>
    public class Chunk : BaseEntity
    {
        public Transcript Transcript { get; set; }
        public string Content { get; set; }

        
        /// <summary>
        /// Queries that have been made on this chunk so far.
        /// </summary>
        public List<Query> Queries { get; set; }
        public override string Slug => "chunk";
        public override string Describe()
        {
            return "Chunk";
        }

        public Chunk() { }
        public Chunk(string content)
        {
            Content = content;
        }
    }
}
