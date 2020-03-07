using PocketBookServer.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace PocketBookAdmin.ViewModels
{
    public class EditMedication
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

        [Required(AllowEmptyStrings = false)]
        public string InclusionCriteria { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Indications { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }

        [DataType(DataType.Date)]
        public DateTime? PolicyDate { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Route { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string SideEffects { get; set; }

        public static implicit operator Medication(EditMedication createMedication)
        {
            return new Medication
            {
                AdviceIfDeclined = createMedication.AdviceIfDeclined,
                AdviceIfTaken = createMedication.AdviceIfTaken,
                Dose = createMedication.Dose,
                ExclusionCriteria = createMedication.ExclusionCriteria,
                Form = createMedication.Form,
                InclusionCriteria = createMedication.InclusionCriteria,
                Indications = createMedication.Indications,
                Name = createMedication.Name,
                PolicyDate = createMedication.PolicyDate,
                Route = createMedication.Route,
                SideEffects = createMedication.SideEffects
            };
        }
    }
}