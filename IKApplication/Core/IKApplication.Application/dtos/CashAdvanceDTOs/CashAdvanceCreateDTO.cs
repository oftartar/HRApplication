﻿using IKApplication.Application.VMs.CashAdvanceVMs;
using IKApplication.Domain.Entites;
using IKApplication.Domain.Enums;

namespace IKApplication.Application.dtos.CashAdvanceDTOs
{
    public class CashAdvanceCreateDTO
    {
        public string Description { get; set; }
        public decimal RequestedAmount { get; set; }
        public Guid? DirectorId { get; set; }
        public Guid? AdvanceToId { get; set; }
        public Guid? CompanyId { get; set; }
        public AppUser? AdvanceTo { get; set; }
        public PaymentStatus? IsPaymentProcessed { get; set; }
        public DateTime? FinalDateRequest { get; set; }
        public DateTime CreateDate => DateTime.Now;
        public Status Status => Status.Passive;
    }
}