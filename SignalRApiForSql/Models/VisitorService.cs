﻿using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SignalRApiForSql.DAL;
using SignalRApiForSql.Hubs;

namespace SignalRApiForSql.Models
{
    public class VisitorService
    {
        private readonly Context _context;
        private readonly IHubContext<VisitorHub> _hubContext;
        public VisitorService(Context context, IHubContext<VisitorHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public IQueryable<Visitor> GetList()
        {
            return _context.Visitors.AsQueryable();
        }

        public async Task SaveVisitor(Visitor visitor)
        {
            await _context.Visitors.AddAsync(visitor);
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("ReceiveVisitorList", GetVisitorChartList());
        }

        public List<VisitorChart> GetVisitorChartList()
        {
            List<VisitorChart> visitorCharts = new List<VisitorChart>();
            try
            {
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = @"
                SELECT 
                    tarih,
                    [1], [2], [3], [4], [5]
                FROM 
                (
                    SELECT 
                        [City], 
                        CityVisitCount, 
                        CAST([VisitDate] AS DATE) AS tarih 
                    FROM 
                        Visitors
                ) AS visitTable 
                PIVOT 
                (
                    SUM(CityVisitCount) 
                    FOR City IN([1], [2], [3], [4], [5])
                ) AS pivottable 
                ORDER BY 
                    tarih ASC";
                    command.CommandType = System.Data.CommandType.Text;

                    _context.Database.OpenConnection();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            VisitorChart visitorChart = new VisitorChart
                            {
                                VisitDate = reader.GetDateTime(0).ToShortDateString()
                            };

                            Enumerable.Range(1, 5).ToList().ForEach(x =>
                            {
                                if (DBNull.Value.Equals(reader[x]))
                                {
                                    visitorChart.Counts.Add(0);
                                }
                                else
                                {
                                    visitorChart.Counts.Add(reader.GetInt32(x));
                                }
                            });

                            visitorCharts.Add(visitorChart);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetVisitorChartList: {ex.Message}");
                throw; // Hata fırlat
            }
            finally
            {
                _context.Database.CloseConnection();
            }

            return visitorCharts;
        }
    }
















    //public class VisitorService
    //{
    //    private readonly Context _context;
    //    private readonly IHubContext<VisitorHub> _hubContext;
    //    public VisitorService(Context context, IHubContext<VisitorHub> hubContext)
    //    {
    //        _context = context;
    //        _hubContext = hubContext;
    //    }
    //    public IQueryable<Visitor> GetList()
    //    {
    //        return _context.Visitors.AsQueryable();
    //    }

    //    public async Task SaveVisitor(Visitor visitor)
    //    {
    //        await _context.Visitors.AddAsync(visitor);
    //        await _context.SaveChangesAsync();
    //        await _hubContext.Clients.All.SendAsync("ReceiveVisitorList", GetVisitorChartList());
    //    }
    //    public List<VisitorChart> GetVisitorChartList()
    //    {
    //        List<VisitorChart> visitorCharts = new List<VisitorChart>();
    //        using (var command = _context.Database.GetDbConnection().CreateCommand())
    //        {
    //            command.CommandText = "Create Procedure SignalRPivot As Select tarih,[1],[2],[3],[4],[5] from (select [City],CityVisitCount,Cast([VisitDate] as Date) as tarih from Visitors) as visitTable Pivot (Sum(CityVisitCount) For City in([1],[2],[3],[4],[5])) as pivottable Order By tarih ASC ";
    //            command.CommandType = System.Data.CommandType.Text;
    //            _context.Database.OpenConnection();
    //            using (var reader = command.ExecuteReader())
    //            {
    //                while (reader.Read())
    //                {
    //                    VisitorChart visitorChart = new VisitorChart();
    //                    visitorChart.VisitDate = reader.GetDateTime(0).ToShortDateString();
    //                    Enumerable.Range(1, 5).ToList().ForEach(x =>
    //                    {
    //                        if (DBNull.Value.Equals(reader[x]))
    //                        {
    //                            visitorChart.Counts.Add(0);
    //                        }
    //                        else
    //                        {
    //                            visitorChart.Counts.Add(reader.GetInt32(x));
    //                        }

    //                    });
    //                    visitorCharts.Add(visitorChart);
    //                }
    //            }
    //            _context.Database.CloseConnection();
    //            return visitorCharts;
    //        }
    //    }
    //}
}