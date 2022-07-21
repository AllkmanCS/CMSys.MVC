namespace CMSys.UI.ViewModels
{
    public class CourseTrainerViewModel
    {
        public Guid? CourseId { get; set; }
        public Guid? TrainerId { get; set; }
        public int? VisualOrder { get; set; }

        public TrainerViewModel Trainer { get; set; }
    }
}
