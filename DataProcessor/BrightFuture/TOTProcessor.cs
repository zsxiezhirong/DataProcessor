using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.BrightFuture
{

    class TOT
    {
        public string Sequence;
        public string EmployeeCode;
        public string EmployeeId;
        public DateTime StartTime;
        public DateTime EndTime;
        public string MonthString;
        public string QuarterString;
        public int DaysInQuarter;
        public float Days;
    }
    class TOTProcessor
    {
        public static List<TOT> Process(List<TOT> rawTOTs)
        {
            List<TOT> result = new List<TOT>();
            
            foreach(TOT rawRecord in rawTOTs)
            {
                List<TOT> processedTOTs = ProcessTOTRecord(rawRecord);

                foreach(TOT t in processedTOTs)
                {
                    result.Add(t);
                }
            }

            return result;
        }

        private static List<TOT> ProcessTOTRecord(TOT rawRecord)
        {
            List<TOT> processedTOTs = new List<TOT>();

            List<DateTime> Months = ExtractMonths(rawRecord.StartTime, rawRecord.EndTime);

            foreach(DateTime month in Months)
            {
                float days = CalculateDaysInMonth(rawRecord.StartTime, rawRecord.EndTime, month);

                TOT newRecord = new TOT();
                newRecord.EmployeeCode = rawRecord.EmployeeCode;
                newRecord.EmployeeId = rawRecord.EmployeeId;
                newRecord.StartTime = rawRecord.StartTime;
                newRecord.EndTime = rawRecord.EndTime;
                newRecord.Days = days;
                newRecord.MonthString = month.ToString("yyyyMM");
                newRecord.QuarterString = GetQuarterString(month);
                newRecord.DaysInQuarter = GetDaysInQuarter(month);
                processedTOTs.Add(newRecord);
            }
            
            return processedTOTs;
        }

        private static int GetDaysInQuarter(DateTime month)
        {
            int daysInQuarter = 0;

            bool isLeapYear = false;
            isLeapYear = ((month.Year % 4 == 0 && month.Year % 100 != 0) || month.Year % 400 == 0);

             if (month.Month <=3 )
            {
                if (isLeapYear)
                    daysInQuarter = 91;
                else
                    daysInQuarter = 90;
            }
            else if(month.Month > 3 && month.Month <= 6)
            {
                daysInQuarter = 91;
            }
            else if(month.Month > 6 )
            {
                daysInQuarter = 92;
            }

            return daysInQuarter;
        }

        private static string GetQuarterString(DateTime month)
        {
            string quarterString = string.Empty;

            if(month.Month <=3 )
            {
                quarterString = month.Year.ToString() + "Q1";
            }
            else if(month.Month > 3 && month.Month <= 6)
            {
                quarterString = month.Year.ToString() + "Q2";
            }
            else if(month.Month > 6 && month.Month <= 9)
            {
                quarterString = month.Year.ToString() + "Q3";
            }
            else if(month.Month > 9 && month.Month <= 12)
            {
                quarterString = month.Year.ToString() + "Q4";
            }

            return quarterString;
        }

        private static float CalculateDaysInMonth(DateTime startTime, DateTime endTime, DateTime month)
        {
            float days = 0;

            if (month > endTime) return 0;

            DateTime startTimeThisMonth;
            DateTime endTimeThisMonth;

            if (startTime <= month)
                startTimeThisMonth = new DateTime(month.Year,month.Month,month.Day,9,0,0);
            else
                startTimeThisMonth = startTime;

            if (endTime < month.AddMonths(1))
                endTimeThisMonth = endTime;
            else
                endTimeThisMonth = month.AddMonths(1).AddHours(-6);

            TimeSpan timeSpan = endTimeThisMonth - startTimeThisMonth;
            //9:00-12:00 请假的日期含这个区间，即使是1分钟，按半天
            //13:00 - 18:00 请假的日期含这个区间，即使是1分钟，按半天

            if (timeSpan.Ticks < 0) return 0;
            else if (timeSpan.Days == 0)
            {
                DateTime time12pm = new DateTime(startTimeThisMonth.Year, startTimeThisMonth.Month, startTimeThisMonth.Day, 12, 0, 0);
                DateTime time13pm = new DateTime(startTimeThisMonth.Year, startTimeThisMonth.Month, startTimeThisMonth.Day, 13, 0, 0);
                DateTime time9am = new DateTime(startTimeThisMonth.Year, startTimeThisMonth.Month, startTimeThisMonth.Day, 9, 0, 0);
                DateTime time18pm = new DateTime(startTimeThisMonth.Year, startTimeThisMonth.Month, startTimeThisMonth.Day, 18, 0, 0);

                if (startTimeThisMonth <= time9am && endTimeThisMonth <= time9am) return 0f;
                if (startTimeThisMonth >= time18pm && endTimeThisMonth >= time18pm) return 0f;

                if (startTimeThisMonth < time12pm && endTimeThisMonth <= time13pm)
                    days = 0.5f;
                else if (startTimeThisMonth < time12pm && endTimeThisMonth > time13pm)
                    days = 1.0f;
                else if (startTimeThisMonth >= time12pm && endTimeThisMonth <= time13pm)
                    days = 0f;
                else 
                    days = 0.5f;
            }
            else if(timeSpan.Days > 0)
            {
                DateTime startTime12pm = new DateTime(startTimeThisMonth.Year, startTimeThisMonth.Month, startTimeThisMonth.Day, 12, 0, 0);
                DateTime startTime18pm = new DateTime(startTimeThisMonth.Year, startTimeThisMonth.Month, startTimeThisMonth.Day, 18, 0, 0);
                DateTime endTime13pm = new DateTime(endTimeThisMonth.Year, endTimeThisMonth.Month, endTimeThisMonth.Day, 13, 0, 0);
                DateTime endTime9am = new DateTime(endTimeThisMonth.Year, endTimeThisMonth.Month, endTimeThisMonth.Day, 9, 0, 0);

                float firstDays = 0f;
                float lastDays = 0f;

                if (startTimeThisMonth >= startTime18pm)
                    firstDays = 0f;
                else if (startTimeThisMonth >= startTime12pm)
                    firstDays = 0.5f;
                else
                    firstDays = 1.0f;

                if (endTimeThisMonth <= endTime9am)
                    lastDays = 0f;
                else if (endTimeThisMonth <= endTime13pm)
                    lastDays = 0.5f;
                else
                    lastDays = 1.0f;

                days = firstDays + lastDays + timeSpan.Days - 1.0f;
            }

            return days;
        }

        private static List<DateTime> ExtractMonths(DateTime start, DateTime end)
        {
            List<DateTime> dateTimes = new List<DateTime>();

            DateTime month = new DateTime(start.Year, start.Month, 1, 0, 0, 0);
            while(month < new DateTime(end.Year,end.Month + 1,1,0,0,0))
            {
                dateTimes.Add(month);
                month = month.AddMonths(1);
            }

            return dateTimes;
        }
    }
}
