using System;

namespace PocketBookServer.Models
{
    public class MedicationSummary
    {
        public MedicationSummary(Medication medication)
        {
            Id = medication.Id;
            LastModified = medication.LastModified;
            Name = medication.Name;
        }

        public int Id { get; }
        public DateTimeOffset LastModified { get; }
        public string Name { get; }
    }
}