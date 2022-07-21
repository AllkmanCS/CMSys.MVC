namespace CMSys.UI.ViewModels
{
    public class TrainerGroupViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int VisualOrder { get; set; }
        public string Description { get; set; }
        public List<List<TrainerViewModel>> TrainersInGroup { get; set; } = new List<List<TrainerViewModel>>();

    }
}
