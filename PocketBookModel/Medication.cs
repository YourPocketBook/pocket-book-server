using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace PocketBookServer.Models
{
    public class Medication
    {
        [Required(AllowEmptyStrings = false)]
        public string AdviceIfDeclined { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string AdviceIfTaken { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Dose { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string ExclusionCriteria { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Form { get; set; }

        public int Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string InclusionCriteria { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Indications { get; set; }

        public DateTimeOffset LastModified { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }

        [DataType(DataType.Date)]
        public DateTime? PolicyDate { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Route { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string SideEffects { get; set; }

        public string GetEtag()
        {
            using var hasher = MD5.Create();
            var bytes = Encoding.UTF8.GetBytes($"{Id}.{Name}.{LastModified.ToString("s")}");
            var hash = hasher.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}