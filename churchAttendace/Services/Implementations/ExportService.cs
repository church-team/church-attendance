using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using churchAttendace.Services.Interfaces;

namespace churchAttendace.Services.Implementations
{
    public class ExportService : IExportService
    {
        public byte[] ExportToExcel(string sheetName, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.AddWorksheet(sheetName);

            for (var i = 0; i < headers.Count; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }

            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];
                for (var colIndex = 0; colIndex < row.Count; colIndex++)
                {
                    worksheet.Cell(rowIndex + 2, colIndex + 1).Value = row[colIndex];
                }
            }

            worksheet.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] ExportToPdf(string title, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text(title)
                        .FontSize(18)
                        .Bold()
                        .AlignCenter();

                    page.Content()
                        .PaddingTop(10)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                for (var i = 0; i < headers.Count; i++)
                                {
                                    columns.RelativeColumn();
                                }
                            });

                            table.Header(header =>
                            {
                                foreach (var cell in headers)
                                {
                                    header.Cell().Element(CellStyle).Text(cell).Bold();
                                }
                            });

                            foreach (var row in rows)
                            {
                                foreach (var cell in row)
                                {
                                    table.Cell().Element(CellStyle).Text(cell);
                                }
                            }
                        });
                });
            });

            return document.GeneratePdf();
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container.Padding(5).Border(1).BorderColor(Colors.Grey.Lighten2);
        }
    }
}
