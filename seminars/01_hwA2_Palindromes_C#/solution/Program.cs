using System;
using System.IO;

namespace Palindroms
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //Чтение файла, содержащего входные данные
                string input = File.ReadAllText(args[0]);

                int odd = OddPodpalindroms(input); //количество палиндромов нечётной длины
                int even = EvenPodpalindroms(input); //количество палиндромов чётной длины

                //Запись в файл выходных данных в формате "<общее количество> <чётной длины> <нечётной длины>"
                File.WriteAllText(args[1], $"{odd + even} {even} {odd}");
            }
            catch
            {
                Console.WriteLine("Ooops, something is wrong...");
            }
        }

        /// <summary>
        /// Считает количество палиндромов нечётной длины в исходной строке
        /// </summary>
        /// <param name="input">исходная строка</param>
        /// <returns>количество палиндромов нечётной длины</returns>
        public static int OddPodpalindroms(string input)
        {
            int n = input.Length; //длина строки
            int l = 0; //левая граница самого правого палиндрома
            int r = -1;//правая граница самого правого палиндрома
            int[] d1 = new int[n]; //d1[i] - количество палиндромов нечётной длины с центром в позиции i

            //Проход по всем элементам
            for (int i = 0; i < n; ++i)
            {
                /*
                 * Если мы находимся внутри самого правого палиндрома (i <= r), то можем отразить позицию i 
                 * внутри палиндрома в позицию l + r - i, для которой уже посчитано количество палиндромов, 
                 * но палиндром может касаться границы или выходить за границу большого внешнего палиндрома, то есть 
                 * i + d1[j] - 1 >= r (или j - d1[j] + 1 <= l), тогда сократим длину до этого значения.
                 * Если мы находимся не внутри самого правого палиндрома (i > r), то запускаем поиск обычным способом.
                 */
                int j = i <= r ? Math.Min(d1[l + r - i], r - i + 1) : 1; //количество палиндромов нечётной длины с центром в позиции i
                //Поиск палиндромов обычным способом
                while (i - j >= 0 && i + j < n && input[i + j] == input[i - j])
                    ++j;
                d1[i] = j;
                //Обновление значений границ самого правого палиндрома
                if (i + j - 1 > r)
                {
                    l = i - j + 1;
                    r = i + j - 1;
                }
            }
            //Просмотр массива d1
            //Array.ForEach(d1, x => Console.Write($"{x} ")); Console.WriteLine();
            return ArraySum(d1);
        }

        /// <summary>
        /// Считает количество палиндромов четной длины в исходной строке
        /// </summary>
        /// <param name="input">исходная строка</param>
        /// <returns>количество палиндромов чётной длины</returns>
        public static int EvenPodpalindroms(string input)
        {
            int n = input.Length; //длина строки
            int l = 0; //левая граница самого правого палиндрома
            int r = -1;//правая граница самого правого палиндрома
            int[] d2 = new int[n]; //d2[i] - количество палиндромов чётной длины с центром в позиции i

            //Проход по всем элементам
            for (int i = 0; i < n; ++i)
            {
                //Аналогичный алгоритм, описанный в методе OddPodPalindroms, но с иными индексами ввиду чётности длины
                int j = i > r ? 0 : Math.Min(d2[l + r - i + 1], r - i + 1);//количество палиндромов чётной длины с центром в позиции i
                //Поиск палиндрома обычным способом
                while (i + j < n && i - j - 1 >= 0 && input[i + j] == input[i - j - 1])
                    ++j;
                d2[i] = j;
                //Обновление значений границ самого правого палиндрома
                if (i + j - 1 > r)
                {
                    l = i - j;
                    r = i + j - 1;
                }
            }
            //Просмотр массива d2
            //Array.ForEach(d2, x => Console.Write($"{x} ")); Console.WriteLine();
            return ArraySum(d2);
        }

        /// <summary>
        /// Считает сумму всех элементов массива
        /// </summary>
        /// <param name="arr">исходный массив</param>
        /// <returns>сумма элементов</returns>
        public static int ArraySum(int[] arr)
        {
            int sum = 0;
            for (int i = 0; i < arr.Length; ++i) sum += arr[i];
            return sum;
        }
    }
}
