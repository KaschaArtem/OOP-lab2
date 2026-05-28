using Business.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Service.Reporting;

public class PdfReportGenerator : IPdfReportGenerator
{
    private const string ReportFontFamily = "Adwaita Sans";

    public void GenerateDailyRationReport(DailyRation ration, string filename)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontFamily(ReportFontFamily).FontSize(11));

                page.Content().Column(column =>
                {
                    column.Spacing(6);
                    column.Item().Text("Daily Ration Report").Bold().FontSize(18);
                    column.Item().Text($"Total calories: {Math.Round(ration.GetTotalCalories(), 1)}");
                    column.Item().Text($"Total protein: {Math.Round(ration.GetTotalProtein(), 1)} g");
                    column.Item().Text($"Total fats: {Math.Round(ration.GetTotalFats(), 1)} g");
                    column.Item().Text($"Total carbs: {Math.Round(ration.GetTotalCarbs(), 1)} g");
                    column.Item().PaddingTop(4);

                    foreach (var mealTime in ration.MealTimes)
                    {
                        column.Item().Text(mealTime.Key).Bold().FontSize(13);

                        if (mealTime.Value.Meal.Count == 0)
                        {
                            column.Item().Text("- No products");
                            continue;
                        }

                        foreach (Product product in mealTime.Value.Meal)
                        {
                            string line = $"- {product.Name}: {Math.Round(product.Weight, 1)} g | " +
                                          $"Kcal {Math.Round(product.Calories, 1)}, " +
                                          $"P {Math.Round(product.Protein, 1)}, F {Math.Round(product.Fats, 1)}, C {Math.Round(product.Carbs, 1)}";
                            column.Item().Text(line);
                        }

                        column.Item().PaddingBottom(2);
                    }
                });
            });
        }).GeneratePdf(filename);
    }
}
