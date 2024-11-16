using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dapper.Contrib.Extensions;

namespace EPYSLTEXCore.Infrastructure.Static
{
    public static class EntityReflectionHelper
    {
        // Method to get the key column name
        public static string GetKeyColumnName<T>()
        {
            var keyProperty = typeof(T).GetProperties()
                .FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(ExplicitKeyAttribute))
                                      || Attribute.IsDefined(prop, typeof(KeyAttribute)));
            return keyProperty?.Name;


            ////// Get the key column name
            ////string keyColumnName = EntityReflectionHelper.GetKeyColumnName<ReportAPISetup>();
            ////Console.WriteLine($"Key Column Name: {keyColumnName}");

        }

        public static string GetKeyPropertyName(Type type)
        {
            var keyProperty = type.GetProperties()
                .FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(ExplicitKeyAttribute))
                                      || Attribute.IsDefined(prop, typeof(KeyAttribute)));
            return keyProperty?.Name;
        }
        public static List<string> GetMultipleKeyColumnName<T>()
        {
            var keyColumnNames = typeof(T).GetProperties()
                .Where(prop => Attribute.IsDefined(prop, typeof(ExplicitKeyAttribute))
                                      || Attribute.IsDefined(prop, typeof(KeyAttribute)))
                .Select(prop => prop.Name)
                .ToList();

            return keyColumnNames;
        }

        // Method to get the table name
        public static string GetTableName(Type type)
        {
            return type.GetCustomAttributes(typeof(Dapper.Contrib.Extensions.TableAttribute), true)
                                          .FirstOrDefault() as Dapper.Contrib.Extensions.TableAttribute is Dapper.Contrib.Extensions.TableAttribute tableAttribute ? tableAttribute.Name : type.Name;
        }

        // Method to get the table name
        public static string GetTableName<T>()
        {
            var tableAttribute = typeof(T).GetCustomAttributes(typeof(Dapper.Contrib.Extensions.TableAttribute), true)
                                          .FirstOrDefault() as Dapper.Contrib.Extensions.TableAttribute;

            return tableAttribute?.Name;

            ////// Get the table name
            ////string tableName = EntityReflectionHelper.GetTableName<ReportAPISetup>();
            ////Console.WriteLine($"Table Name: {tableName}");

        }

        // Method to get all column names excluding those with [Write(false)]
        public static List<string> GetColumnNames<T>()
        {
            var columnNames = typeof(T).GetProperties()
                .Where(prop => !Attribute.IsDefined(prop, typeof(WriteAttribute)) ||
                               !((WriteAttribute)Attribute.GetCustomAttribute(prop, typeof(WriteAttribute))).Write)
                .Select(prop => prop.Name)
                .ToList();

            return columnNames;

            ////// Get all column names excluding those marked with [Write(false)]
            ////List<string> columnNames = EntityReflectionHelper.GetColumnNames<ReportAPISetup>();

        }

        // Method to check if a property is a foreign key
        public static bool IsForeignKey(PropertyInfo property)
        {
            // Check if the property has a ForeignKeyAttribute
            if (property.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute), true).Any())
            {
                return true;
            }

            // Alternatively, check if the property name follows a convention (e.g., ends with "Id")
            return property.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase);
        }
    }
}
