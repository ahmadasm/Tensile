using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniTensile.Models
{
    public interface IParser<TDataType>
    {
         TDataType Pars(string data);
    }
    public  class ParserApsTensileV1 : IParser<IControlData>
    {
        const string CodeLoad = "!1\t";
        const string CodeExtension = "!2\t";

        public  IControlData Pars(string data)
        {
            //if (data is null) return null;
            //if (string.IsNullOrEmpty(data)) return null;
            try
            {
                //if (data.Contains(CodeLoad))
                if(data.IndexOf(CodeLoad) > -1)
                {
                    //string str = data.Split('\t')[1];
                    string str = data.Split(new char[] { '\t' })[1];
                    double val;
                    double.TryParse(str,out val);
                    return new Load(val);
                }
                //else if (data.Contains(CodeExtension))
                else if(data.IndexOf(CodeExtension) > -1)
                {
                    //string str = data.Split('\t')[1];
                    string str = data.Split(new char[] { '\t' })[1];
                    //UInt32 val = UInt32.Parse(str);
                    UInt32 val;
                    UInt32.TryParse(str, out val);
                    return new Extension(val);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
    public interface IControlData
    {
        object Value { get; }
        object Kind { get; }

    }
    public enum ApsTensileDataKind
    {
        Load,
        Extension,
    }
    public abstract class ApsTensileData : IControlData
    {
        protected ApsTensileDataKind kind;

        public abstract object Value { get; }

        public object Kind => kind;
    }
    public class Load : ApsTensileData
    {
        private readonly double mv;
        public Load(double val)
        {
            mv = val;
            kind = ApsTensileDataKind.Load;
        }

        public override object Value => mv;

    }

    public class Extension : ApsTensileData
    {
        private readonly UInt32 extension;
        public Extension(UInt32 val)
        {
            extension = val;
            kind = ApsTensileDataKind.Extension;
        }

        public override object Value => extension;
    }
}
