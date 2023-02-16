using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

using static DiaryUI.Config;

namespace DiaryUI
{
    public class Transcript : BaseEntity
    {
        //[Key]
        //public int Id { get; set; }
        public string Filename { get; set; }

        public Person? Recorder { get; set; }
        public string TranscriptPath { get; set; } = "";
        public string Content { get; set; } = "";

        public List<TagInstance> TagInstances { get; set; }

        /// <summary>
        /// The date recorded
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime? Date { get; set; }

        /// <summary>
        /// N token long chunks
        /// </summary>
        public List<Chunk> Chunks { get; set; }


        [NotMapped]
        public override string Slug => "Transcript";

        public override string Describe()
        {
            return Filename;
        }

        public Transcript() { }
        
        public Transcript(string fp, DiaryDbContext db)
        {
            TranscriptPath = fp;
            Filename = Path.GetFileName(fp);
            Date = File.GetCreationTime(fp);
            CreatedUtc = DateTime.UtcNow;
            UpdatedUtc = DateTime.UtcNow;
            Content = System.IO.File.ReadAllText(fp);
            var lower = fp.ToLower();

            if (lower.Contains("ernie diary"))
            {
                var ernie = Person.GetOrCreate("Ernie", db);
                Recorder = ernie;
            }

            else if (lower.Contains("jcf"))
            {
                var jcf = Person.GetOrCreate("JCF", db);
                Recorder = jcf;
            }

            else if (lower.Contains("ccf"))
            {
                var jcf = Person.GetOrCreate("CCF", db);
                Recorder = jcf;
            }

            else if (lower.Contains("cef"))
            {
                var jcf = Person.GetOrCreate("CEF", db);
                Recorder = jcf;
            }

            else if (lower.Contains("faf"))
            {
                var jcf = Person.GetOrCreate("FAF", db);
                Recorder = jcf;
            }

            else if (lower.Contains("eaf"))
            {
                var jcf = Person.GetOrCreate("FAF", db);
                Recorder = jcf;
            }
            else
            {
                Recorder = Person.GetOrCreate("Unknown", db);
            }

            var yearre = new Regex(@"(\d{4,4})").Match(Filename);
            if (yearre.Success)
            {
                var year = int.Parse(yearre.Groups[1].Value);
                var newDate = new DateTime(year, 1, 1);
                Date = newDate;
            }
        }
    }
}
