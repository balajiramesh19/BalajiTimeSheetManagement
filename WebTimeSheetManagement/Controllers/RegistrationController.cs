﻿using Amazon.CloudSearchDomain.Model;
using EventApplicationCore.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebTimeSheetManagement.Concrete;
using WebTimeSheetManagement.Filters;
using WebTimeSheetManagement.Interface;
using WebTimeSheetManagement.Models;
using WebTimeSheetManagement.Service;

namespace WebTimeSheetManagement.Controllers
{
     static class EmailConstants
    {
        public static readonly string RegistrationSubject = "Welcome to Tresume Timesheet";
        public static readonly string TimesheetStatusUpdate = "Timesheet status update";
        public static readonly List<string> ToEmail = new List<string> { "balaji@tresume.us" };
        public static readonly List<string> CCEmail = new List<string> { "karthik@tresume.us" };//, "prab@astacrs.com","rohit@tresume.us"
    }

    [ValidateSuperAdminSession]
    public class RegistrationController : Controller
    {
        private IRegistration _IRegistration;
        private IRoles _IRoles;
        public RegistrationController()
        {
            _IRegistration = new RegistrationConcrete();
            _IRoles = new RolesConcrete();
        }

        // GET: Registration/Create
        public ActionResult Registration()
        {
            return View(new Registration());
        }

        // POST: Registration/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration(Registration registration)
        {
            try
            {
                var isUsernameExists = _IRegistration.CheckUserNameExists(registration.Username);

                if (isUsernameExists)
                {
                    ModelState.AddModelError("", errorMessage: "Username Already Used try unique one!");
                }
                else
                {
                    registration.CreatedOn = DateTime.Now;
                    registration.RoleID = _IRoles.getRolesofUserbyRolename("Users");
                    var tempPlainPassword = registration.Password;
                    registration.Password = EncryptionLibrary.EncryptText(registration.Password);
                    registration.ConfirmPassword = EncryptionLibrary.EncryptText(registration.ConfirmPassword);
                    if (_IRegistration.AddUser(registration) > 0)
                    {
                        TempData["MessageRegistration"] = "Data Saved Successfully!";
                        EmailUtility.SendMailAsync(EmailConstants.RegistrationSubject, GetEmailTemplate(registration),  EmailConstants.ToEmail, EmailConstants.CCEmail, EmailUtility.EnumEmailSentType.Login);
                        return RedirectToAction("Registration");
                    }
                    else
                    {
                        return View(registration);
                    }
                }
                return RedirectToAction("Registration");
            }
            catch
            {
                return View(registration);
            }
        }

        public JsonResult CheckUserNameExists(string Username)
        {
            try
            {
                var isUsernameExists = false;

                if (Username != null)
                {
                    isUsernameExists = _IRegistration.CheckUserNameExists(Username);
                }

                if (isUsernameExists)
                {
                    return Json(data: true);
                }
                else
                {
                    return Json(data: false);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetEmailTemplate(Registration registration)
        {
            return $"Hi {registration.Name},<br/><br/><br>Welcome to Tresume TimeSheet and below are your credentials:<br/><br/><br><b>User Name: </b> " + registration.Username+ "<br/><br/><br> <b>TempPassword : </b>" + EncryptionLibrary.DecryptText(registration.Password)+
                "<br/><br/><br> You can change password after you login with this TempPassword.";
        }

    }
}
