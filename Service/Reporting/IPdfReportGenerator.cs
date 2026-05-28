using Business.Entities;

namespace Service.Reporting;

public interface IPdfReportGenerator
{
    void GenerateDailyRationReport(DailyRation ration, string filename);
}
