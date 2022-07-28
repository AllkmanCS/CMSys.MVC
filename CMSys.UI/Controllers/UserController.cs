﻿using AutoMapper;
using CMSys.Core.Entities.Membership;
using CMSys.Core.Repositories;
using CMSys.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using CMSys.UI.ViewModels;
using CMSys.Common.Paging;
using CMSys.UI.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CMSys.UI.Controllers
{
    public class UserController : Controller
    {
        private IUnitOfWork _context;
        private IMapper _mapper;
        public UserController(IUnitOfWork context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        //[Route("/account/login")]
        //[HttpGet("login")] //specify route to be /login instead of /account/login
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid) return View();
            var user = _context.UserRepository.FindByEmail(loginViewModel.Email);
            user.ChangePassword("admin");
            Console.WriteLine(user.Email);
            var userPassword = user.VerifyPassword(loginViewModel.Password);
            Console.WriteLine(userPassword);


            if (user.Email == loginViewModel.Email && userPassword)
            {
                var claimsIdentity = new ClaimsIdentity(user.GetClaims(), CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                HttpContext.SignInAsync(claimsPrincipal);
                return RedirectToAction("Index", "Course");
            }
            TempData["Error"] = "Username or password is invalid";
            return View("login");
        }
        [Route("admin/users")]
        public IActionResult IndexUsers(int page, int perPage, string fullName)
        {
            var usersViewModel = new UsersViewModel();
            page = usersViewModel.PageInfo.Page;
            perPage = usersViewModel.PerPage;

            var pagedList = _context.UserRepository.GetPagedList(new PageInfo(page, perPage), fullName,
              u => string.IsNullOrEmpty(fullName) ? true : u.FullName == fullName);
            var mappedUsers = _mapper.Map(pagedList, usersViewModel);
            //pagination
            for (int i = 1; i < pagedList.TotalPages; i++)
            {
                if (pagedList.IsNearFromPageOrBoundary(i))
                {
                    usersViewModel.Pagination.Add(i);
                }
            }
            var users = new List<UserViewModel>();
            foreach (var item in mappedUsers.Items)
            {
                users.Add(item);
                var filePath = $"../CMSys.UI/wwwroot/img/{item.FullName}.png";
                using (var ms = new MemoryStream(item.Photo))
                    FileWriter.WriteBytesToFile(filePath, item.Photo);
            }

            return View(mappedUsers);
        }
        [Authorize]
        [HttpGet]
        [Route("admin/users/{id}")]
        public IActionResult GetUser(Guid id)
        {
            var userViewModel = new UserViewModel();
            var user = _context.UserRepository.Find(x => x.Id == id);
            // var userRoles = _context.RoleRepository.Filter(x => x.)

            var mappedUser = _mapper.Map(user, userViewModel);
            return View(mappedUser);
        }
        [Authorize]
        [Route("admin/users/update/{id}")]
        public IActionResult UpdateUserForm(UserViewModel userViewModel)
        {
            var user = _context.UserRepository.Find(x => x.Id == userViewModel.Id);
            userViewModel = _mapper.Map(user, userViewModel);
            var roles = _context.RoleRepository.All().ToList();

            foreach (var role in roles)
            {
                if (!userViewModel.Roles.Any(x => x.Id == role.Id))
                {
                    userViewModel.RolesSelection.Add(new SelectListItem(role.Name, role.Id.ToString()));
                }
            }

            _context.Commit();
            return View(userViewModel);
        }
    }
}
