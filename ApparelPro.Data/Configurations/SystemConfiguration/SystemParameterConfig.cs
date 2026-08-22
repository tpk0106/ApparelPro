using ApparelPro.Data.Models.SystemConfiguration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.SystemConfiguration
{
    public class SystemParameterConfig : IEntityTypeConfiguration<SystemParameter>
    {
        public void Configure(EntityTypeBuilder<SystemParameter> entity)
        {
            entity.ToTable("SystemParameters");
            entity.HasKey(e => e.ParameterKey);
            entity.Property(e => e.ParameterKey).HasColumnType("varchar(50)").IsRequired();
            entity.Property(e => e.Value).HasColumnType("varchar(200)").IsRequired();
            entity.Property(e => e.Description).HasColumnType("varchar(1000)");
            entity.Property(e => e.Category).HasColumnType("varchar(50)").IsRequired().HasDefaultValue("General");
            entity.Property(e => e.DataType).HasColumnType("varchar(20)").IsRequired().HasDefaultValue("Text");
            entity.Property(e => e.Options).HasColumnType("varchar(1000)");

            // Seed the one parameter this feature needs. Default OFF (strict enforcement):
            // when an apparel buyer confirms an order quantity, that figure is the contractual
            // source of truth, so style-level entries must reconcile to it unless an admin
            // explicitly opts an order into being overridden.
            entity.HasData(new SystemParameter
            {
                ParameterKey = "AllowOrderQuantityOverride",
                Value = "false",
                Description = "When false (default), the sum of a purchase order's style quantities " +
                    "(converted into the order's own unit) cannot exceed the order's Total Quantity - " +
                    "Add/Update Style Details is rejected if it would. When true, the save is allowed " +
                    "even if it exceeds Total Quantity (the Styles grid still visually flags the overage).",
                Category = "Order Management",
                DataType = "Boolean"
            });

            // Production Control (PR_OPD2.PRG's FACTPARA.DBF - factory-wide line-balancing
            // defaults). Seeded 1:1 from the legacy singleton row's actual values. EFF1 isn't
            // read by the Style Operation Breakdown save routine (only EFF2/WorkHours/NoMcs
            // are), but is seeded now since it's part of the same legacy record and later
            // phases (Reports > Employee Efficiency) are expected to need it.
            entity.HasData(
                new SystemParameter
                {
                    ParameterKey = "ProductionWorkHoursPerDay",
                    Value = "8",
                    Description = "Standard shift length in hours, used to compute each operation's " +
                        "Quota (pieces/day at 100% = (WorkHoursPerDay*60)/SAM) and the line's target " +
                        "daily output in Style Operation Breakdown.",
                    Category = "Production Control",
                    DataType = "Number"
                },
                new SystemParameter
                {
                    ParameterKey = "ProductionEfficiency1Percent",
                    Value = "80",
                    Description = "Legacy FACTPARA.EFF1 - factory efficiency percentage. Not currently " +
                        "consumed by any migrated screen; carried over for the Reports phase.",
                    Category = "Production Control",
                    DataType = "Number"
                },
                new SystemParameter
                {
                    ParameterKey = "ProductionEfficiency2Percent",
                    Value = "65",
                    Description = "Legacy FACTPARA.EFF2 - efficiency percentage applied to raw machine " +
                        "throughput in the Style Operation Breakdown line-balancing calculation.",
                    Category = "Production Control",
                    DataType = "Number"
                },
                new SystemParameter
                {
                    ParameterKey = "ProductionDefaultMachineCountPerLine",
                    Value = "50",
                    Description = "Legacy FACTPARA.NO_MCS - default assumed machine count used to size " +
                        "a style's target daily output before it's actually assigned to a specific " +
                        "Production Line (see ProductionLines.NumberOfMachines for the per-line figure " +
                        "used once a real line assignment exists).",
                    Category = "Production Control",
                    DataType = "Number"
                },
                new SystemParameter
                {
                    ParameterKey = "ProductionContractSectionCode",
                    Value = "001",
                    Description = "Legacy FACTPARA.CONTR_SECT - the Section (see Sections.Code) treated " +
                        "as the contractual production ceiling. No other section's running to-date " +
                        "quantity may exceed this section's running to-date quantity for the same " +
                        "style/line when saving Actual Production Entry. Defaults to 001 (Cutting).",
                    Category = "Production Control",
                    DataType = "Text"
                }
            );

            // Home dashboard - fallback style shown when the floor hasn't
            // logged anything yet (no DailyProductionEntries or
            // DailyProductionTimeTicketEntries rows exist at all). Blank by
            // default; an Administrator pins a Buyer/Order/Type/Style once
            // there's a style they want shown before the first entry of a
            // new day/style comes in. Four separate keys rather than one
            // delimited value, so each stays readable/editable on its own.
            entity.HasData(
                new SystemParameter
                {
                    ParameterKey = "DashboardPinnedBuyerCode",
                    Value = "",
                    Description = "Fallback dashboard style - Buyer code. Only used when no Actual " +
                        "Production Entry or Daily Production Time Ticket rows exist yet.",
                    Category = "Dashboard",
                    DataType = "Text"
                },
                new SystemParameter
                {
                    ParameterKey = "DashboardPinnedOrder",
                    Value = "",
                    Description = "Fallback dashboard style - Order number.",
                    Category = "Dashboard",
                    DataType = "Text"
                },
                new SystemParameter
                {
                    ParameterKey = "DashboardPinnedTypeCode",
                    Value = "",
                    Description = "Fallback dashboard style - Garment type code.",
                    Category = "Dashboard",
                    DataType = "Text"
                },
                new SystemParameter
                {
                    ParameterKey = "DashboardPinnedStyleCode",
                    Value = "",
                    Description = "Fallback dashboard style - Style code.",
                    Category = "Dashboard",
                    DataType = "Text"
                }
            );
        }
    }
}
