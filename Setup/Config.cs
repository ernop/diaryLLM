namespace DiaryUI
{
    public static class Config
    {
        //public static string TranscriptRawFP { get; } = "d:/proj/diary/output";
        public static string TranscriptRawFP { get; } = "D:/proj/whisper";
        public static int ChunkLength { get; set; } = 3000;
        public static string OpenAIApiKey { get; set; } = System.IO.File.ReadAllText("openAIKey.secret");
            
    }
}
