using AutoMapper;
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
            usersViewModel = _mapper.Map(pagedList, usersViewModel);
            //pagination
            for (int i = 1; i < pagedList.TotalPages; i++)
            {
                if (pagedList.IsNearFromPageOrBoundary(i))
                {
                    usersViewModel.Pagination.Add(i);
                }
            }
            var users = new List<UserViewModel>();
            foreach (var item in usersViewModel.Items)
            {
                users.Add(item);
                var filePath = $"../CMSys.UI/wwwroot/img/{item.FullName}.png";
                using (var ms = new MemoryStream(item.Photo))
                    FileWriter.WriteBytesToFile(filePath, item.Photo);
            }

            return View(usersViewModel);
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
            var roles = _context.RoleRepository.All().ToList();
            userViewModel = _mapper.Map(user, userViewModel);

            foreach (var role in roles)
            {
                if (!userViewModel.Roles.Any(x => x.Id == role.Id))
                {
                    userViewModel.RolesSelection.Add(new SelectListItem(role.Name, role.Id.ToString()));
                }
            }

            return View(userViewModel);
        }
        [Authorize]
        [HttpPost]
        [Route("admin/users/update/{id}")]
        public IActionResult UpdateRole(UserViewModel userViewModel)
        {
            var user = _context.UserRepository.Find(x => x.Id == userViewModel.Id);
            //mapping to get role.Name in the view
            userViewModel = _mapper.Map(user, userViewModel);
            
            var role = _context.RoleRepository.Filter(x => x.Id == userViewModel.Role.Id).FirstOrDefault();

            user.AddRole(role);
            _context.Commit();
            return Redirect($"/admin/users/update/{userViewModel.Id}");
        }
        [Authorize]
        [HttpGet]
        [Route("admin/users/update/removerole/{id}/{roleid}")]
        public IActionResult RemoveRole(UserViewModel userViewModel, Guid roleId)
        {
            var user = _context.UserRepository.Find(x => x.Id == userViewModel.Id);
            var role = _context.RoleRepository.Find(x => x.Id == roleId);
            user.RemoveRole(role);
            _context.Commit();
            return Redirect($"/admin/users/update/{userViewModel.Id}");
        }
        [Authorize]
        [Route("admin/users/update/changepassword")]
        public IActionResult ChangePassword(UserViewModel userViewModel)
        {
            var user = _context.UserRepository.Find(x => x.Id == userViewModel.Id);
            var newPassword = userViewModel.PasswordInput;
            var newPasswordVerify = userViewModel.PasswordInput;

            if (newPassword == newPasswordVerify)
            {
                user.ChangePassword(newPasswordVerify);
            }

            return Redirect($"/admin/users/update/{userViewModel.Id}");
        }
        [Authorize]
        [Route("admin/users/{searchText}")]
        public IActionResult SearchUser(string searchText)
        {
            var query = _context.UserRepository.All().AsQueryable();
            if (!String.IsNullOrEmpty(searchText))
            {
                string[] collection = searchText.Split(' ', ',');
                foreach (var item in collection)
                {
                    query = query.Where(x =>
                    x.FirstName.Contains(item)
                    || x.LastName.Contains(item));
                }
            }
            var result = query.ToList();
            
            var usersViewModel = new UsersViewModel();
            usersViewModel.Items = _mapper.Map(result, usersViewModel.Items);
            return View(usersViewModel);
        }
    }
}
