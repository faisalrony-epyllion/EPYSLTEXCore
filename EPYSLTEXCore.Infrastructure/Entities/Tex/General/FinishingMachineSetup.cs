using Dapper.Contrib.Extensions;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace EPYSLTEX.Core.Entities.Tex
{
	[Table("FinishingMachineSetup")]
	public class FinishingMachineSetup : IDapperBaseEntity
	{
		
		[ExplicitKey]
		public int FMSID { get; set; }
		public int FMCMasterID { get; set; }

		public int BrandID { get; set; }

		public int UnitID { get; set; }

		public int Capacity { get; set; }
		public string MachineNo { get; set; }

		public string Param1Value { get; set; }

		public string Param2Value { get; set; }

		public string Param3Value { get; set; }

		public string Param4Value { get; set; }

		public string Param5Value { get; set; }

		public string Param6Value { get; set; }

		public string Param7Value { get; set; }

		public string Param8Value { get; set; }

		public string Param9Value { get; set; }

		public string Param10Value { get; set; }

		public string Param11Value { get; set; }

		public string Param12Value { get; set; }

		public string Param13Value { get; set; }

		public string Param14Value { get; set; }

		public string Param15Value { get; set; }

		public string Param16Value { get; set; }

		public string Param17Value { get; set; }

		public string Param18Value { get; set; }

		public string Param19Value { get; set; }

		public string Param20Value { get; set; }

		public int AddedBy { get; set; }
		public DateTime DateAdded { get; set; }
		public int? UpdatedBy { get; set; }
		public DateTime? DateUpdated { get; set; }

		//public virtual FinishingMachineConfigurationMaster FinishingMachineConfigurationMaster { get; set; }
		#region Additional Fields

		[Write(false)]
		public string BrandName { get; set; }

		[Write(false)]
		public string ProcessName { get; set; }
		[Write(false)]
		public EntityState EntityState { get; set; }

		[Write(false)]
		public int TotalRows { get; set; }

		[Write(false)]
		public bool IsModified => EntityState == EntityState.Modified;

		[Write(false)]
		public bool IsNew => EntityState == EntityState.Added;
		//[Write(false)]
		//public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.FMSID > 0;

		[Write(false)]
		public int ProcessTypeID { get; set; }
		[Write(false)]
		public int ProcessType { get; set; }
		[Write(false)]
		public string UnitName { get; set; }
		[Write(false)]
		public List<FinishingMachineConfigurationChild> FinishingMachineConfigurationChildList { get; set; }

		#endregion Additional Fields
		public FinishingMachineSetup()
		{
			EntityState = EntityState.Added;
			DateAdded = DateTime.Now;
		}
	}
	#region
	public class FinishingMachineSetupValidator : AbstractValidator<FinishingMachineSetup>
	{
		public FinishingMachineSetupValidator()
		{
			RuleFor(x => x.FMCMasterID).NotEmpty();
			RuleFor(x => x.BrandID).NotEmpty();
			RuleFor(x => x.UnitID).NotEmpty();
			RuleFor(x => x.Capacity).NotEmpty();
			RuleFor(x => x.MachineNo).NotEmpty();
			RuleFor(x => x.Param1Value).NotEmpty().WithMessage("Please enter atleast one parameter value!");
		}
	}
	#endregion
}
