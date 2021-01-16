using namespace std;
#include <iostream>
#include <fstream>
#include <exception>
#include <cmath>
#include <algorithm>
#include <vector>
#include <string>

/// <summary>
/// Представляет точку на плоскости
/// </summary>
class Point
{
public:
    Point() : Point(0, 0) {}
    Point(int x, int y)
    {
        if (x >= 0 && x <= 10000 && y >= 0 && y <= 10000)
        {
            this->x = x;
            this->y = y;
        }
        else throw "Неверные параметры для координат точки.";
    }
    /// <summary>
    /// Координата по оси абцисс
    /// </summary>
    /// <returns></returns>
    int getX()
    {
        return x;
    }
    /// <summary>
    /// Координата по оси ординат
    /// </summary>
    /// <returns></returns>
    int getY()
    {
        return y;
    }
    /// <summary>
    /// Квадрат расстояния между точками
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    int distance(Point& other)
    {
        return pow(x - other.getX(), 2) + pow(y + other.getY(), 2);
    }
    /// <summary>
    /// Полярный угол между точками
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    double polarAngle(Point& other)
    {
        return atan2(other.getY() - y, other.getX() - x);
    }

private:
    int x;
    int y;
};





/// <summary>
/// Представляет структуру данных стек
/// </summary>
class Stack
{
public:
    static const int maxCapacity = 10000; //максимально допустимый размер стека
    Stack(int capacity = maxCapacity)
    {
        if (0 < capacity && capacity <= maxCapacity)
        {
            this->capacity = capacity;
        }
        else throw "Неверный параметр для размера стека.";
    }
    /// <summary>
    /// Добавляет новый элемент в стек. Если стек переполнен, выбрасывает исключение overflow_exception
    /// </summary>
    /// <param name="item"></param>
    void push(Point& item)
    {
        if (capacity != items.size())
        {
            items.push_back(item);
        }
        else throw overflow_error("Стек переполнен.");
    }
    /// <summary>
    /// Возвращает и удаляет элемент из вершины стека. Если стек пуст, выбрасывает исключение underflow_exception
    /// </summary>
    /// <returns></returns>
    Point pop()
    {
        if (!isEmpty())
        {
            Point item = items[size() - 1];
            items.pop_back();
            return item;
        }
        else throw underflow_error("Стек пуст.");
    }
    /// <summary>
    /// Возращает элемент из вершины стека
    /// </summary>
    /// <returns></returns>
    Point top()
    {
        return items.back();
    }
    /// <summary>
    /// Возвращает следующий элемент после вершины стека
    /// </summary>
    /// <returns></returns>
    Point nextToTop()
    {
        Point top = pop();
        Point next = items.back();
        items.push_back(top);
        return next;
    }
    /// <summary>
    /// Возвращает размер стека
    /// </summary>
    /// <returns></returns>
    int size()
    {
        return items.size();
    }
    /// <summary>
    /// Возращает флаг, пуст ли стек
    /// </summary>
    /// <returns></returns>
    bool isEmpty()
    {
        return items.empty();
    }

private:
    vector<Point> items; //данные стека
    int capacity; //вместимость стека
};





/// <summary>
/// Осуществляет чтение файла
/// </summary>
/// <param name="path">путь к файлу</param>
/// <returns></returns>
static vector<Point> readFile(string path)
{
    vector<Point> result;
    try
    {
        ifstream in;
        in.open(path);
        int n;
        in >> n;
        for (int i = 0; i < n; ++i)
        {
            int x, y;
            in >> x >> y;
            result.push_back(Point(x, y));
        }
        in.close();
        return result;
    }
    catch (exception)
    {
        cout << "Ooops... Something wrong with reading the file.";
        return vector<Point>();
    }
}

/// <summary>
/// Осуществляет запись в файл в формате WKT
/// </summary>
/// <param name="path">путь к файлу</param>
/// <param name="input">последовательность точек MULTIPOINT</param>
/// <param name="points">последовательность точек POLYGON</param>
static void writeFileWKT(string path, vector<Point> input, vector<Point> points)
{
    //Формирование текста для записи в файл
    string output;
    output += "MULTIPOINT ((" + to_string(input[0].getX()) + " " + to_string(input[0].getY()) + "),";
    for (int i = 1; i < input.size() - 1; ++i)
    {
        output += " (" + to_string(input[i].getX()) + " " + to_string(input[i].getY()) + "),";
    }
    output += " (" + to_string(input[input.size() - 1].getX()) + " " + to_string(input[input.size() - 1].getY()) + "))\n";

    output += "POLYGON ((" + to_string(points[0].getX()) + " " + to_string(points[0].getY()) + "),";
    for (int i = 1; i < points.size() - 1; ++i)
    {
        output += " (" + to_string(points[i].getX()) + " " + to_string(points[i].getY()) + "),";
    }
    output += " (" + to_string(points[points.size() - 1].getX()) + " " + to_string(points[points.size() - 1].getY()) + "))\n";

    //Запись в файл
    try
    {
        ofstream out(path);
        if (out.is_open())
        {
            out << output;
        }
        out.close();
    }
    catch (exception)
    {
        cout << "Ooops.. Something wrong with writing the file.";
        return;
    }
}

