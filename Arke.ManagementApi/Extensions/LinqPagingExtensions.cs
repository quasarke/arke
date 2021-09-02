using System.Collections.Generic;
using System.Linq;

namespace Arke.ManagementApi.Extensions
{
    public static class LinqPagingExtensions
    {
        public static IQueryable<TSource> Paginate<TSource>(this IQueryable<TSource> source, int page, int pageSize)
        {
            return source.Skip((page - 1) * pageSize).Take(pageSize);
        }

        public static IEnumerable<TSource> Paginate<TSource>(this IEnumerable<TSource> source, int page, int pageSize)
        {
            return source.Skip((page - 1) * pageSize).Take(pageSize);
        }
    }
}
