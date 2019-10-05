using System.Text;

namespace DependencyInjectionWorkshop.Models
{
    public class Sha256Adapter : IHashAdapter
    {
        public string ComputeHash(string input)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(input));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            return hash.ToString();
        }
    }
}