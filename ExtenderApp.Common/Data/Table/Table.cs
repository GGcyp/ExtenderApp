namespace ExtenderApp.Common.Data
{
    public class Table : Table<object>
    {

    }

    public struct TableRow<T>
    {
        public int columnCount => m_ColumnDataList.Count;
        private ValueList<T> m_ColumnDataList;
        public T this[int index]
        {
            get => m_ColumnDataList[index - 1];
            set => m_ColumnDataList[index] = value;
        }

        public TableRow(int cloumn)
        {
            m_ColumnDataList = new ValueList<T>(cloumn);
        }

        public TableRow(T[] columnArray)
        {
            m_ColumnDataList = new ValueList<T>(columnArray);
        }

        public void Add(T item) => m_ColumnDataList.Add(item);

        public void Remove(T item) => m_ColumnDataList.Remove(item);

        public void Remove(int index) => m_ColumnDataList.RemoveAt(index - 1);

        public bool Equals(TableRow<T> row)
        {
            return row.m_ColumnDataList.Equals(m_ColumnDataList);
        }
    }

    public class Table<T>
    {
        public int rowCount => m_RowDataList.Count;

        private ValueList<TableRow<T>> m_RowDataList;
        public TableRow<T> this[int rowIndex]
        {
            get => m_RowDataList[rowIndex - 1];
            set => m_RowDataList[rowIndex - 1] = value;
        }
        public T this[int rowIndex, int colIndex]
        {
            get => m_RowDataList[rowIndex - 1][colIndex];
            set
            {
                TableRow<T> row = m_RowDataList[rowIndex - 1];
                row[colIndex] = value;
            }
        }

        public Table() : this(4)
        {

        }

        public Table(int rowCount)
        {
            m_RowDataList = new ValueList<TableRow<T>>(rowCount);
        }

        public Table(TableRow<T>[] tableRowArray)
        {
            m_RowDataList = new ValueList<TableRow<T>>(tableRowArray);
        }

        public void Add(T[] array)
        {
            TableRow<T> row = new TableRow<T>(array);
            Add(row);
        }

        public void Add(TableRow<T> RowData) => m_RowDataList.Add(RowData);

        public void Remove(TableRow<T> tableRow) => m_RowDataList.Remove(tableRow);

        public void RemoveAt(int index) => m_RowDataList.RemoveAt(index);
    }
}
