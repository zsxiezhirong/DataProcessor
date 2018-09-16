using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.BrightFuture
{
    public class Meina_Territory
    {
        public int ym { get; set; }
        public string region { get; set; }
        public string gm_position_name { get; set; }
        public string gm_emp_code { get; set; }
        public string gm_name { get; set; }
        public string sd_position_name { get; set; }
        public string sd_emp_code { get; set; }
        public string sd_name { get; set; }
        public string rd_position_name { get; set; }
        public string rd_emp_code { get; set; }
        public string rd_name { get; set; }
        public string asm_position_name { get; set; }
        public string asm_emp_code { get; set; }
        public string asm_name { get; set; }
        public string rep_position_name { get; set; }
        public string rep_emp_code { get; set; }
        public string rep_name { get; set; }
        public string hco_code { get; set; }
        public string province { get; set; }
        public string city { get; set; }
        public string hco_name { get; set; }
        public string product_name { get; set; }
        public string Dermatix { get; set; }
        public string Espumisan { get; set; }
        public string Ezerra { get; set; }
        public string Fastum { get; set; }
        public string Gengigel { get; set; }
        public string Kestine { get; set; }
        public string Letrox { get; set; }
        public string Neuquinon { get; set; }
        public string Priligy { get; set; }
        public string Spasmomen { get; set; }
        public string Zostex { get; set; }
        public string Rilaten { get; set; }
        public string Easyhaler { get; set; }
    }

    public class Meina_TerritoryClassMap : ClassMap<Meina_Territory>
    {
        public Meina_TerritoryClassMap()
        {
            Map(m => m.ym).Name("ym");
            Map(m => m.region).Name("region");
            Map(m => m.gm_position_name).Name("gm_position_name");
            Map(m => m.gm_emp_code).Name("gm_emp_code");
            Map(m => m.gm_name).Name("gm_name");
            Map(m => m.sd_position_name).Name("sd_position_name");
            Map(m => m.sd_emp_code).Name("sd_emp_code");
            Map(m => m.sd_name).Name("sd_name");
            Map(m => m.rd_position_name).Name("rd_position_name");
            Map(m => m.rd_emp_code).Name("rd_emp_code");
            Map(m => m.rd_name).Name("rd_name");
            Map(m => m.asm_position_name).Name("asm_position_name");
            Map(m => m.asm_emp_code).Name("asm_emp_code");
            Map(m => m.asm_name).Name("asm_name");
            Map(m => m.rep_position_name).Name("rep_position_name");
            Map(m => m.rep_emp_code).Name("rep_emp_code");
            Map(m => m.rep_name).Name("rep_name");
            Map(m => m.hco_code).Name("hco_code");
            Map(m => m.province).Name("province");
            Map(m => m.city).Name("city");
            Map(m => m.hco_name).Name("hco_name");
            Map(m => m.product_name).Name("product_name_0");
            Map(m => m.Dermatix).Name("product_name");
            Map(m => m.Espumisan).Name("列1");
            Map(m => m.Ezerra).Name("列2");
            Map(m => m.Fastum).Name("列3");
            Map(m => m.Gengigel).Name("列4");
            Map(m => m.Kestine).Name("列5");
            Map(m => m.Letrox).Name("列6");
            Map(m => m.Neuquinon).Name("列7");
            Map(m => m.Priligy).Name("列8");
            Map(m => m.Spasmomen).Name("列9");
            Map(m => m.Zostex).Name("列10");
            Map(m => m.Rilaten).Name("列11");
            Map(m => m.Easyhaler).Name("列12");
        }
    }

    public class Meina_Territory_Processor
    {
        public static List<Meina_Territory> Process(IEnumerable<Meina_Territory> rawList)
        {
            List<Meina_Territory> results = new List<Meina_Territory>();

            foreach(Meina_Territory t in rawList)
            {
                if(t.Dermatix.Trim().ToLower() == "y")
                {
                    Meina_Territory row = new Meina_Territory();
                    CloneTerritory(t, row);

                    row.product_name = "Dermatix";
                    results.Add(row);
                }
                if (t.Espumisan.Trim().ToLower() == "y")
                {
                    Meina_Territory row = new Meina_Territory();
                    CloneTerritory(t, row);

                    row.product_name = "Espumisan";
                    results.Add(row);
                }
                if (t.Ezerra.Trim().ToLower() == "y")
                {
                    Meina_Territory row = new Meina_Territory();
                    CloneTerritory(t, row);

                    row.product_name = "Ezerra";
                    results.Add(row);
                }
                if (t.Fastum.Trim().ToLower() == "y")
                {
                    Meina_Territory row = new Meina_Territory();
                    CloneTerritory(t, row);

                    row.product_name = "Fastum";
                    results.Add(row);
                }
                if (t.Gengigel.Trim().ToLower() == "y")
                {
                    Meina_Territory row = new Meina_Territory();
                    CloneTerritory(t, row);

                    row.product_name = "Gengigel";
                    results.Add(row);
                }
                if (t.Kestine.Trim().ToLower() == "y")
                {
                    Meina_Territory row = new Meina_Territory();
                    CloneTerritory(t, row);

                    row.product_name = "Kestine";
                    results.Add(row);
                }
                if (t.Letrox.Trim().ToLower() == "y")
                {
                    Meina_Territory row = new Meina_Territory();
                    CloneTerritory(t, row);

                    row.product_name = "Letrox";
                    results.Add(row);
                }
                if (t.Neuquinon.Trim().ToLower() == "y")
                {
                    Meina_Territory row = new Meina_Territory();
                    CloneTerritory(t, row);

                    row.product_name = "Neuquinon";
                    results.Add(row);
                }
                if (t.Priligy.Trim().ToLower() == "y")
                {
                    Meina_Territory row = new Meina_Territory();
                    CloneTerritory(t, row);

                    row.product_name = "Priligy";
                    results.Add(row);
                }
                if (t.Spasmomen.Trim().ToLower() == "y")
                {
                    Meina_Territory row = new Meina_Territory();
                    CloneTerritory(t, row);

                    row.product_name = "Spasmomen";
                    results.Add(row);
                }
                if (t.Zostex.Trim().ToLower() == "y")
                {
                    Meina_Territory row = new Meina_Territory();
                    CloneTerritory(t, row);

                    row.product_name = "Zostex";
                    results.Add(row);
                }
                if (t.Rilaten.Trim().ToLower() == "y")
                {
                    Meina_Territory row = new Meina_Territory();
                    CloneTerritory(t, row);

                    row.product_name = "Rilaten";
                    results.Add(row);
                }
                if (t.Easyhaler.Trim().ToLower() == "y")
                {
                    Meina_Territory row = new Meina_Territory();
                    CloneTerritory(t, row);

                    row.product_name = "Easyhaler";
                    results.Add(row);
                }

            }

            return results;
        }

        private static void CloneTerritory(Meina_Territory t, Meina_Territory row)
        {
            row.ym = t.ym;
            row.region = t.region;
            row.gm_position_name = t.gm_position_name;
            row.gm_emp_code = t.gm_emp_code;
            row.gm_name = t.gm_name;
            row.sd_position_name = t.sd_position_name;
            row.sd_emp_code = t.sd_emp_code;
            row.sd_name = t.sd_name;
            row.rd_position_name = t.rd_position_name;
            row.rd_emp_code = t.rd_emp_code;
            row.rd_name = t.rd_name;
            row.asm_position_name = t.asm_position_name;
            row.asm_emp_code = t.asm_emp_code;
            row.asm_name = t.asm_name;
            row.rep_position_name = t.rep_position_name;
            row.rep_emp_code = t.rep_emp_code;
            row.rep_name = t.rep_name;
            row.hco_code = t.hco_code;
            row.province = t.province;
            row.city = t.city;
            row.hco_name = t.hco_name;
        }
    }
}
