﻿namespace NCS.DSS.Transfer.Models
{
    public class Subscription
    {
        public Guid id { get; set; }
        public Guid CustomerId { get; set; }
        public string TouchPointId { get; set; }
        public bool Subscribe { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string LastModifiedBy { get; set; }
    }
}