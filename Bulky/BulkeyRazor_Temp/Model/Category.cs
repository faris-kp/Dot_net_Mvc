using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BulkeyRazor_Temp.Model
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(30)]
        [DisplayName("Category Name")]
        public string Name { get; set; }
        [Range(1, 100, ErrorMessage = "Display Order Must Be Between 1-100")]
        [DisplayName("Display Order")]
        public int DiplayOrder { get; set; }
    }
}
