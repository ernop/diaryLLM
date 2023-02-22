namespace DiaryUI
{
    public class TranscriptsModel
    {
        public List<Transcript> Transcripts { get; set; }
    }


    public class TagsIndexModel
    {
        public List<Tag> Tags { get; set; }
    }

    public class TranscriptModel
    {
        public Transcript Transcript { get; set; }
    }

    public class ChunksModel
    {
        public List<Chunk> Chunks { get; set; }
    }

    public class IndexModel { }

    public class PersonIndexModel
    {
        public List<Person> People { get; set; }
    }
}
