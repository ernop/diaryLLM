using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Python.Runtime;
using System.Diagnostics;

using static DiaryLLM.Helpers;

namespace DiaryLLM
{
    internal class Program
    {

        /// <summary>
        /// program to: 
        /// 1. download all new diary m4a etc files from gdrive
        /// 2. run slave versions of whisper transcription on them (in parallel)?
        /// 3. do other stuff
        /// 4. magic
        /// 5. later - extract metadata via claude, provide illustrations from mj/SD, link in images from gphotos, link in 
        /// </summary>
        static void Main(string[] args)
        {
            DownloadAllFiles();
            ProcessAllFiles();
        }

        public static void ProcessAllFiles()
        {
            foreach (var fp in Directory.GetFiles(RawDiaryFilesHoldFolder))
            {
                var process = GetProcess();
                CW($"Processing: {fp}");
                var audioFile = new AudioFile(fp.Replace('\\', '/'));
                var exi = GetTxtForAudio(audioFile);
                if (System.IO.File.Exists(exi))
                {
                    CW($"Audio file for this already exists, skipping {exi}");
                    continue;
                }

                CW($"processing {audioFile.FP} length {audioFile.LengthInSeconds}s");
                var res = ProcessFile(process, audioFile);
                if (!res)
                {
                    CW($"Failed to process {audioFile.FP}");
                }
            }
        }



        /// <summary>
        /// Must have mounted gdrive at X:
        /// this downloads them all to the static targets.
        /// </summary>
        public static void DownloadAllFiles()
        {
            if (!System.IO.Directory.Exists(MountedGDriveFolder))
            {
                Console.WriteLine($"You need to mount the drive. Run this in powershell: \n\nd:\\dl\\rclone-v1.61.1-windows-amd64\\rclone.exe mount gdrive: q:");
                return;
            }
            var files = System.IO.Directory.GetFiles(MountedGDriveFolder);
            foreach (var fp in files)
            {
                var file = System.IO.Path.GetFileName(fp);

                var fileinfo = new FileInfo(fp);

                var localCandidateFp = RawDiaryFilesHoldFolder + "/" + file;
                if (System.IO.File.Exists(localCandidateFp))
                {
                    CW("file exists:"+localCandidateFp, false);
                    var remoteLength = fileinfo.Length;
                    var localfile = new FileInfo(localCandidateFp);
                    if (localfile.Length == remoteLength)
                    {
                        CW("and is identical; skip");
                        continue;

                    }
                    CW("but remote length is diff so getting again.");
                }

                CW("Copying file locally - " + fp+" => "+localCandidateFp);
                System.IO.File.Copy(fp, localCandidateFp);
            }
        }

        public static bool ProcessFile(Process process, AudioFile audioFile)
        {
            var sw = process.StandardInput;
            var cmd = $"python.exe d:/proj/whisper/whisper_file.py '{OutputFolder}' '{audioFile.FP}'";
            if (sw.BaseStream.CanWrite)
            {
                sw.WriteLine(cmd);
                CW($"wrote:{cmd}");
                sw.Flush();
                sw.Close();
            }
            else
            {
                CW("Error.");
                return false;
            }

            if (process.HasExited)
            {
                CW("HasExited Early.");
                return false;
            }
            while (!process.HasExited)
            {
                var r = process.StandardOutput.ReadLine();
                CW(r);
            }

            var error = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(error))
            {
                CW(error);
                return false;
            }
            return true;
        }

    }
}
