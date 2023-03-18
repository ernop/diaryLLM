using System.ComponentModel.DataAnnotations.Schema;


namespace DiaryUI
{
    public class Person : BaseEntity
    {
        public string Name { get; set; } = "";
        public List<Transcript> Transcripts { get; set; }
        public override string Describe()
        {
            return $"{Name}";
        }

        internal static Person GetOrCreate(string name, DiaryDbContext db)
        {
            var person = db.People.FirstOrDefault(p => p.Name == name);
            if (person == null)
            {
                person = new Person { Name = name };
                db.People.Add(person);
                db.SaveChanges();
            }

            return person;
        }

        [NotMapped]
        public override string Slug => "Person";
    }
}
