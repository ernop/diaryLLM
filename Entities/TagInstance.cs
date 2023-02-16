using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiaryUI
{
    public class TagInstance : BaseEntity
    {
        
        [ForeignKey("Tag.Id")]
        public Tag Tag { get; set; }

        [ForeignKey("Transcript.Id")]
        public Transcript Transcript { get; set; }
        
        [NotMapped]
        public override string Slug => "TagInstance";

        public override string Describe()
        {
            return $"Instance of tag:{Tag.Name} on transcript:{Transcript.Filename}";
        }
    }
}
