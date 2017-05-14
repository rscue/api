namespace Rscue.Api.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public enum RepositoryOutcome
    {
        Ok,
        Created,
        ValidationError,
        NotFound
    }
}
