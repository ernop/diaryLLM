using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiaryUI
{
    public class Tag : BaseEntity
    {
        public string Name { get; set; }
        public List<TagInstance> TagInstances { get; set; }
        
        [NotMapped]
        public override string Slug => "tag";

        public override string Describe()
        {
            return $"Tag:{Name}";
        }
    }
}
