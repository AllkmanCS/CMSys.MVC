using AutoMapper;
using CMSys.Core.Entities.Catalog;
using CMSys.Core.Entities.Membership;
using CMSys.Core.Repositories;
using CMSys.UI.Helpers;
using CMSys.UI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CMSys.UI.Controllers
{
    public class TrainerController : Controller
    {
        private IUnitOfWork _context;
        private IMapper _mapper;
        public TrainerController(IUnitOfWork context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        [Authorize]
        [Route("trainers")]
        public IActionResult Index(List<TrainerGroupViewModel> trainersGroupViewModel)
        {
            List<List<TrainerViewModel>> trainersInGroup = new List<List<TrainerViewModel>>();
            List<TrainerViewModel> trainerViewModels = new List<TrainerViewModel>();

            var trainers = _context.TrainerRepository.All().ToList();
            var trainerGroups = _context.TrainerGroupRepository.All().ToList();

            var trainersGroupModel = _mapper.Map(trainerGroups, trainersGroupViewModel);
            var trainersModel = _mapper.Map(trainers, trainerViewModels);

            foreach (var item in trainersGroupModel)
            {
                var listOfTrainersInGroup = trainersModel
                        .Where(y => y.TrainerGroupId == item.Id)
                        .GroupBy(x => x.VisualOrder)
                        .SelectMany(trainersModel => trainersModel).ToList();
                item.TrainersInGroup.Add(listOfTrainersInGroup);
            }
            return View(trainersGroupModel);
        }
        [Authorize]
        [Route("admin/trainers")]
        public IActionResult TrainersCollection(List<TrainerViewModel> trainersViewModel)
        {
            var trainers = _context.TrainerRepository.All().ToList();
            var trainersModel = _mapper.Map(trainers, trainersViewModel);

            return View(trainersModel);
        }
        [Authorize]
        [Route("admin/trainergroups")]
        public IActionResult TrainerGroupsCollection(List<TrainerGroupViewModel> trainerGroupsViewModel)
        {
            var trainerGroups = _context.TrainerGroupRepository.All().ToList();
            var trainerGroupsModel = _mapper.Map(trainerGroups, trainerGroupsViewModel);

            return View(trainerGroupsModel);
        }
        [Authorize]
        [Route("admin/trainers/create")]
        [HttpGet]
        public IActionResult TrainerForm(TrainerViewModel trainerViewModel)
        {
            trainerViewModel = new TrainerViewModel();

            trainerViewModel.Users = Users();
            trainerViewModel.TrainerGroupSelector = TrainerGroup();
            return View(trainerViewModel);
        }
        [Authorize]
        [HttpPost]
        [Route("admin/trainers/create")]
        public IActionResult CreateTrainer([FromForm] TrainerViewModel trainerViewModel)
        {
            var userViewModel = new UserViewModel();
            trainerViewModel.Users = Users();
            trainerViewModel.TrainerGroupSelector = TrainerGroup();
            if (trainerViewModel == null)
            {
                return BadRequest("Trainer object is null.");
            }
            var selectedUserId = trainerViewModel.User.Id;

            var user = _context.UserRepository.Find(x => x.Id == selectedUserId);
            var mappedUser = _mapper.Map(user, userViewModel);
            trainerViewModel.Id = selectedUserId;
            trainerViewModel.User = mappedUser;
            trainerViewModel.TrainerGroupId = trainerViewModel.TrainerGroup.Id;
            var trainerPhoto = _context.UserRepository.Find(x => x.Id == selectedUserId)
                .Photo;
            var filePath = $"../CMSys.UI/wwwroot/img/{trainerViewModel.User.FullName}.png";
            using (var ms = new MemoryStream(trainerPhoto))
            {
                FileWriter.WriteBytesToFile(filePath, trainerPhoto);
            }
            var mappedTrainer = _mapper.Map<Trainer>(trainerViewModel);

            _context.TrainerRepository.Add(mappedTrainer);

            _context.Commit();
            return RedirectToAction("TrainersCollection");
        }
        [Authorize]
        [Route("admin/trainers/update/{id}")]
        public IActionResult Update(Guid? id)
        {
            var trainer = _context.TrainerRepository.Filter(x => x.Id == id).FirstOrDefault();
            var trainerViewModel = new TrainerViewModel();
            trainerViewModel.Users = Users();
            trainerViewModel.TrainerGroupSelector = TrainerGroup();
            if (id == null)
            {
                return BadRequest();
            }
            var mappedTrainer = _mapper.Map(trainer, trainerViewModel);
            if (mappedTrainer == null)
            {
                return NotFound();
            }
            return View(mappedTrainer);
        }
        [Authorize]
        [HttpPost]
        [Route("admin/trainers/update/{id}")]
        public IActionResult Update(TrainerViewModel trainerViewModel, Guid? id)
        {
            var trainer = _context.TrainerRepository.Filter(t => t.Id == id).FirstOrDefault();
            trainerViewModel.TrainerGroupSelector = TrainerGroup();

            trainerViewModel.TrainerGroupId = trainerViewModel.TrainerGroup.Id;


            //var users = _context.UserRepository.Filter(x => x.Id == id).ToList();
            //var trainerGroup = _context.TrainerGroupRepository.Filter(x => x.Id == id).FirstOrDefault();

            //var selectedTrainerGroup = trainerViewModel.TrainerGroupId;
            if (trainer.Id == trainerViewModel.Id)
            {
                var mappedTrainer = _mapper.Map(trainerViewModel, trainer);
                trainer = mappedTrainer;
                _context.Commit();
            }

            return RedirectToAction("TrainersCollection");
        }
        [Authorize]
        [HttpGet]
        [Route("admin/trainers/delete/{id}")]
        public IActionResult RemoveTrainer(Guid? id)
        {
            var trainer = _context.TrainerRepository.Filter(x => x.Id == id).FirstOrDefault();
            _context.TrainerRepository.Remove(trainer);
            _context.Commit();
            return RedirectToAction("TrainersCollection");
        }
        private ICollection<SelectListItem> Users()
        {
            var users = _context.UserRepository.All()
                .Select(u => (u.FullName, u.Id)).ToList();
            var list = new List<SelectListItem>() { new SelectListItem() };
            foreach (var item in users)
            {
                list.Add(new SelectListItem(item.FullName, item.Id.ToString()));
            }
            return list;
        }
        private ICollection<SelectListItem> TrainerGroup()
        {
            var users = _context.TrainerGroupRepository.All()
            .Select(tg => (tg.Name, tg.VisualOrder, tg.Id)).ToList();
            var list = new List<SelectListItem>() { new SelectListItem() };
            foreach (var item in users)
            {
                list.Add(new SelectListItem(item.Name, item.Id.ToString()));
            }
            return list;
        }

    }
}
