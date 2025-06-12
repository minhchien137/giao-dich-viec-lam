using System.ComponentModel.DataAnnotations;

namespace WebGiaoDichViecLam.Models
{
    public class tblCompany
    {
        [Key]
        public int iCompanyID { get; set; }

        [Required]
        public string sNameCompany { get; set; }

        public string sPhotoCompany { get; set; }

        public int iPhoneNumber { get; set; }

        public string sAddressCompany { get; set; }


        public int iNumberEmployees { get; set; }

        public string sDescriptionCompany { get; set; }


    }
}
