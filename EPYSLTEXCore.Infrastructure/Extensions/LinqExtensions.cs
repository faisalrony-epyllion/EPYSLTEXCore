using EPYSLTEXCore.Infrastructure.Entities;
using System.Collections.Generic;

namespace System.Linq
{
    public static class LinqExtensions
    {
        public static void SetUnchanged(this IEnumerable<BaseEntity> list)
        {
            foreach (var item in list)
            {
                item.EntityState = Data.Entity.EntityState.Unchanged;
            }
        }

        public static void SetModified(this IEnumerable<BaseEntity> list)
        {
            foreach (var item in list)
            {
                item.EntityState = Data.Entity.EntityState.Modified;
            }
        }

        public static void SetDeleted(this IEnumerable<BaseEntity> list)
        {
            foreach (var item in list)
            {
                item.EntityState = Data.Entity.EntityState.Deleted;
            }
        }

        public static void SetUnchanged(this IEnumerable<IDapperBaseEntity> list)
        {
            foreach (var item in list)
            {
                item.EntityState = Data.Entity.EntityState.Unchanged;
            }
        }

        public static void SetModified(this IEnumerable<IDapperBaseEntity> list)
        {
            foreach (var item in list)
            {
                item.EntityState = Data.Entity.EntityState.Modified;
            }
        }

        public static void SetDeleted(this IEnumerable<IDapperBaseEntity> list)
        {
            foreach (var item in list)
            {
                item.EntityState = Data.Entity.EntityState.Deleted;
            }
        }

        //public static void SetUnchanged(this IEnumerable<IDapperBaseModel> list)
        //{
        //    foreach (var item in list)
        //    {
        //        item.EntityState = Data.Entity.EntityState.Unchanged;
        //    }
        //}

        //public static void SetModified(this IEnumerable<IDapperBaseModel> list)
        //{
        //    foreach (var item in list)
        //    {
        //        item.EntityState = Data.Entity.EntityState.Modified;
        //    }
        //}

        //public static void SetDeleted(this IEnumerable<IDapperBaseModel> list)
        //{
        //    foreach (var item in list)
        //    {
        //        item.EntityState = Data.Entity.EntityState.Deleted;
        //    }
        //}
    }
}
