using System.ComponentModel.DataAnnotations;

namespace WebGiaoDichViecLam.Models
{
    public class tblCategory
    {
        [Key]
        public int iCategoryID { get; set; }

        [Required]
        public string sCategoryName { get; set; }
    }
}
