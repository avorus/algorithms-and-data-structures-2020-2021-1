#include <iostream>
#include <vector>
#include <string>
#include <fstream>
#include <exception>

using namespace std;

/// <summary>
/// Структура узла B-дерева
/// </summary>
struct Node {
    int count; //количество ключей
    vector<pair<int, int>> keys; //пары ключ-значение
    bool leaf; //является ли узел листом
    vector<Node*> children; //дети
};

/// <summary>
/// Класс B-дерева
/// </summary>
class Tree {
private:
    Node* root; //корень
    int t; //минимальная степень ветвления

    /// <summary>
    /// Поиск значения по ключу в узле В-дерева
    /// </summary>
    /// <param name="node">узел</param>
    /// <param name="key">ключ</param>
    /// <returns>пару значений: флаг, есть ли такой ключ, и значение по данному ключу</returns>
    pair<bool, int> searchNode(Node* node, int key) {
        if (node != nullptr)
        {
            int i = 0;
            while ((i < node->count) && (key > node->keys[i].first)) {
                i++;
            }
            if ((i < node->count) && (key == node->keys[i].first)) {
                return pair<bool, int>(true, node->keys[i].second);
            }
            else if (node->leaf) {
                return pair<bool, int>(false, NULL);
            }
            else {
                return searchNode(node->children[i], key);
            }
        }
        return pair<bool, int>(false, NULL);
    }

    /// <summary>
    /// Разбиение узла В-дерева
    /// </summary>
    /// <param name="node">родительский узел ребёнка, который подлежит разбиению</param>
    /// <param name="index">индекс нового ключа в данном узле</param>
    void splitNode(Node* node, int index) {
        Node* childNode = node->children[index];
        Node* newNode = new Node();
        newNode->leaf = childNode->leaf;
        newNode->count = t - 1;
        for (int i = 0; i < t - 1; ++i) {
            newNode->keys.push_back(childNode->keys[i + t]);
        }
        if (!childNode->leaf) {
            for (int i = 0; i < t; ++i) {
                newNode->children.push_back(childNode->children[i + t]);
            }
            for (int i = 0; i < t; ++i) {
                childNode->children.pop_back();
            }
        }
        childNode->count = t - 1;
        node->children.push_back(nullptr);
        for (int i = node->count; i >= index + 1; --i) {
            node->children[i + 1] = node->children[i];
        }
        node->children[index + 1] = newNode;
        node->keys.emplace_back(0, 0);
        for (int i = node->count - 1; i >= index; --i) {
            node->keys[i + 1] = node->keys[i];
        }
        node->keys[index] = childNode->keys[t - 1];
        node->count++;
    }

    /// <summary>
    /// Добавление ключа с значением в узел В-дерева
    /// </summary>
    /// <param name="node">узел</param>
    /// <param name="key">ключ</param>
    /// <param name="value">значение, связанное с данным ключом (payload)</param>
    void insertNode(Node* node, int key, int value) {
        int i = node->count - 1;
        if (node->leaf) {
            node->keys.emplace_back(0, 0);
            while ((i >= 0) && (key < node->keys[i].first)) {
                node->keys[i + 1] = node->keys[i];
                i--;
            }
            node->keys[i + 1] = pair<int, int>(key, value);
            node->count++;
        }
        else {
            while ((i >= 0) && (key < node->keys[i].first)) {
                i--;
            }
            i++;
            Node* child = node->children[i];
            if (child->count == 2 * t - 1) {
                splitNode(node, i);
                if (key > node->keys[i].first) {
                    i++;
                }
            }
            insertNode(node->children[i], key, value);
        }
    }

    /// <summary>
    /// Очистка узла дерева
    /// </summary>
    /// <param name="node">узел</param>
    void deleteNode(Node* node) {
        if (node != nullptr) {
            for (int i = 0; i < node->count + 1; ++i) {
                deleteNode(node->children[i]);
            }
            delete (node);
        }
    }

public:
    /// <summary>
    /// Конструктор нового B-дерева
    /// </summary>
    /// <param name="degree">минимальная степень ветвления</param>
    explicit Tree(int degree) {
        root = nullptr;
        t = degree;
    }

    /// <summary>
    /// Поиск значения по ключу
    /// </summary>
    /// <param name="key">ключ</param>
    /// <returns>пару значений: флаг, есть ли такой ключ, и значение по данному ключу</returns>
    pair<bool, int> search(int key) {
        return searchNode(root, key);
    }

    /// <summary>
    /// Добавление ключа с значением
    /// </summary>
    /// <param name="key">ключ</param>
    /// <param name="value">значение, связанное с данным ключом (payload)</param>
    /// <returns>флаг, успешно ли добавление ключа</returns>
    bool insert(int key, int value) {
        if (root != nullptr)
        {
            if (!search(key).first) {
                if (root->count == 2 * t - 1) {
                    Node* oldRoot = root;
                    Node* newRoot = new Node();
                    root = newRoot;
                    newRoot->leaf = false;
                    newRoot->count = 0;
                    newRoot->children.push_back(oldRoot);
                    splitNode(newRoot, 0);
                    insertNode(newRoot, key, value);
                }
                else {
                    insertNode(root, key, value);
                }
                return true;
            }
            return false;
        }
        //создание корня для пустого дерева
        root = new Node();
        root->keys.emplace_back(key, value);
        root->count = 1;
        root->leaf = true;
        return true;
    }

    ~Tree() {
        deleteNode(root);
    }
};

int main(int argc, char** argv) {
    int t = 1; //минимальная степень ветвления
    string ipath; //путь к файлу с входными данными
    string opath; //путь к файлу с выходными данными
    try
    {
        t = stoi(argv[1]);
        if (t <= 1)
        {
            cout << "Ooops.. Something wrong with arguments ." << endl;
            cout << "The minimum number of keys must be a natural number." << endl;
            return 0;
        }
        ipath = argv[2];
        opath = argv[3];
    }
    catch (exception ex)
    {
        cout << "Ooops.. Something wrong with arguments ." << endl;
        cout << ex.what() << endl;
        return 0;
    }

    Tree* tree = new Tree(t); //В-дерево
    try
    {
        ifstream in; //поток для чтения
        ofstream out; //поток для записи
        in.open(ipath);
        out.open(opath);
        string command; //ввод команд "insert" или "find"
        string key, value; //ввод ключа и значения
        pair<bool, int> result; //результат работы поиска значения по ключу
        while (in >> command >> key)
        {
            if (command == "insert")
            {
                in >> value;
                out << (tree->insert(stoi(key), stoi(value)) ? "true" : "false") << endl;
            }
            else if (command == "find")
            {
                result = tree->search(stoi(key));
                out << (result.first ? to_string(result.second) : "null") << endl;
            }
        }
        in.close();
        out.close();
    }
    catch (exception ex)
    {
        cout << "Ooops... Something wrong with reading and writing the file." << endl;
        cout << ex.what() << endl;
        return 0;
    }
    return 0;
}
