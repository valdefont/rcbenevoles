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
            // Always include the base "alert" class
            const string baseClass = "alert ";

            switch (this.Type)
            {
                case EGlobalMessageType.Info:
                    return baseClass + "alert-info";
                case EGlobalMessageType.Warning:
                    return baseClass + "alert-warning";
                case EGlobalMessageType.Error:
                    return baseClass + "alert-danger"; // <-- was alert-error
                case EGlobalMessageType.Success:
                    return baseClass + "alert-success";
                default:
                    return baseClass + "alert-secondary"; // for Bootstrap 4/5 fallback
            }
        }
    }
}
