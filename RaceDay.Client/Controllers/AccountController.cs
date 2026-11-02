using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Client.Models;

namespace RaceDay.Client.Controllers;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _factory;

    public AccountController(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var client = _factory.CreateClient("Api");
            var response = await client.PostAsJsonAsync("api/Auth/login", new
            {
                email = model.Email,
                password = model.Password
            });

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Login failed. Check your email and password.");
                return View(model);
            }

            var login = await response.Content.ReadFromJsonAsync<ApiLoginResponse>();
            if (login == null || string.IsNullOrWhiteSpace(login.Token))
            {
                ModelState.AddModelError("", "The API returned an invalid login response.");
                return View(model);
            }

            HttpContext.Session.SetString("Token", login.Token);
            HttpContext.Session.SetString("Role", login.Role);
            HttpContext.Session.SetString("FullName", login.FullName);
            HttpContext.Session.SetString("UserId", login.UserId.ToString());

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Events");
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError("", "Cannot reach the RaceDay API. Check that RaceDay.Api is running and ApiSettings:BaseUrl is correct.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}
