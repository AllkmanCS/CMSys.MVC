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
        [HttpGet]
        [Route("courses")]
        public IActionResult Index(int page, int perPage, string courseTypeName)
        {
            CoursesViewModel courses = GetCoursesViewModel(page, perPage, courseTypeName);
            return View(courses);
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
                var filePath = $"../CMSys.UI/wwwroot/img/{item.Trainer.User.FullName}.png";
                using (var ms = new MemoryStream(item.Trainer.User.Photo))
                    FileWriter.WriteBytesToFile(filePath, item.Trainer.User.Photo);
            }
            return View(mappedCourse);
        }
        [Authorize]
        [Route("admin/coursegroups")]
        public IActionResult CourseGroupsCollection()
        {
            var courseGroups = _context.CourseGroupRepository.All().ToList();
            return View(new CourseGroupsViewModel()
            {
                CourseGroups = _mapper.Map<List<CourseGroupViewModel>>(courseGroups)
            });
        }
        [Authorize]
        [Route("admin/courses/create")]
        [HttpGet]
        public IActionResult CourseForm()
        {
            return View(new CourseViewModel()
            {
                CourseTypes = CourseTypes(),
                CourseGroups = CourseGroups()
            }) ;
        }
        [Authorize]
        [HttpPost]
        [Route("admin/courses/create")]
        public IActionResult CreateCourse([FromForm] CourseViewModel courseViewModel)
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
        public IActionResult UpdateForm(CourseViewModel courseViewModel)
        {
            var course = _context.CourseRepository.Filter(c => c.Id == courseViewModel.Id).FirstOrDefault();
            courseViewModel.CourseGroups = CourseGroups();
            courseViewModel.CourseTypes = CourseTypes();
            courseViewModel = _mapper.Map(course, courseViewModel);

            return View(courseViewModel);
        }
        [Authorize]
        [HttpPost]
        [Route("admin/courses/update/{id}")]
        public IActionResult Update(CourseViewModel courseViewModel)
        {
            var course = _context.CourseRepository.Filter(c => c.Id == courseViewModel.Id).FirstOrDefault();
            course = _mapper.Map(courseViewModel, course);
            _context.Commit();

            return RedirectToAction("CourseCollection");
        }
        [Authorize]
        [HttpGet]
        [Route("admin/courses/delete/{id}")]
        public IActionResult RemoveCourse(Guid? id)
        {
            var course = _context.CourseRepository.Filter(c => c.Id == id).FirstOrDefault();
            _context.CourseRepository.Remove(course);
            _context.Commit();
            return RedirectToAction("CourseCollection");
        }
        [Authorize]
        [HttpGet]
        [Route("admin/courses/trainers/{id}")]
        public IActionResult AddCourseTrainersForm(Guid id)
        {
            var courseViewModel = new CourseViewModel();
            var course = _context.CourseRepository.Filter(c => c.Id == id).FirstOrDefault();
            var trainers = _context.TrainerRepository.All().ToList();
            var courseTrainers = _context.CourseTrainerRepository.Filter(x => x.CourseId == id).ToList();
            courseViewModel = _mapper.Map(course, courseViewModel);

            foreach (var trainer in trainers)
            {
                //if course has trainer selected it will not appear in <Select> and re-appear once i remove trainer from course
                if (!courseTrainers.Any(x => x.TrainerId == trainer.Id))
                {
                    courseViewModel.SelectionTrainers.Add(new SelectListItem(trainer.User.FullName, trainer.User.Id.ToString()));
                }
            }
            courseViewModel.Trainers = _mapper.Map(courseTrainers, courseViewModel.Trainers);

            return View(courseViewModel);
        }
        [Authorize]
        [HttpPost]
        [Route("admin/courses/trainers/{id}")]
        public IActionResult AddCourseTrainers(CourseViewModel courseViewModel)
        {
            var trainers = _context.TrainerRepository.All().ToList();
            var trainersViewModel = new List<TrainerViewModel>();
            trainersViewModel = _mapper.Map(trainers, trainersViewModel);

            var selectedTrainerId = courseViewModel.Trainer.User.Id;

            var courseTrainerViewModel = new CourseTrainerViewModel();
            var courseTrainers = _context.CourseTrainerRepository.All().Where(x => x.CourseId == courseViewModel.Id).ToList();

            courseTrainerViewModel.Trainer = trainersViewModel
                .Where(x => x.User.Id == selectedTrainerId).FirstOrDefault();

            courseTrainerViewModel.CourseId = courseViewModel.Id;
            courseTrainerViewModel.TrainerId = selectedTrainerId;

            var courseTrainersViewModel = new List<CourseTrainerViewModel>();
            courseTrainersViewModel = _mapper.Map(courseTrainers, courseTrainersViewModel); //view model now has all current course trainers.
            //getting courseTrainer props from viewModel and mapping them, then adding them into db entity coureTrainer
            var courseTrainer = _mapper.Map<CourseTrainer>(courseTrainerViewModel);
            _context.CourseTrainerRepository.Add(courseTrainer);
            _context.Commit();
            return RedirectToAction("AddCourseTrainers");
        }
        [Authorize]
        [HttpGet]
        [Route("admin/courses/trainers/delete/{id}")]
        public IActionResult RemoveCourseTrainer(Guid? id)
        {
            //sometimes it finds two same Trainer records with same id so I use FirstOrDaufault()
            var courseTrainer = _context.CourseTrainerRepository.Filter(x => x.TrainerId == id).FirstOrDefault();
            _context.CourseTrainerRepository.Remove(courseTrainer);
            _context.Commit();
            //path to the url of the view (trainers update)
            return Redirect($"/admin/courses/trainers/{courseTrainer.CourseId}");
        }
        [Authorize]
        public IActionResult PostCourseGroup(CourseGroupsViewModel courseGroupsViewModel)
        {
            var courseGroup = _mapper.Map<CourseGroup>(courseGroupsViewModel.CourseGroup);
            _context.CourseGroupRepository.Add(courseGroup);
            _context.Commit();
            return RedirectToAction("CourseGroupsCollection");
        }
        [Authorize]
        public IActionResult UpdateCourseGroup(CourseGroupsViewModel courseGroupsViewModel)
        {
            var courseGroup = _context.CourseGroupRepository.Find(x => x.Id == courseGroupsViewModel.CourseGroup.Id);
            courseGroup = _mapper.Map(courseGroupsViewModel.CourseGroup, courseGroup);
            _context.Commit();
            return RedirectToAction("CourseGroupsCollection");
        }
        [Authorize]
        [HttpGet]
        [Route("admin/coursegroups/delete/{id}")]
        public IActionResult RemoveCourseGroup(Guid? id)
        {
            var courseGroup = _context.CourseGroupRepository.Find(x => x.Id == id);
            _context.CourseGroupRepository.Remove(courseGroup);
            _context.Commit();
            return RedirectToAction("CourseGroupsCollection");
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
            var courseGroups = _context.CourseGroupRepository.All()
                .Select(ct => (ct.Name, ct.VisualOrder, ct.Id)).ToList();
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