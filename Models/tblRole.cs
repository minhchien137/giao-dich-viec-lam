using System.ComponentModel.DataAnnotations;

namespace WebGiaoDichViecLam.Models
{
    public class tblRole
    {

        [Key]
        public int iRoleID { get; set; }

        [Required]
        public string sNameRole { get; set; }

    }
}
