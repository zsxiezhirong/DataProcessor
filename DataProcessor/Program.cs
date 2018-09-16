using DataProcessor.BrightFuture;
using SimpleCSV;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;

namespace DataProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            //ProcessTOT();
            #region Process Last Year Sales
            //处理压货，增幅的时候要考虑补充自然流的数据
           //           UpdateCredit();
           // PorcessYearlySales();
            #endregion
            //TestLINQ();
            //ProcessMeinaData();
            // PorcessMRSalesTarget();
            //PorcessRSMTarget();
            //PorcessRSMCredit();
            //PorcessDSMTarget();
           // PorcessDSMCredit();
            //PorcessRSMSalesTarget();
            #region Process Newly DevelopedHCO
            PorcessNewlyDevelopedHCO();
            #endregion
            Console.WriteLine("Done！");
            Console.ReadLine();
        }

        static void ProcessTOT()
        {
            List<TOT> TOTs = new List<TOT>();
            string fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\3、请假明细（18年1-3）.csv";
            using (var csvReader = new CSVReader(new StreamReader(fileName)))
            {
                string[] line;
                int lineNumber = 0;
                while ((line = csvReader.ReadNext()) != null)
                {
                    TOT t = new TOT();
                    if (lineNumber > 0)
                    {
                        t.Sequence = line[0];
                        t.EmployeeCode = line[1];
                        t.EmployeeId = line[2];

                        t.StartTime = DateTime.Parse(line[3]);
                        t.EndTime = DateTime.Parse(line[4]);

                        TOTs.Add(t);
                    }
                    lineNumber++;
                }

                List<TOT> results = TOTProcessor.Process(TOTs);

                string resultFilePath = @"C:\Users\Zhirong.Xie\Desktop\Testing Data" + "\\TOT_converted.csv";

                using (var csvWriter = new CSVWriter(new StreamWriter(resultFilePath, false)))
                {
                    csvWriter.WriteNext(new string[] { "月度","季度","季度日历日数", "员工编码","员工Id","休假天数","开始时间","结束时间"});
                    foreach (TOT t in results)
                    {
                        string[] line1 = new string[] { t.MonthString,t.QuarterString,t.DaysInQuarter.ToString(), t.EmployeeCode,t.EmployeeId,t.Days.ToString(),t.StartTime.ToString(),t.EndTime.ToString()};
                        csvWriter.WriteNext(line1);
                    }
                }
            }

        }
        static void PorcessYearlySales()
        {
            IEnumerable<Sales> sales;
            //string fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2 Testing\7、已确认流向（201701-201806).csv";
            string fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\Updated_Sales_Data.csv";
            using (var reader = new CsvReader(new StreamReader(fileName)))
            {
                reader.Configuration.HeaderValidated = null;
 //               reader.Configuration.HasHeaderRecord = true;
                reader.Configuration.RegisterClassMap<UpdatedSalesClassMap>();
                sales = reader.GetRecords<Sales>().ToList();
                //foreach(Sales s in sales)
                //{
                //    Console.WriteLine(string.Format("Sequence {3},HCO Id: {0}, Month {1}, Sales {2}", s.HCOId, s.Month, s.Credit,s.Sequence));
                //}

            }

            //List<YearlySales> yearlySales = SalesProcessor.Process(sales, "2018Q2");
            List<YearlySales> yearlySales = SalesProcessor.Process(sales, "2018Q1");
            string resultFileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q1\Last_Year_Sales_Data.csv";
            //string resultFileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\Last_Year_Sales_Data.csv";
            //string resultFileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\New_Sales_Data.csv";

            using (var writer = new CsvWriter(new StreamWriter(resultFileName)))
            {
                writer.Configuration.RegisterClassMap<YearlySalesClassMap>();
                writer.WriteRecords<YearlySales>(yearlySales);
                writer.Flush();
            }

        }
        static void TestLINQ()
        {
            //資料來源
            string[] words = { "Hello", "wonderful", "LINQ", "beautiful", "World" };
            //LINQ查詢表達式
            var shortWords =
              from word in words
              where word.Length <= 5
              select word;
            //顯示查詢結果
            foreach (var e in shortWords)
            {
                Console.WriteLine(e.ToString());
            }

            var longWords = from word in words
                            where word.Length > 5
                            select word;

            foreach (var e in longWords)
            {
                Console.WriteLine(e.ToString());
            }
        }
        static void ProcessMeinaData()
        {
            IEnumerable<Meina_Territory> territories;
            string fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\美纳里尼数据收集模板- 20180511.csv";
            using (var reader = new CsvReader(new StreamReader(fileName)))
            {
                reader.Configuration.HeaderValidated = null;
                //               reader.Configuration.HasHeaderRecord = true;
                reader.Configuration.MissingFieldFound = null;
                reader.Configuration.RegisterClassMap<Meina_TerritoryClassMap>();
                territories = reader.GetRecords<Meina_Territory>().ToList();

                List<Meina_Territory> results = Meina_Territory_Processor.Process(territories);
;
                string resultFileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\New_Meina_Data.csv";

                using (var writer = new CsvWriter(new StreamWriter(resultFileName)))
                {
                    writer.Configuration.RegisterClassMap<Meina_TerritoryClassMap>();
                    writer.WriteRecords<Meina_Territory>(results);
                    writer.Flush();
                }
            }


        }
        static void PorcessMRSalesTarget() {
            IEnumerable<Sales> sales;
            //string fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\7、已确认流向（201701-201803).csv";
//            string fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2 Testing\7、已确认流向（201701-201806).csv";
            string fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\Updated_Sales_Data.csv";
            using (var reader = new CsvReader(new StreamReader(fileName)))
            {
                reader.Configuration.HeaderValidated = null;
                //               reader.Configuration.HasHeaderRecord = true;
                reader.Configuration.RegisterClassMap<UpdatedSalesClassMap>();
                sales = reader.GetRecords<Sales>().ToList();
                //foreach(Sales s in sales)
                //{
                //    Console.WriteLine(string.Format("Sequence {3},HCO Id: {0}, Month {1}, Sales {2}", s.HCOId, s.Month, s.Credit,s.Sequence));
                //}

            }

            IEnumerable<MRTarget> targets;
            fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\6、高端医院分解指标.csv";
            using (var reader = new CsvReader(new StreamReader(fileName)))
            {
                reader.Configuration.HeaderValidated = null;
                //               reader.Configuration.HasHeaderRecord = true;
                reader.Configuration.RegisterClassMap<MRTargetClassMap>();
                targets = reader.GetRecords<MRTarget>().ToList();
                //foreach(Sales s in sales)
                //{
                //    Console.WriteLine(string.Format("Sequence {3},HCO Id: {0}, Month {1}, Sales {2}", s.HCOId, s.Month, s.Credit,s.Sequence));
                //}

            }

            IEnumerable<Employee> employees;
            fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\2、每月员工信息.csv";
            //fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\mr1.csv";
            using (var reader = new CsvReader(new StreamReader(fileName)))
            {
                reader.Configuration.HeaderValidated = null;
                //               reader.Configuration.HasHeaderRecord = true;
                reader.Configuration.RegisterClassMap<EmployeeClassMap>();
                employees = reader.GetRecords<Employee>().ToList();
                //foreach(Sales s in sales)
                //{
                //    Console.WriteLine(string.Format("Sequence {3},HCO Id: {0}, Month {1}, Sales {2}", s.HCOId, s.Month, s.Credit,s.Sequence));
                //}

            }


            List<MRSalesTarget> mrSalesTargets = MRSalesTarget.Process(sales,employees,targets);
            string resultFileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\MR_Saels_Target_Data.csv";

            using (var writer = new CsvWriter(new StreamWriter(resultFileName)))
            {
                writer.Configuration.RegisterClassMap<MRSalesTargetClassMap>();
                writer.WriteRecords<MRSalesTarget>(mrSalesTargets);
                writer.Flush();
            }

        }
        static void PorcessRSMSalesTarget()
        {
            IEnumerable<Sales> sales;
            string fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2 Testing\7、已确认流向（201701-201806).csv";
           // string fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\7、已确认流向（201701-201803).csv";
            using (var reader = new CsvReader(new StreamReader(fileName)))
            {
                reader.Configuration.HeaderValidated = null;
                //               reader.Configuration.HasHeaderRecord = true;
                reader.Configuration.RegisterClassMap<SalesClassMap>();
                sales = reader.GetRecords<Sales>().ToList();
                //foreach(Sales s in sales)
                //{
                //    Console.WriteLine(string.Format("Sequence {3},HCO Id: {0}, Month {1}, Sales {2}", s.HCOId, s.Month, s.Credit,s.Sequence));
                //}

            }

            IEnumerable<Employee> employees;
            fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2 Testing\2、每月员工信息（18年1-6）.csv";
            //fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\dsm.csv";
            using (var reader = new CsvReader(new StreamReader(fileName)))
            {
                reader.Configuration.HeaderValidated = null;
                //               reader.Configuration.HasHeaderRecord = true;
                reader.Configuration.RegisterClassMap<EmployeeClassMap>();
                employees = reader.GetRecords<Employee>().ToList();
                //foreach(Sales s in sales)
                //{
                //    Console.WriteLine(string.Format("Sequence {3},HCO Id: {0}, Month {1}, Sales {2}", s.HCOId, s.Month, s.Credit,s.Sequence));
                //}

            }


            List<RSMSalesTarget> dsmSalesTargets = RSMSalesTarget.Process(sales,employees);
            string resultFileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2 Testing\RSM_Sales_Target_Data.csv";
            //string resultFileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\DSM_Sales_Target_Data.csv";

            using (var writer = new CsvWriter(new StreamWriter(resultFileName)))
            {
                writer.Configuration.RegisterClassMap<RSMSalesTargetMap>();
                writer.WriteRecords<RSMSalesTarget>(dsmSalesTargets);
                writer.Flush();
            }

        }
        static void UpdateCredit()
        {
            IEnumerable<Sales> sales;
            string fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\CCBU已确认流向（201608-201806）.csv";
            //string fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\7、已确认流向（201608-201806).csv";
            // string fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\7、已确认流向（201701-201803).csv";
            using (var reader = new CsvReader(new StreamReader(fileName)))
            {
                reader.Configuration.HeaderValidated = null;
                //               reader.Configuration.HasHeaderRecord = true;
                reader.Configuration.RegisterClassMap<SalesClassMap>();
                sales = reader.GetRecords<Sales>().ToList();
                //foreach(Sales s in sales)
                //{
                //    Console.WriteLine(string.Format("Sequence {3},HCO Id: {0}, Month {1}, Sales {2}", s.HCOId, s.Month, s.Credit,s.Sequence));
                //}

            }
            IEnumerable<UnitPrice> unitPrices;
            fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\11、201806产品积分标准.csv";
            // string fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\7、已确认流向（201701-201803).csv";
            using (var reader = new CsvReader(new StreamReader(fileName)))
            {
                reader.Configuration.HeaderValidated = null;
                //               reader.Configuration.HasHeaderRecord = true;
                reader.Configuration.MissingFieldFound = null;
                reader.Configuration.RegisterClassMap<UnitPriceClassMap>();
                unitPrices = reader.GetRecords<UnitPrice>().ToList();

            }

            IEnumerable<UnitPrice> lastQuarter_unitPrices;
            fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\11、201803产品积分标准.csv";
            // string fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\7、已确认流向（201701-201803).csv";
            using (var reader = new CsvReader(new StreamReader(fileName)))
            {
                reader.Configuration.HeaderValidated = null;
                //               reader.Configuration.HasHeaderRecord = true;
                reader.Configuration.MissingFieldFound = null;
                reader.Configuration.RegisterClassMap<UnitPriceClassMap>();
                lastQuarter_unitPrices = reader.GetRecords<UnitPrice>().ToList();

            }

            IEnumerable<Sales> updatedSales = UnitPriceProcessor.Process(sales, unitPrices, lastQuarter_unitPrices);
            string resultFileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\Updated_Sales_Data.csv";
            //string resultFileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\DSM_Sales_Target_Data.csv";

            using (var writer = new CsvWriter(new StreamWriter(resultFileName)))
            {
                writer.Configuration.RegisterClassMap<UpdatedSalesClassMap>();
                writer.WriteRecords<Sales>(updatedSales);
                writer.Flush();
            }

        }

        static void PorcessDSMTarget()
        {

            IEnumerable<Employee> employees;
            
            string fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\2、每月员工信息.csv";
            //fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\dsm.csv";
            using (var reader = new CsvReader(new StreamReader(fileName)))
            {
                reader.Configuration.HeaderValidated = null;
                //               reader.Configuration.HasHeaderRecord = true;
                reader.Configuration.RegisterClassMap<EmployeeClassMap>();
                employees = reader.GetRecords<Employee>().ToList();
                //foreach(Sales s in sales)
                //{
                //    Console.WriteLine(string.Format("Sequence {3},HCO Id: {0}, Month {1}, Sales {2}", s.HCOId, s.Month, s.Credit,s.Sequence));
                //}

            }

            IEnumerable<SubAreaTarget> targets;
            fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\5、季度标准办事处指标.csv";
            using (var reader = new CsvReader(new StreamReader(fileName)))
            {
                reader.Configuration.HeaderValidated = null;
                //               reader.Configuration.HasHeaderRecord = true;
                reader.Configuration.RegisterClassMap<SubAreaTargetClassMap>();
                targets = reader.GetRecords<SubAreaTarget>().ToList();
                //foreach(Sales s in sales)
                //{
                //    Console.WriteLine(string.Format("Sequence {3},HCO Id: {0}, Month {1}, Sales {2}", s.HCOId, s.Month, s.Credit,s.Sequence));
                //}

            }


            List<DSMTarget> dsmSalesTargets = DSMTarget.Process(targets,employees);
            string resultFileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\DSM_Target.csv";
            //string resultFileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\DSM_Sales_Target_Data.csv";

            using (var writer = new CsvWriter(new StreamWriter(resultFileName)))
            {
                writer.Configuration.RegisterClassMap<DSMTargetMap>();
                writer.WriteRecords<DSMTarget>(dsmSalesTargets);
                writer.Flush();
            }

        }
        static void PorcessDSMCredit()
        {
            IEnumerable<Sales> sales;
            string fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\7、已确认流向（201608-201806).csv";
            //// string fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\7、已确认流向（201701-201803).csv";
            using (var reader = new CsvReader(new StreamReader(fileName)))
            {
                reader.Configuration.HeaderValidated = null;
                //               reader.Configuration.HasHeaderRecord = true;
                reader.Configuration.RegisterClassMap<SalesClassMap>();
                sales = reader.GetRecords<Sales>().ToList();
                //foreach(Sales s in sales)
                //{
                //    Console.WriteLine(string.Format("Sequence {3},HCO Id: {0}, Month {1}, Sales {2}", s.HCOId, s.Month, s.Credit,s.Sequence));
                //}

            }

            IEnumerable<Employee> employees;

            fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\2、每月员工信息.csv";
            //fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\dsm.csv";
            using (var reader = new CsvReader(new StreamReader(fileName)))
            {
                reader.Configuration.HeaderValidated = null;
                //               reader.Configuration.HasHeaderRecord = true;
                reader.Configuration.RegisterClassMap<EmployeeClassMap>();
                employees = reader.GetRecords<Employee>().ToList();
                //foreach(Sales s in sales)
                //{
                //    Console.WriteLine(string.Format("Sequence {3},HCO Id: {0}, Month {1}, Sales {2}", s.HCOId, s.Month, s.Credit,s.Sequence));
                //}

            }

            IEnumerable<MRTarget> targets;
            fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\6、高端医院分解指标.csv";
            using (var reader = new CsvReader(new StreamReader(fileName)))
            {
                reader.Configuration.HeaderValidated = null;
                //               reader.Configuration.HasHeaderRecord = true;
                reader.Configuration.RegisterClassMap<MRTargetClassMap>();
                targets = reader.GetRecords<MRTarget>().ToList();
                //foreach(Sales s in sales)
                //{
                //    Console.WriteLine(string.Format("Sequence {3},HCO Id: {0}, Month {1}, Sales {2}", s.HCOId, s.Month, s.Credit,s.Sequence));
                //}

            }


            List<DSMCredit> dsmSalesTargets = DSMCredit.Process(sales, employees,targets);
            string resultFileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\DSM_Credit.csv";
            //string resultFileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\DSM_Sales_Target_Data.csv";

            using (var writer = new CsvWriter(new StreamWriter(resultFileName)))
            {
                writer.Configuration.RegisterClassMap<DSMCreditMap>();
                writer.WriteRecords<DSMCredit>(dsmSalesTargets);
                writer.Flush();
            }

        }
        static void PorcessRSMTarget()
        {

            IEnumerable<Employee> employees;

            string fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\2、每月员工信息.csv";
            //fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\dsm.csv";
            using (var reader = new CsvReader(new StreamReader(fileName)))
            {
                reader.Configuration.HeaderValidated = null;
                //               reader.Configuration.HasHeaderRecord = true;
                reader.Configuration.RegisterClassMap<EmployeeClassMap>();
                employees = reader.GetRecords<Employee>().ToList();
                //foreach(Sales s in sales)
                //{
                //    Console.WriteLine(string.Format("Sequence {3},HCO Id: {0}, Month {1}, Sales {2}", s.HCOId, s.Month, s.Credit,s.Sequence));
                //}

            }

            IEnumerable<AreaTarget> targets;
            fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\4、季度标准省区指标.csv";
            using (var reader = new CsvReader(new StreamReader(fileName)))
            {
                reader.Configuration.HeaderValidated = null;
                //               reader.Configuration.HasHeaderRecord = true;
                reader.Configuration.RegisterClassMap<AreaTargetClassMap>();
                targets = reader.GetRecords<AreaTarget>().ToList();
                //foreach(Sales s in sales)
                //{
                //    Console.WriteLine(string.Format("Sequence {3},HCO Id: {0}, Month {1}, Sales {2}", s.HCOId, s.Month, s.Credit,s.Sequence));
                //}

            }

            List<RSMTarget> rsmSalesTargets = RSMTarget.Process(targets, employees);
            string resultFileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\RSM_Target.csv";
            //string resultFileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\DSM_Sales_Target_Data.csv";

            using (var writer = new CsvWriter(new StreamWriter(resultFileName)))
            {
                writer.Configuration.RegisterClassMap<RSMTargetMap>();
                writer.WriteRecords<RSMTarget>(rsmSalesTargets);
                writer.Flush();
            }

        }
        static void PorcessRSMCredit()
        {
            IEnumerable<Sales> sales;
            string fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\7、已确认流向（201608-201806).csv";
            //// string fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\7、已确认流向（201701-201803).csv";
            using (var reader = new CsvReader(new StreamReader(fileName)))
            {
                reader.Configuration.HeaderValidated = null;
                //               reader.Configuration.HasHeaderRecord = true;
                reader.Configuration.RegisterClassMap<SalesClassMap>();
                sales = reader.GetRecords<Sales>().ToList();
                //foreach(Sales s in sales)
                //{
                //    Console.WriteLine(string.Format("Sequence {3},HCO Id: {0}, Month {1}, Sales {2}", s.HCOId, s.Month, s.Credit,s.Sequence));
                //}

            }

            IEnumerable<Employee> employees;

            fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\2、每月员工信息.csv";
            //fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\dsm.csv";
            using (var reader = new CsvReader(new StreamReader(fileName)))
            {
                reader.Configuration.HeaderValidated = null;
                //               reader.Configuration.HasHeaderRecord = true;
                reader.Configuration.RegisterClassMap<EmployeeClassMap>();
                employees = reader.GetRecords<Employee>().ToList();
                //foreach(Sales s in sales)
                //{
                //    Console.WriteLine(string.Format("Sequence {3},HCO Id: {0}, Month {1}, Sales {2}", s.HCOId, s.Month, s.Credit,s.Sequence));
                //}

            }

            IEnumerable<MRTarget> targets;
            fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\6、高端医院分解指标.csv";
            using (var reader = new CsvReader(new StreamReader(fileName)))
            {
                reader.Configuration.HeaderValidated = null;
                //               reader.Configuration.HasHeaderRecord = true;
                reader.Configuration.RegisterClassMap<MRTargetClassMap>();
                targets = reader.GetRecords<MRTarget>().ToList();
                //foreach(Sales s in sales)
                //{
                //    Console.WriteLine(string.Format("Sequence {3},HCO Id: {0}, Month {1}, Sales {2}", s.HCOId, s.Month, s.Credit,s.Sequence));
                //}

            }

            List<RSMCredit> rsmSalesTargets = RSMCredit.Process(sales, employees, targets);
            string resultFileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\RSM_Credit.csv";
            //string resultFileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\DSM_Sales_Target_Data.csv";

            using (var writer = new CsvWriter(new StreamWriter(resultFileName)))
            {
                writer.Configuration.RegisterClassMap<RSMCreditMap>();
                writer.WriteRecords<RSMCredit>(rsmSalesTargets);
                writer.Flush();
            }

        }

        static void PorcessNewlyDevelopedHCO()
        {
            IEnumerable<Sales> sales;
            //string fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\7、已确认流向（201610-201803).csv";
            //string fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\7、已确认流向（201608-201806).csv";
            string fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\Updated_Sales_Data.csv";
            using (var reader = new CsvReader(new StreamReader(fileName)))
            {
                reader.Configuration.HeaderValidated = null;
                //               reader.Configuration.HasHeaderRecord = true;
                reader.Configuration.RegisterClassMap<UpdatedSalesClassMap>();
                sales = reader.GetRecords<Sales>().ToList();
                //foreach(Sales s in sales)
                //{
                //    Console.WriteLine(string.Format("Sequence {3},HCO Id: {0}, Month {1}, Sales {2}", s.HCOId, s.Month, s.Credit,s.Sequence));
                //}

            }

            IEnumerable<Employee> employees;
            fileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\2、每月员工信息.csv";
            using (var reader = new CsvReader(new StreamReader(fileName)))
            {
                reader.Configuration.HeaderValidated = null;
                //               reader.Configuration.HasHeaderRecord = true;
                reader.Configuration.RegisterClassMap<EmployeeClassMap>();
                employees = reader.GetRecords<Employee>().ToList();
                //foreach(Sales s in sales)
                //{
                //    Console.WriteLine(string.Format("Sequence {3},HCO Id: {0}, Month {1}, Sales {2}", s.HCOId, s.Month, s.Credit,s.Sequence));
                //}

            }

            //List<NewlyDevelopedHCO> dsmSalesTargets = NewlyDevelopedHCOProcessor.Process(sales, employees);
            //List<NewlyDevelopedHCO> dsmSalesTargets = NewlyDevelopedHCOProcessor.ProcessBySalesDate_v2(sales, employees, "2018Q2");
            List<NewlyDevelopedHCO> dsmSalesTargets = NewlyDevelopedHCOProcessor.ProcessBySalesDate_v2(sales, employees, "2018Q1");
            string resultFileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q1\newly_developed_hco_Data.csv";
            //string resultFileName = @"C:\Users\Zhirong.Xie\Desktop\Testing Data\Q2\newly_developed_hco_Data.csv";

            using (var writer = new CsvWriter(new StreamWriter(resultFileName)))
            {
                writer.Configuration.RegisterClassMap<NewlyDevelopedHCOMap>();
                writer.WriteRecords<NewlyDevelopedHCO>(dsmSalesTargets);
                writer.Flush();
            }

        }

    }

}
