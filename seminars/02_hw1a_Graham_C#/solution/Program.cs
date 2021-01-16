using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Graham
{
    class Program
    {
        static void Main(string[] args)
        {
            //Чтение файла
            List<Point> points = ReadFile(args[2]);
            if (points != null)
            {
                //Осуществление прохода по точкам
                List<Point> mbo = GrahamAlgorithm(points);
                if (mbo != null)
                {
                    //Добавляем первую точку в конец стека для удобных преобразований при выводе без потери первой точки
                    mbo.Add(mbo[0]);
                    //Если нужен проход по часовой стрелке, то переворачиваем последовательность
                    if (args[0] == "cw")
                    {
                        mbo.Reverse();
                    }
                    //Выбор соответствующего формата записи данных в файл
                    if (args[1] == "plain")
                    {
                        mbo.RemoveAt(mbo.Count - 1);
                        WriteFilePlain(args[3], mbo);
                    }
                    else
                    {
                        WriteFileWKT(args[3], points, mbo);
                    }
                }
            }
        }

        /// <summary>
        /// Осуществляет чтение файла. Если возникла ошибка, то возвращает null
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<Point> ReadFile(string path)
        {
            try
            {
                string[] input = File.ReadAllLines(path);
                int count = int.Parse(input[0]);
                List<Point> points = new List<Point>();
                for (int i = 1; i <= count; ++i)
                {
                    string[] point = input[i].Split(' ');
                    int x = int.Parse(point[0]);
                    int y = int.Parse(point[1]);
                    points.Add(new Point(x, y));
                }
                return points;
            }
            catch
            {
                Console.WriteLine("Oops... Something wrong with reading the file.");
                return null;
            }
        }

        /// <summary>
        /// Осуществляет поиск минимальной выпуклой оболочки с помощью прохода Грэхема на основе стека
        /// Проход осуществляется против часовой стрелки
        /// </summary>
        /// <param name="input">исходная последовательность точек</param>
        /// <returns>последовательность точек, которые образуют минимальную выпуклую оболочку</returns>
        public static List<Point> GrahamAlgorithm(List<Point> input)
        {
            List<Point> points = input;
            //Первая точка - точка с минимальной координатой Y и с минимальной координатой X при наличии совпадений
            Point p0 = points.OrderBy(point => point.Y).ThenBy(point => point.X).First();
            //Удаляем первую точку из последовательности для дальнейшего прохода
            points.Remove(p0);
            //Сортируем последовательность в порядке возрастания полярного угла относительно первой точки
            //и при равенстве полярных углов в порядке возрастания расстояния относительно первой точки
            points = points.OrderBy(point => p0.PolarAngle(point)).ThenBy(point => p0.Distance(point)).ToList();
            //Удаляем точки с одинаковыми полярными углами относительно первой точки, оставляя самую дальнюю
            points.RemoveAll(point => point != points.Last() && p0.PolarAngle(point) == p0.PolarAngle(points[points.IndexOf(point) + 1]));

            if(points.Count >= 2)
            {
                //Создаём пустой стек
                Stack<Point> stack = new Stack<Point>(points.Count);
                //Добавляем первые точки
                stack.Push(p0);
                stack.Push(points[0]);
                //Осуществляем проход по остальным точкам
                for (int i = 1; i < points.Count; ++i)
                {
                    //Удаляем точки из стека, пока они не образуют левый поворот
                    while(!IsLeftTurn(stack.NextToTop(), stack.Top(), points[i]))
                    {
                        stack.Pop();
                    }
                    //Добавляем текущую точку в стек для дальнейшего прохода
                    stack.Push(points[i]);
                }
                //Извлекаем данные из стека, сохраняя порядок
                int size = stack.Size();
                List<Point> result = new List<Point>();
                for(int i = 0; i < size; ++i)
                {
                    result.Add(stack.Pop());
                }
                result.Reverse();
                return result;
            }
            //Выпуклая оболочка пуста
            return null;
        }

        /// <summary>
        /// Возвращает значение, образуют ли точки поворот влево
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        public static bool IsLeftTurn(Point p1, Point p2, Point p3)
        {
            //Если векторное произведение положительно, то точки образуют левый поворот
            return (p2.X - p1.X) * (p3.Y - p2.Y) - (p2.Y - p1.Y) * (p3.X - p2.X) > 0;
        }

        /// <summary>
        /// Осуществляет запись в файл в формате Plain
        /// </summary>
        /// <param name="path">путь к файлу</param>
        /// <param name="points">последовательность точек</param>
        public static void WriteFilePlain(string path, List<Point> points)
        {
            //Формирование текста для записи в файл
            string result = string.Empty;
            result += $"{points.Count}\n";
            for(int i = 0; i < points.Count - 1; ++i)
            {
                result += $"{points[i].X} {points[i].Y}\n";
            }
            result += $"{points[points.Count - 1].X} {points[points.Count - 1].Y}";

            //Запись в файл
            try
            {
                File.WriteAllText(path, result);
            }
            catch
            {
                Console.WriteLine("Oops... Something wrong with writing to the file.");
            }
        }

        /// <summary>
        /// Осуществляет запись в файл в формате WKT
        /// </summary>
        /// <param name="path">путь к файлу</param>
        /// <param name="input">последовательность точек MULTIPOINT</param>
        /// <param name="points">последовательность точек POLYGON</param>
        public static void WriteFileWKT(string path, List<Point> input, List<Point> points)
        {
            //Формирование текста для записи в файл
            string result = string.Empty;
            result += $"MULTIPOINT (({input[0].X} {input[0].Y}),";
            for(int i = 1; i < input.Count - 1; ++i)
            {
                result += $" ({input[i].X} {input[i].Y}),";
            }
            result += $" ({input[input.Count - 1].X} {input[input.Count - 1].Y}))\n";

            result += $"POLYGON (({points[0].X} {points[0].Y}),";
            for (int i = 1; i < points.Count - 1; ++i)
            {
                result += $" ({points[i].X} {points[i].Y}),";
            }
            result += $" ({points[points.Count - 1].X} {points[points.Count - 1].Y}))";

            //Запись в файл
            try
            {
                File.WriteAllText(path, result);
            }
            catch
            {
                Console.WriteLine("Oops... Something wrong with writing to the file.");
            }
        }
    }

    /// <summary>
    /// Представляет структуру данных стек
    /// </summary>
    /// <typeparam name="T">тип элементов стека</typeparam>
    class Stack<T>
    {
        List<T> items; //данные стека
        public const int maxCapacity = int.MaxValue; //максимально допустимый размер стека

        public Stack(int capacity = maxCapacity)
        {
            if (capacity > 0 && capacity <= maxCapacity)
            {
                items = new List<T>(capacity);
            }
            else throw new ArgumentException("Неверный параметр для размера стека.");
        }

        /// <summary>
        /// Добавляет новый элемент в стек. Если стек переполнен, выбрасывает исключение StackOverflowException
        /// </summary>
        /// <param name="item"></param>
        public void Push(T item)
        {
            if (items.Capacity != Size())
            {
                items.Add(item);
            }
            else throw new StackOverflowExceprion();
        }

        /// <summary>
        /// Возвращает и удаляет элемент из вершины стека. Если стек пуст, выбрасывает исключение StackUnderflowException
        /// </summary>
        /// <returns></returns>
        public T Pop()
        {
            if (!IsEmpty())
            {
                T item = items[Size() - 1];
                items.RemoveAt(Size() - 1);
                return item;
            }
            else throw new StackUnderflowExceprion();
        }

        /// <summary>
        /// Возращает элемент из вершины стека. Если стек пуст, возвращает значение по умолчанию
        /// </summary>
        /// <returns></returns>
        public T Top()
        {
            if (!IsEmpty())
            {
                return items[Size() - 1];
            }
            else return default;
        }

        /// <summary>
        /// Возвращает следующий элемент после вершины стека. Если в стеке меньше 2 элементов, возвращает значение по умолчанию
        /// </summary>
        /// <returns></returns>
        public T NextToTop()
        {
            if (Size() > 1)
            {
                return items[Size() - 2];
            }
            else return default;
        }

        /// <summary>
        /// Возвращает размер стека
        /// </summary>
        /// <returns></returns>
        public int Size() => items.Count;

        /// <summary>
        /// Возращает флаг, пуст ли стек
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty() => Size() == 0;
    }

    /// <summary>
    /// Представляет точку на плоскости
    /// </summary>
    class Point
    {
        public Point(int x, int y)
        {
            if (x >= 0 && x <= 10000 && y >= 0 && y <= 10000)
            {
                X = x;
                Y = y;
            }
            else throw new ArgumentException("Неверные параметры для координат точки.");
        }

        /// <summary>
        /// Координата по оси абцисс
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Координата по оси ординат
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Квадрат расстояния между точками
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int Distance(Point other) => (int)(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));

        /// <summary>
        /// Полярный угол между точками
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public double PolarAngle(Point other) => Math.Atan2(other.Y - Y, other.X - X);
    }

    /// <summary>
    /// Это исключение выбрасывается при переполнении стека
    /// </summary>
    class StackOverflowExceprion : Exception
    {
        public StackOverflowExceprion() : base() { }
        public StackOverflowExceprion(string message) : base(message) { }
        public StackOverflowExceprion(string message, Exception innerException) : base (message, innerException) { }
        protected StackOverflowExceprion(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base (info, context) { }
    }

    /// <summary>
    /// Это исключение выбрасывается при удалении элемента из пустого стека
    /// </summary>
    class StackUnderflowExceprion : Exception
    {
        public StackUnderflowExceprion() : base() { }
        public StackUnderflowExceprion(string message) : base(message) { }
        public StackUnderflowExceprion(string message, Exception innerException) : base(message, innerException) { }
        protected StackUnderflowExceprion(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
