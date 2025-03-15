using System;

namespace Billy.IdGeneration
{
    public interface IIdGenerator
    {
        Guid NewId();
    }
}