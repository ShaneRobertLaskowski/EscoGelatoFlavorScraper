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
        private string? firstName;
        private string? lastName;
        private string? phno;
        private List<string>? favoriteFlavors;

        public string? Firstname { get => firstName; set => firstName = value; }
        public string? Lastname { get => lastName; set => lastName = value; }
        public string? Phno { get => phno; set => phno = value; }
        public List<string>? FavoriteFlavors { get => favoriteFlavors; set => favoriteFlavors = value; }

        public Customer()
        {
            Firstname = null;
            Lastname = null;
            Phno = null;
            FavoriteFlavors = new List<string>();
        }
        public Customer(string fName, string lName, string phno, List<string> favoriteFlavors)
        {
            Firstname = fName;
            Lastname = lName;
            Phno = phno;
            FavoriteFlavors = favoriteFlavors;
        }
        public Customer(string fName, string lName, string phno)
        {
            Firstname = fName;
            Lastname = lName;
            Phno = phno;
            FavoriteFlavors = null;
        }
        public Customer(string fName, string phno)
        {
            Firstname = fName;
            Phno = phno;
        }
        public static bool operator ==(Customer custX, Customer custY)
        {
            if (custX.Firstname == custY.Firstname && custX.phno == custY.phno)
                return true;
            else return false;
        }
        public static bool operator !=(Customer custX, Customer custY)
        {
            if (custX.Firstname != custY.Firstname || custX.phno != custY.phno)
                return true;
            else return false;
        }

        public override bool Equals(object? obj)
        {
            //consider adding lastname as part of equality check
            if (this.firstName == ((Customer)obj).firstName && this.phno == ((Customer)obj).phno)
                return true;
            else return false;
        }
        public override int GetHashCode()
        {
            //consider adding lastname as part of equality check
            return (this.firstName + this.phno).GetHashCode();
        }
    }
}
