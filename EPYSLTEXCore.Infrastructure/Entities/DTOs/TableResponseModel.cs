

using EPYSLTEXCore.Infrastructure.Entities;

namespace EPYSLTEXCore.Infrastructure.DTOs
{
    public class TableResponseModel
    {
        //public TableResponseModel(int t, IEnumerable<object> r)
        //{
        //    total = t;
        //    rows = r;
        //}

        public TableResponseModel(IEnumerable<dynamic> r, string gridType = "")
        {
            int count;
            try
            {
                count = r.Any() ? r.FirstOrDefault()?.TotalRows : 0;
            }
            catch (Exception)
            {
                count = r.Count();
            }

            if (count == 0) count = r.Count();

            if (gridType == "bootstrap-table")
            {
                total = count;
                rows = r;
            }
            else
            {
                Count = count;
                Items = r;
            }            
        }

        public int total { get; set; }
        public IEnumerable<object> rows { get; set; }
        public int Count { get; set; }
        public IEnumerable<object> Items { get; set; }
    }

    public class TableResponseModel<T> where T : BaseModel
    {
        public TableResponseModel(IEnumerable<T> r)
        {
            rows = r;
            total = r.FirstOrDefault() == null ? 0 : r.FirstOrDefault().TotalRows;
        }

        public int total { get; set; }
        public IEnumerable<T> rows { get; set; }
    }
}