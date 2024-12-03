using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Gmt.General
{
    public class MessageEvent : IBaseEntity
    {
        ///<summary>
        /// EventID (Primary key)
        ///</summary>
        public int Id { get; set; }

        ///<summary>
        /// EventName (length: 50)
        ///</summary>
        public string EventName { get; set; }

        [NotMapped]
        public EntityState EntityState { get; set; }

        /// <summary>
        /// Child MessageQueues where [MessageQueue].[EventID] point to this entity (FK_MessageQueue_MessageEvent)
        /// </summary>
        public virtual ICollection<MessageQueue> MessageQueues { get; set; }

        public MessageEvent()
        {
            EntityState = EntityState.Added;
            MessageQueues = new List<MessageQueue>();
        }
    }
}
