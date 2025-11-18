using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProcurementSystem.Models.Enums
{
    public enum PaymentStatus
    {
        ОЧІКУЄТЬСЯ,
        ОПЛАЧЕНО,
        ЧАСТКОВО_ОПЛАЧЕНО,
        ПРОТЕРМІНОВАНО,
        АРХІВОВАНО
    }
}