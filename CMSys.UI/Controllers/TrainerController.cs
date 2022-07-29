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
        public IActionResult Index(TrainerGroupsViewModel trainerGroupsViewModel)
        {
            List<List<TrainerViewModel>> trainersInGroup = new List<List<TrainerViewModel>>();
            List<TrainerViewModel> trainersViewModel = new List<TrainerViewModel>();

            var trainers = _context.TrainerRepository.All().ToList();
            var trainerGroups = _context.TrainerGroupRepository.All().ToList();

            trainerGroupsViewModel.TrainerGroups = _mapper.Map(trainerGroups, trainerGroupsViewModel.TrainerGroups);
            trainersViewModel = _mapper.Map(trainers, trainersViewModel);

            foreach (var item in trainerGroupsViewModel.TrainerGroups)
            {
                var listOfTrainersInGroup = trainersViewModel
                        .Where(y => y.TrainerGroupId == item.Id)
                        .GroupBy(x => x.VisualOrder)
                        .SelectMany(trainersModel => trainersModel).ToList();
                item.TrainersInGroup.Add(listOfTrainersInGroup);
            }
            return View(trainerGroupsViewModel);
        }
        [Authorize]
        [Route("admin/trainers")]
        public IActionResult TrainersCollection(TrainersViewModel trainersViewModel)
        {
            var trainers = _context.TrainerRepository.All().ToList();
            trainersViewModel.Trainers = _mapper.Map(trainers, trainersViewModel.Trainers);
            return View(trainersViewModel);
        }
        [Authorize]
        [Route("admin/trainergroups")]
        public IActionResult TrainerGroupsCollection(TrainerGroupsViewModel trainerGroupsViewModel)
        {
            var trainerGroups = _context.TrainerGroupRepository.All().ToList();
            trainerGroupsViewModel.TrainerGroups = _mapper.Map(trainerGroups, trainerGroupsViewModel.TrainerGroups);
            return View(trainerGroupsViewModel);
        }
        [Authorize]
        [Route("admin/trainers/create")]
        [HttpGet]
        public IActionResult TrainerForm()
        {
            var trainerViewModel = new TrainerViewModel();

            trainerViewModel.Users = Users();
            trainerViewModel.TrainerGroupSelector = TrainerGroup();
            return View(trainerViewModel);
        }
        [Authorize]
        [HttpPost]
        [Route("admin/trainers/create")]
        public IActionResult CreateTrainer([FromForm] TrainerViewModel trainerViewModel)
        {
            trainerViewModel.Users = Users();
            trainerViewModel.TrainerGroupSelector = TrainerGroup();
            if (trainerViewModel == null)
            {
                return BadRequest("Trainer object is null.");
            }
            var selectedUserId = trainerViewModel.User.Id;

            var user = _context.UserRepository.Find(x => x.Id == selectedUserId);

            trainerViewModel.Id = selectedUserId;
            trainerViewModel.TrainerGroupId = trainerViewModel.TrainerGroup.Id;

            var trainerPhoto = _context.UserRepository.Find(x => x.Id == selectedUserId).Photo;
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
        public IActionResult CreateTrainerGroup([FromForm] TrainerGroupsViewModel trainerGroupsViewModel)
        {
            var mappedTrainerGroup = _mapper.Map<TrainerGroup>(trainerGroupsViewModel.TrainerGroup);
            _context.TrainerGroupRepository.Add(mappedTrainerGroup);
            _context.Commit();
            return RedirectToAction("TrainerGroupsCollection");
        }
        [Authorize]
        public IActionResult UpdateTrainerGroup(TrainerGroupsViewModel trainerGroupsViewModel)
        {
            var trainerGroup = _context.TrainerGroupRepository.Find(x => x.Id == trainerGroupsViewModel.TrainerGroup.Id);
            trainerGroup = _mapper.Map(trainerGroupsViewModel.TrainerGroup, trainerGroup);
            _context.Commit();
            return RedirectToAction("TrainerGroupsCollection");
        }
        [Authorize]
        [HttpGet]
        [Route("admin/trainergroups/delete/{id}")]
        public IActionResult RemoveTrainerGroup(Guid? id)
        {
            var trainerGroup = _context.TrainerGroupRepository.Find(x => x.Id == id);
            _context.TrainerGroupRepository.Remove(trainerGroup);
            _context.Commit();
            return RedirectToAction("TrainerGroupsCollection");
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
        public IActionResult Update(TrainerViewModel trainerViewModel)
        {
            var trainer = _context.TrainerRepository.Filter(t => t.Id == trainerViewModel.Id).FirstOrDefault();
            trainerViewModel.TrainerGroupSelector = TrainerGroup();
            trainerViewModel.TrainerGroupId = trainerViewModel.TrainerGroup.Id;
            var mappedTrainer = _mapper.Map(trainerViewModel, trainer);
            trainer = mappedTrainer;
            _context.Commit();

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
