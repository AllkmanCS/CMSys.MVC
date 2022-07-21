using AutoMapper;
using CMSys.Core.Repositories;
using CMSys.UI.Helpers;
using CMSys.UI.ViewModels;
using Microsoft.AspNetCore.Mvc;

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
                Console.WriteLine(listOfTrainersInGroup);


                item.TrainersInGroup.Add(listOfTrainersInGroup);
            }
            return View(trainersGroupModel);
        }
        [Route("admin/trainers")]
        public IActionResult TrainersCollection(List<TrainerViewModel> trainersViewModel)
        {
            var trainers = _context.TrainerRepository.All().ToList();
            var trainersModel = _mapper.Map(trainers, trainersViewModel);

            return View(trainersViewModel);
        }
        [Route("admin/trainergroups")]
        public IActionResult TrainerGroupsCollection(List<TrainerGroupViewModel> trainerGroupsViewModel)
        {
            var trainerGroups = _context.TrainerGroupRepository.All().ToList();
            var trainerGroupsModel = _mapper.Map(trainerGroups, trainerGroupsViewModel);

            return View(trainerGroupsModel);
        }
    }
}
