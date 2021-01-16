using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Bitmaps
{
    class Program
    {
        /// <summary>
        /// Путь к каталогу с файлами таблиц данных
        /// </summary>
        static string dataPath;
        /// <summary>
        /// Путь к файлу с входными данными
        /// </summary>
        static string inputPath;
        /// <summary>
        /// Путь к файлу с выходными данными
        /// </summary>
        static string outputPath;

        static void Main(string[] args)
        {
            try
            {
                //Чтение аргументов командной строки
                dataPath = args[0];
                inputPath = args[1];
                outputPath = args[2];

                InvisibleJoin();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"При выполнении программы возникло следующее исключение: {ex.Message}");
            }
        }

        //Поля таблицы DimProduct
        static string[] product = {"ProductKey", "ProductAlternateKey", "EnglishProductName", "Color", "SafetyStockLevel", "ReorderPoint",
                    "SizeRange", "DaysToManufacture", "StartDate"};
        //Поля таблицы DimReseller
        static string[] reseller = {"ResellerKey", "ResellerAlternateKey", "Phone", "BusinessType", "ResellerName",
                    "NumberEmployees", "OrderFrequency", "ProductLine", "AddressLinel", "BankName", "YearOpened"};
        //Поля таблицы DimCurrency
        static string[] currency = { "CurrencyKey", "CurrencyAlternateKey", "CurrencyName" };
        //Поля таблицы DimPromotion
        static string[] promotion = {"PromotionKey", "PromotionAlternateKey", "EnglishPromotionName",
                    "EnglishPromotionType", "EnglishPromotionCategory", "StartDate", "EndDate", "MinQty"};
        //Поля таблицы DimSalesTerritory
        static string[] sales = {"SalesTerritoryKey", "SalesTerritoryAlternateKey", "SalesTerritoryRegion",
                    "SalesTerritoryCountry", "SalesTerritoryGroup"};
        //Поля таблицы DimEmployee
        static string[] employee = {"EmployeeKey", "FirstName", "LastName", "Title", "BirthDate", "LoginID",
                    "EmailAddress", "Phone", "MaritalStatus", "Gender", "PayFrequency", "VacationHours",
                    "SickLeaveHours", "DepartmentName", "StartDate"};
        //Поля таблицы DimDate
        static string[] date = {"DateKey", "FullDateAlternateKey", "DayNumberOfWeek", "EnglishDayNameOfWeek",
                    "DayNumberOfMonth", "DayNumberOfYear", "WeekNumberOfYear", "EnglishMonthName", "MonthNumberOfYear",
                    "CalendarQuarter", "CalendarYear", "CalendarSemester", "FiscalQuarter", "FiscalYear",
                    "FiscalSemester"};

        public static void InvisibleJoin()
        {
            List<string> outFields; //список полей таблиц, которые соответствуют выходным данным
            List<string> filters = new List<string>(); //список фильтров в формате {<table>.<field> <operation> <value>}

            //Чтение входных данных
            using (StreamReader sr = new StreamReader(inputPath))
            {
                outFields = new List<string>(sr.ReadLine().Split(','));
                int amountFilters = int.Parse(sr.ReadLine());
                for (int i = 0; i < amountFilters; ++i)
                {
                    filters.Add(sr.ReadLine());
                }
            }

            BitArray bitmap = null; //общий битмап, равный побитовой конъюкции остальных
            foreach (var filter in filters)
            {
                //Обработка фильтра
                string[] filt = SplitFilter(filter); //{table.field, operation, value}
                int index = filt[0].IndexOf('.');
                string tableName = filt[0].Substring(0, index);
                string fieldName = filt[0].Substring(index + 1);
                BitArray current = null; //текущий bitmap
                //Получение bitmap в зависимости от таблицы (фаза 1 и часть фазы 2 алгоритма)
                switch (tableName)
                {
                    case "DimProduct":
                        current = FilterDimTable(tableName, Array.IndexOf(product, fieldName), product[0], filt);
                        break;
                    case "DimReseller":
                        current = FilterDimTable(tableName, Array.IndexOf(reseller, fieldName), reseller[0], filt);
                        break;
                    case "DimCurrency":
                        current = FilterDimTable(tableName, Array.IndexOf(currency, fieldName), currency[0], filt);
                        break;
                    case "DimPromotion":
                        current = FilterDimTable(tableName, Array.IndexOf(promotion, fieldName), promotion[0], filt);
                        break;
                    case "DimSalesTerritory":
                        current = FilterDimTable(tableName, Array.IndexOf(sales, fieldName), sales[0], filt);
                        break;
                    case "DimEmployee":
                        current = FilterDimTable(tableName, Array.IndexOf(employee, fieldName), employee[0], filt);
                        break;
                    case "DimDate":
                        current = FilterDimTable(tableName, Array.IndexOf(date, fieldName), "OrderDateKey", filt);
                        break;
                    case "FactResellerSales":
                        current = FilterFactTable(filt[0], filt);
                        break;
                }
                //Конъюнкция bitmaps (часть фазы 2 алгоритма)
                if (bitmap == null)
                {
                    bitmap = current;
                }
                else
                {
                    bitmap.And(current);
                }
            }

            //Вывод данных в файл
            File.WriteAllText(outputPath, GetOutput(outFields, bitmap));
        }

        /// <summary>
        /// Обрабатывает фильтр таблицы, представляя его в виде массива значимых частей.
        /// </summary>
        /// <param name="filter">исходный фильтр</param>
        /// <returns>фильтр в формате {tableName.fieldName, operation, value}</returns>
        public static string[] SplitFilter(string filter)
        {
            //Примечание. Метод Split из стандартной библиотеки не подходит, так как в value может быть пробел. 
            string[] result = new string[3];
            int index1 = filter.IndexOf(" ");
            int index2 = filter.Substring(index1 + 1).IndexOf(" ") + index1 + 1;
            result[0] = filter.Substring(0, index1); //tableName.fieldName
            result[1] = filter.Substring(index1 + 1, index2 - index1 - 1); //operation
            result[2] = filter.Substring(index2 + 1); //value
            return result;
        }

        /// <summary>
        /// Применяет итоговый bitmap к таблице фактов (столбцу), получая значений полей выходных данных для вывода.
        /// </summary>
        /// <param name="tableName">имя таблицы фактов (столбца)</param>
        /// <param name="bitmap"></param>
        /// <param name="output"></param>
        public static string GetOutput(List<string> tablesNames, BitArray bitmap)
        {
            bool flag = (bitmap == null); //если bitmap пустой, то нет фильтрации
            StringBuilder[] outputs = flag
                ? new StringBuilder[File.ReadAllLines(Path.Combine(dataPath, $"{tablesNames[0]}.csv")).Count()]
                : new StringBuilder[bitmap.Capacity];
            foreach (var tableName in tablesNames)
            {
                int index = 0;
                int counter = 0;
                using (StreamReader sr = new StreamReader(Path.Combine(dataPath, $"{tableName}.csv")))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (flag || bitmap.Get(index++))
                        {
                            if (outputs[counter] == null)
                            {
                                outputs[counter++] = new StringBuilder($"{line}");
                            }
                            else
                            {
                                outputs[counter++].Append($"|{line}");
                            }
                        }
                    }
                }
            }
            string output = (outputs.Count(line => line != null) > 0) //если в массиве есть значения
                ? String.Join("\n", outputs.Where(line => line != null).Select(line => line.ToString()).ToArray()) + "\n"
                : "\n";
            return output;
        }

        /// <summary>
        /// Фильтрует таблицу измерений и применяет полученные ключи к таблице фактов, 
        /// возвращая полученный на фазе 2 агоритма bitmap.
        /// </summary>
        /// <param name="tableName">имя соответствующей таблицы измерений</param>
        /// <param name="fieldIndex">индекс поля в таблице измерений, по которому идёт фильтрация</param>
        /// <param name="key">ключ таблицы измерений</param>
        /// <param name="filter">фильтр</param>
        /// <returns></returns>
        public static BitArray FilterDimTable(string tableName, int fieldIndex, string key, string[] filter)
        {
            //Фаза 1 алгоритма
            HashSet<uint> keys = new HashSet<uint>(); //множество ключей, полученных на фазе 1 алгоритма
            BitArray bitmap;
            using (StreamReader sr = new StreamReader(Path.Combine(dataPath, $"{tableName}.csv")))
            {
                while (!sr.EndOfStream)
                {
                    string[] line = sr.ReadLine().Split('|');
                    if (Operation(line[fieldIndex], filter[2], filter[1]))
                    {
                        keys.Add(uint.Parse(line[0]));
                    }
                }
            }

            //Часть фазы 2 алгоритма
            int size = File.ReadAllLines(Path.Combine(dataPath, $"FactResellerSales.{key}.csv")).Length;
            int index = 0;
            bitmap = new BitArray(size);
            using (StreamReader sr = new StreamReader(Path.Combine(dataPath, $"FactResellerSales.{key}.csv")))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    bitmap.Set(index++, keys.Contains(uint.Parse(line)));
                }
            }
            return bitmap;
        }

        /// <summary>
        /// Фильтрует таблицу фактов, возвращая полученный на фазе 2 агоритма bitmap.
        /// </summary>
        /// <param name="tableName">имя соответствующей таблицы фактов</param>
        /// <param name="filter">фильтр</param>
        /// <returns></returns>
        public static BitArray FilterFactTable(string tableName, string[] filter)
        {
            BitArray bitmap;
            int size = File.ReadAllLines(Path.Combine(dataPath, $"{tableName}.csv")).Length;
            int index = 0;
            bitmap = new BitArray(size);
            using (StreamReader sr = new StreamReader(Path.Combine(dataPath, $"{tableName}.csv")))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    bitmap.Set(index++, Operation(line, filter[2], filter[1]));
                }
            }
            return bitmap;
        }

        /// <summary>
        /// Обрабатывает операцию сравнения и применяет её к данным целочисленным или строковым значениям.
        /// В случае некорректной операции или некорректного значения выбрасывает исключение.
        /// </summary>
        /// <param name="a">значение слева</param>
        /// <param name="b">значение справа</param>
        /// <param name="op">оператор</param>
        /// <returns>результат операции</returns>
        public static bool Operation(string a, string b, string op)
        {
            //Примечание. Представлено две реализации:
            //1) Если мы не выделяем строки с помощью '<string>'
            /*int x, y;
                    if (int.TryParse(a, out x) && int.TryParse(b, out y))
            {
                return OperationInt(x, y, op);
            }
            else
            {
                return OperationString(a, b, op);
            }
            */
            //2) Если строковые значения выделяются с помощью '<string>'
            if (b.Contains('\''))
            {
                return OperationString(a, b.Substring(1, b.Length - 2), op);
            }
            return OperationInt(int.Parse(a), int.Parse(b), op);
        }

        /// <summary>
        /// Обрабатывает операцию сравнения и применяет её к данным строковым значениям.
        /// В случае некорректной операции выбрасывает исключение.
        /// </summary>
        /// <param name="a">значение слева</param>
        /// <param name="b">значение справа</param>
        /// <param name="op">оператор</param>
        /// <returns>результат операции</returns>
        public static bool OperationString(string a, string b, string op)
        {
            switch (op)
            {
                case "=": return a.Equals(b);
                case "<>": return !a.Equals(b);
                default: throw new ArgumentException("Некорректная операция для строк.");
            }
        }

        /// <summary>
        /// Обрабатывает операцию сравнения и применяет её к данным целочисленным значениям.
        /// В случае некорректной операции выбрасывает исключение.
        /// </summary>
        /// <param name="a">значение слева</param>
        /// <param name="b">значение справа</param>
        /// <param name="op">оператор</param>
        /// <returns>результат операции</returns>
        public static bool OperationInt(int a, int b, string op)
        {
            switch (op)
            {
                case "<": return a < b;
                case ">": return a > b;
                case "<=": return a <= b;
                case ">=": return a >= b;
                case "=": return a == b;
                case "<>": return a != b;
                default: throw new ArgumentException("Некорректная операция для целых чисел.");
            }
        }
    }





    /// <summary>
    /// Абстрактный класс, представляющий структуру данных "Битовая карта".
    /// </summary>
    abstract class Bitmap
    {
        /// <summary>
        /// Выполняет операцию логического И текущего Bitmap с другим, записывая результат в вызываемый объект.
        /// </summary>
        /// <param name="other"></param>
        public abstract void And(Bitmap other);
        /// <summary>
        /// Устанавливает значение бита по определенному индексу.
        /// </summary>
        /// <param name="i">индекс бита</param>
        /// <param name="value">устанавливаемое значение</param>
        public abstract void Set(int i, bool value);
        /// <summary>
        /// Возвращает значение бита по определенному индексу.
        /// </summary>
        /// <param name="i">индекс</param>
        /// <returns>значение данного бита</returns>
        public abstract bool Get(int i);
    }

    /// <summary>
    /// Класс, представляющий структуру данных "Битовая карта" с помощью массива 32-ух битных значений.
    /// </summary>
    class BitArray : Bitmap
    {
        private uint[] data; //данные
        public int Capacity { get; private set; } //вместимость СД
        public BitArray(int capacity)
        {
            data = new uint[capacity / 32 + 1];
            Capacity = capacity;
        }

        public override void And(Bitmap other)
        {
            if (other is BitArray) //реализация для 32-ух битных значений
            {
                for (int i = 0; i < data.Length; ++i)
                {
                    data[i] = data[i] & (other as BitArray).Get32(i);
                }
            }
            else //общая реализация
            {
                for (int i = 0; i < data.Length; ++i)
                {
                    for (int j = 0; j < 32; ++j)
                    {
                        int index = (i << 5) + j; //(i << 5) == 32 * i
                        Set(index, Get(index) & other.Get(index));
                    }
                }
            }
        }

        public override void Set(int i, bool value)
        {
            //Примечание. Предложено две реализации:
            //1) Если данный бит не равен устанавливаемому значению, то его необходимо инвертировать.
            if (Get(i) != value)
            {
                data[i / 32] = data[i / 32] ^ (uint)(1 << (i % 32));
            }
            //2) Прямая установка значения бита без проверки.
            /*if(value)
            {
                data[i / 32] = data[i / 32] | (uint)(1 << (i % 32));
            }
            else
            {
                data[i / 32] = data[i / 32] & (~((uint)(1 << (i % 32))));
            }*/
        }

        public override bool Get(int i)
        {
            uint temp = (uint)(1 << (i % 32));
            return (data[i / 32] & temp) == temp;
        }

        private uint Get32(int i) => data[i]; //для 32-ух битных значений; возвращает все 32 бита
    }
}
