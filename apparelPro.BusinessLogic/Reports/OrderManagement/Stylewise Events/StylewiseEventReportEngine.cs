using apparelPro.BusinessLogic.Services.Models.OrderManagement.IStylewiseEvents;
using System.Reflection.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

//using Document = QuestPDF.Fluent.Document;

namespace ApparelPro.WebApi.Reports.OrderManagement.Stylewise_Events
{
    public class StylewiseEventReportEngine
    {
        public static byte[] GenerateCriticalPathPdf(List<StylewiseEventServiceModel> dataset, string managerApprovalText)
        {
            // Initialise the QuestPDF Community License runner context safely
            QuestPDF.Settings.License = LicenseType.Community;

            var firstRow = dataset.FirstOrDefault();
            string buyer = firstRow?.BuyerCode.ToString() ?? "N/A";
            string order = firstRow?.Order ?? "N/A";
            string type = firstRow?.TypeCode.ToString() ?? "N/A";
            string style = firstRow?.StyleCode ?? "N/A";

            using (var memoryStream = new MemoryStream())
            {
                QuestPDF.Fluent.Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(10));

                        // 1. REPORT CORNER HEADER PANEL
                        page.Header().Column(column =>
                        {
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text("[ STYLE-WISE EVENTS PRODUCTION STATUS REPORT ]")
                                    .FontSize(14).Bold().FontColor(Colors.Blue.Darken4); // Fixed: FontColor
                                row.ConstantItem(100).Text($"Date: {DateTime.Now:dd-MMM-yyyy}")
                                    .FontSize(9).AlignRight().Style(TextStyle.Default.Italic());
                            });

                            column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1); // Fixed: LineColor

                            // Scope Metadata coordinates row block layout
                            column.Item().PaddingTop(8).Row(row =>
                            {
                                row.RelativeItem().Text(t => { t.Span("Buyer Code: ").Bold(); t.Span(buyer); });
                                row.RelativeItem().Text(t => { t.Span("Order Reference: ").Bold(); t.Span(order); });
                                row.RelativeItem().Text(t => { t.Span("Garment Type: ").Bold(); t.Span(type); });
                                row.RelativeItem().Text(t => { t.Span("Active Style: ").Bold(); t.Span(style); });
                            });

