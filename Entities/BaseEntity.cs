using System.ComponentModel.DataAnnotations;

namespace DiaryUI
{
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public abstract string Describe();
        
        [DataType(DataType.DateTime)]
        public DateTime CreatedUtc { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime UpdatedUtc { get; set; }

        public abstract string Slug { get; }
        
        public string Link()
        {
            return $"<a href=/{Slug}/{Id}>{Describe()}</a>";
        }
    }
}
