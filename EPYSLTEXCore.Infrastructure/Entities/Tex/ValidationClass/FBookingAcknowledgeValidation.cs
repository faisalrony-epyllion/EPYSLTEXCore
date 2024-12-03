using EPYSLTEXCore.Infrastructure.Entities.Tex.Booking;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.ValidationClass
{
    public static class FBookingAcknowledgeValidation
    {
        public static string IsValidFBookingAcknowledge(FBookingAcknowledge fba)
        {
            if (fba.AddedBy == 0 || fba.AddedBy == null)
            {
                return "Added By missing FBookingAcknowledge";
            }
            if (fba.EntityState == EntityState.Modified && (fba.UpdatedBy == 0 || fba.UpdatedBy == null))
            {
                return "Updated By missing FBookingAcknowledge";
            }
            return "";
        }
    }
}
