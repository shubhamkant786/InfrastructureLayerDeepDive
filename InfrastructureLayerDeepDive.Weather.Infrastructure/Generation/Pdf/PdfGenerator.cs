using InfrastructureLayerDeepDive.Weather.Domain;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Globalization;



namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Generation.Pdf
{
    public class PdfGenerator
    {
        #region Generate Pdf File
        private byte[] GeneratePdfDocument(IEnumerable<WeatherEntity> weatherList, IReadOnlyCollection<WeatherEntity> childWeatherList)
        {

            using (var memoryStream = new MemoryStream())
            {
                // Initialize PDF writer
                using (var writer = new PdfWriter(memoryStream))
                {
                    // Initialize PDF document
                    using (var pdf = new PdfDocument(writer))
                    {
                        // Document to add layout elements
                        var document = new Document(pdf, iText.Kernel.Geom.PageSize.A4);
                        document.SetMargins(54, 37, 104, 54); // Set margins (top, right, bottom, left)

                        var dates = weatherList.Select(w => w.Date).Distinct().ToList();

                        var table = GetItineraryPointTableWithHeader();

                        foreach (var weather in weatherList)
                        {
                            if (!dates.Contains(weather.Date))
                            {
                                if (dates.Any())
                                {
                                    document.Add(table);
                                    document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                                    table = GetItineraryPointTableWithHeader();
                                }

                                dates.Add(weather.Date);

                                //Add train header
                                document.Add(GetTrainHeaderTable(weather));
                            }

                            AddWeatherPointRow(table, weather, childWeatherList);
                        }
                        document.Add(table);

                        document.Close();
                    }
                }

                return AddPageNumbers(memoryStream.ToArray());
            }
        }

        private byte[] AddPageNumbers(byte[] pdfBytes)
        {
            using (var msOutput = new MemoryStream())
            {
                using (var reader = new PdfReader(new MemoryStream(pdfBytes)))
                {
                    using (var writer = new PdfWriter(msOutput))
                    {
                        using (var pdfDoc = new PdfDocument(reader, writer))
                        {
                            var total = pdfDoc.GetNumberOfPages();
                            var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                            for (int i = 1; i <= total; i++)
                            {
                                var page = pdfDoc.GetPage(i);
                                var pageSize = page.GetPageSize();
                                var x = pageSize.GetWidth() - 100;
                                var y = pageSize.GetBottom() + 20;
                                var footer = $"{"Page"} {i} {"Of"} {total}";

                                var pdfCanvas = new PdfCanvas(page);
                                pdfCanvas.BeginText()
                                         .SetFontAndSize(font, 10)
                                         .MoveText(x, y)
                                         .ShowText(footer)
                                         .EndText();
                            }
                        }
                    }
                }

                return msOutput.ToArray();
            }
        }

        private Table GetTrainHeaderTable(WeatherEntity weather)
        {

            var table = new Table(2).SetWidth(300);

            AddCell(table, "Date", 12, true, null!, true, TextAlignment.LEFT);

            AddCell(table, "Temperature", 12, true, null!, true, TextAlignment.LEFT);

            return table;
        }

        private Table GetItineraryPointTableWithHeader()
        {

            var table = new Table(UnitValue.CreatePercentArray(new float[] { 15, 10, 25, 17, 17, 16 }))
                                           .UseAllAvailableWidth()
                                           .SetBorder(Border.NO_BORDER);

            AddCell(table, "Date", 12, true, ColorConstants.LIGHT_GRAY);
            AddCell(table, "Temperature", 12, true, ColorConstants.LIGHT_GRAY, false, TextAlignment.LEFT);


            return table;
        }

        private void AddWeatherPointRow(Table table, WeatherEntity weather, IReadOnlyCollection<WeatherEntity> infraPtcars)
        {
            var passagePointsOperationalCodes = new string[] { "P", "D" };

            AddCell(table, weather.WeatherPoints?.FirstOrDefault()?.MaxTemperatureC.ToString(), 14);

            AddCell(table, weather.WeatherPoints?.FirstOrDefault()?.MaxTemperatureC.ToString(), 14);

        }

        private void AddCell(Table table, string? value, int fontSize, bool isHeader = false, Color color = null!, bool noBorder = false, TextAlignment textAlignment = TextAlignment.CENTER)
        {
            var font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);

            var cell = new Cell().Add(new Paragraph(value ?? ""))
                                 .SetFont(font)
                                 .SetFontSize(fontSize)
                                 .SetBorderLeft(Border.NO_BORDER)
                                 .SetBorderRight(Border.NO_BORDER)
                                 .SetTextAlignment(textAlignment);
            if (noBorder)
            {
                cell.SetBorder(Border.NO_BORDER);
            }

            if (color != null)
            {
                cell.SetBackgroundColor(color);
            }

            if (isHeader)
                table.AddHeaderCell(cell);
            else
                table.AddCell(cell);

        }

        #endregion
    }
}
