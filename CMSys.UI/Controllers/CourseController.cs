using AutoMapper;
using CMSys.Common.Paging;
using CMSys.Core.Entities.Catalog;
using CMSys.Core.Repositories;
using CMSys.UI.Helpers;
using CMSys.UI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using System.IO;

namespace CMSys.UI.Controllers
{
    public class CourseController : Controller
    {
        private IUnitOfWork _context;
        private IMapper _mapper;
        public CourseController(IUnitOfWork context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        [Route("courses")]
        public IActionResult Index(int page, int perPage, string courseTypeName)
        {
            CoursesViewModel courses = GetCoursesViewModel(page, perPage, courseTypeName);
            return View(courses);
        }
        [Authorize]
        [Route("admin/courses")]
        public IActionResult CourseCollection(int page, int perPage, string courseTypeName)
        {
            var mappedCourses = GetCoursesViewModel(page, perPage, courseTypeName);
            return View(mappedCourses);
        }
        [Route("courses/{id}")]
        public IActionResult GetCourse(Guid id, int page, int perPage, string courseTypeName)
        {
            var courseViewModel = new CourseViewModel();
            page = courseViewModel.PageInfo.Page;
            perPage = courseViewModel.PageInfo.PerPage;
            var pagedList = _context.CourseRepository.GetPagedList(new PageInfo(page, perPage),
                c => string.IsNullOrEmpty(courseTypeName) ? true : c.CourseType.Name == courseTypeName);
            var course = pagedList.Items.Where(x => x.Id == id).FirstOrDefault();
            if (course == null)
            {
                return NotFound();
            }
            var mappedCourse = _mapper.Map<CourseViewModel>(course);
            foreach (var item in mappedCourse.Trainers)
            {
                var trainerPhoto = item.Trainer.User.Photo;
                var filePath = $"../CMSys.UI/wwwroot/img/{item.Trainer.User.FullName}.png";
                using (var ms = new MemoryStream(trainerPhoto))
                FileWriter.WriteBytesToFile(filePath, trainerPhoto);
            }
                return View(mappedCourse);
        }
        [Authorize]
        [Route("admin/coursegroups")]
        public IActionResult CourseGroupsCollection(List<CourseGroupViewModel> courseGroupsViewModel)
        {
            var courseGroups = _context.CourseGroupRepository.All();
            var mappedCourseGroups = _mapper.Map(courseGroups, courseGroupsViewModel);
            return View(mappedCourseGroups);
        }
        [Authorize]
        [Route("admin/courses/create")]
        [HttpGet]
        public IActionResult CourseForm(CourseViewModel courseViewModel)
        {
            courseViewModel = new CourseViewModel();

            courseViewModel.CourseTypes = CourseTypes();
            courseViewModel.CourseGroups = CourseGroups();
            return View(courseViewModel);
        }
        [Authorize]
        [HttpPost]
        [Route("admin/courses/create")]
        public IActionResult CreateCourse([FromForm]CourseViewModel courseViewModel)
        {
            courseViewModel.CourseTypes = CourseTypes();
            courseViewModel.CourseGroups = CourseGroups();

            if (courseViewModel == null)
            {
                return BadRequest("Course object is null.");
            }

            var mappedCourse = _mapper.Map<Course>(courseViewModel);

            _context.CourseRepository.Add(mappedCourse);
           
            _context.Commit();

            return RedirectToAction("CourseCollection");
        }
        [Authorize]
        [Route("admin/courses/update/{id}")]
        public IActionResult Update(Guid? id)
        {
            var course = _context.CourseRepository.Filter(c => c.Id == id).FirstOrDefault();
            var courseViewModel = new CourseViewModel();
            courseViewModel.CourseGroups = CourseGroups();
            courseViewModel.CourseTypes = CourseTypes();
            if (id == null)
            {
                return BadRequest();
            }
            var mappedCourse = _mapper.Map(course, courseViewModel);
            if (mappedCourse == null)
            {
                return NotFound();
            }
            return View(mappedCourse);
        }
        [Authorize]
        [HttpPost]
        [Route("admin/courses/update/{id}")]
        public IActionResult Update(CourseViewModel courseViewModel, Guid? id)
        {
            var course = _context.CourseRepository.Filter(c => c.Id == id).FirstOrDefault();

            if (course.Id == courseViewModel.Id)
            {
                var mappedCourse = _mapper.Map(courseViewModel, course);
                course = mappedCourse;
                _context.Commit();

            }
            return RedirectToAction("CourseCollection");
        }
        [Authorize]
        [HttpGet]
        [Route("admin/courses/trainers/{id}")]
        public IActionResult Trainers(Guid id)
        {
            var courseViewModel = new CourseViewModel();
            var course = _context.CourseRepository.Filter(c => c.Id == id).FirstOrDefault();

            var trainers = _context.TrainerRepository.All().ToList();
           // var trainersViewModel = new List<TrainerViewModel>();
           // var mappedTrainers = _mapper.Map(trainers, trainersViewModel);
            foreach (var trainer in trainers)
            {
                courseViewModel.SelectionTrainers.Add(new SelectListItem(trainer.User.FullName, trainer.User.Id.ToString()));
            }
            var mappedCourse = _mapper.Map(course, courseViewModel);

            var courseTrainers = _context.CourseTrainerRepository.All().Where(x => x.CourseId == id).ToList();
            var mappedCourseTrainers = _mapper.Map(courseTrainers, mappedCourse.Trainers);
            mappedCourse.Trainers = mappedCourseTrainers;
            return View(mappedCourse);
        }
        [Authorize]
        [HttpPost]
        [Route("admin/courses/trainers/{id}")]
        public IActionResult Trainers(CourseViewModel courseViewModel, Guid? id)
        {
            var trainers = _context.TrainerRepository.All().ToList();
            var trainersViewModel = new List<TrainerViewModel>();
            var mappedTrainers = _mapper.Map(trainers, trainersViewModel);
            var selectedTrainerId = courseViewModel.Trainer.User.Id;

            var courseTrainerViewModel = new CourseTrainerViewModel();
            var courseTrainers = _context.CourseTrainerRepository.All().Where(x => x.CourseId == id).ToList();

            courseTrainerViewModel.Trainer = trainersViewModel
                .Where(x => x.User.Id == selectedTrainerId).FirstOrDefault();
            courseTrainerViewModel.CourseId = id;
            courseTrainerViewModel.TrainerId = selectedTrainerId;

            var courseTrainersViewModel = new List<CourseTrainerViewModel>();
            var mappedCourseTrainers = _mapper.Map(courseTrainers, courseTrainersViewModel); //view model now has all current course trainers.

            //mappedCourseTrainers.Add(courseTrainerViewModel);
            var courseTrainer = _mapper.Map<CourseTrainer>(courseTrainerViewModel);
            _context.CourseTrainerRepository.Add(courseTrainer);
            _context.Commit();
            //var mappedCourse = _mapper.Map(mappedCourseTrainers, courseViewModel.Trainers);
            return RedirectToAction("Trainers");
        }
        [Authorize]
        [HttpGet]
        [Route("admin/courses/trainers/delete/{id}")]
        public IActionResult RemoveTrainer(Guid? id)
        {
            //sometimes it finds two same Trainer records with same id so I use FirstOrDaufault()
            var courseTrainer = _context.CourseTrainerRepository.Filter(x => x.TrainerId == id).FirstOrDefault();
            _context.CourseTrainerRepository.Remove(courseTrainer);
            _context.Commit();
            //path to the url of the view (trainers update)
            return Redirect($"/admin/courses/trainers/{courseTrainer.CourseId}");
        }
        private CoursesViewModel GetCoursesViewModel(int page, int perPage, string courseTypeName)
        {
            var coursesViewModel = new CoursesViewModel();
            page = coursesViewModel.Page;
            perPage = coursesViewModel.PerPage;

            var pagedList = _context.CourseRepository.GetPagedList(new PageInfo(page, perPage),
              c => string.IsNullOrEmpty(courseTypeName) ? true : c.CourseType.Name == courseTypeName);
            var mappedCourses = _mapper.Map(pagedList, coursesViewModel);
            //pagination
            
            for (int i = 1; i < pagedList.TotalPages; i++)
            {
                if (pagedList.IsNearFromPageOrBoundary(i))
                {
                    coursesViewModel.Pagination.Add(i);
                }
            }
            coursesViewModel.CourseTypes = CourseTypes();
            coursesViewModel.CourseGroups = CourseGroups();
            return mappedCourses;
        }
        private ICollection<SelectListItem> CourseTypes()
        {
            var courseTypes = _context.CourseTypeRepository.All().Select(ct => (ct.Name, ct.VisualOrder, ct.Id)).ToList();
            var list = new List<SelectListItem>() { new SelectListItem() };
            foreach (var item in courseTypes)
            {
                list.Add(new SelectListItem(item.Name, item.Id.ToString()));
            }
            return list;
        }
        private ICollection<SelectListItem> CourseGroups()
        {
            var courseGroups = _context.CourseGroupRepository.All().Select(ct => (ct.Name, ct.VisualOrder, ct.Id)).ToList();
            var list = new List<SelectListItem>() { new SelectListItem() };
            foreach (var item in courseGroups)
            {
                list.Add(new SelectListItem(item.Name, item.Id.ToString()));
                
            }
            return list;
        }
        public IActionResult Privacy()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}