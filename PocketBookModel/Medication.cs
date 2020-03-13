using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace PocketBookModel
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

        public override bool Equals(object obj)
        {
            if (!(obj is Medication other))
                return false;

            return AdviceIfDeclined.Equals(other.AdviceIfDeclined)
                && AdviceIfTaken.Equals(other.AdviceIfTaken)
                && Dose.Equals(other.Dose)
                && ExclusionCriteria.Equals(other.ExclusionCriteria)
                && Form.Equals(other.Form)
                && Id == other.Id
                && InclusionCriteria.Equals(other.InclusionCriteria)
                && Indications.Equals(other.Indications)
                && LastModified.Equals(other.LastModified)
                && Name.Equals(other.Name)
                && PolicyDate == other.PolicyDate
                && Route.Equals(other.Route)
                && SideEffects.Equals(other.SideEffects);
        }

        public string GetEtag()
        {
            using var hasher = MD5.Create();
            var bytes = Encoding.UTF8.GetBytes($"{Id}.{Name}.{LastModified.ToString("s")}");
            var hash = hasher.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override string ToString()
        {
            return $"{Id} : {Name}";
        }
    }
}
