#include <iostream>
#include <fstream>
#include <vector>
#include "gdal.h"
#include "ogrsf_frmts.h"
#include "boost/geometry.hpp"

using namespace std;
using boost::geometry::model::box;
using boost::geometry::model::point;
using boost::geometry::cs::cartesian;
using boost::geometry::index::rtree;
using boost::geometry::index::quadratic;
using boost::geometry::index::intersects;

/// <summary>
/// Формирует список OSM_ID всех пересекающих данный прямоугольник MBRs 
/// </summary>
/// <param name="dpath">путь до директории с векторными данными</param>
/// <param name="rectangle">прямоугольник, для которого производится поиск пересечений</param>
/// <returns>отсортированный по возрастанию список OSM_ID</returns>
vector<int> getIntersects(const string &dpath, box<point<double, 2, cartesian>> &rectangle)
{
	//Подгрузка данных
	GDALAllRegister();
	GDALDataset* dataset = static_cast<GDALDataset*>(GDALOpenEx(
		dpath.c_str(),
		GDAL_OF_VECTOR,
		nullptr, nullptr, nullptr));

	//Заполнение R-дерева MBRs геометрий и их OSM_ID
	rtree<pair<box<point<double, 2, cartesian>>, int>, quadratic<8, 4>> tree; //R-дерево
	for (auto&& feature : dataset->GetLayer(0)) {
		OGRGeometry* geom = feature->GetGeometryRef();
		const shared_ptr<OGREnvelope> geomEnvelope(new OGREnvelope);
		geom->getEnvelope(geomEnvelope.get());
		tree.insert(pair<box<point<double, 2, cartesian>>, int>(
			box<point<double, 2, cartesian>>(point<double, 2, cartesian>(geomEnvelope->MinX, geomEnvelope->MinY), point<double, 2, cartesian>(geomEnvelope->MaxX, geomEnvelope->MaxY)),
			feature->GetFieldAsInteger(0)));
	}

	//Поиск по R-дереву MBRs, которые пересекают данный прямоугольник
	vector<pair<box<point<double, 2, cartesian>>, int>> intersect_rectangles;
	tree.query(intersects(rectangle), back_inserter(intersect_rectangles));

	//Сохранение и сортировка OSM_ID найденных MBRs
	vector<int> intersect_ids;
	for (auto rectangle : intersect_rectangles) {
		intersect_ids.push_back(rectangle.second);
	}
	sort(intersect_ids.begin(), intersect_ids.end());
	return intersect_ids;
}

int main(int argc, char** argv)
{
	//Чтение входного прямоугольника из файла
	double xMin, yMin, xMax, yMax;
	ifstream in;
	try {
		in.open(argv[2]);
		in >> xMin >> yMin >> xMax >> yMax;
		in.close();
	}
	catch (exception &ex)
	{
		cout << "Ooops... Something wrong with reading input file." << endl;
		cout << ex.what() << endl;
		return -1;
	}
	box<point<double, 2, cartesian>> rectangle(point<double, 2, cartesian>(xMin, yMin), point<double, 2, cartesian>(xMax, yMax)); //исходный прямоугольник

	//Поиск OSM_ID всех пересекающих исходный прямоугольник MBRs
	vector<int> ids;
	try {
		ids = getIntersects(argv[1], rectangle); //список OSM_ID
	}
	catch (exception& ex)
	{
		cout << "Ooops... Something wrong with getting data from directory." << endl;
		cout << ex.what() << endl;
		return -1;
	}

	//Вывод OSM_IDs в файл
	try {
		ofstream out;
		out.open(argv[3]);
		for (int id : ids) {
			out << id << endl;
		}
		out.close();
	}
	catch (exception& ex)
	{
		cout << "Ooops... Something wrong with writing output file." << endl;
		cout << ex.what() << endl;
		return -1;
	}
}
