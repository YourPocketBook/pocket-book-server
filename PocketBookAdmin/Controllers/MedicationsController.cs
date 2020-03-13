using Microsoft.AspNetCore.Mvc;
using PocketBookAdmin.ViewModels;
using PocketBookModel;
using PocketBookModel.Services;
using System.Threading.Tasks;

namespace PocketBookAdmin.Controllers
{
    public class MedicationsController : Controller
    {
        private readonly IMedicationService _service;

        public MedicationsController(IMedicationService service)
        {
            _service = service;
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(EditMedication model)
        {
            if (!await _service.CheckMedicationName(model.Name))
                ModelState.AddModelError("name", "That name is already in use.");

            if (ModelState.IsValid)
            {
                await _service.AddAsync(model);
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        public async Task<ActionResult> Delete(int id)
        {
            var item = await _service.GetAsync(id);

            if (item == null)
                return NotFound();

            return View(item);
        }

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeletePost(int id)
        {
            await _service.DeleteAsync(id);

            return RedirectToAction(nameof(Index));
        }

        public async Task<ActionResult> Details(int id)
        {
            var item = await _service.GetAsync(id);

            if (item == null)
                return NotFound();

            return View(item);
        }

        public async Task<ActionResult> Edit(int id)
        {
            var item = await _service.GetAsync(id);

            if (item == null)
                return NotFound();

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, EditMedication model)
        {
            if (!await _service.CheckMedicationName(model.Name, id))
                ModelState.AddModelError("name", "That name is already in use.");

            if (ModelState.IsValid)
            {
                var medication = (Medication)model;
                medication.Id = id;

                await _service.UpdateAsync(medication);
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        public async Task<ActionResult> Index()
        {
            var items = await _service.GetAllAsync();

            return View(items);
        }
    }
}
