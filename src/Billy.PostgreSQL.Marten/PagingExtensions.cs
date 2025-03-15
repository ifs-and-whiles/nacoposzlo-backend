using System.Linq;

namespace Billy.PostgreSQL.Marten
{
    public static class PagingExtensions
    {
        public static IQueryable<T> Page<T>(this IQueryable<T> collection, int pageSize, int pageNumber)
        {
            return collection.Skip((pageNumber - 1 ) * pageSize).Take(pageSize);
        }
    }
}