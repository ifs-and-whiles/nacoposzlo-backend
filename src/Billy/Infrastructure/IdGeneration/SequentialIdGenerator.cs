using System;
using Billy.IdGeneration;
using Marten.Schema.Identity;
using RT.Comb;

namespace Billy.Infrastructure.IdGeneration
{
    public class SequentialIdGenerator : IIdGenerator
    {
        public Guid NewId() => Provider.PostgreSql.Create();
    }
}