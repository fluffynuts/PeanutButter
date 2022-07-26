using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global

namespace PeanutButter.TestUtils.Entity.Tests
{
    public interface ICommunicatorContext
    {
        IDbSet<COMBlockListReason> BlockListReasons { get; set; }
        IDbSet<COMBlockList> BlockLists { get; set; }
    }

    public class CommunicatorContext: DbContext, ICommunicatorContext
    {
        public IDbSet<COMBlockListReason> BlockListReasons { get; set; }
        public IDbSet<COMBlockList> BlockLists { get; set; }

        public CommunicatorContext()
        {
        }

        public CommunicatorContext(DbConnection connection) : base(connection, false)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
			base.OnModelCreating(modelBuilder);
		}
    }

    public class COMBlockList
    {
        public int COMBlockListID { get; set; }
        public int COMBlockListReasonID { get; set; }
        public int COREClientID { get; set; }
        public string Source { get; set; }
        public DateTime Date { get; set; }
        public string ReasonDescription { get; set; }
        public bool IsActive { get; set; }

        public virtual COMBlockListReason COMBlockListReason { get; set; }
    }

    public class COMBlockListReason
    {
        public int COMBlockListReasonID { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
    }
}
