using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.BrightFuture
{
    class HCO
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public int? Potential { get; set; }
        public string Level { get; set; }
    }
    class BusinessInfoHelper
    {       
            /// <summary>
             /// 获取最新的员工信息
             /// </summary>
             /// <param name="employeeCode"></param>
             /// <param name="employees"></param>
             /// <returns></returns>
        public static Employee GetLatestEmployeeInfo(string employeeCode, IEnumerable<Employee> employees)
        {
            var first_employees = (from e in employees
                                   where e.EmployeeCode == employeeCode
                                   orderby e.Month descending
                                   select e).FirstOrDefault();

            return first_employees;
        }
        /// <summary>
        /// 根据员工编码，季度获取员工信息。如果找不到则取最新的员工信息
        /// </summary>
        /// <param name="employeeCode"></param>
        /// <param name="employees"></param>
        /// <param name="quarterString"></param>
        /// <returns></returns>
        public static Employee GetEmployeeInfoByQuarter(string employeeCode, IEnumerable<Employee> employees, string quarterString)
        {
            var first_employees = (from e in employees
                                   where e.EmployeeCode == employeeCode && DateTimeUtility.GetQuarterString(e.Month) == quarterString
                                   orderby e.Month descending
                                   select e).FirstOrDefault();
            if (first_employees == null)
                first_employees = GetLatestEmployeeInfo(employeeCode, employees);

            return first_employees;
        }

        public static HCO GetHCOInfobyQuarter(string hcoId, IEnumerable<Sales> sales, string quarterString)
        {
            var first_hco = (from s in sales
                             where DateTimeUtility.GetQuarterString(s.Month) == quarterString && s.HCOId == hcoId
                             orderby s.Month descending
                             select s).FirstOrDefault();


            HCO h = new HCO();

            if(first_hco != null)
            {
                h.ID = hcoId;
                h.Potential = first_hco.Potential;
                h.Level = first_hco.Level;
            }
            else
            {
                h = GetLatestHCOInfo(hcoId, sales);
            }

            return h;
        }
        public static HCO GetLatestHCOInfo(string hcoId, IEnumerable<Sales> sales)
        {
            var first_hco = (from s in sales
                             where s.HCOId == hcoId
                             orderby s.Month descending
                             select s).FirstOrDefault();
            HCO h = new HCO();

            if (first_hco != null)
            {
                h.ID = hcoId;
                h.Potential = first_hco.Potential;
                h.Level = first_hco.Level;
            }

            return h;
        }
            public static int IsOndutyThisMonth(int month, DateTime? onboardDate, DateTime? offboardDate)
            {
                int isOnduty = 0;

                if (month < 201701 || onboardDate == null) return 0;

                int year = month / 100;
                int iMonth = month % 100;

                DateTime currentMonth12thDay = new DateTime(year, iMonth, 12);
                if (onboardDate < currentMonth12thDay && (offboardDate == null || offboardDate >= currentMonth12thDay))
                    isOnduty = 1;

                return isOnduty;
            }
        public static double CalculateTarget(int month, DateTime? onboardDate, DateTime? offboardDate, double target)
        {
            double calculatedTarget = 0d;

            DateTime date = DateTimeUtility.GetDate(month);

            int currentMonth = date.Month;
            int currentYear = date.Year;
            int MonthlyDays = DateTime.DaysInMonth(currentYear, currentMonth);
            int startDay = 1;
            int endDay = MonthlyDays;

            //如果入职日期在12号之前，指标按日历数比例计算
            if(onboardDate != null && onboardDate.Value.Month == currentMonth && onboardDate.Value.Year == currentYear && onboardDate.Value.Day < 12)
            {
                startDay = onboardDate.Value.Day;
            }

            //如果入职日期之前的月份，或者在12号之后，指标为0
            if((onboardDate != null && onboardDate.Value.Month > currentMonth && onboardDate.Value.Year >= currentYear ) ||
                (onboardDate != null && onboardDate.Value.Month == currentMonth && onboardDate.Value.Year == currentYear && onboardDate.Value.Day >= 12))
            {
                startDay = endDay + 1;
            }
            //if(offboardDate != null && offboardDate.Value.Month == currentMonth && onboardDate.Value.Year == currentYear)
            //{
            //    endDay = offboardDate.Value.Day;
            //}

            calculatedTarget = target * (endDay - startDay + 1) / (MonthlyDays);

            return calculatedTarget;
        }

    }


}
