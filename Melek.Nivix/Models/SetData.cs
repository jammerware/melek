using System.Text;

namespace Nivix.Models
{
    public class SetData
    {
        public string CfName { get; set; }
        public string Code { get; set; }
        public string GathererCode { get; set; }
        public string MtgImageCode { get; set; }
        public string Name { get; set; }
        public string TcgPlayerName { get; set; }

        public SetData()
        {
            CfName = string.Empty;
            Code = string.Empty;
            GathererCode = string.Empty;
            MtgImageCode = string.Empty;
            Name = string.Empty;
            TcgPlayerName = string.Empty;
        }

        public override bool Equals(object obj)
        {            
            if (obj is SetData) {
                SetData other = obj as SetData;
                if (
                    CfName != other.CfName || 
                    Code != other.Code ||
                    GathererCode != other.GathererCode ||
                    MtgImageCode != other.MtgImageCode ||
                    Name != other.Name ||
                    TcgPlayerName != other.TcgPlayerName
                ) return false;

                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(CfName);
            builder.Append(Code);
            builder.Append(GathererCode);
            builder.Append(MtgImageCode);
            builder.Append(Name);
            builder.Append(TcgPlayerName);

            return builder.ToString().GetHashCode();
        }
    }
}