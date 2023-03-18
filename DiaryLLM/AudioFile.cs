using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static DiaryLLM.Helpers;

namespace DiaryLLM
{
    public class AudioFile
    {
        public string FP { get; set; }
        public long SizeInBytes { get; set; }
        public double LengthInSeconds { get; set; }
        
        public AudioFile(string fp)
        {
            FP = fp;
            var localfile = new FileInfo(fp);
            SizeInBytes = localfile.Length;
            LengthInSeconds = GetLengthOfAudioFileInSeconds(fp);
        }
    }
}
