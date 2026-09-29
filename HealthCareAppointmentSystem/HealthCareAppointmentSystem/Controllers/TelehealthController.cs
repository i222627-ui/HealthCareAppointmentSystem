using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCareAppointmentSystem.Controllers
{
    [Authorize]
    public class TelehealthController : Controller
    {
        // GET: /Telehealth/Room/{id}
        public IActionResult Room(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            ViewBag.RoomId = id;
            return View();
        }
    }
}
