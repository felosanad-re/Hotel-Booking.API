using Hotel_booking.Core.Models;
using System.Linq.Expressions;

namespace Hotel_booking.Core.Specifications
{
    public interface ISpecifications<T> where T : ModelBase
    {
        Expression<Func<T, bool>>? Criteria { get; }
        IReadOnlyList<Expression<Func<T, object>>> Includes { get; }
        IReadOnlyList<string> IncludesString { get; }
        Expression<Func<T, object>>? OrderBy { get; }
        Expression<Func<T, object>>? OrderByDesc { get; }
        IReadOnlyList<Expression<Func<T, object>>> ThenByExpressions { get; }
        IReadOnlyList<Expression<Func<T, object>>> ThenByDescendingExpressions { get; }
        bool IsTracking { get; }
        bool IsSplitQuery { get; }
        int Skip { get; }
        int Take { get; }
        bool IsPagination { get; }
    }
}
