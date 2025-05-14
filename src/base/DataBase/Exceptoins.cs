using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase
{
    internal class DBException : Exception
    {
        public DBException(string msg) : base(msg) { }
        {
            
        }
    }
}