/// <summary>
/// Осуществляет запись в файл в формате Plain
/// </summary>
/// <param name="path">путь к файлу</param>
/// <param name="points">последовательность точек</param>
static void writeFilePlain(string path, vector<Point> points)
{
    //Формирование текста для записи в файл
    string output;
    output += to_string(points.size()) + "\n";
    for (int i = 0; i < points.size() - 1; ++i)
    {
        output += to_string(points[i].getX()) + " " + to_string(points[i].getY()) + "\n";
    }
    output += to_string(points[points.size() - 1].getX()) + " " + to_string(points[points.size() - 1].getY());

    //Запись в файл
    try
    {
        ofstream out;
        out.open(path);
        out << output;
        out.close();
    }
    catch (exception)
    {
        cout << "Ooops.. Something wrong with writing the file.";
        return;
    }
}





/// <summary>
/// Возвращает значение, образуют ли точки поворот влево
/// </summary>
/// <param name="p1"></param>
/// <param name="p2"></param>
/// <param name="p3"></param>
/// <returns></returns>
static bool isLeftTurn(Point p1, Point p2, Point p3)
{
    //Если векторное произведение положительно, то точки образуют левый поворот
    return (p2.getX() - p1.getX()) * (p3.getY() - p2.getY()) - (p2.getY() - p1.getY()) * (p3.getX() - p2.getX()) > 0;
}

/// <summary>
/// Компоратор для сортировки сначала по осе ординат, потом по оси абцисс
/// </summary>
/// <param name="p1"></param>
/// <param name="p2"></param>
/// <returns></returns>
static bool cmp1(Point p1, Point p2)
{
    return p1.getY() < p2.getY() || p1.getY() == p2.getY() && p1.getX() < p2.getX();
}

/// <summary>
/// Компоратор для сортировки сначала по полярному углу с точкой p0, потом по дистанции относительно точки p0
/// </summary>
/// <param name="p0"></param>
/// <param name="p1"></param>
/// <param name="p2"></param>
/// <returns></returns>
static bool cmp2(Point p0, Point p1, Point p2)
{
    return p0.polarAngle(p1) < p0.polarAngle(p2) || p0.polarAngle(p1) == p0.polarAngle(p2) && p0.distance(p1) < p0.distance(p2);
}

/// <summary>
/// Осуществляет поиск минимальной выпуклой оболочки с помощью прохода Грэхема на основе стека
/// Проход осуществляется против часовой стрелки
/// </summary>
/// <param name="input">исходная последовательность точек</param>
/// <returns>последовательность точек, которые образуют минимальную выпуклую оболочку</returns>
static vector<Point> GrahamAlgorithm(vector<Point> input)
{
    vector<Point> points = input;
    //Первая точка - точка с минимальной координатой Y и с минимальной координатой X при наличии совпадений
    sort(points.begin(), points.end(), cmp1);
    Point p0 = points[0];
    //Удаляем первую точку из последовательности для дальнейшего прохода
    points.erase(points.cbegin());
    //Сортируем последовательность в порядке возрастания полярного угла относительно первой точки
    //и при равенстве полярных углов в порядке возрастания расстояния относительно первой точки
    for (int i = 0; i < points.size() - 1; ++i)
    {
        for (int j = 0; j < points.size() - 1; ++j)
        {
            if (!(cmp2(p0, points[j], points[j + 1])))
            {
                Point temp = points[j];
                points[j] = points[j + 1];
                points[j + 1] = temp;
            }
        }
    }
    //Удаляем точки с одинаковыми полярными углами относительно первой точки, оставляя самую дальнюю
    for (auto it = points.begin(); it != points.end() - 1;)
    {
        if (p0.polarAngle(*it) == p0.polarAngle(*(it + 1)))
        {
            it = points.erase(it);
        }
        else ++it;
    }
    if (points.size() >= 2)
    {
        //Создаём пустой стек
        Stack stack = Stack(points.size());
        //Добавляем первые точки
        stack.push(p0);
        stack.push(points[0]);
        //Осуществляем проход по остальным точкам
        for (int i = 1; i < points.size(); ++i)
        {
            //Удаляем точки из стека, пока они не образуют левый поворот
            while (!isLeftTurn(stack.nextToTop(), stack.top(), points[i]))
            {
                stack.pop();
            }
            //Добавляем текущую точку в стек для дальнейшего прохода
            stack.push(points[i]);
        }
        //Извлекаем данные из стека, сохраняя порядок
        int size = stack.size();
        vector<Point> result;
        for (int i = 0; i < size; ++i)
        {
            result.push_back(stack.pop());
        }
        reverse(result.begin(), result.end());
        return result;
    }
    else
    {
        cout << "MBO is empty.";
        return vector<Point>();
    }
}





int main(int argc, char **argv)
{
    //Чтение файла
    vector<Point> points = readFile((string)(argv[3]));
    if (!points.empty())
    {
        //Осуществление прохода по точкам
        vector<Point> mbo = GrahamAlgorithm(points);
        if (!mbo.empty())
        {
            //Добавляем первую точку в конец стека для удобных преобразований при выводе без потери первой точки
            mbo.push_back(mbo[0]);
            //Если нужен проход по часовой стрелке, то переворачиваем последовательность
            if ((string)(argv[1]) == "cw")
            {
                reverse(mbo.begin(), mbo.end());
            }
            //Выбор соответствующего формата записи данных в файл
            if ((string)(argv[2]) == "plain")
            {
                mbo.pop_back();
                writeFilePlain((string)(argv[4]), mbo);
            }
            else
            {
                writeFileWKT((string)(argv[4]), points, mbo);
            }
        }
    }
    return 0;
}