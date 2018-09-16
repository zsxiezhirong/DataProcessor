using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.BrightFuture
{
    /// <summary>
    /// MR销量指标表
    /// </summary>
    public class MRSalesTarget
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string Quarter { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public int EmployeeId { get; set; }
        public DateTime? OnboardDate { get; set; }
        public DateTime? OffboardDate { get; set; }
        public int IsOnDuty { get; set; }
        public double OnboardMonths { get; set; }
        public string TerritoryCode { get; set; }
        public string Position { get; set; }
        public string ParentTerritoryCode { get; set; }
        public int ParentId { get; set; }
        public string Area { get; set; }
        public string AreaId { get; set; }
        public string HCOId { get; set; }
        public string Level { get; set; }
        public int Potential { get; set; }
        public string ProductGroupId { get; set; }
        public string ProductGroup { get; set; }
        public string ProductId { get; set; }
        public string Product { get; set; }
        public double Credit { get; set; }
        public double ThisQuarterCredit { get; set; } //以本季度最后一个月的积分标准计算的积分
        public double LastQuarterCredit { get; set; } //以上季度最后一个月的积分标准计算的积分
        public double SalesQty { get; set; } //按品规的销售盒数
        public double Target { get; set; }
        public string SubArea { get; set; }
        public string SubAreaId { get; set; }
        public string BU { get; set; }

        public static List<MRSalesTarget> Process(IEnumerable<Sales> sales0, IEnumerable<Employee> employees0, IEnumerable<MRTarget> MRTargets)
        {
            List<MRSalesTarget> mRSalesTargets = new List<MRSalesTarget>();

            var employees = from e in employees0
                            where e.Position == "学术专员"
                            select e;

            var sales = (from s in sales0
                        where s.MRCode != string.Empty && s.HospitalDistribution == 1 && s.Month > 201800
                         select s).ToList();


            var targetSum = (from MRTarget t in MRTargets
                             group t by new { t.TerritoryId, t.Month, t.HCOId, t.ProductGroupId, t.ProductId } into g
                             select new { g.Key, TotalTarget = g.Sum(p => p.Target)}).ToList();

            var salesResults = (from Sales s in sales
                                group s by new { s.TerritoryId, s.Year, s.Month, s.MRCode, s.ProductGroupId, s.ProductId, s.HCOId } into g
                                select new { g.Key, TotalCredit = g.Sum(p => p.Credit), ThisQuarterTotalCredit = g.Sum(p=>p.ThisQuarterCredit),LastQuarterTotalCredit = g.Sum(p=>p.LastQuarterCredit),
                                TotalSalesQty = g.Sum(p=>p.SalesQty)}).ToList();
            

            var employee_targets = (from Employee e in employees
                                    join t in targetSum on new { Territory = e.TerritoryCode, Month = e.Month } equals
                                    new { Territory = t.Key.TerritoryId, Month = t.Key.Month }
                                    select new
                                    {
                                        HCOId = t.Key.HCOId,
                                        Month = t.Key.Month,
                                        EmployeeCode = e.EmployeeCode,
                                        EmployeeName = e.EmployeeName,
                                        OnboardDate = e.OnboardDate,
                                        OffboardDate = e.OffboardDate,
                                        ProductGroupId = t.Key.ProductGroupId,
                                        ProductId = t.Key.ProductId,
                                        TerritoryCode = t.Key.TerritoryId,
                                        SubAreaId = e.SubAreaCode,
                                        AreaCode = e.AreaCode,
                                        Area = e.Area,
                                        BU = e.BU
                                    }).ToArray();

            var employee_sales = (from  t in salesResults 
                                  join Employee e in employees on
                                  new { EmployeeCode = t.Key.MRCode, Month = t.Key.Month
                                  } equals new { EmployeeCode = e.EmployeeCode, Month = e.Month }  into employee_sales_tmp
                                  from es in employee_sales_tmp.DefaultIfEmpty()
                                  select new
                                  {
                                      HCOId = t.Key.HCOId,
                                      Month = t.Key.Month,
                                      EmployeeCode = t.Key.MRCode,
                                      EmployeeName = (es != null ? es.EmployeeName : ""),
                                      OnboardDate = (es != null ? es.OnboardDate : null),
                                      OffboardDate = (es != null ? es.OffboardDate : null),
                                      ProductGroupId = t.Key.ProductGroupId,
                                      ProductId = t.Key.ProductId,
                                      TerritoryCode = t.Key.TerritoryId,
                                      SubAreaId = (es != null ? es.SubAreaCode : ""),
                                      AreaCode = (es != null ? es.AreaCode : ""),
                                      Area = (es != null ? es.Area : ""),
                                      BU = (es != null ? es.BU : ""),
                                  }).ToArray();

            var employee_sales_1 = (from Employee e in employees 
                                  join t in salesResults on
                                   new { EmployeeCode = e.EmployeeCode, Month = e.Month } equals
                                  new
                                  {
                                      EmployeeCode = t.Key.MRCode,
                                      Month = t.Key.Month
                                  } into employee_sales_tmp
                                  from es in employee_sales_tmp.DefaultIfEmpty()
                                  select new
                                  {
                                      HCOId = (es != null ? es.Key.HCOId : ""),
                                      Month = e.Month,
                                      EmployeeCode = e.EmployeeCode,
                                      EmployeeName = e.EmployeeName,
                                      OnboardDate = e.OnboardDate,
                                      OffboardDate = e.OffboardDate,
                                      ProductGroupId = (es != null ? es.Key.ProductGroupId : ""),
                                      ProductId = (es != null ? es.Key.ProductId : ""),
                                      //TerritoryCode = (es != null ? es.Key.TerritoryId : ""),
                                      TerritoryCode = e.TerritoryCode,
                                      SubAreaId = e.SubAreaCode,
                                      AreaCode = e.AreaCode,
                                      Area = e.Area,
                                      BU = e.BU
                                  }).ToArray();

            var employee_sales_targets = employee_sales.Union(employee_targets).Union(employee_sales_1); //UnComment out this to align with BF IT

            var employee_sales_target_result_0 = (from e in employee_sales_targets.Distinct()
                                                  join t in targetSum on new { Territory = e.TerritoryCode, Month = e.Month,
                                                      ProductGroupId = e.ProductGroupId, ProductId = e.ProductId, HCOId = e.HCOId } equals
                                                  new { Territory = t.Key.TerritoryId, Month = t.Key.Month,
                                                      ProductGroupId = t.Key.ProductGroupId,
                                                      ProductId = t.Key.ProductId,
                                                      HCOId = t.Key.HCOId
                                                  } into employee_target
                                                  from et in employee_target.DefaultIfEmpty()
                                                  select new
                                                  {
                                                      BU = e.BU,
                                                      EmployeeCode = e.EmployeeCode,
                                                      EmployeeName = e.EmployeeName,
                                                      SubAreadId = e.SubAreaId,
                                                      AreaCode = e.AreaCode,
                                                      Area = e.Area,
                                                      OnboardDate = e.OnboardDate,
                                                      OffboardDate = e.OffboardDate,
                                                      HCOId = e.HCOId,
                                                      Month = e.Month,
                                                      ProductGroupId = e.ProductGroupId,
                                                      ProductId = e.ProductId,
                                                      TerritoryCode = e.TerritoryCode,
                                                      Target = (et != null ? et.TotalTarget : 0)
                                                  }).ToArray();

            var employee_sales_target_result_1 = from e in employee_sales_target_result_0
                                                 join t in salesResults on new {
                                                     EmployeeCode = e.EmployeeCode, Month = e.Month,
                                                     ProductGroupId = e.ProductGroupId, ProductId = e.ProductId, HCOId = e.HCOId,
                                                     TerritoryId = e.TerritoryCode}
                                                    equals
                                                   new
                                                   {
                                                       EmployeeCode = t.Key.MRCode,
                                                       Month = t.Key.Month,
                                                       ProductGroupId = t.Key.ProductGroupId,
                                                       ProductId = t.Key.ProductId,
                                                       HCOId = t.Key.HCOId,
                                                       TerritoryId = t.Key.TerritoryId
                                                   } into employee_target_1
                                                 from et in employee_target_1.DefaultIfEmpty()
                                                 select new
                                                 {
                                                     BU = e.BU,
                                                     EmployeeCode = e.EmployeeCode,
                                                     EmployeeName = e.EmployeeName,
                                                     SubAreaId = e.SubAreadId,
                                                     AreaId = e.AreaCode,
                                                     Area = e.Area,
                                                     HCOId = e.HCOId,
                                                      OnboardDate = e.OnboardDate,
                                                      OffboardDate = e.OffboardDate,
                                                     Month = e.Month,
                                                     ProductGroupId = e.ProductGroupId,
                                                     ProductId = e.ProductId,
                                                     TerritoryCode = e.TerritoryCode,
                                                     Target = e.Target,
                                                     Credit = (et != null ? et.TotalCredit : 0),
                                                     ThisQuarterCredit = (et != null ? et.ThisQuarterTotalCredit : 0),
                                                     LastQuarterCredit = (et != null ? et.LastQuarterTotalCredit : 0),
                                                     SalesQty = (et != null? et.TotalSalesQty : 0)
                                                 };

            var sales_hco_potential = (from s in sales
                                       group s by new { HCOId = s.HCOId, Month = s.Month } into temp
                                       select new { HCOId = temp.Key.HCOId, Month = temp.Key.Month, Potential = temp.Max(x=>x.Potential) }).Distinct().ToArray();

            var employee_sales_target_result_2 = (from e in employee_sales_target_result_1
                                                  join t in sales_hco_potential on new
                                                  {
                                                      HCOId = e.HCOId,
                                                      Month = e.Month
                                                  }
                                                     equals
                                                    new
                                                    {
                                                        HCOId = t.HCOId,
                                                        Month = t.Month
                                                    } into employee_target_2
                                                  from et in employee_target_2.DefaultIfEmpty()
                                                  select new
                                                  {
                                                      BU = e.BU,
                                                      EmployeeCode = e.EmployeeCode,
                                                      EmployeeName = e.EmployeeName,
                                                      SubAreaId = e.SubAreaId,
                                                      AreaId = e.AreaId,
                                                      Area = e.Area,
                                                      HCOId = e.HCOId,
                                                      OnboardDate = e.OnboardDate,
                                                      OffboardDate = e.OffboardDate,
                                                      Month = e.Month,
                                                      ProductGroupId = e.ProductGroupId,
                                                      ProductId = e.ProductId,
                                                      TerritoryCode = e.TerritoryCode,
                                                      Target = e.Target,
                                                      Credit = e.Credit,
                                                      LastQuarterCredit = e.LastQuarterCredit,
                                                      ThisQuarterCredit = e.ThisQuarterCredit,
                                                      Potential = (et != null ? et.Potential : 0),
                                                      SalesQty = e.SalesQty
                                                  }).ToArray();
            
            var employee_sales_target_result_3 = (from e in employee_sales_target_result_2
                                                 join t in employees on new
                                                 {
                                                    // TerritoryCode = e.TerritoryCode,
                                                     Month = e.Month,
                                                     EmployeeCode = e.EmployeeCode
                                                 }
                                                    equals
                                                   new
                                                   {
                                                    // TerritoryCode = t.TerritoryCode,
                                                     Month = t.Month,
                                                     EmployeeCode = t.EmployeeCode
                                                   } into employee_target_2
                                                 from et in employee_target_2.DefaultIfEmpty()
                                                 select new
                                                 {
                                                     BU = e.BU,
                                                     EmployeeCode = e.EmployeeCode,
                                                     EmployeeName = e.EmployeeName,
                                                     ParentTerritoryCode = (et != null ? et.ParentTerritoryCode : ""),
                                                     SubArea = (et != null ? et.SubArea : ""),
                                                     SubAreaId = e.SubAreaId,
                                                     AreaId = e.AreaId,
                                                     Area = e.Area,
                                                     HCOId = e.HCOId,
                                                     OnboardDate = e.OnboardDate,
                                                     OffboardDate = e.OffboardDate,
                                                     Month = e.Month,
                                                     ProductGroupId = e.ProductGroupId,
                                                     ProductId = e.ProductId,
                                                     TerritoryCode = e.TerritoryCode,
                                                     Target = e.Target,
                                                     Credit = e.Credit,
                                                     LastQuarterCredit = e.LastQuarterCredit,
                                                     ThisQuarterCredit = e.ThisQuarterCredit,
                                                     Potential = e.Potential,
                                                     SalesQty = e.SalesQty
                                                 }).ToArray();


            //var employee_filtered = from Employee e in employees
            //                        where e.Position == "学术专员"
            //                        select new { e.EmployeeCode, e.EmployeeId, e.Month, e.Position, e.TerritoryCode };
            //                       join t in targetSum on new { Territory = e.TerritoryCode, Month = e.Month } equals
            //                       new { Territory = t.Key.TerritoryId, Month = t.Key.Month } into employee_target
            //                       from et in employee_target.DefaultIfEmpty()
            //                       select new
            //                       {
            //                           HCOId = (et != null? et.Key.HCOId : ""),
            //                           Month = (et != null? et.Key.Month : 0),
            //                           EmployeeCode = e.EmployeeCode,
            //                           ProductGroupId = (et != null? et.Key.ProductGroupId : ""),
            //                           ProductId = (et != null? et.Key.ProductGroupId : ""),
            //                           TerritoryCode = (et != null ? et.Key.TerritoryId : "")
            //                       }).ToArray();

            //var employee_targets = (from Employee e in employees
            //                       join t in targetSum on new { Territory = e.TerritoryCode, Month = e.Month } equals
            //                       new { Territory = t.Key.TerritoryId, Month = t.Key.Month } into employee_target
            //                       from et in employee_target.DefaultIfEmpty()
            //                       select new
            //                       {
            //                           HCOId = (et != null? et.Key.HCOId : ""),
            //                           Month = (et != null? et.Key.Month : 0),
            //                           EmployeeCode = e.EmployeeCode,
            //                           ProductGroupId = (et != null? et.Key.ProductGroupId : ""),
            //                           ProductId = (et != null? et.Key.ProductGroupId : ""),
            //                           TerritoryCode = (et != null ? et.Key.TerritoryId : "")
            //                       }).ToArray();


            //var salesTargetResult = from et in employee_targets 
            //                        join s in salesResults on new { Territory = et.TerritoryCode, Month = et.Month, EmployeeCode = et.EmployeeCode,
            //                            HCOId = et.HCOId, ProductGroupId = et.ProductGroupId,
            //                            ProductId = et.ProductId }
            //                         equals new { Territory = s.Key.TerritoryId, Month = s.Key.Month, EmployeeCode = s.Key.MRCode,
            //                             HCOId = s.Key.HCOId, ProductGroupId = s.Key.ProductGroupId,
            //                             ProductId = s.Key.ProductId } into employee_sales
            //                        from es in employee_sales.DefaultIfEmpty()
            //                       select new {HCOId = es.Key.HCOId, Month = es.Key.Month, Year = es.Key.Year, EmployeeCode = et.EmployeeCode
            //                       , ProductGroupId = es.Key.ProductGroupId, ProductId = es.Key.ProductGroupId, TerritoryCode = et.Key.TerritoryId,
            //                       TotalCredit = es.TotalCredit };

            foreach (var s in employee_sales_target_result_3.Distinct() )
            {
                if (!s.HCOId.Equals(string.Empty))
                {
                    MRSalesTarget target = new MRSalesTarget();
                    target.HCOId = s.HCOId;
                    target.Potential = s.Potential??0;
                    target.ParentTerritoryCode = s.ParentTerritoryCode;
                    target.Month = s.Month;
                    target.Quarter = DateTimeUtility.GetQuarterString(target.Month);
                    target.BU = s.BU;
                    target.EmployeeCode = s.EmployeeCode;
                    target.EmployeeName = s.EmployeeName;
                    target.ProductGroupId = s.ProductGroupId;
                    target.OnboardDate = s.OnboardDate;
                    target.OffboardDate = s.OffboardDate;
                    target.ProductId = s.ProductId;
                    target.TerritoryCode = s.TerritoryCode;
                    target.Credit = Math.Round(s.Credit, 2);
                    target.SubAreaId = s.SubAreaId;
                    target.IsOnDuty = BusinessInfoHelper.IsOndutyThisMonth(s.Month, s.OnboardDate, s.OffboardDate);
                     target.Target = BusinessInfoHelper.CalculateTarget(s.Month,s.OnboardDate,s.OffboardDate,s.Target);//Math.Round(s.Target, 2);
                    if (target.IsOnDuty == 0)
                        target.Target = 0;
                   target.AreaId = s.AreaId;
                    target.SubArea = s.SubArea;
                    target.Area = s.Area;
                    target.Position = "学术专员";
                    target.ThisQuarterCredit = s.ThisQuarterCredit;
                    target.LastQuarterCredit = s.LastQuarterCredit;
                    target.SalesQty = s.SalesQty;

                    target.OnboardMonths = DateTimeUtility.GetOnboardMonths(target.OnboardDate);

                    if(s.BU.Equals(string.Empty))
                    {
                        Employee offboardEmployee = BusinessInfoHelper.GetLatestEmployeeInfo(s.EmployeeCode, employees);
                        if(offboardEmployee != null)
                        {
                            target.BU = offboardEmployee.BU;
                            target.AreaId = offboardEmployee.AreaCode;
                            target.Area = offboardEmployee.Area;
                            target.SubArea = offboardEmployee.SubArea;
                            target.SubAreaId = offboardEmployee.SubAreaCode;
                            target.EmployeeName = offboardEmployee.EmployeeName;
                            target.OnboardDate = offboardEmployee.OnboardDate;
                            target.OffboardDate = offboardEmployee.OffboardDate;
                            target.ParentTerritoryCode = offboardEmployee.ParentTerritoryCode;
                        }
                    }
                    if(s.EmployeeName != string.Empty)
                        mRSalesTargets.Add(target);
                }
            }

            return mRSalesTargets;
        }



    }

    public class DateTimeUtility
    {
        /// <summary>
        /// Month parameter is in the format of '201712'
        /// </summary>
        /// <param name="month"></param>
        /// <returns></returns>
        public static string GetQuarterString(int month)
        {
            string result = "";
            int year = month / 100;
            month = month % 100;
            int quarter = (month - 1) / 3 + 1;

            result = year.ToString() + "Q" + quarter.ToString();
            return result;
        }
        public static DateTime GetDate(int month)
        {
            int year = month / 100;
            int month0 = month % 100;

            return new DateTime(year,month0,1);
        }
        public static int GetMonthInt(DateTime date)
        {
            int result = 0;

            result = date.Year * 100 + date.Month;

            return result;
        }
        public static string GetNextQuarterString(string quarterString)
        {
            string nextQuarterString = "";

            int year = Int32.Parse(quarterString.Substring(0, 4));
            int quarterNumber = Int32.Parse(quarterString.Substring(5, 1));

            if(quarterNumber > 3)
            {
                year = year + 1;
                quarterNumber = 1;
            }
            else
            {
                quarterNumber++;
            }

            nextQuarterString = year.ToString() + "Q" + quarterNumber.ToString();
            return nextQuarterString;
        }
        public static string GetQuarterString(DateTime date )
        {
            string result = "";
            int year = date.Year;
            int month = date.Month;
            int quarter = (month - 1) / 3 + 1;

            result = year.ToString() + "Q" + quarter.ToString();

            return result;
        }
        /// <summary>
        /// Get interval in month between start month and calculating quarter
        /// </summary>
        /// <param name="startMonth">201808</param>
        /// <param name="calculatingQuarter">2018Q1</param>
        /// <returns></returns>
        public static int GetMonthInterval(int start, string calculatingQuarter)
        {
            int interval = 0;
            int startYear = start / 100;
            int startMonth = start % 100; 

            CalculatingTimeFrame calculatingTimeFrame = GetCalculatingTimeFrame(calculatingQuarter);
            DateTime endTime = calculatingTimeFrame.ThisQuarterStartDate;

            DateTime startTime = new DateTime(startYear, startMonth, 1);

            interval = (endTime.Year - startTime.Year)*12 + endTime.Month - startTime.Month;

            return interval;
        }
        public static int GetMonthInterval(DateTime start, DateTime end)
        {
            int interval = 0;

            if(start.Day <= end.Day)
                interval = (end.Year - start.Year) * 12 + end.Month - start.Month;
            else
                interval = (end.Year - start.Year) * 12 + end.Month - start.Month - 1;

            return interval;
        }
        public static int GetMonthInterval(int start, int end)
        {
            int interval = 0;
            int startYear = start / 100;
            int startMonth = start % 100;
            int endYear = end / 100;
            int endMonth = end % 100;

            DateTime startTime = new DateTime(startYear, startMonth, 1);
            DateTime endTime = new DateTime(endYear, endMonth, 1);

            interval = (endTime.Year - startTime.Year)*12 + endTime.Month - startTime.Month;

            return interval;
        }
        public static double GetOnboardMonths(DateTime? onboardDate)
        {
            double result = 0d;

            if (onboardDate == null) return 0d;

            DateTime calculatingDate = new DateTime(2018, 6, 30);//hard-code
            result = (calculatingDate.Year - onboardDate.Value.Year) * 12 + (calculatingDate.Month - onboardDate.Value.Month);
            return result;
        }

        public static int GetQuarterInterval(string startQuarterString, string endQuarterString)
        {
            int result = 0;

            int startYear = Int32.Parse(startQuarterString.Substring(0, 4));
            int startQuarter= Int32.Parse(startQuarterString.Substring(5, 1));

            int endYear = Int32.Parse(endQuarterString.Substring(0, 4));
            int endQuarter= Int32.Parse(endQuarterString.Substring(5, 1));

            result = (endYear - startYear) * 4 + endQuarter - startQuarter;
            return result;
        }
        /// <summary>
        /// There is no validation for the parameter of calculatingQuarter. This parameter should be in the format of '2018Q1'.
        /// </summary>
        /// <param name="calculatingQuarter"></param>
        /// <returns></returns>
        public static CalculatingTimeFrame GetCalculatingTimeFrame(string calculatingQuarter)
        {
            CalculatingTimeFrame result = new CalculatingTimeFrame();
            
            result.ThisYear = Int32.Parse(calculatingQuarter.Substring(0, 4));
            int quarterNumber = Int32.Parse(calculatingQuarter.Substring(5, 1));
            result.LastYear = result.ThisYear - 1;
            result.TotalYTDMonth = quarterNumber * 3;

            result.ThisQuarterStartDate = new DateTime(result.ThisYear, (quarterNumber - 1) * 3 + 1, 1);
            result.ThisQuarterEndDate = result.ThisQuarterStartDate.AddMonths(2);
            result.LastTwoQuarterStartDate = result.ThisQuarterStartDate.AddMonths(-6);
            result.LastTwoQuarterEndDate = result.ThisQuarterStartDate.AddMonths(-1);
            result.LastQuarterStartDate = result.ThisQuarterStartDate.AddMonths(-3);
            result.LastQuarterEndDate = result.ThisQuarterStartDate.AddMonths(-1);


            result.ThisQuarterStartMonth = Int32.Parse(result.ThisQuarterStartDate.ToString("yyyyMM"));
            result.ThisQuarterEndMonth = Int32.Parse(result.ThisQuarterEndDate.ToString("yyyyMM"));

            result.LastTwoQuarterStartMonth = Int32.Parse(result.LastTwoQuarterStartDate.ToString("yyyyMM"));
            result.LastTwoQuarterEndMonth = Int32.Parse(result.LastTwoQuarterEndDate.ToString("yyyyMM"));

            result.LastQuarterStartMonth = Int32.Parse(result.LastQuarterStartDate.ToString("yyyyMM"));
            result.LastQuarterEndMonth = Int32.Parse(result.LastTwoQuarterEndDate.ToString("yyyyMM"));

            int startMonthLastTwoQuarter = (quarterNumber - 1) * 3 + 1;

            return result;
        }
    }
    public class CalculatingTimeFrame
    {
        public int ThisYear;
        public int ThisQuarter;
        public int LastYear;
        public int TotalYTDMonth;

        public DateTime ThisQuarterStartDate;
        public DateTime ThisQuarterEndDate;
        public DateTime LastTwoQuarterStartDate;
        public DateTime LastTwoQuarterEndDate;
        public DateTime LastQuarterStartDate;
        public DateTime LastQuarterEndDate;

        public int ThisQuarterStartMonth;
        public int ThisQuarterEndMonth;
        public int LastQuarterStartMonth;
        public int LastQuarterEndMonth;
        public int LastTwoQuarterStartMonth;
        public int LastTwoQuarterEndMonth;
    }
    public class MRSalesTargetClassMap : ClassMap<MRSalesTarget>
    {
        public MRSalesTargetClassMap()
        {
            Map(m => m.Month).Name("记录月度");
            Map(m => m.Quarter).Name("季度");
            Map(m => m.EmployeeCode).Name("员工编码");
            Map(m => m.EmployeeName).Name("员工姓名");
            Map(m => m.EmployeeId).Name("员工ID");
            Map(m => m.OnboardDate).Name("入职日期");
            Map(m => m.OffboardDate).Name("离职日期");
            Map(m => m.IsOnDuty).Name("本月是否在岗");
            Map(m => m.OnboardMonths).Name("入职月数");
            Map(m => m.TerritoryCode).Name("岗位码");
            Map(m => m.Position).Name("职位");
            Map(m => m.ParentTerritoryCode).Name("上级岗位码");
            Map(m => m.ParentId).Name("上级ID");
            Map(m => m.Area).Name("省区");
            Map(m => m.AreaId).Name("省区Code");
            Map(m => m.HCOId).Name("终端编码");
            Map(m => m.Level).Name("行政级别");
            Map(m => m.Potential).Name("医院潜力级别");
            Map(m => m.ProductGroupId).Name("产品系列ID");
            Map(m => m.ProductGroup).Name("产品系列");
            Map(m => m.ProductId).Name("产品品规ID");
            Map(m => m.Product).Name("产品品规");
            Map(m => m.Credit).Name("积分");
            Map(m => m.SalesQty).Name("销量");
            Map(m => m.ThisQuarterCredit).Name("本季度标准积分");
            Map(m => m.LastQuarterCredit).Name("上季度标准积分");
            Map(m => m.Target).Name("指标额");
            Map(m => m.BU).Name("BU");
            Map(m => m.SubArea).Name("办事处");
            Map(m => m.SubAreaId).Name("办事处ID");

        }
    }

    public class Employee
    {
        public int Month { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public int EmployeeId { get; set; }
        public string BU { get; set; }
        public string SubAreaCode { get; set; }
        public string SubArea{ get; set; }
        public string AreaCode { get; set; }
        public string Area { get; set; }
        public string RegionCode { get; set; }
        public string Region { get; set; }
        public string Position { get; set; }
        public string TerritoryCode { get; set; }
        public string ParentTerritoryCode { get; set; }
        public DateTime? OnboardDate { get; set; }
        public DateTime? OffboardDate { get; set; }
 
    }
    public class EmployeeClassMap : ClassMap<Employee>
    {
        public EmployeeClassMap()
        {
            Map(m => m.Month).Name("BONUS_IN");
            Map(m => m.EmployeeCode).Name("EMPLOYEE_NO");
            Map(m => m.EmployeeName).Name("USERNAME");
            Map(m => m.EmployeeId).Name("USERID");
            Map(m => m.BU).Name("DISTRICT_NAME");
            Map(m => m.SubAreaCode).Name("SUB_AREA_CODE");
            Map(m => m.SubArea).Name("SUBAREANAME");
            Map(m => m.AreaCode).Name("AREA_CODE");
            Map(m => m.Area).Name("AREANAME");
            Map(m => m.RegionCode).Name("REGION_CODE");
            Map(m => m.Region).Name("REGION_NAME");
            Map(m => m.TerritoryCode).Name("JOB_CODE");
            Map(m => m.ParentTerritoryCode).Name("LEADER_JOB_CODE");
            Map(m => m.Position).Name("JOB_NAME");
            Map(m => m.OnboardDate).Name("JOIN_DATE").TypeConverterOption.NullValues(string.Empty); 
            Map(m => m.OffboardDate).Name("LEAVE_DATE").TypeConverterOption.NullValues(string.Empty); 
        }
    }
   public class RSMSalesTarget
    {
        public int Month { get; set; }
        public string EmployeeCode { get; set; }
        public string Quarter{ get; set; }
        public int EmployeeId { get; set; }
        public string HCOId { get; set; }
        public string Level { get; set; }
        public int Potential { get; set; }
        public string ProductGroupId { get; set; }
        public string ProductGroup { get; set; }
        public string ProductId { get; set; }
        public string Product { get; set; }
        public double Credit { get; set; }
        public double OnboardMonths { get; set; }
        public double Target { get; set; }
        public string TerritoryCode { get; set; }
        public string AreaId { get; set; }
        public string RegionId { get; set; }
        public DateTime? OnboardDate { get; set; }
        public DateTime? OffboardDate { get; set; }

        public static List<RSMSalesTarget> Process(IEnumerable<Sales> sales, IEnumerable<Employee> employees)
        {
            List<RSMSalesTarget> dSMSalesTargets = new List<RSMSalesTarget>();

            var salesList = (from Sales s in sales
                                group s by new { s.RegionId,  s.Year, s.Month, s.ProductGroupId, s.ProductId, s.HCOId } into g
                                select new { g.Key, TotalCredit = g.Sum(p => p.Credit) });
            var RSMSalesList = from s in salesList
                               join e in employees on new { Key = s.Key.RegionId, Month = s.Key.Month } equals new { Key = e.RegionCode, Month = e.Month }
                               where e.Position == "大区经理"
                               select new { e.EmployeeCode,e.TerritoryCode, e.EmployeeId, e.OnboardDate,e.OffboardDate, s.Key.RegionId,s.Key.Month, s.Key.HCOId, s.Key.ProductGroupId, s.Key.ProductId, s.TotalCredit };

            foreach(var d in RSMSalesList)
            {
                RSMSalesTarget target = new RSMSalesTarget();
                target.EmployeeCode = d.EmployeeCode;
                target.EmployeeId = d.EmployeeId;
                target.TerritoryCode = d.TerritoryCode;
                target.Month = d.Month;
                target.Quarter = DateTimeUtility.GetQuarterString(d.Month);
                target.HCOId = d.HCOId;
                target.ProductGroupId = d.ProductGroupId;
                target.RegionId = d.RegionId;
                target.ProductId = d.ProductId;
                target.Credit = d.TotalCredit;
                target.OnboardDate = d.OnboardDate;
                target.OffboardDate = d.OffboardDate;
                target.OnboardMonths = DateTimeUtility.GetOnboardMonths(target.OnboardDate);

                dSMSalesTargets.Add(target);
            }

            return dSMSalesTargets;
        }
    }
    public class RSMSalesTargetMap : ClassMap<RSMSalesTarget>
    {
        public RSMSalesTargetMap()
        {
            Map(m => m.Month).Name("记录月度");
            Map(m => m.Quarter).Name("季度");
            Map(m => m.EmployeeCode).Name("员工编码");
            Map(m => m.EmployeeId).Name("员工ID");
            Map(m => m.HCOId).Name("终端编码");
            Map(m => m.Level).Name("行政级别");
            Map(m => m.Potential).Name("医院潜力级别");
            Map(m => m.ProductGroupId).Name("产品系列ID");
            Map(m => m.ProductGroup).Name("产品系列");
            Map(m => m.ProductId).Name("产品品规ID");
            Map(m => m.Product).Name("产品品规");
            Map(m => m.Credit).Name("积分");
            Map(m => m.Target).Name("指标额");
            Map(m => m.OnboardMonths).Name("入职月数");
            Map(m => m.TerritoryCode).Name("岗位码");
            Map(m => m.AreaId).Name("省区ID");
            Map(m => m.RegionId).Name("子大区ID");
            Map(m => m.OnboardDate).Name("入职日期").TypeConverterOption.NullValues(string.Empty); 
            Map(m => m.OffboardDate).Name("离职日期").TypeConverterOption.NullValues(string.Empty); 
        }
    }

   public class DSMSalesTarget
    {
        public int Month { get; set; }
        public string EmployeeCode { get; set; }
        public string Quarter{ get; set; }
        public int EmployeeId { get; set; }
        public string SubAreaCode { get; set; }
        public string HCOId { get; set; }
        public string Level { get; set; }
        public int Potential { get; set; }
        public string ProductGroupId { get; set; }
        public string ProductGroup { get; set; }
        public string ProductId { get; set; }
        public string Product { get; set; }
        public double Credit { get; set; }
        public double OnboardMonths { get; set; }
        public double Target { get; set; }
        public string TerritoryCode { get; set; }
        public string AreaId { get; set; }
        public DateTime? OnboardDate { get; set; }
        public DateTime? OffboardDate { get; set; }

        public static List<DSMSalesTarget> Process(IEnumerable<Sales> sales, IEnumerable<Employee> employees)
        {
            List<DSMSalesTarget> dSMSalesTargets = new List<DSMSalesTarget>();

            var salesList = (from Sales s in sales
                                group s by new { s.AreaId, s.SubAreaId, s.Year, s.Month, s.ProductGroupId, s.ProductId, s.HCOId } into g
                                select new { g.Key, TotalCredit = g.Sum(p => p.Credit) });
            var DSMSalesList = from s in salesList
                               join e in employees on new { Key = s.Key.SubAreaId, Month = s.Key.Month } equals new { Key = e.SubAreaCode, Month = e.Month }
                               where e.Position == "地区经理"
                               select new { e.EmployeeCode,e.TerritoryCode, e.EmployeeId, e.SubAreaCode,e.OnboardDate,e.OffboardDate, s.Key.AreaId,s.Key.Month, s.Key.HCOId, s.Key.ProductGroupId, s.Key.ProductId, s.TotalCredit };

            foreach(var d in DSMSalesList)
            {
                DSMSalesTarget target = new DSMSalesTarget();
                target.EmployeeCode = d.EmployeeCode;
                target.EmployeeId = d.EmployeeId;
                target.TerritoryCode = d.TerritoryCode;
                target.Month = d.Month;
                target.Quarter = DateTimeUtility.GetQuarterString(d.Month);
                target.HCOId = d.HCOId;
                target.AreaId  = d.AreaId;
                target.ProductGroupId = d.ProductGroupId;
                target.ProductId = d.ProductId;
                target.Credit = d.TotalCredit;
                target.SubAreaCode = d.SubAreaCode;
                target.OnboardDate = d.OnboardDate;
                target.OffboardDate = d.OffboardDate;
                target.OnboardMonths = DateTimeUtility.GetOnboardMonths(target.OnboardDate);

                dSMSalesTargets.Add(target);
            }

            return dSMSalesTargets;
        }
    }
    public class DSMSalesTargetMap : ClassMap<DSMSalesTarget>
    {
        public DSMSalesTargetMap()
        {
            Map(m => m.Month).Name("记录月度");
            Map(m => m.Quarter).Name("季度");
            Map(m => m.EmployeeCode).Name("员工编码");
            Map(m => m.EmployeeId).Name("员工ID");
            Map(m => m.HCOId).Name("终端编码");
            Map(m => m.Level).Name("行政级别");
            Map(m => m.Potential).Name("医院潜力级别");
            Map(m => m.ProductGroupId).Name("产品系列ID");
            Map(m => m.ProductGroup).Name("产品系列");
            Map(m => m.ProductId).Name("产品品规ID");
            Map(m => m.Product).Name("产品品规");
            Map(m => m.Credit).Name("积分");
            Map(m => m.OnboardMonths).Name("入职月数");
            Map(m => m.Target).Name("指标额");
            Map(m => m.SubAreaCode).Name("办事处ID");
            Map(m => m.TerritoryCode).Name("岗位码");
            Map(m => m.AreaId).Name("省区ID");
            Map(m => m.OnboardDate).Name("入职日期").TypeConverterOption.NullValues(string.Empty); 
            Map(m => m.OffboardDate).Name("离职日期").TypeConverterOption.NullValues(string.Empty); 
        }
    }
    public class DSMTarget
    {
        public string Quarter { get; set; }
        public int Month { get; set; }
        public string EmployeeCode { get; set; }
        public int EmployeeId { get; set; }

        public string EmployeeName { get; set; }

        public DateTime? OnboardDate { get; set; }
        public DateTime? OffboardDate { get; set; }
        public string TerritoryCode { get; set; }

        public string Position { get; set; }
        public int IsOnduty { get; set; }
       
        public double OnboardMonths { get; set; }
        public string ManagerTerritoryCode { get; set; }
        public string ManagerId { get; set; }

        public string AreaCode { get; set; }
        public string Area { get; set; }
        public string BUCode { get; set; }
        public string BU { get; set; }

        public string SubAreaCode { get; set; }

        public string SubArea { get; set; }

        public string RegionCode { get; set; }
        public string Region { get; set; }
        public string ProductGroupId { get; set; }
        public string ProductId { get; set; }
        public double Target { get; set; }

        public static List<DSMTarget> Process(IEnumerable<SubAreaTarget> targets, IEnumerable<Employee> employees)
        {

            List<DSMTarget> dsmTarget = new List<DSMTarget>();

            var targetList = (from t in targets
                              //where t.SUB_AREA_CODE == "60107" && (t.Month == 5 || t.Month == 6) && t.YEAR == 2018
                             group t by new { t.DISTRICT_CODE, t.AREA_CODE, t.REGION_CODE,t.SUB_AREA_CODE, t.YEAR, t.Month, t.PRODS_NO, t.PROD_NO} into g
                             select new { g.Key, TotalTarget = g.Sum(p => p.PLAN_AMT) }).ToArray();
            var DSMTargetList = (from s in targetList
                               join e in employees on new { Key = s.Key.SUB_AREA_CODE, Month = s.Key.YEAR * 100 + s.Key.Month } equals new { Key = e.SubAreaCode, Month = e.Month }
                               where e.Position == "地区经理"
                                select new
                                { e.EmployeeCode,
                                    e.TerritoryCode,
                                    e.EmployeeId,
                                    e.EmployeeName,
                                    e.SubAreaCode,
                                    e.OnboardDate,
                                    e.OffboardDate,
                                    e.BU,
                                    e.Area,
                                    e.SubArea,
                                    s.Key.DISTRICT_CODE,
                                    s.Key.AREA_CODE,
                                    s.Key.REGION_CODE,
                                    s.Key.SUB_AREA_CODE,
                                    s.Key.YEAR,
                                    e.Month,
                                    s.Key.PRODS_NO,
                                    s.Key.PROD_NO,
                                    s.TotalTarget
                                }).ToArray();

            foreach (var d in DSMTargetList)
            {
                DSMTarget target = new DSMTarget();
                target.EmployeeCode = d.EmployeeCode;
                target.EmployeeId = d.EmployeeId;
                target.EmployeeName = d.EmployeeName;
                target.TerritoryCode = d.TerritoryCode;
                target.Month = d.Month;
                target.Quarter = DateTimeUtility.GetQuarterString(d.Month);
                target.BUCode = d.DISTRICT_CODE;
                target.BU = d.BU;
                target.RegionCode = d.REGION_CODE;
                target.AreaCode = d.AREA_CODE;
                target.Area = d.Area;
                target.SubAreaCode = d.SubAreaCode;
                target.SubArea = d.SubArea;
                target.ProductGroupId = d.PRODS_NO;
                target.ProductId = d.PROD_NO;
                target.OnboardDate = d.OnboardDate;
                target.OffboardDate = d.OffboardDate;
                target.OnboardMonths = DateTimeUtility.GetOnboardMonths(target.OnboardDate);
                target.IsOnduty = BusinessInfoHelper.IsOndutyThisMonth(target.Month, target.OnboardDate, target.OffboardDate);
                target.Target =  BusinessInfoHelper.CalculateTarget(target.Month,target.OnboardDate,target.OffboardDate,d.TotalTarget);
                
                dsmTarget.Add(target);
            }

            return dsmTarget;
        }
    }
    public class DSMTargetMap : ClassMap<DSMTarget>
    {
        public DSMTargetMap()
        {
            Map(m => m.Month).Name("记录月度");
            Map(m => m.Quarter).Name("季度");
            Map(m => m.EmployeeCode).Name("员工编码");
            Map(m => m.EmployeeName).Name("员工姓名");
            Map(m => m.EmployeeId).Name("员工ID");
            Map(m => m.Position).Name("职位");
            Map(m => m.IsOnduty).Name("本月是否在岗");
            Map(m => m.OnboardMonths).Name("入职月数");
            Map(m => m.ManagerId).Name("上级ID");
            Map(m => m.ManagerTerritoryCode).Name("上级岗位码");
            Map(m => m.ProductGroupId).Name("产品系列ID");
            Map(m => m.ProductId).Name("产品品规ID");
            Map(m => m.OnboardMonths).Name("入职月数");
            Map(m => m.Target).Name("指标额");
            Map(m => m.TerritoryCode).Name("岗位码");
            Map(m => m.AreaCode).Name("省区Code");
            Map(m => m.Area).Name("省区");
            Map(m => m.BUCode).Name("BUID");
            Map(m => m.BU).Name("BU");
            Map(m => m.Region).Name("子大区");
            Map(m => m.RegionCode).Name("子大区ID");
            Map(m => m.SubAreaCode).Name("办事处ID");
            Map(m => m.SubArea).Name("办事处");
            Map(m => m.OnboardDate).Name("入职日期").TypeConverterOption.NullValues(string.Empty);
            Map(m => m.OffboardDate).Name("离职日期").TypeConverterOption.NullValues(string.Empty);
        }
    }
    public class RSMTarget
    {
        public string Quarter { get; set; }
        public int Month { get; set; }
        public string EmployeeCode { get; set; }
        public int EmployeeId { get; set; }

        public string EmployeeName { get; set; }

        public DateTime? OnboardDate { get; set; }
        public DateTime? OffboardDate { get; set; }
        public string TerritoryCode { get; set; }

        public string Position { get; set; }
        public int IsOnduty { get; set; }

        public double OnboardMonths { get; set; }
        public string ManagerTerritoryCode { get; set; }
        public string ManagerId { get; set; }

        public string BUCode { get; set; }
        public string BU { get; set; }

        public string RegionCode { get; set; }
        public string Region { get; set; }
        public string ProductGroupId { get; set; }
        public string ProductId { get; set; }
        public double Target { get; set; }

        public static List<RSMTarget> Process(IEnumerable<AreaTarget> targets, IEnumerable<Employee> employees)
        {

            List<RSMTarget> dsmTarget = new List<RSMTarget>();

            var targetList = (from t in targets
                                  //where t.SUB_AREA_CODE == "60107" && (t.Month == 5 || t.Month == 6) && t.YEAR == 2018
                              group t by new { t.DISTRICT_CODE, t.AREA_CODE, t.REGION_CODE,  t.YEAR, t.Month, t.PRODS_NO, t.PROD_NO } into g
                              select new { g.Key, TotalTarget = g.Sum(p => p.PLAN_AMT) }).ToArray();
            var RSMTargetList = (from s in targetList
                                 join e in employees on new { Key = s.Key.REGION_CODE, Month = s.Key.YEAR * 100 + s.Key.Month } equals new { Key = e.RegionCode, Month = e.Month }
                                 where e.Position == "大区经理"
                                 select new
                                 {
                                     e.EmployeeCode,
                                     e.TerritoryCode,
                                     e.EmployeeId,
                                     e.EmployeeName,
                                     e.OnboardDate,
                                     e.OffboardDate,
                                     e.BU,
                                     e.Region,
                                     s.Key.DISTRICT_CODE,
                                     s.Key.REGION_CODE,
                                     s.Key.YEAR,
                                     e.Month,
                                     s.Key.PRODS_NO,
                                     s.Key.PROD_NO,
                                     s.TotalTarget
                                 }).ToArray();

            foreach (var d in RSMTargetList)
            {
                RSMTarget target = new RSMTarget();
                target.EmployeeCode = d.EmployeeCode;
                target.EmployeeId = d.EmployeeId;
                target.EmployeeName = d.EmployeeName;
                target.TerritoryCode = d.TerritoryCode;
                target.Month = d.Month;
                target.Quarter = DateTimeUtility.GetQuarterString(d.Month);
                target.BUCode = d.DISTRICT_CODE;
                target.BU = d.BU;
                target.RegionCode = d.REGION_CODE;
                target.ProductGroupId = d.PRODS_NO;
                target.Region = d.Region;
                target.ProductId = d.PROD_NO;
                target.OnboardDate = d.OnboardDate;
                target.OffboardDate = d.OffboardDate;
                target.OnboardMonths = DateTimeUtility.GetOnboardMonths(target.OnboardDate);
                target.IsOnduty = BusinessInfoHelper.IsOndutyThisMonth(target.Month, target.OnboardDate, target.OffboardDate);
                target.Target = BusinessInfoHelper.CalculateTarget(target.Month,target.OnboardDate,target.OffboardDate,d.TotalTarget);

                dsmTarget.Add(target);
            }

            return dsmTarget;
        }
    }
    public class RSMTargetMap : ClassMap<RSMTarget>
    {
        public RSMTargetMap()
        {
            Map(m => m.Month).Name("记录月度");
            Map(m => m.Quarter).Name("季度");
            Map(m => m.EmployeeCode).Name("员工编码");
            Map(m => m.EmployeeName).Name("员工姓名");
            Map(m => m.EmployeeId).Name("员工ID");
            Map(m => m.Position).Name("职位");
            Map(m => m.IsOnduty).Name("本月是否在岗");
            Map(m => m.OnboardMonths).Name("入职月数");
            Map(m => m.ManagerId).Name("上级ID");
            Map(m => m.ManagerTerritoryCode).Name("上级岗位码");
            Map(m => m.ProductGroupId).Name("产品系列ID");
            Map(m => m.ProductId).Name("产品品规ID");
            Map(m => m.OnboardMonths).Name("入职月数");
            Map(m => m.Target).Name("指标额");
            Map(m => m.TerritoryCode).Name("岗位码");
            Map(m => m.BUCode).Name("BUID");
            Map(m => m.BU).Name("BU");
            Map(m => m.Region).Name("子大区");
            Map(m => m.RegionCode).Name("子大区ID");
            Map(m => m.OnboardDate).Name("入职日期").TypeConverterOption.NullValues(string.Empty);
            Map(m => m.OffboardDate).Name("离职日期").TypeConverterOption.NullValues(string.Empty);
        }
    }

    public class DSMCredit
    {
        public string Quarter { get; set; }
        public int Month { get; set; }
        public string EmployeeCode { get; set; }
        public int EmployeeId { get; set; }

        public string EmployeeName { get; set; }

        public string TerritoryCode { get; set; }

        public string Position { get; set; }

        public string ManagerTerritoryCode { get; set; }
        public string ManagerId { get; set; }

        public string AreaCode { get; set; }
        public string Area { get; set; }

        public string SubAreaCode { get; set; }

        public string ProductGroupId { get; set; }
        public string ProductGroup { get; set; }
        public string ProductId { get; set; }
        public string Product { get; set; }

        public string HCOId { get; set; }
        public string Level { get; set; }
        public int? Potential { get; set; }
 
        public double Credit { get; set; }

        public static List<DSMCredit> Process(IEnumerable<Sales> sales, IEnumerable<Employee> employees, IEnumerable<MRTarget> MRTargets)
        {

            List<DSMCredit> dsmCredit = new List<DSMCredit>();

            var salesList = (from t in sales
                             where t.HospitalDistribution == 1 && t.Month > 201800
                              group t by new { t.SubAreaId, t.Year, t.Month, t.ProductGroupId, t.ProductId, t.HCOId,t.Potential} into g
                              select new { g.Key, TotalCredit = g.Sum(p => p.Credit) });
            var targetList = (from MRTarget t in MRTargets
                              //where t.SUB_AREA_CODE == "60314" && t.HCOId == "50908"
                              group t by new { t.SUB_AREA_CODE, t.YEAR, t.Month, t.HCOId, t.ProductGroupId, t.ProductId } into g
                             select new {g.Key, TotalTarget = g.Sum(p=>p.Target)}
                             ).ToList();
 
            var DSMSalesList = (from s in salesList
                                 join e in employees on new { Key = s.Key.SubAreaId, Month =s.Key.Month } equals new { Key = e.SubAreaCode, Month = e.Month }
                                 where e.Position == "地区经理"
                                 select new
                                 {
                                     EmployeeCode = e.EmployeeCode,
                                     TerritoryCode = e.TerritoryCode,
                                     EmployeeId = e.EmployeeId,
                                     EmployeeName = e.EmployeeName,
                                     SubAreaCode = e.SubAreaCode,
                                     SubArea = e.SubArea,
                                     Area = e.Area,
                                     AreaCode = e.AreaCode,
                                     Position = e.Position,
                                     ParentTerritoryCode = e.ParentTerritoryCode,
                                     ProductGroupId = s.Key.ProductGroupId,
                                     ProductId = s.Key.ProductId,
                                     Month = e.Month,
                                     HCOId = s.Key.HCOId,
                                    // Potential = s.Key.Potential,
                                     TotalCredit = s.TotalCredit
                                 }).ToArray();

           var zero_sales_list = (from t in targetList
                                  join s in salesList on new
                                  {
                                      SubAreaId = t.Key.SUB_AREA_CODE,
                                      Year = t.Key.YEAR,
                                      Month = t.Key.Month,
                                      HCOId = t.Key.HCOId,
                                      ProductGroupId = t.Key.ProductGroupId,
                                      ProductId = t.Key.ProductId
                                  } equals new
                                  {
                                      SubAreaId = s.Key.SubAreaId,
                                      Year = s.Key.Year,
                                      Month = s.Key.Month,
                                      HCOId = s.Key.HCOId,
                                      ProductGroupId = s.Key.ProductGroupId,
                                      ProductId = s.Key.ProductId
                                  } into sales_tmp
                                  from zs in sales_tmp.DefaultIfEmpty()
                                  where zs == null
                                  select
                                    new
                                    {
                                        SubAreaId = t.Key.SUB_AREA_CODE,
                                        Year = t.Key.YEAR,
                                        Month = t.Key.Month,
                                        HCOId = t.Key.HCOId,
                                        ProductGroupId = t.Key.ProductGroupId,
                                        ProductId = t.Key.ProductId,
                                        Credit = (zs != null ? zs.TotalCredit : 0d)
                                    }).ToList();
            var sales_hco_potential = (from s in sales
                                       group s by new { HCOId = s.HCOId, Month = s.Month } into temp
                                       select new { HCOId = temp.Key.HCOId, Month = temp.Key.Month, Potential = temp.Max(x => x.Potential) }).Distinct().ToArray();

            var DSMTargetList = (from s in zero_sales_list
                                 join e in employees on new { Key = s.SubAreaId, Month = s.Month } equals new { Key = e.SubAreaCode, Month = e.Month }
                                 //join h in sales_hco_potential on new { HCOId = s.HCOId, Month = s.Month }  equals new { HCOId = h.HCOId, Month = h.Month }
                                 where e.Position == "地区经理" 
                                 
                                 select new
                                 {
                                     EmployeeCode = e.EmployeeCode,
                                     TerritoryCode = e.TerritoryCode,
                                     EmployeeId = e.EmployeeId,
                                     EmployeeName = e.EmployeeName,
                                     SubAreaCode = e.SubAreaCode,
                                     SubArea = e.SubArea,
                                     Area = e.Area,
                                     AreaCode = e.AreaCode,
                                     Position = e.Position,
                                     ParentTerritoryCode = e.ParentTerritoryCode,
                                     ProductGroupId = s.ProductGroupId,
                                     ProductId = s.ProductId,
                                     Month = e.Month,
                                     HCOId = s.HCOId,
                                     //Potential = h.Potential,
                                     TotalCredit = 0d
                                 }).ToArray();


            foreach (var d in DSMSalesList.Union(DSMTargetList))
            {
                DSMCredit target = new DSMCredit();
                target.EmployeeCode = d.EmployeeCode;
                target.EmployeeId = d.EmployeeId;
                target.EmployeeName = d.EmployeeName;
                target.TerritoryCode = d.TerritoryCode;
                target.Month = d.Month;
                target.Quarter = DateTimeUtility.GetQuarterString(d.Month);
                target.AreaCode = d.AreaCode;
                target.Area = d.Area;
                target.SubAreaCode = d.SubAreaCode;
                target.ManagerTerritoryCode = d.ParentTerritoryCode;
                target.ProductGroupId = d.ProductGroupId;
                target.ProductId = d.ProductId;
                target.Credit = d.TotalCredit;
                target.Position = d.Position;
                target.HCOId = d.HCOId;
                //target.Potential = d.Potential;
                //if(target.Credit == 0)
                //{
                //    HCO h = BusinessInfoHelper.GetHCOInfobyQuarter(target.HCOId, sales, target.Quarter);
                //    target.Potential = h.Potential;
                //}

                dsmCredit.Add(target);
            }

            return dsmCredit;
        }
    }
    public class DSMCreditMap : ClassMap<DSMCredit>
    {
        public DSMCreditMap()
        {
            Map(m => m.Month).Name("记录月度");
            Map(m => m.Quarter).Name("季度");
            Map(m => m.EmployeeCode).Name("员工编码");
            Map(m => m.EmployeeName).Name("员工姓名");
            Map(m => m.EmployeeId).Name("员工ID");
            Map(m => m.Position).Name("职位");
            Map(m => m.ManagerId).Name("上级ID");
            Map(m => m.ManagerTerritoryCode).Name("上级岗位码");
            Map(m => m.ProductGroupId).Name("产品系列ID");
            Map(m => m.ProductGroup).Name("产品系列");
            Map(m => m.ProductId).Name("产品品规ID");
            Map(m => m.Product).Name("产品品规");
            Map(m => m.TerritoryCode).Name("岗位码");
            Map(m => m.AreaCode).Name("省区Code");
            Map(m => m.Area).Name("省区");
            Map(m => m.SubAreaCode).Name("办事处ID");
            
            Map(m => m.HCOId).Name("终端编码");
            Map(m => m.Level).Name("行政级别");
            Map(m => m.Potential).Name("医院潜力级别");
            Map(m => m.Credit).Name("积分");
        }
    }
    public class RSMCredit
    {
        public string Quarter { get; set; }
        public int Month { get; set; }
        public string EmployeeCode { get; set; }
        public int EmployeeId { get; set; }

        public string EmployeeName { get; set; }

        public string RegionCode { get; set; }
        public string TerritoryCode { get; set; }

        public string Position { get; set; }

        public string ManagerTerritoryCode { get; set; }
        public string ManagerId { get; set; }

        public string ProductGroupId { get; set; }
        public string ProductGroup { get; set; }
        public string ProductId { get; set; }
        public string Product { get; set; }

        public string HCOId { get; set; }
        public string Level { get; set; }
        public int? Potential { get; set; }

        public double Credit { get; set; }

        public static List<RSMCredit> Process(IEnumerable<Sales> sales, IEnumerable<Employee> employees, IEnumerable<MRTarget> MRTargets)
        {

            List<RSMCredit> rsmCredit = new List<RSMCredit>();

            var salesList = (from t in sales
                             where t.HospitalDistribution == 1 && t.Month > 201800
                             group t by new { t.RegionId, t.Year, t.Month, t.ProductGroupId, t.ProductId, t.HCOId, t.Potential } into g
                             select new { g.Key, TotalCredit = g.Sum(p => p.Credit) });
            var targetList = (from MRTarget t in MRTargets
                              group t by new { t.REGION_CODE, t.YEAR, t.Month, t.HCOId, t.ProductGroupId, t.ProductId } into g
                              select new { g.Key, TotalTarget = g.Sum(p => p.Target) }
                             ).ToList();

            var RSMSalesList = (from s in salesList
                                join e in employees on new { Key = s.Key.RegionId, Month = s.Key.Month } equals new { Key = e.RegionCode, Month = e.Month }
                                where e.Position == "大区经理"
                                select new
                                {
                                    EmployeeCode = e.EmployeeCode,
                                    TerritoryCode = e.TerritoryCode,
                                    EmployeeId = e.EmployeeId,
                                    EmployeeName = e.EmployeeName,
                                    RegionCode = e.RegionCode,
                                    Position = e.Position,
                                    ParentTerritoryCode = e.ParentTerritoryCode,
                                    ProductGroupId = s.Key.ProductGroupId,
                                    ProductId = s.Key.ProductId,
                                    Month = e.Month,
                                    HCOId = s.Key.HCOId,
                                    Potential = s.Key.Potential,
                                    TotalCredit = s.TotalCredit
                                }).ToArray();

            var zero_sales_list = (from t in targetList
                                   join s in salesList on new
                                   {
                                       RegionCode = t.Key.REGION_CODE,
                                       Year = t.Key.YEAR,
                                       Month = t.Key.Month,
                                       HCOId = t.Key.HCOId,
                                       ProductGroupId = t.Key.ProductGroupId,
                                       ProductId = t.Key.ProductId
                                   } equals new
                                   {
                                       RegionCode = s.Key.RegionId,
                                       Year = s.Key.Year,
                                       Month = s.Key.Month,
                                       HCOId = s.Key.HCOId,
                                       ProductGroupId = s.Key.ProductGroupId,
                                       ProductId = s.Key.ProductId
                                   } into sales_tmp
                                   from zs in sales_tmp.DefaultIfEmpty()
                                   where zs == null
                                   select
                                     new
                                     {
                                         RegionCode = t.Key.REGION_CODE,
                                         Year = t.Key.YEAR,
                                         Month = t.Key.Month,
                                         HCOId = t.Key.HCOId,
                                         ProductGroupId = t.Key.ProductGroupId,
                                         ProductId = t.Key.ProductId,
                                         Credit = (zs != null ? zs.TotalCredit : 0d)
                                     }).ToList();
            var sales_hco_potential = (from s in sales
                                       group s by new { HCOId = s.HCOId, Month = s.Month } into temp
                                       select new { HCOId = temp.Key.HCOId, Month = temp.Key.Month, Potential = temp.Max(x => x.Potential) }).Distinct().ToArray();

            var RSMTargetList = (from s in zero_sales_list
                                 join e in employees on new { Key = s.RegionCode, Month = s.Month } equals new { Key = e.RegionCode, Month = e.Month }
                                 join h in sales_hco_potential on new { HCOId = s.HCOId, Month = s.Month } equals new { HCOId = h.HCOId, Month = h.Month }
                                 where e.Position == "大区经理"
                                 select new
                                 {
                                     EmployeeCode = e.EmployeeCode,
                                     TerritoryCode = e.TerritoryCode,
                                     EmployeeId = e.EmployeeId,
                                     EmployeeName = e.EmployeeName,
                                     RegionCode = e.RegionCode,
                                     Position = e.Position,
                                     ParentTerritoryCode = e.ParentTerritoryCode,
                                     ProductGroupId = s.ProductGroupId,
                                     ProductId = s.ProductId,
                                     Month = e.Month,
                                     HCOId = s.HCOId,
                                     Potential = h.Potential,
                                     TotalCredit = 0d
                                 }).ToArray();


            foreach (var d in RSMSalesList.Union(RSMTargetList))
            {
                RSMCredit target = new RSMCredit();
                target.EmployeeCode = d.EmployeeCode;
                target.EmployeeId = d.EmployeeId;
                target.EmployeeName = d.EmployeeName;
                target.TerritoryCode = d.TerritoryCode;
                target.Month = d.Month;
                target.Quarter = DateTimeUtility.GetQuarterString(d.Month);
                target.RegionCode = d.RegionCode;
                target.ManagerTerritoryCode = d.ParentTerritoryCode;
                target.ProductGroupId = d.ProductGroupId;
                target.ProductId = d.ProductId;
                target.Credit = d.TotalCredit;
                target.Position = d.Position;
                target.HCOId = d.HCOId;
                target.Potential = d.Potential;
                //if(target.Credit == 0)
                //{
                //    HCO h = BusinessInfoHelper.GetHCOInfobyQuarter(target.HCOId, sales, target.Quarter);
                //    target.Potential = h.Potential;
                //}

                rsmCredit.Add(target);
            }

            return rsmCredit;
        }
    }
    public class RSMCreditMap : ClassMap<RSMCredit>
    {
        public RSMCreditMap()
        {
            Map(m => m.Month).Name("记录月度");
            Map(m => m.Quarter).Name("季度");
            Map(m => m.EmployeeCode).Name("员工编码");
            Map(m => m.EmployeeName).Name("员工姓名");
            Map(m => m.EmployeeId).Name("员工ID");
            Map(m => m.Position).Name("职位");
            Map(m => m.ManagerId).Name("上级ID");
            Map(m => m.ManagerTerritoryCode).Name("上级岗位码");
            Map(m => m.ProductGroupId).Name("产品系列ID");
            Map(m => m.ProductGroup).Name("产品系列");
            Map(m => m.ProductId).Name("产品品规ID");
            Map(m => m.Product).Name("产品品规");
            Map(m => m.TerritoryCode).Name("岗位码");
            Map(m => m.RegionCode).Name("子大区ID");

            Map(m => m.HCOId).Name("终端编码");
            Map(m => m.Level).Name("行政级别");
            Map(m => m.Potential).Name("医院潜力级别");
            Map(m => m.Credit).Name("积分");
        }
    }

    public class NewlyDevelopedHCO
    {
        public string EmployeeCode { get; set; }
        public string Quarter { get; set; }
        public string EmployeeId { get; set; }
        public string SubAreaCode { get; set; }
        public string HCOId { get; set; }
        public int? Potential { get; set; }
        public string ProductGroupId { get; set; }
        public string TerritoryCode { get; set; }
        public int StartSalesMonth { get; set; }
        public string RegionCode { get; set; }
        public bool IsNewlyDevelopedIn6Months { get; set; }
        public double AverageCredit { get; set; }

        public List<int> SalesMonthList = new List<int>();
        public List<int> StartSalesMonthList = new List<int>();
    }
    public class NewlyDevelopedHCOMap : ClassMap<NewlyDevelopedHCO>
    {
        public NewlyDevelopedHCOMap ()
        {
            Map(m => m.Quarter).Name("季度");
            Map(m => m.EmployeeCode).Name("员工编码");
            Map(m => m.EmployeeId).Name("员工ID");
            Map(m => m.HCOId).Name("终端编码");
            Map(m => m.Potential).Name("潜力等级");
            Map(m => m.ProductGroupId).Name("产品系列ID");
            Map(m => m.SubAreaCode).Name("办事处ID");
            Map(m => m.TerritoryCode).Name("岗位码");
            Map(m => m.StartSalesMonth).Name("月度");
            Map(m => m.RegionCode).Name("子大区ID");
            Map(m => m.IsNewlyDevelopedIn6Months).Name("是否半年内新开发医院");
        }
    }

    public class HCOSalesPerMonth
    {
        public string HCOId;
        public string ProductGroupId;
        public string SubAreaId;
        public int? Potential;
        public string TerritoryId;
        public int Month;
        public double Credit;
    }

    public class NewlyDevelopedHCOProcessor
    {
        public static List<NewlyDevelopedHCO> Process(IEnumerable<Sales> sales, IEnumerable<Employee> employees)
        {
            List<NewlyDevelopedHCO> newHCOs = new List<NewlyDevelopedHCO>();

            var salesList = (from Sales s in sales
                             where  //s.AreaId == "164" || s.AreaId == "165" || s.AreaId == "168" || s.AreaId == "87" || s.AreaId == "202" || s.AreaId == "85" || s.AreaId == "5" &&
                             s.HCOId != null
                             select new { HCOId = s.HCOId, ProductGroupId = s.ProductGroupId, SubAreaId = s.SubAreaId, Potential = s.Potential,
                                 TerritoryId = s.TerritoryId, Month = s.Month,
                                 ThisQuarterCredit = s.ThisQuarterCredit, RegionId = s.RegionId}).Distinct();
//user this quarter credit to avoid the credit change 
            var keyList = (from s in salesList
                          select new {s.HCOId, s.ProductGroupId}).Distinct();

            var salesMonthList = (from s1 in salesList
                          group s1 by new { s1.HCOId, s1.ProductGroupId, s1.Month } into g
                          select new { g.Key, TotalCredit = g.Sum(p => p.ThisQuarterCredit) } into keys
                          where keys.TotalCredit > 0d select keys).ToList();

            foreach (var d in keyList)
            {
                NewlyDevelopedHCO target = new NewlyDevelopedHCO();
                target.HCOId = d.HCOId;
                target.ProductGroupId = d.ProductGroupId;

                var monthList = from s2 in salesMonthList
                                where s2.Key.HCOId == d.HCOId && s2.Key.ProductGroupId == d.ProductGroupId
                                orderby s2.Key.Month ascending
                                select s2.Key.Month;

                int previousMonth = 0;
                int i = 0;
                foreach(var m in monthList)
                {
                    if(i == 0)
                    {
                        if (DateTimeUtility.GetMonthInterval(201610,m) >= 12)
                        {
                            target.StartSalesMonthList.Add(m);
                            target.Quarter = DateTimeUtility.GetQuarterString(m);
                        }
                    }
                    else if (i > 0) 
                    {
                        if (DateTimeUtility.GetMonthInterval(previousMonth,m) > 12)
                        {
                            target.StartSalesMonthList.Add(m);
                            target.Quarter = DateTimeUtility.GetQuarterString(m);
                        }
                    }

                    previousMonth = m;
                    i++;
                    target.SalesMonthList.Add(m);
                }

                newHCOs.Add(target);
            }

            List<NewlyDevelopedHCO> newHCOs_converted = new List<NewlyDevelopedHCO>();
            foreach(var newHCO in newHCOs)
            {
                foreach (var m1 in newHCO.StartSalesMonthList)
                {
                    NewlyDevelopedHCO h = new NewlyDevelopedHCO();
                    h.HCOId = newHCO.HCOId;
                    h.ProductGroupId = newHCO.ProductGroupId;
                    h.StartSalesMonth = m1;
                    h.Quarter = DateTimeUtility.GetQuarterString(m1);

                    newHCOs_converted.Add(h);
                }               

            }

            //var newHCOs_withEmployeeInfo = from h in newHCOs_converted
            //                               join s in salesList on new {HCOId = h.HCOId, ProductGroupId = h.ProductGroupId, Month = h.StartSalesMonth } equals new {HCOId = s.HCOId, ProductGroupId = s.ProductGroupId, Month = s.Month  }
            //                               join e in employees on new { TerritoryCode = s.TerritoryId, Month = s.Month } equals
            //                               new { TerritoryCode = e.TerritoryCode, Month = e.Month }
            //                               select new { h.HCOId, s.Potential, h.ProductGroupId, h.Quarter,
            //                                   h.StartSalesMonth, s.SubAreaId, s.TerritoryId, e.EmployeeCode, e.EmployeeId, s.RegionId };

            var newHCOs_withEmployeeInfo = from h in newHCOs_converted
                                           join s in sales on new {HCOId = h.HCOId, ProductGroupId = h.ProductGroupId, Month = h.StartSalesMonth } equals new {HCOId = s.HCOId, ProductGroupId = s.ProductGroupId, Month = s.Month  }
                                           select new { h.HCOId, s.Potential, h.ProductGroupId, h.Quarter,
                                               h.StartSalesMonth, s.SubAreaId, s.TerritoryId, s.MRCode, s.RegionId, s.MRId };

            List<NewlyDevelopedHCO> newHCOs_withEmployeeList = new List<NewlyDevelopedHCO>();

            foreach(var h in newHCOs_withEmployeeInfo.Distinct())
            {
                NewlyDevelopedHCO target = new NewlyDevelopedHCO();
                target.HCOId = h.HCOId;
                target.Potential = (h.Potential == null? 0 : h.Potential);
                target.SubAreaCode = h.SubAreaId;
                target.TerritoryCode = h.TerritoryId;
                target.ProductGroupId = h.ProductGroupId;
                target.StartSalesMonth = h.StartSalesMonth;
                target.RegionCode = h.RegionId;
                target.EmployeeCode = h.MRCode;
                target.EmployeeId = h.MRId;
                target.Quarter = "2018Q1"; // h.Quarter;
                target.IsNewlyDevelopedIn6Months = true;

                newHCOs_withEmployeeList.Add(target);

            }
            return newHCOs_withEmployeeList;
        }

        /// <summary>
        /// Calculating Quarter is in the format of 2018Q1 or 2018Q2.以销售日期按照月来统计销量判断新开发医院
        /// </summary>
        /// <param name="sales"></param>
        /// <param name="employees"></param>
        /// <param name="calculatingQuarter"></param>
        /// <returns></returns>
        public static List<NewlyDevelopedHCO> ProcessBySalesDate_Monthly(IEnumerable<Sales> sales0, IEnumerable<Employee> employees, string calculatingQuarter)
        {
            var sales = from s in sales0
                         where s.MRCode != string.Empty && s.HospitalDistribution == 1
                         select s;
            List < NewlyDevelopedHCO > newHCOs = GetNewlyDevlopedHCOs(sales, calculatingQuarter);

            //Consider the situation where multiple times of start sales month for one hco
            List<NewlyDevelopedHCO> newHCOs_converted = new List<NewlyDevelopedHCO>();
            foreach (var newHCO in newHCOs)
            {
                foreach (var m1 in newHCO.StartSalesMonthList)
                {
                    NewlyDevelopedHCO h = new NewlyDevelopedHCO();
                    h.HCOId = newHCO.HCOId;
                    h.ProductGroupId = newHCO.ProductGroupId;
                    h.StartSalesMonth = m1;
                    h.Quarter = DateTimeUtility.GetQuarterString(m1);

                    newHCOs_converted.Add(h);
                }

            }

            //var newHCOs_withEmployeeInfo = from h in newHCOs_converted
            //                               join s in salesList on new {HCOId = h.HCOId, ProductGroupId = h.ProductGroupId, Month = h.StartSalesMonth } equals new {HCOId = s.HCOId, ProductGroupId = s.ProductGroupId, Month = s.Month  }
            //                               join e in employees on new { TerritoryCode = s.TerritoryId, Month = s.Month } equals
            //                               new { TerritoryCode = e.TerritoryCode, Month = e.Month }
            //                               select new { h.HCOId, s.Potential, h.ProductGroupId, h.Quarter,
            //                                   h.StartSalesMonth, s.SubAreaId, s.TerritoryId, e.EmployeeCode, e.EmployeeId, s.RegionId };

            var newHCOs_withMoreInfo = from h in newHCOs_converted
                                       join s in sales on new { HCOId = h.HCOId, ProductGroupId = h.ProductGroupId, Quarter = h.Quarter }
                                           equals new { HCOId = s.HCOId, ProductGroupId = s.ProductGroupId, Quarter = DateTimeUtility.GetQuarterString(s.SalesDate) }
                                       select new
                                       {
                                           h.HCOId,
                                           s.Potential,
                                           h.ProductGroupId,
                                           h.Quarter,
                                           h.StartSalesMonth,
                                           s.SubAreaId,
                                           s.TerritoryId,
                                           s.RegionId,
                                       };

            //销售季度和确认流向的季度做匹配，eg.销售是9月份（Q3）发生的而且Q3确认的，则找到该新开发医院对应的Q3的跟催代表。
            //如果Q4才确认，则找不到对应的跟催代表，只能通过后面一步的特殊处理，才能找到后一个季度的跟催代表。

            var newHCOs_withEmployeeInfo = from h in newHCOs_withMoreInfo
                                           join s in sales on new { HCOId = h.HCOId, ProductGroupId = h.ProductGroupId, Quarter = h.Quarter }
                                           equals new { HCOId = s.HCOId, ProductGroupId = s.ProductGroupId, Quarter = DateTimeUtility.GetQuarterString(s.SalesDate) } into hco_employee_tmp
                                           from hco_employee in hco_employee_tmp.DefaultIfEmpty()
                                           select new
                                           {
                                               HCOId = h.HCOId,
                                               Potential = h.Potential,
                                               ProductGroupId = h.ProductGroupId,
                                               Quarter = h.Quarter,
                                               StartSalesMonth = h.StartSalesMonth,
                                               SubAreaId = h.SubAreaId,
                                               TerritoryId = h.TerritoryId,
                                               RegionId = h.RegionId,
                                               MRCode = (hco_employee == null ? "": hco_employee.MRCode),
                                               MRId = (hco_employee == null ? "": hco_employee.MRId)
                                           };
            //Consolidate the data and consider the situation where sales confirmed in the second quarter.
            
            var newHCOs_nextQuarterInfo = from h in newHCOs_withEmployeeInfo
                                           join s in sales on new { HCOId = h.HCOId, ProductGroupId = h.ProductGroupId}
                                           equals new { HCOId = s.HCOId, ProductGroupId = s.ProductGroupId}
                                           where DateTimeUtility.GetQuarterString(s.SalesDate) == DateTimeUtility.GetNextQuarterString(h.Quarter) && h.MRCode != s.MRCode 
                                           select new
                                           {
                                               HCOId = h.HCOId,
                                               Potential = h.Potential,
                                               ProductGroupId = h.ProductGroupId,
                                               Quarter = DateTimeUtility.GetNextQuarterString(h.Quarter),
                                               StartSalesMonth = h.StartSalesMonth,
                                               SubAreaId = h.SubAreaId,
                                               TerritoryId = s.TerritoryId,
                                               RegionId = h.RegionId,
                                               MRCode = s.MRCode,
                                               MRId = s.MRId
                                           };
            //2017Q4数据的特殊处理，因为Q4的新开发奖没有发,算在2018Q1中补发
            var newHCOs_2017Q4 = from h in newHCOs_withEmployeeInfo
                                          join s in sales on new { HCOId = h.HCOId, ProductGroupId = h.ProductGroupId}
                                           equals new { HCOId = s.HCOId, ProductGroupId = s.ProductGroupId}
                                           where DateTimeUtility.GetQuarterString(s.SalesDate) == DateTimeUtility.GetNextQuarterString(h.Quarter) && h.MRCode != s.MRCode && h.Quarter == "2017Q4"
                                           select new
                                           {
                                               HCOId = h.HCOId,
                                               Potential = h.Potential,
                                               ProductGroupId = h.ProductGroupId,
                                               Quarter = "2018Q1",
                                               StartSalesMonth = h.StartSalesMonth,
                                               SubAreaId = h.SubAreaId,
                                               TerritoryId = s.TerritoryId,
                                               RegionId = h.RegionId,
                                               MRCode = s.MRCode,
                                               MRId = s.MRId
                                           };

            List<NewlyDevelopedHCO> newHCOs_withEmployeeList = new List<NewlyDevelopedHCO>();

            foreach (var h in newHCOs_withEmployeeInfo.Union(newHCOs_nextQuarterInfo).Union(newHCOs_2017Q4).Distinct())
            {
                NewlyDevelopedHCO target = new NewlyDevelopedHCO();
                target.HCOId = h.HCOId;
                target.Potential = (h.Potential == null ? 0 : h.Potential);
                target.SubAreaCode = h.SubAreaId;
                target.TerritoryCode = h.TerritoryId;
                target.ProductGroupId = h.ProductGroupId;
                target.StartSalesMonth = h.StartSalesMonth;
                target.RegionCode = h.RegionId;
                target.EmployeeCode = h.MRCode;
                target.EmployeeId = h.MRId;
                target.Quarter = h.Quarter;
                target.IsNewlyDevelopedIn6Months = DateTimeUtility.GetMonthInterval(h.StartSalesMonth, calculatingQuarter) <= 5;

                newHCOs_withEmployeeList.Add(target);

            }
            return newHCOs_withEmployeeList;
        }
        
        /// <summary>
        /// Calcuate the newly devloped hco by day instead of months 0
        /// </summary>
        /// <param name="sales0"></param>
        /// <param name="employees"></param>
        /// <param name="calculatingQuarter"></param>
        /// <returns></returns>
        public static List<NewlyDevelopedHCO> ProcessBySalesDate(IEnumerable<Sales> sales0, IEnumerable<Employee> employees, string calculatingQuarter)
        {
            var sales = from s in sales0
                        where s.HospitalDistribution == 1
                        select s;
            List<NewlyDevelopedHCO> newHCOs = GetNewlyDevlopedHCOsByDay(sales, calculatingQuarter);

            //Consider the situation where multiple times of start sales month for one hco
            List<NewlyDevelopedHCO> newHCOs_converted = new List<NewlyDevelopedHCO>();
            foreach (var newHCO in newHCOs)
            {
                foreach (var m1 in newHCO.StartSalesMonthList)
                {
                    NewlyDevelopedHCO h = new NewlyDevelopedHCO();
                    h.HCOId = newHCO.HCOId;
                    h.ProductGroupId = newHCO.ProductGroupId;
                    h.StartSalesMonth = m1;
                    h.Quarter = DateTimeUtility.GetQuarterString(m1);

                    newHCOs_converted.Add(h);
                }

            }

            //var newHCOs_withEmployeeInfo = from h in newHCOs_converted
            //                               join s in salesList on new {HCOId = h.HCOId, ProductGroupId = h.ProductGroupId, Month = h.StartSalesMonth } equals new {HCOId = s.HCOId, ProductGroupId = s.ProductGroupId, Month = s.Month  }
            //                               join e in employees on new { TerritoryCode = s.TerritoryId, Month = s.Month } equals
            //                               new { TerritoryCode = e.TerritoryCode, Month = e.Month }
            //                               select new { h.HCOId, s.Potential, h.ProductGroupId, h.Quarter,
            //                                   h.StartSalesMonth, s.SubAreaId, s.TerritoryId, e.EmployeeCode, e.EmployeeId, s.RegionId };

            var newHCOs_withMoreInfo = from h in newHCOs_converted
                                       join s in sales on new { HCOId = h.HCOId, ProductGroupId = h.ProductGroupId, Quarter = h.Quarter }
                                           equals new { HCOId = s.HCOId, ProductGroupId = s.ProductGroupId, Quarter = DateTimeUtility.GetQuarterString(s.SalesDate) }
                                       select new
                                       {
                                           h.HCOId,
                                           s.Potential,
                                           h.ProductGroupId,
                                           h.Quarter,
                                           h.StartSalesMonth,
                                           s.SubAreaId,
                                           s.TerritoryId,
                                           s.RegionId,
                                       };

            //销售季度和确认流向的季度做匹配，eg.销售是9月份（Q3）发生的而且Q3确认的，则找到该新开发医院对应的Q3的跟催代表。
            //如果Q4才确认，则找不到对应的跟催代表，只能通过后面一步的特殊处理，才能找到后一个季度的跟催代表。

            var newHCOs_withEmployeeInfo = from h in newHCOs_withMoreInfo
                                           join s in sales on new { HCOId = h.HCOId, ProductGroupId = h.ProductGroupId, Quarter = h.Quarter }
                                           equals new { HCOId = s.HCOId, ProductGroupId = s.ProductGroupId, Quarter = DateTimeUtility.GetQuarterString(s.SalesDate) } into hco_employee_tmp
                                           from hco_employee in hco_employee_tmp.DefaultIfEmpty()
                                           select new
                                           {
                                               HCOId = h.HCOId,
                                               Potential = h.Potential,
                                               ProductGroupId = h.ProductGroupId,
                                               Quarter = h.Quarter,
                                               StartSalesMonth = h.StartSalesMonth,
                                               SubAreaId = h.SubAreaId,
                                               TerritoryId = h.TerritoryId,
                                               RegionId = h.RegionId,
                                               MRCode = (hco_employee == null ? "" : hco_employee.MRCode),
                                               MRId = (hco_employee == null ? "" : hco_employee.MRId)
                                           };
            //Consolidate the data and consider the situation where sales confirmed in the second quarter.

            var newHCOs_nextQuarterInfo = from h in newHCOs_withEmployeeInfo
                                          join s in sales on new { HCOId = h.HCOId, ProductGroupId = h.ProductGroupId }
                                          equals new { HCOId = s.HCOId, ProductGroupId = s.ProductGroupId }
                                          where DateTimeUtility.GetQuarterString(s.SalesDate) == DateTimeUtility.GetNextQuarterString(h.Quarter) && h.MRCode != s.MRCode
                                          select new
                                          {
                                              HCOId = h.HCOId,
                                              Potential = h.Potential,
                                              ProductGroupId = h.ProductGroupId,
                                              Quarter = DateTimeUtility.GetNextQuarterString(h.Quarter),
                                              StartSalesMonth = h.StartSalesMonth,
                                              SubAreaId = h.SubAreaId,
                                              TerritoryId = s.TerritoryId,
                                              RegionId = h.RegionId,
                                              MRCode = s.MRCode,
                                              MRId = s.MRId
                                          };
            //2017Q4数据的特殊处理，因为Q4的新开发奖没有发,算在2018Q1中补发
            var newHCOs_2017Q4 = from h in newHCOs_withEmployeeInfo
                                 join s in sales on new { HCOId = h.HCOId, ProductGroupId = h.ProductGroupId }
                                  equals new { HCOId = s.HCOId, ProductGroupId = s.ProductGroupId }
                                 where DateTimeUtility.GetQuarterString(s.SalesDate) == DateTimeUtility.GetNextQuarterString(h.Quarter) && h.MRCode != s.MRCode && h.Quarter == "2017Q4"
                                 select new
                                 {
                                     HCOId = h.HCOId,
                                     Potential = h.Potential,
                                     ProductGroupId = h.ProductGroupId,
                                     Quarter = "2018Q1",
                                     StartSalesMonth = h.StartSalesMonth,
                                     SubAreaId = h.SubAreaId,
                                     TerritoryId = s.TerritoryId,
                                     RegionId = h.RegionId,
                                     MRCode = s.MRCode,
                                     MRId = s.MRId
                                 };

            List<NewlyDevelopedHCO> newHCOs_withEmployeeList = new List<NewlyDevelopedHCO>();

            foreach (var h in newHCOs_withEmployeeInfo.Union(newHCOs_nextQuarterInfo).Union(newHCOs_2017Q4).Distinct())
            {
                NewlyDevelopedHCO target = new NewlyDevelopedHCO();
                target.HCOId = h.HCOId;
                target.Potential = (h.Potential == null ? 0 : h.Potential);
                target.SubAreaCode = h.SubAreaId;
                target.TerritoryCode = h.TerritoryId;
                target.ProductGroupId = h.ProductGroupId;
                target.StartSalesMonth = h.StartSalesMonth;
                target.RegionCode = h.RegionId;
                target.EmployeeCode = h.MRCode;
                target.EmployeeId = h.MRId;
                target.Quarter = h.Quarter;
                target.IsNewlyDevelopedIn6Months = DateTimeUtility.GetMonthInterval(h.StartSalesMonth, calculatingQuarter) <= 5;

                newHCOs_withEmployeeList.Add(target);

            }
            return newHCOs_withEmployeeList;
        }

        /// <summary>
        /// 新开发医院的逻辑
        /// 1. 找出新开发医院列表： 季度，终端编码，产品系列编码，新开发月份
        /// 2. 找出针对每家新开发医院的从新开发月份开始到季度末的平均积分：季度（季度从201801到当前计算季度），潜力等级，产品系列编码，新开发月份，员工编码，员工ID，办事处ID ，子大区ID，平均积分
        /// 3. 针对每位员工把前面第2步的列表按照员工编码做group by, 按季度排序，选第一条
        /// 4. 整理列表包括字段：季度	员工编码	员工ID	终端编码	潜力等级	产品系列ID	办事处ID	岗位码	月度	子大区ID	是否半年内新开发医院
        /// </summary>>
        public static List<NewlyDevelopedHCO> ProcessBySalesDate_v2(IEnumerable<Sales> sales0, IEnumerable<Employee> employees, string calculatingQuarter)
        {
            var sales = from s in sales0
                        where s.HospitalDistribution == 1
                        select s;
            //1. 找出新开发医院列表
            List<NewlyDevelopedHCO> newHCOs = GetNewlyDevlopedHCOsByDay(sales, calculatingQuarter);

            List<NewlyDevelopedHCO> newHCOs_converted = new List<NewlyDevelopedHCO>();
            foreach (var newHCO in newHCOs)
            {
                foreach (var m1 in newHCO.StartSalesMonthList)
                {
                    NewlyDevelopedHCO h = new NewlyDevelopedHCO();
                    h.HCOId = newHCO.HCOId;
                    h.ProductGroupId = newHCO.ProductGroupId;
                    h.StartSalesMonth = m1;
                    h.Quarter = DateTimeUtility.GetQuarterString(m1);

                    newHCOs_converted.Add(h);
                }

            }

            //2.找出针对每家新开发医院的从新开发月份开始到季度末的平均积分
            //List<NewlyDevelopedHCO> hcos_employee_quarter_1 = GetAvgCreditList(newHCOs_converted, sales, "2017Q3"); //hard-code
            //List<NewlyDevelopedHCO> hcos_employee_quarter_2 = GetAvgCreditList(newHCOs_converted, sales, "2017Q4"); //hard-code
            List<NewlyDevelopedHCO> hcos_employee_quarter_3 = GetAvgCreditList(newHCOs_converted, sales, "2018Q1"); //hard-code
            //List<NewlyDevelopedHCO> hcos_employee_quarter_4 = GetAvgCreditList(newHCOs_converted, sales, "2018Q2"); //hard-code temporarily commented out for Q1 recalculation
            //var hcos_employee_quarter = hcos_employee_quarter_3.Union(hcos_employee_quarter_4);
            var hcos_employee_quarter = hcos_employee_quarter_3;

            //3. 找出第一次满足入围条件的季度等信息
            var hcos_employee_first = (from h in hcos_employee_quarter
                                      where h.AverageCredit > 1000 && 
                                      //(
                                      //  (h.StartSalesMonth < 201710 && (h.Quarter == "2017Q3" || h.Quarter == "2017Q4")) ||
                                        (h.StartSalesMonth >= 201710 && (h.Quarter == "2018Q1" || h.Quarter == "2018Q2")) //hard-code
                                      //) //hard-code
                                      group h by new
                                      {
                                          HCOId = h.HCOId,
                                          ProductGroupId = h.ProductGroupId,
                                          StartSalesMonth = h.StartSalesMonth,
                                          EmployeeCode = h.EmployeeCode
                                      } into g
                                      select new { g.Key, Quarter = g.Min(x => x.Quarter) }).ToArray();
            //4.补充其余字段信息
            List<NewlyDevelopedHCO> newHCOs_withEmployeeList = new List<NewlyDevelopedHCO>();

            foreach (var h in hcos_employee_first.Distinct())
            {
                NewlyDevelopedHCO target = new NewlyDevelopedHCO();
                Employee employee = BusinessInfoHelper.GetEmployeeInfoByQuarter(h.Key.EmployeeCode, employees, h.Quarter);
                HCO hco = BusinessInfoHelper.GetHCOInfobyQuarter(h.Key.HCOId, sales, h.Quarter);
                target.HCOId = h.Key.HCOId;
                 if(hco != null)
                    target.Potential = hco.Potential;
                target.ProductGroupId = h.Key.ProductGroupId;
                target.StartSalesMonth = h.Key.StartSalesMonth;
                if (employee != null)
                {
                    target.SubAreaCode = employee.SubAreaCode;
                    target.TerritoryCode = employee.TerritoryCode;
                    target.RegionCode = employee.RegionCode;
                    target.EmployeeId = employee.EmployeeId.ToString();
                }
                target.EmployeeCode = h.Key.EmployeeCode;
                target.Quarter = h.Quarter;
                target.IsNewlyDevelopedIn6Months = DateTimeUtility.GetMonthInterval(h.Key.StartSalesMonth, calculatingQuarter) <= 5;

                newHCOs_withEmployeeList.Add(target);

            }

            foreach(var h in newHCOs_converted)
            {
                NewlyDevelopedHCO target = new NewlyDevelopedHCO();
                target.HCOId = h.HCOId;
                target.ProductGroupId = h.ProductGroupId;
                target.StartSalesMonth = h.StartSalesMonth;
                target.Quarter = h.Quarter;

                HCO hco = BusinessInfoHelper.GetHCOInfobyQuarter(h.HCOId, sales, h.Quarter);
                if (hco != null)
                    target.Potential = hco.Potential;
                target.IsNewlyDevelopedIn6Months = DateTimeUtility.GetMonthInterval(h.StartSalesMonth, calculatingQuarter) <= 5;

                newHCOs_withEmployeeList.Add(target);
            }

            return newHCOs_withEmployeeList;
        }

        private static List<NewlyDevelopedHCO> GetAvgCreditList(List<NewlyDevelopedHCO> newHCOs_converted, IEnumerable<Sales> sales, string calculatingQuarter)
        {

            List<NewlyDevelopedHCO> hco_avg_credits = new List<NewlyDevelopedHCO>();

            CalculatingTimeFrame timeFrame = DateTimeUtility.GetCalculatingTimeFrame(calculatingQuarter);

            var hco_credits = (from h in newHCOs_converted
                              join s in sales on new { HCOId = h.HCOId, ProductGroup = h.ProductGroupId } equals
                                     new { HCOId = s.HCOId, ProductGroup = s.ProductGroupId }
                              where s.Month >= h.StartSalesMonth && s.Month <= timeFrame.ThisQuarterEndMonth
                              select new
                              {
                                  HCOId = s.HCOId,
                                  ProductGroupId = s.ProductGroupId,
                                  Quarter = calculatingQuarter,
                                  StartSalesMonth = h.StartSalesMonth,
                                  EmployeeCode = s.MRCode,
                                  Credit = s.Credit,
                                  MonthInterval = DateTimeUtility.GetMonthInterval(s.Month, timeFrame.ThisQuarterEndMonth) +  1
                              }).ToArray();
            var avg_credits = (from h in hco_credits
                              group h by new
                              {
                                  HCOId = h.HCOId,
                                  ProductGroupId = h.ProductGroupId,
                                  Quarter = calculatingQuarter,
                                  StartSalesMonth = h.StartSalesMonth,
                                  EmployeeCode = h.EmployeeCode,
                              } into g
                              select new { g.Key, AverageCredit = g.Sum(x => x.Credit)/g.Max(y => y.MonthInterval) }).ToArray();

            var selling_hcos = from s in sales
                               where s.Month >= timeFrame.ThisQuarterStartMonth && s.Month <= timeFrame.ThisQuarterEndMonth
                               group s by new
                               {
                                   HCOId = s.HCOId,
                                   ProductGroupId = s.ProductGroupId,
                                   EmployeeCode = s.MRCode
                               } into g
                               select new { g.Key, TotalCredit = g.Sum(x => x.Credit) };

            var avg_credits_updated = (from h in avg_credits
                                       join s in selling_hcos
                                       on new
                                       {
                                           HCOId = h.Key.HCOId,
                                           ProductGroupId = h.Key.ProductGroupId,
                                           EmployeeCode = h.Key.EmployeeCode
                                       } equals new
                                       {
                                           HCOId = s.Key.HCOId,
                                           ProductGroupId = s.Key.ProductGroupId,
                                           EmployeeCode = s.Key.EmployeeCode
                                       }
                                       select h
                                       ).ToArray();

            foreach (var i in avg_credits_updated)
            {
                NewlyDevelopedHCO hco = new NewlyDevelopedHCO();

                hco.HCOId = i.Key.HCOId;
                hco.ProductGroupId = i.Key.ProductGroupId;
                hco.Quarter = i.Key.Quarter;
                hco.StartSalesMonth = i.Key.StartSalesMonth;
                hco.EmployeeCode = i.Key.EmployeeCode;
                hco.AverageCredit = i.AverageCredit;

                hco_avg_credits.Add(hco);

            }

            return hco_avg_credits;
        }

        /// <summary>
        /// Get Newly Devloped HCOs in last two quarters
        /// For now, the parameter of the starting point of sales date is fixed as 201710 - 12 months. i.e.201610
        /// </summary>
        /// <param name="sales"></param>
        /// <returns></returns>
        private static List<NewlyDevelopedHCO> GetNewlyDevlopedHCOs(IEnumerable<Sales> sales, string calculatingQuarter)
        {
            List<NewlyDevelopedHCO> newHCOs = new List<NewlyDevelopedHCO>();


            var salesList = (from Sales s in sales
                             where  //s.AreaId == "164" || s.AreaId == "165" || s.AreaId == "168" || s.AreaId == "87" || s.AreaId == "202" || s.AreaId == "85" || s.AreaId == "5" &&
                             s.HCOId != null
                             select new
                             {
                                 HCOId = s.HCOId,
                                 ProductGroupId = s.ProductGroupId,
                                 SubAreaId = s.SubAreaId,
                                 Potential = s.Potential,
                                 TerritoryId = s.TerritoryId,
                                 ConfirmationMonth = s.Month,
                                 SalesMonth = Int32.Parse(s.SalesDate.ToString("yyyyMM")),
                                 Credit = s.ThisQuarterCredit, //用了最新的credit，以避免同一数量的药品不同时间积分不同
                                 RegionId = s.RegionId
                             }).Distinct();

            var keyList = (from s in salesList
                           select new { s.HCOId, s.ProductGroupId }).Distinct();

            var salesMonthList = (from s1 in salesList
                                  group s1 by new { s1.HCOId, s1.ProductGroupId, s1.SalesMonth } into g
                                  select new { g.Key, TotalCredit = g.Sum(p => p.Credit) } into keys
                                  where keys.TotalCredit > 0d
                                  select keys).ToList();

            foreach (var d in keyList)
            {
                NewlyDevelopedHCO target = new NewlyDevelopedHCO();
                target.HCOId = d.HCOId;
                target.ProductGroupId = d.ProductGroupId;

                var monthList = from s2 in salesMonthList
                                where s2.Key.HCOId == d.HCOId && s2.Key.ProductGroupId == d.ProductGroupId
                                orderby s2.Key.SalesMonth ascending
                                select s2.Key.SalesMonth;

                int previousMonth = 0;
                int i = 0;

                int salesStartingPoint = Int32.Parse("201608");//201608就能算出201708及以后的新开发医院
                //int salesStartingPoint = Int32.Parse(DateTimeUtility.GetCalculatingTimeFrame(calculatingQuarter).LastQuarterStartDate.AddMonths(-14).ToString("yyyyMM"));

                foreach (var m in monthList)
                {
                    if (i == 0)
                    {
                        if (DateTimeUtility.GetMonthInterval(salesStartingPoint, m) >= 12)
                        {
                            target.StartSalesMonthList.Add(m);
                            target.Quarter = DateTimeUtility.GetQuarterString(m);
                        }
                    }
                    else if (i > 0)
                    {
                        if (DateTimeUtility.GetMonthInterval(previousMonth, m) > 12)
                        {
                            target.StartSalesMonthList.Add(m);
                            target.Quarter = DateTimeUtility.GetQuarterString(m);
                        }
                    }

                    previousMonth = m;
                    i++;
                    target.SalesMonthList.Add(m);
                }

                newHCOs.Add(target);
            }

            return newHCOs;
        }

        /// <summary>
        /// Get Newly Devloped HCOs in last two quarters
        /// For now, the parameter of the starting point of sales date is fixed as 201710 - 12 months. i.e.201610
        /// </summary>
        /// <param name="sales"></param>
        /// <returns></returns>
        private static List<NewlyDevelopedHCO> GetNewlyDevlopedHCOsByDay(IEnumerable<Sales> sales, string calculatingQuarter)
        {
            List<NewlyDevelopedHCO> newHCOs = new List<NewlyDevelopedHCO>();


            var salesList = (from Sales s in sales
                             where  //s.AreaId == "164" || s.AreaId == "165" || s.AreaId == "168" || s.AreaId == "87" || s.AreaId == "202" || s.AreaId == "85" || s.AreaId == "5" &&
                             s.HCOId != null
                             select new
                             {
                                 HCOId = s.HCOId,
                                 ProductGroupId = s.ProductGroupId,
                                 SubAreaId = s.SubAreaId,
                                 Potential = s.Potential,
                                 TerritoryId = s.TerritoryId,
                                 ConfirmationMonth = s.Month,
                                 SalesDate = s.SalesDate,
                                 Credit = s.ThisQuarterCredit, //用了最新的credit，以避免同一数量的药品不同时间积分不同
                                 RegionId = s.RegionId
                             }).Distinct();

            var keyList = (from s in salesList
                           select new { s.HCOId, s.ProductGroupId }).Distinct();

            var salesDateList = (from s1 in salesList
                                  group s1 by new { s1.HCOId, s1.ProductGroupId, s1.SalesDate } into g
                                  select new { g.Key, TotalCredit = g.Sum(p => p.Credit) } into keys
                                  where keys.TotalCredit > 0d
                                  select keys).ToList();

            foreach (var d in keyList)
            {
                NewlyDevelopedHCO target = new NewlyDevelopedHCO();
                target.HCOId = d.HCOId;
                target.ProductGroupId = d.ProductGroupId;

                var dateList = from s2 in salesDateList
                                where s2.Key.HCOId == d.HCOId && s2.Key.ProductGroupId == d.ProductGroupId
                                orderby s2.Key.SalesDate ascending
                                select s2.Key.SalesDate;

                DateTime previousDate = new DateTime();
                int i = 0;

                DateTime salesStartingDate = new DateTime(2016,8,1);//201608就能算出201708及以后的新开发医院
                //int salesStartingPoint = Int32.Parse(DateTimeUtility.GetCalculatingTimeFrame(calculatingQuarter).LastQuarterStartDate.AddMonths(-14).ToString("yyyyMM"));

                foreach (var salesDate in dateList)
                {
                    if (i == 0)
                    {
                        if(DateTimeUtility.GetMonthInterval(salesStartingDate,salesDate) >= 12)
                        {
                            int m = Int32.Parse(salesDate.ToString("yyyyMM"));
                            target.StartSalesMonthList.Add(m);
                            target.Quarter = DateTimeUtility.GetQuarterString(salesDate);
                        }
                    }
                    else if (i > 0)
                    {
                        if (DateTimeUtility.GetMonthInterval(previousDate, salesDate) >= 12)
                        {
                            int m = Int32.Parse(salesDate.ToString("yyyyMM"));
                            target.StartSalesMonthList.Add(m);
                            target.Quarter = DateTimeUtility.GetQuarterString(salesDate); 
                        }
                    }

                    previousDate = salesDate;
                    i++;
                    target.SalesMonthList.Add(DateTimeUtility.GetMonthInt(salesDate));
                }

                newHCOs.Add(target);
            }

            return newHCOs;
        }

    }
    public class MRTarget
    {
        public string DISTRICT_CODE { get; set; }
        public string REGION_CODE { get; set; }
        public string SUB_AREA_CODE { get; set; }
        public string AREA_CODE { get; set; }
        public string TerritoryId { get; set; }
        public string HCOId { get; set; }
        public string ProductGroupId { get; set; }
        public string ProductId { get; set; }
        public int YEAR { get; set; }
        public double Target{ get; set; }
        public int Month { get; set; }
    }

    public class MRTargetClassMap : ClassMap<MRTarget>
    {
        public MRTargetClassMap()
        {
            Map(m => m.DISTRICT_CODE).Name("DISTRICT_CODE");
            Map(m => m.REGION_CODE).Name("REGION_CODE");
            Map(m => m.SUB_AREA_CODE).Name("SUB_AREA_CODE");
            Map(m => m.AREA_CODE).Name("AREA_CODE");
            //Map(m => m.TerritoryId).Name("岗位码");
            //Map(m => m.HCOId).Name("终端ID");
            //Map(m => m.ProductGroupId).Name("产品系列ID");
            //Map(m => m.ProductId).Name("产品品规ID");
            //Map(m => m.YEAR).Name("YEAR");
            //Map(m => m.Target).Name("指标额");
            //Map(m => m.Month).Name("月度");
            Map(m => m.TerritoryId).Name("JOB_CODE");
            Map(m => m.HCOId).Name("HOS_CUST_CODE");
            Map(m => m.ProductGroupId).Name("PRODS_NO");
            Map(m => m.ProductId).Name("PROD_NO");
            Map(m => m.YEAR).Name("YEAR");
            Map(m => m.Target).Name("PLAN_AMT");
            Map(m => m.Month).Name("CALC_MONTH");
       }
    }
    public class AreaTarget
    {
        public string DISTRICT_CODE { get; set; }
        public string REGION_CODE { get; set; }
        public string AREA_CODE { get; set; }
        public string PRODS_NO { get; set; }
        public string PROD_NO { get; set; }
        public int YEAR { get; set; }
        public double PLAN_AMT { get; set; }
        public int Month { get; set; }
    }

    public class SubAreaTarget
    {
        public string DISTRICT_CODE { get; set; }
        public string REGION_CODE { get; set; }
        public string SUB_AREA_CODE { get; set; }
        public string AREA_CODE { get; set; }
        public string PRODS_NO { get; set; }
        public string PROD_NO { get; set; }
        public int YEAR { get; set; }
        public double PLAN_AMT { get; set; }
        public int Month { get; set; }
    }

    public class SubAreaTargetClassMap : ClassMap<SubAreaTarget>
    {
        public SubAreaTargetClassMap()
        {
            Map(m => m.DISTRICT_CODE).Name("DISTRICT_CODE");
            Map(m => m.REGION_CODE).Name("REGION_CODE");
            Map(m => m.SUB_AREA_CODE).Name("SUB_AREA_CODE");
            Map(m => m.AREA_CODE).Name("AREA_CODE");
            Map(m => m.PRODS_NO).Name("PRODS_NO");
            Map(m => m.PROD_NO).Name("PROD_NO");
            Map(m => m.YEAR).Name("YEAR");
            Map(m => m.PLAN_AMT).Name("PLAN_AMT");
            Map(m => m.Month).Name("MONTH");
       }
    }
    public class AreaTargetClassMap : ClassMap<AreaTarget>
    {
        public AreaTargetClassMap()
        {
            Map(m => m.DISTRICT_CODE).Name("DISTRICT_CODE");
            Map(m => m.REGION_CODE).Name("REGION_CODE");
            Map(m => m.AREA_CODE).Name("AREA_CODE");
            Map(m => m.PRODS_NO).Name("PRODS_NO");
            Map(m => m.PROD_NO).Name("PROD_NO");
            Map(m => m.YEAR).Name("YEAR");
            Map(m => m.PLAN_AMT).Name("PLAN_AMT");
            Map(m => m.Month).Name("MONTH");
        }
    }


}
