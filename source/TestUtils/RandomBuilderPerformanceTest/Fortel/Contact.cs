using System.Collections.Generic;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class Contact : EntityBase
    {
        public string Title { get; set; }
        public string FirstNames { get; set; }
        public string Initials { get; set; }
        public string Surname { get; set; }
        public string IdNumber { get; set; }
        public string Email { get; set; }
       
        public Designation Designation { get; set; }
        public List<ContactNumber> ContactNumbers { get; set; }
        public List<Address> Addresses { get; set; }
        public List<Team> Teams { get; set; }
        public string PersonnelCode { get; set; }
        public Attachment IdCopy { get; set; }
        public Attachment Signature { get; set; }
        public Attachment BankDetailsProof { get; set; }
        public Attachment AttachmentFile { get; set; }
        public Wage Wage { get; set; }
        public List<BankingDetails> BankingDetails { get; set; }

        public Status Status { get; set; }
    }
}