                            column.Item().PaddingTop(8).PaddingBottom(4).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken4); // Fixed: LineColor
                        });

                        // 2. PRIMARY SPREADSHEET MATRIX LEDGER DATA PANEL
                        page.Content().PaddingTop(10).Table(table =>
                        {
                            // Define 6 responsive vector structural column grid size widths matching text limitations
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(70);  // Event Code ID
                                columns.RelativeColumn(2.5f); // Description string text line block
                                columns.ConstantColumn(85);  // Scheduled date format
                                columns.ConstantColumn(85);  // Actual received date format
                                columns.RelativeColumn(2);   // Operations comments
                                columns.ConstantColumn(120); // Milestone dynamic status text
                            });

                            // Generate corporate spreadsheet metadata header rows
                            table.Header(header =>
                            {
                                header.Cell().Text("Event Code").Bold();
                                header.Cell().Text("Milestone Description").Bold();
                                header.Cell().Text("Scheduled Date").Bold();
                                header.Cell().Text("Actual Floor Date").Bold();
                                header.Cell().Text("Operational Remarks / Notes").Bold();
                                header.Cell().Text("Compliance Status").Bold();

                                header.Cell().ColumnSpan(6).PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Darken2); // Fixed: LineColor
                            });

                            // Inject live dataset records line-item rows dynamically loop
                            foreach (var item in dataset)
                            {
                                table.Cell().PaddingVertical(4).Text(item.EventCode).FontFamily(Fonts.CourierNew).Bold();
                                table.Cell().PaddingVertical(4).Text(item.Description);
                                table.Cell().PaddingVertical(4).Text(item.ScheduledDate?.ToString("dd-MMM-yyyy") ?? "***");
                                table.Cell().PaddingVertical(4).Text(item.ActualDate?.ToString("dd-MMM-yyyy") ?? "***");
                                table.Cell().PaddingVertical(4).Text(item.Remarks ?? "-").Style(TextStyle.Default.FontSize(9));

                                // Colour match milestone text styles cleanly according to compliance tracking states
                                // Fixed: Replaced dynamic text builder assignment with functional evaluation blocks
                                if (item.MilestoneStatus == "No Scheduled Date")
                                {
                                    table.Cell().PaddingVertical(4).Text(item.MilestoneStatus).Bold().FontColor(Colors.Red.Medium);
                                }
                                else if (item.MilestoneStatus == "Actual Date Pending")
                                {
                                    table.Cell().PaddingVertical(4).Text(item.MilestoneStatus).Bold().FontColor(Colors.Orange.Darken3);
                                }
                                else
                                {
                                    table.Cell().PaddingVertical(4).Text(item.MilestoneStatus).Bold().FontColor(Colors.Green.Medium);
                                }
                            }
                        });

                        // 3. REPORT TAIL FOOTER PANEL (Digital Signatures & Approvals)
                        page.Footer().Column(column =>
                        {
                            column.Item().PaddingTop(15).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken4); // Fixed: LineColor

                            column.Item().PaddingTop(8).Row(row =>
                            {
                                // If executive signature details are present, render corporate stamp context layout
                                if (!string.IsNullOrEmpty(managerApprovalText))
                                {
                                    row.RelativeItem().Text(managerApprovalText)
                                        .FontSize(9).Bold().FontColor(Colors.Green.Darken4); // Fixed: FontColor
                                }
                                else
                                {
                                    row.RelativeItem().Text("⚠️ VERIFICATION WARNING: This tracking sheet has not been officially approved by management yet.")
                                        .FontSize(9).Style(TextStyle.Default.Italic()).FontColor(Colors.Grey.Darken1); // Fixed: FontColor
                                }

                                row.RelativeItem().Text(x =>
                                {
                                    x.Span("Page ");
                                    x.CurrentPageNumber();
                                    x.Span(" of ");
                         ;
                                    //x.TotalPages();
                                });
                            //}).AlignRight().FontSize(9);
                        });

                            column.Item().PaddingTop(4).Text("[ End of Report - System Generated Ledger Data Document ]")
                                .FontSize(8).AlignCenter().FontColor(Colors.Grey.Lighten1); // Fixed: FontColor
                        });
                    });
                }).GeneratePdf(memoryStream);

                return memoryStream.ToArray();
            }


            //    // Initialise the QuestPDF Community License runner context safely
            //    QuestPDF.Settings.License = LicenseType.Community;

            //    var firstRow = dataset.FirstOrDefault();
            //    string buyer = firstRow?.BuyerCode.ToString() ?? "N/A";
            //    string order = firstRow?.Order ?? "N/A";
            //    string type = firstRow?.TypeCode.ToString() ?? "N/A";
            //    string style = firstRow?.StyleCode ?? "N/A";

            //    using (var memoryStream = new MemoryStream())
            //    {
            //        QuestPDF.Fluent.Document.Create(container =>
            //        {
            //            container.Page(page =>
            //            {
            //                page.Size(PageSizes.A4.Landscape());
            //                page.Margin(2, Unit.Centimetre);
            //                page.PageColor(Colors.White);
            //                page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(10));

            //                // 1. REPORT CORNER HEADER PANEL
            //                page.Header().Column(column =>
            //                {
            //                    column.Item().Row(row =>
            //                    {
            //                        row.RelativeItem().Text("[ STYLE-WISE EVENTS PRODUCTION STATUS REPORT ]")
            //                            .FontSize(14).Bold().TextColor(Colors.Blue.Darken4);
            //                        row.ConstantItem(100).Text($"Date: {DateTime.Now:dd-MMM-yyyy}")
            //                            .FontSize(9).AlignRight().Style(TextStyle.Default.Italic());
            //                    });

            //                    column.Item().PaddingTop(5).LineHorizontal(1).Color(Colors.Grey.Lighten1);

            //                    // Scope Metadata coordinates row block layout
            //                    column.Item().PaddingTop(8).Row(row =>
            //                    {
            //                        row.RelativeItem().Text(t => { t.Span("Buyer Code: ").Bold(); t.Span(buyer); });
            //                        row.RelativeItem().Text(t => { t.Span("Order Reference: ").Bold(); t.Span(order); });
            //                        row.RelativeItem().Text(t => { t.Span("Garment Type: ").Bold(); t.Span(type); });
            //                        row.RelativeItem().Text(t => { t.Span("Active Style: ").Bold(); t.Span(style); });
            //                    });

            //                    column.Item().PaddingTop(8).PaddingBottom(4).LineHorizontal(1.5f).Color(Colors.Blue.Darken4);
            //                });

            //                // 2. PRIMARY SPREADSHEET MATRIX LEDGER DATA PANEL
            //                page.Content().PaddingTop(10).Table(table =>
            //                {
            //                    // Define 6 responsive vector structural column grid size widths matching text limitations
            //                    table.ColumnsDefinition(columns =>
            //                    {
            //                        columns.ConstantColumn(70);  // Event Code ID
            //                        columns.RelativeColumn(2.5f); // Description string text line block
            //                        columns.ConstantColumn(85);  // Scheduled date format
            //                        columns.ConstantColumn(85);  // Actual received date format
            //                        columns.RelativeColumn(2);   // Operations comments
            //                        columns.ConstantColumn(120); // Milestone dynamic status text
            //                    });

            //                    // Generate corporate spreadsheet metadata header rows
            //                    table.Header(header =>
            //                    {
            //                        header.Cell().Text("Event Code").Bold();
            //                        header.Cell().Text("Milestone Description").Bold();
            //                        header.Cell().Text("Scheduled Date").Bold();
            //                        header.Cell().Text("Actual Floor Date").Bold();
            //                        header.Cell().Text("Operational Remarks / Notes").Bold();
            //                        header.Cell().Text("Compliance Status").Bold();

            //                        header.Cell().ColumnSpan(6).PaddingTop(4).LineHorizontal(1).Color(Colors.Grey.Darken2);
            //                    });

            //                    // Inject live dataset records line-item rows dynamically loop
            //                    foreach (var item in dataset)
            //                    {
            //                        table.Cell().PaddingVertical(4).Text(item.EventCode).FontFamily(Fonts.CourierNew).Bold();
            //                        table.Cell().PaddingVertical(4).Text(item.Description);
            //                        table.Cell().PaddingVertical(4).Text(item.ScheduledDate?.ToString("dd-MMM-yyyy") ?? "***");
            //                        table.Cell().PaddingVertical(4).Text(item.ActualDate?.ToString("dd-MMM-yyyy") ?? "***");
            //                        table.Cell().PaddingVertical(4).Text(item.Remarks ?? "-").Style(TextStyle.Default.FontSize(9));

            //                        // Colour match milestone text styles cleanly according to compliance tracking states
            //                        var cellText = table.Cell().PaddingVertical(4).Text(item.MilestoneStatus).Bold();
            //                        if (item.MilestoneStatus == "No Scheduled Date") cellText.TextColor(Colors.Red.Medium);
            //                        else if (item.MilestoneStatus == "Actual Date Pending") cellText.TextColor(Colors.Orange.Darken3);
            //                        else cellText.TextColor(Colors.Green.Medium);
            //                    }
            //                });

            //                // 3. REPORT TAIL FOOTER PANEL (Digital Signatures & Approvals)
            //                page.Footer().Column(column =>
            //                {
            //                    column.Item().PaddingTop(15).LineHorizontal(1.5f).Color(Colors.Blue.Darken4);

            //                    column.Item().PaddingTop(8).Row(row =>
            //                    {
            //                        // If executive signature details are present, render corporate stamp context layout
            //                        if (!string.IsNullOrEmpty(managerApprovalText))
            //                        {
            //                            row.RelativeItem().Text(managerApprovalText)
            //                                .FontSize(9).Bold().TextColor(Colors.Green.Darken4);
            //                        }
            //                        else
            //                        {
            //                            row.RelativeItem().Text("⚠️ VERIFICATION WARNING: This tracking sheet has not been officially approved by management yet.")
            //                                .FontSize(9).Style(TextStyle.Default.Italic()).TextColor(Colors.Grey.Darken1);
            //                        }

            //                        row.RelativeItem().Text(x =>
            //                        {
            //                            x.Span("Page ");
            //                            x.CurrentPageNumber();
            //                            x.Span(" of ");
            //                            x.TotalPages();
            //                        }).AlignRight().FontSize(9);
            //                    });

            //                    column.Item().PaddingTop(4).Text("[ End of Report - System Generated Ledger Data Document ]")
            //                        .FontSize(8).AlignCenter().TextColor(Colors.Grey.Lighten1);
            //                });
            //            });
            //        }).GeneratePdf(memoryStream);

            //        return memoryStream.ToArray();
            //    }
            //}
        }
    }
}
