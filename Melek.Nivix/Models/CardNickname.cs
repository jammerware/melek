using System.Linq;
using System.Text;

namespace Nivix.Models
{
    public class CardNickname
    {
        public string Name { get; set; }
        public string[] Nicknames { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is CardNickname) {
                // cast
                CardNickname other = (obj as CardNickname);
                // check namez
                if (this.Name != other.Name) return false;
                
                // make a collection of each's nicknames that aren't blank for speed
                string[] thisNicknames = this.Nicknames.Where(n => !string.IsNullOrEmpty(n)).ToArray();
                string[] otherNicknames = other.Nicknames.Where(n => !string.IsNullOrEmpty(n)).ToArray();

                // check # of nicks
                if (thisNicknames.Length != otherNicknames.Length) return false;

                // check each nick
                foreach (string nickname in thisNicknames) {
                    if (!otherNicknames.Contains(nickname)) {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(this.Name);

            foreach (string nickname in this.Nicknames) {
                builder.Append(nickname);
            }

            return builder.ToString().GetHashCode();
        }
    }
}