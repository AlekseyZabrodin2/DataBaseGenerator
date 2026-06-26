using System.Collections.Generic;
using System.Collections.ObjectModel;
using DataBaseGenerator.Core.LiteDbGenerator.LiteDbModels;
using DataBaseGenerator.Core.LiteDbGenerator.Models;

namespace DataBaseGenerator.Core.LiteDbGenerator.Contracts
{
    public interface IPlannedStudiesRepository
    {
        public PlannedStudyLiteDb GetById(string id);
        public void Insert(PlannedStudyLiteDb study);
        public void InsertBulk(IEnumerable<PlannedStudyLiteDb> studies);
        public void Update(PlannedStudyLiteDb study);
        public void Delete(string id);
        public void DeleteAll();
        public ObservableCollection<LitePlannedStudy> GetAllStudies();
        bool Any();
    }
}
