﻿using System.ComponentModel.DataAnnotations.Schema;

namespace ApparelPro.Data.Models.References
{
    public class Address
    {
        public int Id { get; set; }
        public Guid AddressId { get; set; }        
        public AddressType? AddressType { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; } 
        public int? PostCode { get; set; }
        public string? State { get; set; }
        public string? CountryCode { get; set; }
        [NotMapped]
        public string? Country { get; set; }
        public bool? Default { get; set; }             
        public int? BuyerCode { get; set; }        
    }

    public enum AddressType
    {
        Residential = 1,
        Postal = 2,
        Corporate = 3,
        Billing = 4,
        Delivery = 5,        
    }
}
