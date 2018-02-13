using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace web.Models
{
    public enum EGlobalMessageType : byte
    {
        Info,
        Warning,
        Success,
        Error
    }

    public class GlobalMessage
    {
        public string Message { get; set; }

        public EGlobalMessageType Type { get; set; }

        public string GetCssClass()
        {
            switch(this.Type)
            {
                case EGlobalMessageType.Info:
                    return "alert-info";
                case EGlobalMessageType.Warning:
                    return "alert-warning";
                case EGlobalMessageType.Error:
                    return "alert-error";
                case EGlobalMessageType.Success:
                    return "alert-success";
            }

            return string.Empty;
        }
    }
}
