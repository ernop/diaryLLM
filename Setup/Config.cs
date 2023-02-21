namespace DiaryUI
{
    public static class Config
    {
        public static string TranscriptRawFP { get; } = "d:/proj/diary/output";
        //public static string TranscriptRawFP { get; } = "D:/proj/whisper";
        public static string BaseFolder { get; } = "d:/proj/diaryui2/diaryui/setup";
        public static int ChunkLength { get; set; } = 3000;
        public static string OpenAIApiKey { get; set; } = System.IO.File.ReadAllText(BaseFolder + "/openAIKey.secret");

    }
}
