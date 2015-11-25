using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        //[Source] [nvarchar](250) NOT NULL,
            //[SourceBy] [int] NULL,
            //[Date] [datetime] NOT NULL,
            //[COMBulkID] [int] NULL,
            //[CORECustomerID] [int] NULL,
            //[COREMemberID] [int] NULL,
            //[Email] [nvarchar](200) NULL,
            //[Mobile] [nvarchar](20) NULL,
            //[DeviceKeyID] [nvarchar](100) NULL,
            //[ReasonDescription] [nvarchar](2000) NOT NULL,
            //[IsActive] [bit] NOT NULL,
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
