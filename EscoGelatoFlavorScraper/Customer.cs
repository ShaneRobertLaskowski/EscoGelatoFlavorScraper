using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscoGelatoFlavorScraper
{
    /// <summary>Instances of this class represent customers.</summary>
    /// <todo></todo>
    internal class Customer
    {
        private string firstName;
        private string lastName;
        private string phno;
        private List<string> favoriteFlavors;

        public string Firstname { get => firstName; set => firstName = value; }
        public string Lastname { get => lastName; set => lastName = value; }
        public string Phno { get => phno; set => phno = value; }
        public List<string> FavoriteFlavors { get => favoriteFlavors; set => favoriteFlavors = value; }

        public Customer(string fName, string lName, string phno, List<string> favoriteFlavors)
        {
            Firstname = fName;
            Lastname = lName;
            Phno = phno;
            FavoriteFlavors = favoriteFlavors;
        }

    }
}
