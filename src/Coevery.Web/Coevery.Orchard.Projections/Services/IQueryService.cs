﻿using System.Linq;
using Coevery.ContentManagement;
using Coevery.Orchard.Projections.Models;

namespace Coevery.Orchard.Projections.Services {
    public interface IQueryService : IDependency {
        QueryPart GetQuery(int id);

        QueryPart CreateQuery(string name);
        void DeleteQuery(int id);
    }
}