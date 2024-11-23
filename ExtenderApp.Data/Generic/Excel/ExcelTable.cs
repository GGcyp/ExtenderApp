namespace ExtenderApp.Data
{
    /// <summary>
    /// 专门用来存放Excel的数据表,默认内部数据为String
    /// </summary>
    public class ExcelTable
    {
        private static readonly string m_DefaulSheetName = "Sheet";

        /// <summary>
        /// 篇幅数
        /// </summary>
        private ValueList<ValueTuple<string, Table<string>, Table<ExcelCellFormat>>> m_TableList;
        public ValueTuple<string, Table<string>, Table<ExcelCellFormat>> this[int index]
        {
            get=> m_TableList[index];
            set=> m_TableList[index] = value;
        }
        public ValueTuple<string, Table<string>, Table<ExcelCellFormat>> this[string sheetName]
        {
            get
            {
                int index = FindFirstSheetIndex(sheetName);
                if (index == -1) return default;

                return m_TableList[index];
            }
            set
            {
                int index = FindFirstSheetIndex(sheetName);
                if (index == -1) return;

                m_TableList[index] = value;
            }
        }
        public int Count => m_TableList.Count;

        public ExcelTable()
        {
            m_TableList = new();
        }

        public ExcelTable(int cloumn)
        {
            m_TableList = new(cloumn);
        }

        public ExcelTable(ValueTuple<string, Table<string>, Table<ExcelCellFormat>>[] columnArray)
        {
            m_TableList = new(columnArray);
        }

        public void Add(Table<string> table)
        {
            Add(string.Empty, table);
        }

        public void Add(string sheetName, Table<string> table)
        {
            if (string.IsNullOrEmpty(sheetName)) sheetName = $"{m_DefaulSheetName}{Count}";
            Add(new ValueTuple<string, Table<string>>(sheetName, table));
        }

        public void Add(string sheetName, Table<string> table, Table<ExcelCellFormat> excelCellFormatTable)
        {
            if (string.IsNullOrEmpty(sheetName)) sheetName = $"{m_DefaulSheetName}{Count}";
            Add(new ValueTuple<string, Table<string>, Table<ExcelCellFormat>>(sheetName, table, excelCellFormatTable));
        }

        public void Add(ValueTuple<string, Table<string>> item)
        {
            Add((item.Item1, item.Item2, null));
        }

        public void Add(ValueTuple<string, Table<string>, Table<ExcelCellFormat>> item)
        {
            if (item.Item2 == null) throw new ArgumentNullException($"table is null of the sheet {item.Item1}");
            m_TableList.Add(item);
        }

        public ValueTuple<string, Table<string>, Table<ExcelCellFormat>> Remove(string sheetName)
        {
            ValueTuple<string, Table<string>, Table<ExcelCellFormat>> result = default;
            int index = FindFirstSheetIndex(sheetName);
            if (index < 0) return result;

            result = m_TableList[index];
            RemoveAt(index);

            return result;
        }

        //private int FindFirstSheetIndex(Table<string> table)
        //{
        //    return FindFirstSheetIndex((string.Empty, table));
        //}

        private int FindFirstSheetIndex(string sheetName)
        {
            return FindFirstSheetIndex((sheetName, null));
        }

        private int FindFirstSheetIndex(ValueTuple<string, Table<string>> item)
        {
            bool hasSheetName = !string.IsNullOrEmpty(item.Item1);
            bool hasTable = item.Item2 != null;

            for (int i = 0; i < Count; i++)
            {
                ValueTuple<string, Table<string>, Table<ExcelCellFormat>> tempItem = m_TableList[i];
                if (hasSheetName && tempItem.Item1 == item.Item1)
                {
                    return i;
                }

                if (hasTable && item.Item2 == tempItem.Item2)
                {
                    return i;
                }
            }

            return -1;
        }

        public void RemoveAt(int index)
        {
            m_TableList.RemoveAt(index);
        }
    }
}
