using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nivix.Models.Comparers
{
    public class CardNicknameComparer : IEqualityComparer<CardNickname>
    {
        public bool Equals(CardNickname a, CardNickname b)
        {
            if (a.Name != b.Name) return false;
            if (a.Nicknames.Length != b.Nicknames.Length) return false;
            foreach (string nickname in a.Nicknames) {
                if (!b.Nicknames.Contains(nickname)) {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(CardNickname nicknameObj)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(nicknameObj.Name);

            foreach (string nickname in nicknameObj.Nicknames) {
                builder.Append(nickname);
            }

            return builder.ToString().GetHashCode();
        }
    }
}