using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiaryLLM
{
    public static class Helpers
    {
        public static string RawDiaryFilesHoldFolder { get; } = "d:/proj/diary/files";
        public static string MountedGDriveFolder { get; } = @"q:/phoneaudio";
        public static string OutputFolder { get; } = "d:/proj/diary/output";

        /// <summary>
        /// interpolate into the .txt file location for it.
        /// </summary>
        public static string GetTxtForAudio(AudioFile audioFile)
        {
            var fn = System.IO.Path.GetFileName(audioFile.FP);
            var fp = OutputFolder + '/' + fn + ".txt";
            return fp;
        }

        public static string GetJsonForAudio(AudioFile audioFile)
        {
            var res = System.IO.Path.GetFullPath(audioFile.FP) + ".json";
            return res;
        }

        public static void CW(string s, bool newline=true)
        {
            if (newline)
                Console.WriteLine(s);
            else
                Console.Write(s);
        }

        public static double GetLengthOfAudioFileInSeconds(string path)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "ffprobe",
                Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{path}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            var process = Process.Start(startInfo);
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            double res;
            try
            {
                res = double.Parse(output.Trim());
            }
            catch (Exception ex)
            {
                //why does this fail?
                res = 0;
            }

            return res;
        }


        public static Process GetProcess()
        {
            // conda install pytorch torchvision torchaudio cudatoolkit=11.3 -c pytorc
            var cmd = "powershell";
            var workingDirectory = "D:/proj/whisper/";
            var commandsToExecute = new List<string>(){
                "Set-ExecutionPolicy Unrestricted",
                "Get-ExecutionPolicy",
                "D:/proj/whisper/env2/Scripts/activate.ps1",
            };

            var startInfo = new ProcessStartInfo
            {
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = "",
                FileName = cmd,
                WorkingDirectory = workingDirectory
            };

            var process = Process.Start(startInfo);
            if (process == null)
            {
                throw new Exception("Could not start process");
            }

            var sw = process.StandardInput;
            if (sw.BaseStream.CanWrite)
            {
                foreach (var setupCmd in commandsToExecute)
                {
                    CW(cmd);
                    sw.WriteLine(setupCmd);
                }
                //sw.Flush();

                CW("flushed.");
            }

            return process;
        }
    }
}