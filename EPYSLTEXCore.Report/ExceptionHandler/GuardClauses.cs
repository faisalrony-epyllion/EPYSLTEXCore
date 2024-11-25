using EPYSLTEXCore.Report.Entities;

namespace EPYSLTEXCore.Report.ExceptionHandler
{
    /// <summary>
    /// Simple interface to provide a generic mechanism to build guard clause extension methods from.
    /// </summary>
    public interface IGuardClause
    {
    }

    /// <summary>
    /// An entry point to a set of Guard Clauses defined as extension methods on IGuardClause.
    /// </summary>
    /// <remarks>See http://www.weeklydevtips.com/004 on Guard Clauses</remarks>
    public class Guard : IGuardClause
    {
        /// <summary>
        /// An entry point to a set of Guard Clauses.
        /// </summary>
        public static IGuardClause Against { get; } = new Guard();

        private Guard() { }
    }

    public static class GuardExtensions
    {
        public static void NullEntity(this IGuardClause guardClause, int id, IBaseEntity entity)
        {
            if (entity == null) throw new ItemNotFoundException(id);
        }

        public static void NullObject(this IGuardClause guardClause, int id, object obj)
        {
            if (obj == null) throw new ItemNotFoundException(id);
        }

        public static void NullObject(this IGuardClause guardClause, object obj, string message = "Item not found.")
        {
            if (obj == null) throw new ItemNotFoundException(message);
        }
    }
}