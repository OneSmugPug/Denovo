using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denovo
{
    public class User
    {
        private string Code, Name;
        private int AccessLevel;

        public User(string Code, int AccessLevel, string Name)
        {
            this.Code = Code;
            this.AccessLevel = AccessLevel;
            this.Name = Name;
        }

        public int GetAccessLevel() { return AccessLevel; }

        public string GetCode() { return Code; }

        public string GetName() { return Name; }
    }
}
