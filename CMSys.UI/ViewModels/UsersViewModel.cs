using CMSys.Common.Paging;

namespace CMSys.UI.ViewModels
{
    public class UsersViewModel
    {
        private int _page;
        private int _nextPageNumber;
        private int _previousPageNumber;
        private List<int>? _pagination = new List<int>();

        public int Total { get; set; }
        public int Page { get => _page; set => _page = value; }
        public int PerPage { get; set; } = 20;
        public int TotalPages { get; set; }
        public bool CanNext { get; set; }
        public bool CanPrevious { get; set; }
        public int From { get; set; }
        public int To { get; set; }
        public int NextPageNumber { get => _nextPageNumber + _page; set => _nextPageNumber = value; }
        public int PreviousPageNumber { get => _previousPageNumber - _page; set => _previousPageNumber = value; }
        public PageInfo PageInfo { get; set; } = new PageInfo();

        public List<int>? Pagination
        {
            get
            {
                foreach (var item in _pagination)
                {
                    _page = item;
                }
                return _pagination;
            }
            set => _pagination = value;
        }
        public List<UserViewModel> Items { get; set; } = new List<UserViewModel>();

    }
}
