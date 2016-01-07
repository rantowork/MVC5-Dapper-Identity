using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DapperIdentity.Core.Entities;
using DapperIdentity.Web.Email;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using ExternalLoginConfirmationViewModel = DapperIdentity.Web.ViewModels.ExternalLoginConfirmationViewModel;
using ForgotPasswordViewModel = DapperIdentity.Web.ViewModels.ForgotPasswordViewModel;
using LoginViewModel = DapperIdentity.Web.ViewModels.LoginViewModel;
using RegisterViewModel = DapperIdentity.Web.ViewModels.RegisterViewModel;
using ResetPasswordViewModel = DapperIdentity.Web.ViewModels.ResetPasswordViewModel;

namespace DapperIdentity.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;

        public AccountController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                //check to see if the account exists
                var user = await _userManager.FindAsync(model.Email.TrimEnd(), model.Password);
                if (user != null)
                {
                    //check if the account has already had its email confirmed
                    if (!user.IsConfirmed)
                    {
                        //check to see if the token is greater than 24 hours old
                        if ((DateTime.UtcNow - user.CreatedDate).TotalDays > 1)
                        {
                            await ResendConfirmationToken(user);
                            ModelState.AddModelError("", "Email address has not been confirmed and has expired.  A new confirmation token has been generated and sent to you.");
                            return View(model);
                        }

                        //account hasn't been confirmed but it also hasn't been 24 hours
                        ModelState.AddModelError("", "Email address has not been confirmed.  Please check your e-mail!");
                        return View(model);
                    }

                    //we're good, sign the user in
                    await SignInAsync(user, model.RememberMe);
                    return RedirectToLocal(returnUrl);
                }
                ModelState.AddModelError("", "Invalid UserName of Password.");
            }

            return View(model);
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var confirmationToken = Guid.NewGuid().ToString();
                var user = new User { UserName = model.Email.TrimEnd(), Nickname = model.Nickname.TrimEnd(), IsConfirmed = false, ConfirmationToken = confirmationToken, CreatedDate = DateTime.UtcNow };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SendGridHelper.SendRegistrationEmail(confirmationToken, model.Email.TrimEnd());
                    return View("ConfirmationLinkSent");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        public async Task<ActionResult> ConfirmationLink(string id)
        {
            //decode the confirmation token
            var token = SendGridHelper.DecodeConfirmationToken(id);

            //find the user based on the email address
            var user = await _userManager.FindByNameAsync(token.Email);

            if (user != null)
            {
                //check if the user has already confirmed their account
                if (user.IsConfirmed)
                {
                    ViewBag.MessageTitle = "Already Confirmed";
                    ViewBag.Message = "Your account is already confirmed!";
                    return View();
                }

                //check if the confirmation token is older than a day, if it is send them a new token
                if ((DateTime.UtcNow - user.CreatedDate).TotalDays > 1)
                {
                    await ResendConfirmationToken(user);
                    ViewBag.MessageTitle = "Token Expired";
                    ViewBag.MessageTitle = "The confirmation token has expired.  A new token has been generated and emailed to you.";
                    return View();
                }

                //set the account to confirmed and updated the user
                user.IsConfirmed = true;
                await _userManager.UpdateAsync(user);

                //pop the view to let the user know the confirmation was successful
                ViewBag.MessageTitle = "Confirmation Successful";
                ViewBag.Message = "Your account has been successfully activated!  Click <a href='/Account/Login'>here</a> to login.";
                return View();
            }

            //if we got this far then the token is completely invalid
            ViewBag.MessageTitle = "Invalid Confirmation Token";
            ViewBag.Message = "The confirmation token is invalid.  If you feel you have received this message in error, please contact [your email]";
            return View();
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Email.TrimEnd());
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByNameAsync(model.Email.TrimEnd());
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await _userManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var user = await _userManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
            return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new User { UserName = model.Email.TrimEnd(), Nickname = model.Nickname.TrimEnd(), CreatedDate = DateTime.UtcNow, IsConfirmed = true };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, isPersistent: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        private async Task SignInAsync(User user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await _userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        private async Task ResendConfirmationToken(User user)
        {
            //create a new confirmation token
            var confirmationToken = Guid.NewGuid().ToString();

            //update the users confirmation token and reset the created date
            user.ConfirmationToken = confirmationToken;
            user.CreatedDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            //send the new confirmation link to the user
            await SendGridHelper.SendRegistrationEmail(confirmationToken, user.UserName);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _userManager?.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}