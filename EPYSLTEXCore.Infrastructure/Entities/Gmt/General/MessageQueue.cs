using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Gmt.General
{
    public class MessageQueue : IBaseEntity
    {
        ///<summary>
        /// MessageID (Primary key)
        ///</summary>
        public int Id { get; set; }

        public int MenuId { get; set; }

        public int ForMenuId { get; set; }

        public int EventId { get; set; }

        public int? ParameterColumn0 { get; set; }

        public int? ParameterColumn1 { get; set; }

        public int ParentColumnId { get; set; }

        ///<summary>
        /// ReferenceValue (length: 200)
        ///</summary>
        public string ReferenceValue { get; set; }

        public int SenderId { get; set; }

        public DateTime SendDate { get; set; }

        public bool Received { get; set; }

        public int? ReceiverId { get; set; }

        public DateTime? ReceivedDate { get; set; }

        public bool AutoReceived { get; set; }

        ///<summary>
        /// Subject (length: 200)
        ///</summary>
        public string Subject { get; set; }

        ///<summary>
        /// Message (length: 500)
        ///</summary>
        public string Message { get; set; }

        ///<summary>
        /// SenderIPAddress (length: 15)
        ///</summary>
        public string SenderIpAddress { get; set; }

        ///<summary>
        /// ReceiverIPAddress (length: 15)
        ///</summary>
        public string ReceiverIpAddress { get; set; }

        ///<summary>
        /// RefNo (length: 20)
        ///</summary>
        public string RefNo { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public int Readable { get; set; }

        public int NotifyFor { get; set; }

        public int? CompanyId { get; set; }

        ///<summary>
        /// EntryFrom (length: 100)
        ///</summary>
        public string EntryFrom { get; set; }

        [NotMapped]
        public EntityState EntityState { get; set; }

        /// <summary>
        /// Parent Menu pointed by [MessageQueue].([ForMenuId]) (FK_MessageQueue_Menu1)
        /// </summary>
        public virtual Menu ForMenu { get; set; }

        /// <summary>
        /// Parent MessageEvent pointed by [MessageQueue].([EventId]) (FK_MessageQueue_MessageEvent)
        /// </summary>
        public virtual MessageEvent MessageEvent { get; set; }

        public MessageQueue()
        {
            Received = false;
            AutoReceived = false;
            RefNo = "";
            Readable = 0;
            NotifyFor = 0;
            EntityState = EntityState.Added;
        }
    }
}
