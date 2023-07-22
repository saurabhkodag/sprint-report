namespace GIH_Report.Models
{
    public class ExcelConfig
    {
        public string SheetName { get; set; }

        public List<Column> Column { get; set; }
    }

    public class Column
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public string DataProperty { get; set; }
    }
}
