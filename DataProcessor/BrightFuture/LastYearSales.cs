using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.BrightFuture
{

    public class YearlySales
    {
        public string HCOId;
        public string ProductGroupId;
        public double ThisYearYTDMonthlyAverageCredit;
        public string LastYearStartSalesMonth;
        public string ThisYearStartSalesMonth;
        public int LastYearSalesMonthNumber;
        public double LastYearMonthlyAverageCredit;
        public double LastYearTotalCredit;
        public double ThisYearYTDCredit;
        public int ThisYearSalesMonthNumber;
        public double ThisQuarterMonthlyAverageCredit;
        public double NewlyDeveloped_ThisQuarterMonthlyAverageCredit;
        public double LastTwoQuarterMonthlyAverageCredit;
        public double LastQuarterMonthlyAverageCredit;
        public double ThisQuarterTotalCredit;
        public int iThisYearStartSalesMonth;
        public int iLastYearStartSalesMonth;

        public int LastYear;
        public List<MonthlySales> lastYearMonthlySales = new List<MonthlySales>();
        public List<MonthlySales> thisYearMonthlySales = new List<MonthlySales>();
     
    }
    public class YearlySalesClassMap : ClassMap<YearlySales>
    {
        public YearlySalesClassMap()
        {
            Map(m => m.HCOId).Name("医院编码");
            Map(m => m.ProductGroupId).Name("产品系列ID");
            Map(m => m.ThisYearYTDMonthlyAverageCredit).Name("今年YTD月均积分");
            Map(m => m.ThisYearYTDCredit).Name("今年YTD积分");
            Map(m => m.LastYearStartSalesMonth).Name("去年起销月");
            Map(m => m.LastYearSalesMonthNumber).Name("去年销售月数");
            Map(m => m.LastYearTotalCredit).Name("去年总积分");
            Map(m => m.LastYearMonthlyAverageCredit).Name("去年月均积分");
            Map(m => m.ThisQuarterMonthlyAverageCredit).Name("本季度月均积分");
            Map(m => m.LastTwoQuarterMonthlyAverageCredit).Name("前两个季度月均积分");
            Map(m => m.LastQuarterMonthlyAverageCredit).Name("前一季度月均积分");
            Map(m => m.ThisQuarterTotalCredit).Name("本季度累计积分");
            Map(m => m.NewlyDeveloped_ThisQuarterMonthlyAverageCredit).Name("本季度新开发月均积分");
        }
    }
    public class SalesProcessor
    {
        public static List<YearlySales> Process(IEnumerable<Sales> sales, string calculatingQuarter)
        {
            List<YearlySales> yearlySales = new List<YearlySales>();

            //var sales = from s in sales0
            //            where s.HCOId == "97118"
            //            select s;

            //Get distinct hcoid + territoryId + product group id
            var HCO_Territorys = (from Sales s in sales
                                  select new { HCOId = s.HCOId, ProductGroupId = s.ProductGroupId }).Distinct();
            int i = 0;

            int thisYear = Int32.Parse(calculatingQuarter.Substring(0, 4));
            int quarterNumber = Int32.Parse(calculatingQuarter.Substring(5, 1));
            int lastYear = thisYear - 1;
            int YTDMonthNumber = quarterNumber * 3;

            DateTime thisQuarterStartMonth = new DateTime(thisYear, (quarterNumber - 1) * 3 + 1, 1);
            DateTime thisQuarterEndMonth = thisQuarterStartMonth.AddMonths(2);
            DateTime lastTwoQuarterStartMonth = thisQuarterStartMonth.AddMonths(-6);
            DateTime lastTwoQuarterEndMonth = thisQuarterStartMonth.AddMonths(-1);
            DateTime lastQuarterStartMonth = thisQuarterStartMonth.AddMonths(-3);
            DateTime lastQuarterEndMonth = thisQuarterStartMonth.AddMonths(-1);


            int iThisQuarterStartMonth = Int32.Parse(thisQuarterStartMonth.ToString("yyyyMM"));
            int iThisQuarterEndMonth = Int32.Parse(thisQuarterEndMonth.ToString("yyyyMM"));

            int iLastTwoQuarterStartMonth = Int32.Parse(lastTwoQuarterStartMonth .ToString("yyyyMM"));
            int iLastTwoQuarterEndMonth = Int32.Parse(lastTwoQuarterEndMonth .ToString("yyyyMM"));

            int iLastQuarterStartMonth = Int32.Parse(lastQuarterStartMonth .ToString("yyyyMM"));
            int iLastQuarterEndMonth = Int32.Parse(lastQuarterEndMonth .ToString("yyyyMM"));



            int startMonthLastTwoQuarter = (quarterNumber - 1) * 3 + 1;


            foreach (var ht in HCO_Territorys)
            {
 //             if (i == 0)
                {
                    Console.WriteLine("HCOId: {0}, ProductGroupId{1}", ht.HCOId, ht.ProductGroupId);
                    //Iterate the list and get the yearly sales info for each item
                    YearlySales salesPerYear = GetYearSales(sales, lastYear, ht.HCOId,  ht.ProductGroupId);

                    //salesPerYear.ThisYearYTDCredit = (from Sales s in sales
                    //                               where s.HCOId == ht.HCOId 
                    //                               && s.ProductGroupId == ht.ProductGroupId 
                    //                               && s.Year == thisYear
                    //                               select s.Credit).Sum();
                    //salesPerYear.ThisYearYTDMonthlyAverageCredit = salesPerYear.ThisYearYTDCredit / YTDMonthNumber;

                    salesPerYear.ThisYearSalesMonthNumber = thisQuarterEndMonth.Month - salesPerYear.iThisYearStartSalesMonth + 1;
                    salesPerYear.ThisYearYTDMonthlyAverageCredit = salesPerYear.ThisYearYTDCredit / YTDMonthNumber; //salesPerYear.ThisYearSalesMonthNumber;



                    //Get this quarter monthly average sales
                    var filterList = from Sales s in sales
                                     where s.HCOId == ht.HCOId
                                     && s.ProductGroupId == ht.ProductGroupId
                                     && s.Month >= iThisQuarterStartMonth && s.Month <= iThisQuarterEndMonth
                                     select s.Credit;


                    if (salesPerYear.iThisYearStartSalesMonth <= thisQuarterStartMonth.Month)
                    {
                        salesPerYear.NewlyDeveloped_ThisQuarterMonthlyAverageCredit = filterList.Sum() / 3.0;
                    }
                    else
                    {
                        salesPerYear.NewlyDeveloped_ThisQuarterMonthlyAverageCredit = filterList.Sum() / (thisQuarterEndMonth.Month - salesPerYear.iThisYearStartSalesMonth + 1);
                        if (salesPerYear.NewlyDeveloped_ThisQuarterMonthlyAverageCredit < 0) salesPerYear.NewlyDeveloped_ThisQuarterMonthlyAverageCredit = 0;
                        if (double.IsNaN(salesPerYear.NewlyDeveloped_ThisQuarterMonthlyAverageCredit)) salesPerYear.NewlyDeveloped_ThisQuarterMonthlyAverageCredit = 0;
                    }

                    //Get this quarter monthly average sales
                    filterList = from Sales s in sales
                                     where s.HCOId == ht.HCOId
                                     && s.ProductGroupId == ht.ProductGroupId
                                     && s.Month >= iThisQuarterStartMonth && s.Month <= iThisQuarterEndMonth
                                     select s.ThisQuarterCredit;

                    salesPerYear.ThisQuarterMonthlyAverageCredit = filterList.Sum() / 3.0;
                    if (salesPerYear.ThisQuarterMonthlyAverageCredit < 0) salesPerYear.ThisQuarterMonthlyAverageCredit = 0;

                    //Get Last two quearter's sales
                    var filterList1 = from Sales s in sales
                                      where s.HCOId == ht.HCOId
                                      && s.ProductGroupId == ht.ProductGroupId
                                      && s.Month >= iLastTwoQuarterStartMonth && s.Month <= iLastTwoQuarterEndMonth
                                      select s.ThisQuarterCredit;
                    int totalMonth = 6;
                    //if(lastTwoQuarterStartMonth.Year > lastYear) //last two quarters are in this year
                    //{
                    //    totalMonth = (lastTwoQuarterEndMonth.Month - salesPerYear.iThisYearStartSalesMonth + 1 >= 6) ? 6 : lastTwoQuarterEndMonth.Month - salesPerYear.iThisYearStartSalesMonth + 1;
                    //}
                    //else if(lastTwoQuarterEndMonth.Year == lastYear)//last two quarters are in last year
                    //{
                    //    totalMonth = (lastTwoQuarterEndMonth.Month - salesPerYear.iLastYearStartSalesMonth + 1 >= 6) ? 6 : lastTwoQuarterEndMonth.Month - salesPerYear.iLastYearStartSalesMonth + 1;
                    //}
                    //else if(lastTwoQuarterStartMonth.Year == lastYear && lastTwoQuarterEndMonth.Year > lastYear)//one of last two quarters in last year and one in this year
                    //{
                    //    totalMonth = ((lastTwoQuarterEndMonth.Month - salesPerYear.iLastYearStartSalesMonth + 1 >= 3) ? 3 : lastTwoQuarterEndMonth.Month - salesPerYear.iLastYearStartSalesMonth + 1) + 
                    //        ((lastTwoQuarterEndMonth.Month - salesPerYear.iThisYearStartSalesMonth + 1 >= 3) ? 3 : lastTwoQuarterEndMonth.Month - salesPerYear.iThisYearStartSalesMonth + 1);
                    //}
                    salesPerYear.LastTwoQuarterMonthlyAverageCredit = filterList1.Sum() / totalMonth;

                    //Get this quarter's total credit for the purpose of claculating the 上季度的压货返回
                    filterList = from Sales s in sales
                                 where s.HCOId == ht.HCOId
                                 && s.ProductGroupId == ht.ProductGroupId
                                 && s.Month >= iThisQuarterStartMonth && s.Month <= iThisQuarterEndMonth
                                 select s.LastQuarterCredit;

                    salesPerYear.ThisQuarterTotalCredit = filterList.Sum();

                   //Get Last quearter's sales
                    filterList1 = from Sales s in sales
                                      where s.HCOId == ht.HCOId
                                      && s.ProductGroupId == ht.ProductGroupId
                                      && s.Month >= iLastQuarterStartMonth && s.Month <= iLastQuarterEndMonth
                                      select s.LastQuarterCredit;

                    //if(lastQuarterStartMonth.Year > lastYear) //last quarter are in this year
                    //{
                    //    totalMonth = (lastQuarterEndMonth.Month - salesPerYear.iThisYearStartSalesMonth + 1 >= 3) ? 3 : lastQuarterEndMonth.Month - salesPerYear.iThisYearStartSalesMonth + 1;
                    //}
                    //else // if(lastQuarterEndMonth.Year == lastYear)//last quarters are in last year
                    //{
                    //    totalMonth = (lastQuarterEndMonth.Month - salesPerYear.iLastYearStartSalesMonth + 1 >= 3) ? 3 : lastQuarterEndMonth.Month - salesPerYear.iLastYearStartSalesMonth;
                    //}
                    totalMonth = 3;
                    salesPerYear.LastQuarterMonthlyAverageCredit = filterList1.Sum() / totalMonth;

                    yearlySales.Add(salesPerYear);
                }

                i++;
            }
            
            return yearlySales;
        }

        private static YearlySales GetYearSales(IEnumerable<Sales> sales, int lastyear, string HCOId, string productGroupId)
        {
            YearlySales sale = new YearlySales();
            sale.LastYear = lastyear;
            sale.HCOId = HCOId;
            sale.ProductGroupId = productGroupId;

            sale.LastYearTotalCredit = 0;

            //Get last year's data
            for(int i = 1; i <= 12; i++)
            {
                DateTime month = new DateTime(lastyear, i, 1);

                MonthlySales monthlySales = new MonthlySales(){ Month = month, MonthInt = Int32.Parse(month.ToString("yyyyMM"))};

                var filteredList = from Sales s in sales
                                   where s.HCOId == HCOId
                                   && s.ProductGroupId == productGroupId && s.Month == monthlySales.MonthInt  //用bonus_in来计算起销月
                                   //&& s.ProductGroupId == productGroupId && DateTimeUtility.GetMonthInt(s.SalesDate) == monthlySales.MonthInt //用sales date来计算起销月
                                   && s.Year == lastyear 
                                   select s.Credit;
                double credit = filteredList.Sum();
                                                   //double credit = sales.Where(s => s.ProductGroupId == productGroupId && s.HCOId == HCOId &&
                                                   //               s.Month == monthlySales.MonthString &&  s.TerritoryId == territoryId && s.Year == year).
                                                   //                Select(x => x.Credit).Sum();
                monthlySales.Credit = credit;
                if(filteredList.Count() > 0)
                    sale.lastYearMonthlySales.Add(monthlySales);


                sale.LastYearTotalCredit += credit;
            }

            int startSalesMonth = GetStartSalesMonth(sale.lastYearMonthlySales);
            if(startSalesMonth > 0)
            {
                sale.LastYearStartSalesMonth = lastyear.ToString() + startSalesMonth.ToString("0#");
                sale.LastYearSalesMonthNumber = 12 - startSalesMonth + 1;
                sale.LastYearMonthlyAverageCredit = sale.LastYearTotalCredit / sale.LastYearSalesMonthNumber;

                sale.iLastYearStartSalesMonth = startSalesMonth;

            }

            int thisYear = lastyear + 1;
            //Get this year's data
            for (int i = 1; i <= 12; i++)
            {
                DateTime month = new DateTime(thisYear, i, 1);

                MonthlySales monthlySales = new MonthlySales() { Month = month, MonthInt = Int32.Parse(month.ToString("yyyyMM")) };

                var filteredList = from Sales s in sales
                                   where s.HCOId == HCOId
                                    && s.ProductGroupId == productGroupId && s.Month == monthlySales.MonthInt  //用bonus_in来计算起销月
                                  //&& s.ProductGroupId == productGroupId && DateTimeUtility.GetMonthInt(s.SalesDate) == monthlySales.MonthInt //use sales date to calculate starting selling month
                                   && s.Year == thisYear
                                   select s.Credit;
                double credit = filteredList.Sum();
                monthlySales.Credit = credit;
                //double credit = sales.Where(s => s.ProductGroupId == productGroupId && s.HCOId == HCOId &&
                //               s.Month == monthlySales.MonthString &&  s.TerritoryId == territoryId && s.Year == year).
                //                Select(x => x.Credit).Sum();
                if(filteredList.Count() > 0)
                    sale.thisYearMonthlySales.Add(monthlySales);

                sale.ThisYearYTDCredit += credit;
            }

            startSalesMonth = GetStartSalesMonth(sale.thisYearMonthlySales);

            if(startSalesMonth > 0)
            {
                sale.ThisYearStartSalesMonth = lastyear.ToString() + startSalesMonth.ToString("0#");
                sale.iThisYearStartSalesMonth = startSalesMonth;
            }

            return sale;
        }
        private static YearlySales GetYearSales_bySalesDate(IEnumerable<Sales> sales, int lastyear, string HCOId, string productGroupId)
        {
            YearlySales sale = new YearlySales();
            sale.LastYear = lastyear;
            sale.HCOId = HCOId;
            sale.ProductGroupId = productGroupId;

            sale.LastYearTotalCredit = 0;

            //Get last year's data
            for (int i = 1; i <= 12; i++)
            {
                DateTime month = new DateTime(lastyear, i, 1);

                MonthlySales monthlySales = new MonthlySales() { Month = month, MonthInt = Int32.Parse(month.ToString("yyyyMM")) };

                var filteredList = from Sales s in sales
                                   where s.HCOId == HCOId
                                   //&& s.ProductGroupId == productGroupId && s.Month == monthlySales.MonthInt  //用bonus_in来计算起销月
                                   && s.ProductGroupId == productGroupId && DateTimeUtility.GetMonthInt(s.SalesDate) == monthlySales.MonthInt //用sales date来计算起销月
                                   && s.Year == lastyear
                                   select s.Credit;
                double credit = filteredList.Sum();
                //double credit = sales.Where(s => s.ProductGroupId == productGroupId && s.HCOId == HCOId &&
                //               s.Month == monthlySales.MonthString &&  s.TerritoryId == territoryId && s.Year == year).
                //                Select(x => x.Credit).Sum();
                monthlySales.Credit = credit;
                if (filteredList.Count() > 0)
                    sale.lastYearMonthlySales.Add(monthlySales);


                sale.LastYearTotalCredit += credit;
            }

            int startSalesMonth = GetStartSalesMonth(sale.lastYearMonthlySales);
            if (startSalesMonth > 0)
            {
                sale.LastYearStartSalesMonth = lastyear.ToString() + startSalesMonth.ToString("0#");
                sale.LastYearSalesMonthNumber = 12 - startSalesMonth + 1;
                sale.LastYearMonthlyAverageCredit = sale.LastYearTotalCredit / sale.LastYearSalesMonthNumber;

                sale.iLastYearStartSalesMonth = startSalesMonth;

            }

            int thisYear = lastyear + 1;
            //Get this year's data
            for (int i = 1; i <= 12; i++)
            {
                DateTime month = new DateTime(thisYear, i, 1);

                MonthlySales monthlySales = new MonthlySales() { Month = month, MonthInt = Int32.Parse(month.ToString("yyyyMM")) };

                var filteredList = from Sales s in sales
                                   where s.HCOId == HCOId
                                   && s.ProductGroupId == productGroupId && DateTimeUtility.GetMonthInt(s.SalesDate) == monthlySales.MonthInt
                                   && s.Year == thisYear
                                   select s.Credit;
                double credit = filteredList.Sum();
                monthlySales.Credit = credit;
                //double credit = sales.Where(s => s.ProductGroupId == productGroupId && s.HCOId == HCOId &&
                //               s.Month == monthlySales.MonthString &&  s.TerritoryId == territoryId && s.Year == year).
                //                Select(x => x.Credit).Sum();
                if (filteredList.Count() > 0)
                    sale.thisYearMonthlySales.Add(monthlySales);

                sale.ThisYearYTDCredit += credit;
            }

            startSalesMonth = GetStartSalesMonth(sale.thisYearMonthlySales);

            if (startSalesMonth > 0)
            {
                sale.ThisYearStartSalesMonth = lastyear.ToString() + startSalesMonth.ToString("0#");
                sale.iThisYearStartSalesMonth = startSalesMonth;
            }

            return sale;
        }

        private static int GetStartSalesMonth(List<MonthlySales> monthlySales)
        {
            int startSalesMonth = 0;

            var monthWithSales = from MonthlySales m in monthlySales
                                 //where m.Credit > 0
                                 orderby m.MonthInt ascending
                                 select m.Month;

            if (monthWithSales.Count() == 0)
            {
                return 0;
            }
            else
                startSalesMonth = monthWithSales.First().Month;
                                 
            return startSalesMonth;
        }
    }
    public class SalesClassMap : ClassMap<Sales>
    {
        public SalesClassMap()
        {
            Map(m => m.Sequence).Index(0);
            Map(m => m.SalesId).Index(1);
            Map(m => m.DistrictId).Index(2);
            Map(m => m.District).Index(3);
            Map(m => m.RegionId).Index(4);
            Map(m => m.Region).Index(5);
            Map(m => m.AreaId).Index(6);
            Map(m => m.Area).Index(7);
            Map(m => m.SubAreaId).Index(8);
            Map(m => m.SubArea).Index(9);
            Map(m => m.Year).Index(10);
            Map(m => m.Month).Index(11);
            Map(m => m.SalesDate).Index(12);
            Map(m => m.HCOId).Index(13);
            Map(m => m.Level).Index(14);
            Map(m => m.Potential).Index(15).TypeConverterOption.NullValues(string.Empty);
            Map(m => m.ProductGroupId).Index(16);
            Map(m => m.ProductGroup).Index(17);
            Map(m => m.ProductId).Index(18);
            Map(m => m.Product).Index(19);
            Map(m => m.SalesQty).Index(20);
            Map(m => m.Credit).Index(21);
            Map(m => m.TerritoryId).Index(22);
            Map(m => m.MR).Index(23);
            Map(m => m.MRId).Index(24);
            Map(m => m.MRCode).Index(25);
            Map(m => m.Position).Index(26);
            Map(m => m.HospitalDistribution).Index(27);
        }
    }
    public class UpdatedSalesClassMap : ClassMap<Sales>
    {
        public UpdatedSalesClassMap()
        {
            Map(m => m.Sequence).Index(0);
            Map(m => m.SalesId).Index(1);
            Map(m => m.DistrictId).Index(2);
            Map(m => m.District).Index(3);
            Map(m => m.RegionId).Index(4);
            Map(m => m.Region).Index(5);
            Map(m => m.AreaId).Index(6);
            Map(m => m.Area).Index(7);
            Map(m => m.SubAreaId).Index(8);
            Map(m => m.SubArea).Index(9);
            Map(m => m.Year).Index(10);
            Map(m => m.Month).Index(11);
            Map(m => m.SalesDate).Index(12);
            Map(m => m.HCOId).Index(13);
            Map(m => m.Level).Index(14);
            Map(m => m.Potential).Index(15).TypeConverterOption.NullValues(string.Empty);
            Map(m => m.ProductGroupId).Index(16);
            Map(m => m.ProductGroup).Index(17);
            Map(m => m.ProductId).Index(18);
            Map(m => m.Product).Index(19);
            Map(m => m.SalesQty).Index(20);
            Map(m => m.Credit).Index(21);
            Map(m => m.TerritoryId).Index(22);
            Map(m => m.MR).Index(23);
            Map(m => m.MRId).Index(24);
            Map(m => m.MRCode).Index(25);
            Map(m => m.Position).Index(26);
            Map(m => m.HospitalDistribution).Index(27);
            Map(m => m.ThisQuarterCredit).Index(28);
            Map(m => m.LastQuarterCredit).Index(29);
        }
    }
    public class Sales
    {
        public int Sequence { get; set; }
        public string SalesId { get; set; }
        public string DistrictId { get; set; }
        public string District { get; set; }
        public string RegionId { get; set; }
        public string Region { get; set; }
        public string AreaId { get; set; }
        public string Area { get; set; }
        public string SubAreaId { get; set; }
        public string SubArea { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime SalesDate { get; set; }
        public string HCOId { get; set; }
        public string Level { get; set; }
        public int? Potential { get; set; }
        public string ProductGroupId { get; set; }
        public string ProductGroup { get; set; }
        public string ProductId { get; set; }
        public string Product { get; set; }
        public int SalesQty { get; set; }
        public double Credit { get; set; }
        public string TerritoryId { get; set; }
        public string MR { get; set; }
        public string MRId { get; set; }
        public string MRCode { get; set; }
        public string Position { get; set; }
        public double ThisQuarterCredit { get; set; }
        public double LastQuarterCredit { get; set; }
        public int HospitalDistribution { get; set; } //1:confirmed sales; 0:natural sales.
    }
}
