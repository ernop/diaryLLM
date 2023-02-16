using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Identity.Client;

using OpenAI;
using OpenAI.Completions;
using OpenAI.Models;

using static DiaryUI.Config;

namespace DiaryUI
{


    public class HomeController : Controller
    {
        [HttpGet("/")]
        public IActionResult Index()
        {
            //force
            return Redirect("/transcript/3");
        }

        [HttpGet("/Query/{queryId}/delete")]
        public IActionResult QueryDelete(int queryId, bool delete)
        {
            using (var db = new DiaryDbContext())
            {
                var query = db.Queries
                    .Include(el=>el.Chunk)
                    .ThenInclude(el => el.Transcript)
                    .FirstOrDefault(x => x.Id == queryId);
                if (query == null)
                {
                    throw new Exception("No query.");
                }

                db.Queries.Remove(query);
                db.SaveChanges();

                return Redirect($"/transcript/{query.Chunk.Transcript.Id}");
            }
        }

        [HttpGet("/DoLookup/{chunkId}/{kind}")]
        public IActionResult DoLookup(string kind, int chunkId)
        {
            var client = new LLMClient();
            using (var db = new DiaryDbContext())
            {
                var chunk = db.Chunks
                    .Include(el => el.Transcript)
                    .FirstOrDefault(x => x.Id == chunkId);
                if (chunk == null)
                {
                    throw new Exception("No chunk.");
                }

                var transcript = chunk.Transcript;

                //generate different prompts for different types of query.
                var q = Util.GetQuery(kind);

                var fullPrompt = q.GenerateFullPrompt(chunk, transcript.Filename);
                var humanPrompt = q.HumanReadablePrompt;
                var res = client.MakeRequest(fullPrompt);
                if (res == null)
                {
                    //failed to make query.
                }
                else
                {
                    var query = new Query();
                    query.Prompt = fullPrompt;
                    query.HumanPrompt = humanPrompt;
                    query.Response = res;
                    query.Chunk = chunk;
                    query.Kind = kind;
                    db.Queries.Add(query);
                    db.SaveChanges();
                }
                
                return Redirect($"/transcript/{transcript.Id}");
            }
        }

        [HttpGet("/transcripts/deleteall")]
        public IActionResult TranscriptsDelete()
        {
            //set breakpoint here and step past the false to really do this.
            var a = 4;
            if (false)
            {
                using (var db = new DiaryDbContext())
                {
                    foreach (var t in db.Transcripts)
                    {
                        db.Transcripts.Remove(t);

                    }
                    db.SaveChanges();
                }
            }
            return View();
        }

        [HttpGet("/transcripts/load")]
        public IActionResult TranscriptsLoad()
        {
            using (var db = new DiaryDbContext())
            {
                var loadCt = 0;
                foreach (var fp in System.IO.Directory.GetFiles(Config.TranscriptRawFP))
                {
                    if (!fp.EndsWith(".txt")) { continue; }

                    //existence.
                    var exiTranscript = db.Transcripts.FirstOrDefault(p => p.TranscriptPath == fp);
                    if (exiTranscript != null)
                    {
                        if (exiTranscript.Chunks == null)
                        {
                            Util.Chunkify(exiTranscript, db);
                        }
                        continue;
                    }
                    var transcript = new Transcript(fp, db);
                    db.Add(transcript);
                    db.SaveChanges();
                    Util.Chunkify(transcript, db);
                    loadCt++;
                    if (loadCt > 50)
                    {
                        break;
                    }
                }

                return View();
            }
        }

        [HttpGet("/transcripts")]
        public IActionResult Transcripts()
        {
            using (var db = new DiaryDbContext())
            {
                var model = new TranscriptsModel();
                model.Transcripts = db.Transcripts
                    .Include(el => el.Recorder)
                    .Include(el => el.Chunks)
                    .Include(el => el.TagInstances)
                        .ThenInclude(el => el.Tag)
                    .ToList();

                return View(model);
            }
        }

        [HttpGet("/transcript/{id}")]
        public IActionResult Transcript(int id)
        {
            using (var db = new DiaryDbContext())
            {
                var model = new TranscriptModel();
                var transcript = db.Transcripts
                    .Include(el => el.TagInstances)
                        .ThenInclude(el => el.Tag)
                    .Include(el => el.Chunks)
                        .ThenInclude(el => el.Queries)
                    .FirstOrDefault(el => el.Id == id);
                if (transcript == null)
                {
                    return View();
                }

                model.Transcript = transcript;
                return View(model);
            }
        }

        [HttpGet("/tags")]
        public IActionResult Tags()
        {
            using (var db = new DiaryDbContext())
            {
                var model = new TagsIndexModel();
                model.Tags = db.Tags
                    .Include(el => el.TagInstances)
                        .ThenInclude(el => el.Transcript)
                    .ToList();

                return View(model);
            }
        }

        [HttpGet("/people")]
        public IActionResult People()
        {
            using (var db = new DiaryDbContext())
            {
                var model = new PersonIndexModel();
                model.People = db.People
                    .Include(el => el.Transcripts)
                    .ToList();

                return View(model);
            }
        }
    }

    public class PersonIndexModel
    {
        public List<Person> People { get; set; }
    }
}
