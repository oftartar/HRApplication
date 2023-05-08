﻿using AutoMapper;
using IKApplication.Application.AbstractRepositories;
using IKApplication.Application.AbstractServices;
using IKApplication.Application.DTOs.CompanyDTOs;
using IKApplication.Application.DTOs.UserDTOs;
using IKApplication.Application.VMs.CompanyVMs;
using IKApplication.Application.VMs.UserVMs;
using IKApplication.Domain.Entites;
using IKApplication.Domain.Enums;
using IKApplication.Persistance.ConcreteRepositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace IKApplication.Infrastructure.ConcreteServices
{
    public class AppUserService : IAppUserService
    {
        private readonly IAppUserRepository _appUserRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ISectorRepository _sectorRepository;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        //Dependency Injection
        public AppUserService(IAppUserRepository appUserRepository, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IMapper mapper, ICompanyRepository companyRepository, ISectorRepository sectorRepository)
        {
            _appUserRepository = appUserRepository;
            _signInManager = signInManager;
            _userManager = userManager;
            _mapper = mapper;
            _companyRepository = companyRepository;
            _sectorRepository = sectorRepository;
        }
        //USerName ile AppUser tablosunda bulunan (eğer varsa) AppUser satrını çekeriz ve UpdateProfileDTO nesnesini doldururuz.
        public async Task<AppUserUpdateDTO> GetByUserName(string userName)
        {
            AppUserUpdateDTO result = await _appUserRepository.GetFilteredFirstOrDefault(
                select: x => new AppUserUpdateDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    SecondName = x.SecondName,
                    Surname = x.Surname,
                    Title = x.Title,
                    BloodGroup = x.BloodGroup,
                    Profession = x.Profession,
                    BirthDate = x.BirthDate,
                    IdentityId = x.IdentityId,
                    Email = x.Email,
                    ImagePath = x.ImagePath,
                    CompanyId = x.CompanyId,
                    CreateDate = x.CreateDate,
                },
                where: x => x.UserName == userName,
                include: x => x.Include(x => x.Company));

            return result;
        }
        public async Task<List<AppUserVM>> GetAllUsers()
        {
            var users = await _appUserRepository.GetFilteredList(
                select: x => new AppUserVM
                {
                    Id = x.Id,
                    Name = x.Name,
                    SecondName = x.SecondName,
                    Surname = x.Surname,
                    Title = x.Title,
                    BloodGroup = x.BloodGroup,
                    Profession = x.Profession,
                    BirthDate = x.BirthDate,
                    IdentityId = x.IdentityId,
                    CompanyId = x.CompanyId,
                    CompanyName = x.Company.Name,
                    ImagePath = x.ImagePath,
                    UserName = x.UserName,
                    Email = x.Email,
                },
                where: x => (x.Status == Status.Active || x.Status == Status.Modified),
                include: x => x.Include(x => x.Company));

            foreach (var user in users)
            {
                user.Roles = (List<string>)await _userManager.GetRolesAsync(await _userManager.FindByNameAsync(user.UserName));
            }

            return users;
        }
        public async Task<bool> Login(LoginDTO model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                await _signInManager.SignInAsync(user, true);
                return true;
            }

            return false;
        }

        public async Task<IdentityResult> CreateUser(AppUserCreateDTO model, string role)
        {
            var appUser = _mapper.Map<AppUser>(model);
            //!!!!!!!!!!Email'in daha önce kullanılıp kullanılmadığını kontrol et!!!!!!!!!!!!
            appUser.UserName = model.Email;
            var result = await _userManager.CreateAsync(appUser, string.IsNullOrEmpty(model.Password) ? "" : model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(appUser, role);
                return result;
            }

            return result;
        }

        public async Task Delete(Guid id)
        {
            AppUser user = await _userManager.FindByIdAsync(id.ToString());

            if (user != null)
            {
                user.DeleteDate = DateTime.Now;
                user.Status = Status.Deleted;

                await _appUserRepository.Delete(user);
            }
        }

        //sistemden çıkıç için kullanırız. User bilgileri sessiondan silinir.
        public async Task LogOut()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task UpdateUser(AppUserUpdateDTO model)
        {
            //Update işlemlerind eönce Id ile ilgili nesneyi rame çekeriz. Dışarıdan gelen güncel bilgilerle değişiklikleri yaparız.
            //En Son SaveChanges ile veri tabanına güncellemeleri göndeririz. 

            AppUser user = await _userManager.FindByIdAsync(model.Id.ToString());

            if (user != null)
            {
                if (model.Password != null)
                {
                    user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, model.Password);
                }

                user.Name = model.Name;
                user.SecondName = model.SecondName;
                user.Surname = model.Surname;
                user.Title = model.Title;
                user.BloodGroup = model.BloodGroup;
                user.Profession = model.Profession;
                user.BirthDate = model.BirthDate;
                user.IdentityId = model.IdentityId;
                user.ImagePath = model.ImagePath;
                user.CompanyId = model.CompanyId;
                user.CreateDate = model.CreateDate;
                user.UpdateDate = model.UpdateDate;
                user.Status = model.Status;

                await _userManager.UpdateAsync(user);
            }
        }

        public async Task<AppUserUpdateDTO> GetById(Guid id)
        {
            AppUser appUser = await _appUserRepository.GetDefault(x => x.Id == id);

            if (appUser != null)
            {
                var model = _mapper.Map<AppUserUpdateDTO>(appUser);

                return model;
            }

            return null;
        }

        public async Task<List<Sector>> GetSectorsAsync()
        {
            return await _sectorRepository.GetDefaults(x => x.Status == Status.Active || x.Status == Status.Modified);
        }

        public async Task RegisterUserWithCompany(RegisterDTO register, string role)
        {
            Company newCompany = new Company()
            {
                Id = Guid.NewGuid(),
                Name = register.CompanyName,
                Email = register.CompanyEmail,
                PhoneNumber = register.CompanyPhoneNumber,
                SectorId = register.CompanySectorId,
                NumberOfEmployees = register.CompanyNumberOfEmployees,
                CreateDate = register.CompanyCreateDate,
                Status = register.CompanyStatus
            };

            AppUser newUser = new AppUser()
            {
                Name = register.UserName,
                SecondName = register.UserSecondName,
                Surname = register.UserSurname,
                Title = register.UserTitle,
                BloodGroup = register.UserBloodGroup,
                Profession = register.UserProfession,
                BirthDate = register.UserBirthDate,
                IdentityId = register.UserIdentityId,
                Email = register.UserEmail,
                ImagePath = register.UserImagePath,
                CreateDate = register.UserCreateDate,
                Status = register.UserStatus,
                CompanyId = newCompany.Id,
                Company = newCompany
            };

            newUser.UserName = register.UserEmail;

            if (register.UserPassword == register.UserConfirmPassword)
            {
                await _companyRepository.Create(newCompany);
                var result = await _userManager.CreateAsync(newUser, string.IsNullOrEmpty(register.UserPassword) ? "" : register.UserPassword);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newUser, role);
                }
            }
        }
        public async Task<AppUserVM> GetCurrentUserInfo(string userName)
        {
            AppUserVM result = await _appUserRepository.GetFilteredFirstOrDefault(
                select: x => new AppUserVM
                {
                    Name = x.Name,
                    SecondName = x.SecondName,
                    Surname = x.Surname,
                    Title = x.Title,
                    BloodGroup = x.BloodGroup,
                    Profession = x.Profession,
                    BirthDate = x.BirthDate,
                    IdentityId = x.IdentityId,
                    CompanyId = x.CompanyId,
                    CompanyName = x.Company.Name,
                    ImagePath = x.ImagePath,
                    UserName = x.UserName,
                    Email = x.Email,
                },
                where: x => x.UserName == userName,
                include: x => x.Include(x => x.Company));

            result.Roles = (List<string>)await _userManager.GetRolesAsync(await _userManager.FindByNameAsync(userName));

            return result;
        }

        public async Task<List<RegisterVM>> GetAllRegistrations()
        {
            var registers = await _appUserRepository.GetFilteredList(
                select: x => new RegisterVM
                {
                    UserId = x.Id,
                    UserName = x.Name,
                    UserSecondName = x.SecondName,
                    UserSurname = x.Surname,
                    UserTitle = x.Title,
                    UserEmail = x.Email,
                    CompanyId = x.CompanyId,
                    CompanyName = x.Company.Name,
                    CompanySector = x.Company.Sector.Name,
                    NumberOfEmployees = x.Company.NumberOfEmployees
                },
                where: x => (x.Status == Status.Passive),
                include: x => x.Include(x => x.Company));

            return registers;
        }
    }
}
