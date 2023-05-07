﻿using IKApplication.Application.AbstractServices;
using IKApplication.Application.DTOs.CompanyDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IKApplication.MVC.Areas.SiteAdministrator.Controllers
{
    [Area("CompanyAdministrator")]
    [Authorize(Roles = "Company Administrator")]
    public class CompanyController : Controller
    {
        private readonly ICompanyService _companyService;
        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }
        public async Task<IActionResult> Update(Guid id)
        {
            return View(await _companyService.GetById(id));
        }
        [HttpPost]
        public async Task<IActionResult> Update(CompanyUpdateDTO updateCompanyDTO)
        {
            if (ModelState.IsValid)
            {
                await _companyService.Update(updateCompanyDTO);
                return RedirectToAction("Index", "Dashboard");
            }

            return View(updateCompanyDTO);
        }
    }
}