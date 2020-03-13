using Microsoft.EntityFrameworkCore;
using PocketBookServer.Data;
using PocketBookServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketBookModel.Services
{
    public interface IMedicationService
    {
        Task AddAsync(Medication medication);

        Task<bool> CheckMedicationName(string name, int? id);

        Task DeleteAsync(int id);

        Task<IQueryable<Medication>> GetAllAsync();

        Task<Medication> GetAsync(int id);

        Task UpdateAsync(Medication medication);
    }

    public class MedicationService : IMedicationService
    {
        private readonly ApplicationDataContext _dataContext;

        public MedicationService(ApplicationDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public Task AddAsync(Medication medication)
        {
            medication.LastModified = DateTimeOffset.UtcNow;
            _dataContext.Add(medication);
            return _dataContext.SaveChangesAsync();
        }

        public async Task<bool> CheckMedicationName(string name, int? id)
        {
            return !(await _dataContext.Medications.AnyAsync(m => m.Name == name && (id == null || m.Id != id)));
        }

        public async Task DeleteAsync(int id)
        {
            var item = await _dataContext.Medications.FindAsync(id);

            if (item != null)
            {
                _dataContext.Remove(item);
                await _dataContext.SaveChangesAsync();
            }
        }

        public Task<IQueryable<Medication>> GetAllAsync()
        {
            return Task.FromResult(_dataContext.Medications.AsQueryable());
        }

        public async Task<Medication> GetAsync(int id)
        {
            return await _dataContext.Medications.FindAsync(id);
        }

        public Task UpdateAsync(Medication medication)
        {
            medication.LastModified = DateTimeOffset.UtcNow;
            _dataContext.Medications.Update(medication);
            return _dataContext.SaveChangesAsync();
        }
    }
}