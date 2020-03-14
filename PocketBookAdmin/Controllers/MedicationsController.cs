using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PocketBookAdmin.ViewModels;
using PocketBookModel;
using PocketBookModel.Services;
using System.Text.Json;
using System.Threading.Tasks;

namespace PocketBookAdmin.Controllers
{
    public class MedicationsController : Controller
    {
        private const string CreateMedicationSessionKey = "_Medication";
        private const string EditMedicationSessionKey = "_EditMedication";
        private readonly IMedicationService _service;

        public MedicationsController(IMedicationService service, EndpointDataSource endpointDataSource)
        {
            _service = service;
        }

        public ActionResult Create()
        {
            ViewBag.EditMode = false;

            var medication = HttpContext.Session.GetString(CreateMedicationSessionKey);
            EditMedication model = null;

            if (!string.IsNullOrEmpty(medication))
                model = JsonSerializer.Deserialize<EditMedication>(medication);

            return View(model ?? new EditMedication());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(EditMedication model)
        {
            ViewBag.EditMode = false;

            if (!await _service.CheckMedicationName(model.Name))
                ModelState.AddModelError("name", "That name is already in use.");

            if (ModelState.IsValid)
            {
                HttpContext.Session.SetString(CreateMedicationSessionKey, JsonSerializer.Serialize(model));
                return RedirectToAction(nameof(CreatePreview));
            }

            return View();
        }

        [HttpGet]
        public ActionResult CreatePreview()
        {
            var medication = HttpContext.Session.GetString(CreateMedicationSessionKey);

            if (string.IsNullOrWhiteSpace(medication))
                return RedirectToAction(nameof(Create));

            ViewBag.EditMode = false;

            return View(JsonSerializer.Deserialize<EditMedication>(medication));
        }

        [HttpPost("CreatePreview/")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreatePreviewPost()
        {
            var medication = HttpContext.Session.GetString(CreateMedicationSessionKey);

            if (string.IsNullOrWhiteSpace(medication))
                return RedirectToAction(nameof(Create));

            HttpContext.Session.Remove(CreateMedicationSessionKey);

            var data = JsonSerializer.Deserialize<EditMedication>(medication);

            await _service.AddAsync(data);
            return RedirectToAction(nameof(Index));
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

            ViewBag.EditMode = true;
            ViewBag.Id = id;

            return View(nameof(Create), (EditMedication)item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, EditMedication model)
        {
            if (!await _service.CheckMedicationName(model.Name, id))
                ModelState.AddModelError("name", "That name is already in use.");

            if (ModelState.IsValid)
            {
                HttpContext.Session.SetString(EditMedicationSessionKey, JsonSerializer.Serialize(model));
                return RedirectToAction(nameof(EditPreview), new { id });
            }

            ViewBag.EditMode = true;
            ViewBag.Id = id;

            return View(nameof(Create), model);
        }

        [HttpGet]
        public ActionResult EditPreview(int id)
        {
            var medication = HttpContext.Session.GetString(EditMedicationSessionKey);

            if (string.IsNullOrWhiteSpace(medication))
                return RedirectToAction(nameof(Edit), new { id });

            ViewBag.EditMode = true;
            ViewBag.Id = id;

            return View(nameof(CreatePreview), JsonSerializer.Deserialize<EditMedication>(medication));
        }

        [HttpPost("EditPreview/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPreviewPost(int id)
        {
            var medication = HttpContext.Session.GetString(EditMedicationSessionKey);

            if (string.IsNullOrWhiteSpace(medication))
                return RedirectToAction(nameof(Edit), new { id });

            HttpContext.Session.Remove(EditMedicationSessionKey);

            var data = (Medication)JsonSerializer.Deserialize<EditMedication>(medication);
            data.Id = id;

            await _service.UpdateAsync(data);
            return RedirectToAction(nameof(Index));
        }

        public async Task<ActionResult> Index()
        {
            var items = await _service.GetAllAsync();

            return View(items);
        }
    }
}
