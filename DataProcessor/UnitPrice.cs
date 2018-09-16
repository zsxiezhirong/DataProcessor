using CsvHelper.Configuration;
using DataProcessor.BrightFuture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor
{
    public class UnitPrice
    {
        public int NO { get; set; }
        public int PRIMARY_ID { get; set; }
        public int YEAR { get; set; }
        public int BONUS_IN { get; set; }
        public string AREA_CODE { get; set; }
        public string AREA_NAME { get; set; }
        public string PROD_NO { get; set; }
        public string PROD_NAME { get; set; }
        public double PROD_SCORE { get; set; }
        public int FILLER_CODE { get; set; }
        public string FILLER_NAME { get; set; }
        public string FILLER_DATE { get; set; }
        public int STATUS { get; set; }
    }

    public class UnitPriceClassMap : ClassMap<UnitPrice>
    {
        public UnitPriceClassMap()
        {
            Map(m => m.NO).Name("NO");
            Map(m => m.PRIMARY_ID).Name("PRIMARY_ID");
            Map(m => m.YEAR).Name("YEAR");
            Map(m => m.BONUS_IN).Name("BONUS_IN");
            Map(m => m.AREA_CODE).Name("AREA_CODE");
            Map(m => m.AREA_NAME).Name("AREA_NAME");
            Map(m => m.PROD_NO).Name("PROD_NO");
            Map(m => m.PROD_NAME).Name("PROD_NAME");
            Map(m => m.PROD_SCORE).Name("PROD_SCORE");
            Map(m => m.FILLER_CODE).Name("FILLER_CODE");
            Map(m => m.FILLER_NAME).Name("FILLER_NAME");
            Map(m => m.FILLER_DATE).Name("FILLER_DATE");
            Map(m => m.STATUS).Name("STATUS");
        }
    }
    public class UnitPriceProcessor
    {
        public static IEnumerable<Sales> Process(IEnumerable<Sales> sales, IEnumerable<UnitPrice> thisQuarterUnitPrices, IEnumerable<UnitPrice> lastQuarterUnitPrices)
        {

            foreach (var s in sales)
            {
                double unitPrice = 0d;
                var thisQuarterUnitPrice = from p in thisQuarterUnitPrices
                                       where p.AREA_CODE == s.AreaId && p.PROD_NO == s.ProductId
                                       select p.PROD_SCORE;
                unitPrice = thisQuarterUnitPrice.FirstOrDefault();

                s.ThisQuarterCredit = s.SalesQty * unitPrice;

               var lastQuarterUnitPrice = from p in lastQuarterUnitPrices
                                       where p.AREA_CODE == s.AreaId && p.PROD_NO == s.ProductId
                                       select p.PROD_SCORE;
                unitPrice = lastQuarterUnitPrice.FirstOrDefault();

                s.LastQuarterCredit = s.SalesQty * unitPrice;


            }

            return sales;
        }
    }

    }
