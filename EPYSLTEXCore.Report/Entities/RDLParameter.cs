using System;
using System.Data;

namespace EPYSLTEXCore.Report.Entities
{
    public class RDLParameter
    {
        private String name = String.Empty;
        public String Name
        {
            get { return name; }
            set { name = value; }
        }
        private DbType dbType = DbType.String;
        public DbType DbType
        {
            get { return dbType; }
            set { dbType = value; }
        }

        private String prompt = String.Empty;
        public String Prompt
        {
            get { return prompt; }
            set { prompt = value; }
        }

        private Object value = null;
        public Object Value
        {
            get { return value; }
            set { this.value = value; }
        }

        private Object defaultValue = null;
        public Object DefaultValue
        {
            get { return defaultValue; }
            set { this.defaultValue = value; }
        }

        public override String ToString()
        {
            return name;            
        }     
       
    }
}
