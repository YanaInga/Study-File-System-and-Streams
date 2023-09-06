using System.Xml;
using static System.Console;
using static System.Environment;
using static System.IO.Path;
using System.IO.Compression; // BrotliStream, GZipStream, CompressionMode

//WorkWithText();
WorkWithXml();
WorkWithCompression(false);
WorkWithCompression();

static void WorkWithText()
{
    // определяем файл для записи
    string textFile = Combine(CurrentDirectory, "streams.txt");
    // создаем текстовый файл и возвращаем вспомогательную функцию записи
    StreamWriter text = File.CreateText(textFile);
    // перечисляем строки, записывая каждую
    // в поток на отдельной строке
    foreach (string item in Viper.Callsigns)
    {
        text.WriteLine(item);
    }
    text.Close(); // release resources
    // выводим содержимое файла
    WriteLine("{0} contains {1:N0} bytes.",
    arg0: textFile,
    arg1: new FileInfo(textFile).Length);

    WriteLine(File.ReadAllText(textFile));
}
//Запись в XML-потоки
static void WorkWithXml()
{
    FileStream? xmlFileStream = null;
    XmlWriter? xml = null;
    try
    {
        // определяем файл для записи
        string xmlFile = Combine(CurrentDirectory, "streams.xml");
        // создаем файловый поток
        xmlFileStream = File.Create(xmlFile);
        // оборачиваем файловый поток во вспомогательную функцию XML
        // и создаем автоматический отступ для вложенных элементов
        xml = XmlWriter.Create(xmlFileStream,
        new XmlWriterSettings { Indent = true });
        // пишем XML-декларацию
        xml.WriteStartDocument();
        // записываем корневой элемент
        xml.WriteStartElement("callsigns");
        // перечисляем строки, записывая каждую в поток
        foreach (string item in Viper.Callsigns)
        {
            xml.WriteElementString("callsign", item);
        }
        // записываем закрывающий корневой элемент
        xml.WriteEndElement();
        // закрываем вспомогательную функцию и поток
        xml.Close();
        xmlFileStream.Close();
        // выводим все содержимое файла
        WriteLine($"{0} contains {1:N0} bytes.",
        arg0: xmlFile,
        arg1: new FileInfo(xmlFile).Length);
        WriteLine(File.ReadAllText(xmlFile));
    }
    catch (Exception ex)
    {
        // если путь не существует, то будет выдано исключение
        WriteLine($"{ex.GetType()} says {ex.Message}");
    }
    finally
    {
        if (xml != null)
        {
            xml.Dispose();
            WriteLine("The XML writer's unmanaged resources have been disposed.");
        }
        if (xmlFileStream != null)
        {
            xmlFileStream.Dispose();
            WriteLine("The file stream's unmanaged resources have been disposed.");
        }
    }
}
//Сжатие потоков
static void WorkWithCompression(bool useBrotli = true)
{
    string fileExt = useBrotli ? "brotli" : "gzip";
    // сжимаем XML-вывод
    string filePath = Combine(
    CurrentDirectory, $"streams.{fileExt}");
    FileStream file = File.Create(filePath);
    Stream compressor;
    if (useBrotli)
    {
        compressor = new BrotliStream(file, CompressionMode.Compress);
    }
    else
    {
        compressor = new GZipStream(file, CompressionMode.Compress);
    }
    using (compressor)
    {
        using (XmlWriter xml = XmlWriter.Create(compressor))
        {
            xml.WriteStartDocument();
            xml.WriteStartElement("callsigns");
            foreach (string item in Viper.Callsigns)
            {
                xml.WriteElementString("callsign", item);
            }
            // обычный вызов WriteEndElement не требуется,
            // поскольку, освобождая неуправляемые ресурсы,
            // используемые объектом XmlWriter,
            // автоматически завершает любые элементы любой глубины
        }
    } // также закрывает базовый поток
      // вывод всего содержимого сжатого файла
    WriteLine("{0} contains {1:N0} bytes.",
    filePath, new FileInfo(filePath).Length);
    WriteLine($"The compressed contents:");
    WriteLine(File.ReadAllText(filePath));
    // чтение сжатого файла
    WriteLine("Reading the compressed XML file:");
    file = File.Open(filePath, FileMode.Open);
    Stream decompressor;
    if (useBrotli)
    {
        decompressor = new BrotliStream(
        file, CompressionMode.Decompress);
    }
    else
    {
        decompressor = new GZipStream(
        file, CompressionMode.Decompress);
    }
    using (decompressor)
    {
        using (XmlReader reader = XmlReader.Create(decompressor))
        {
            while (reader.Read()) // чтение следующего XML-узла
            {
                // проверяем, находимся ли мы на узле элемента с именем позывной
                if ((reader.NodeType == XmlNodeType.Element)
                && (reader.Name == "callsign"))
                {
                    reader.Read(); // переходим к тексту внутри элемента
                    WriteLine($"{reader.Value}"); // читаем его значение
                }
            }
        }
    }
}
static class Viper
{
    // определяем массив позывных пилотов
    public static string[] Callsigns = new[]
    {"Husker", "Starbuck", "Apollo", "Boomer",
"Bulldog", "Athena", "Helo", "Racetrack"};
}