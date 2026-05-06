using Chat_App.Models;
using Microsoft.AspNetCore.Mvc;

namespace Chat_App.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDBContext context;

        public AccountController(ApplicationDBContext context)
        {
            this.context = context;
        }

        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Login(UsersList users)
        {

            var existinguser = context.user.FirstOrDefault(user => user.username == users.username && user.password == users.password);

            if (existinguser != null)
            {
                HttpContext.Session.SetInt32("UserId", existinguser.Id);
                return RedirectToAction("Index", "Chat");
            }
            return View();
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login","Account");
        }
    }
}
