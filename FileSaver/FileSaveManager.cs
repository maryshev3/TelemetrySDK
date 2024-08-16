using TelemetrySetterBase.Abstracts;
using TelemetrySetterBase.Models;
using Newtonsoft.Json;
using System.Text;

namespace FileSaver;

public class FileSaveManager : ISaveManager
{
    private readonly string _savePath;

    private readonly object _locker = new();

    private readonly Encoding _encoding = Encoding.ASCII;

    public FileSaveManager(string savePath)
    {
        _savePath = savePath;
        
        CreateFileIfNotExist(savePath);
    }

    private void CreateFileIfNotExist(string savePath)
    {
        if (!File.Exists(savePath))
            File.WriteAllText(savePath, "[]", _encoding);
    }

    private void SaveTelemetry(TelemetryItem telemetryItem)
    {
        string serializedTelemetryItem = JsonConvert.SerializeObject(telemetryItem);
        byte[] bytesTelemetryItem = _encoding.GetBytes(serializedTelemetryItem);

        char point = ',';
        byte bytePoint = Convert.ToByte(point);

        char closeBracket = ']';
        byte byteCloseBracket = Convert.ToByte(closeBracket);

        using (var fileStream = new FileStream(_savePath, FileMode.Open, FileAccess.ReadWrite))
        {
            fileStream.Seek(-1, SeekOrigin.End);

            fileStream.SetLength(fileStream.Length - 1);

            fileStream.Seek(-1, SeekOrigin.End);

            bool isOpenBracketMinus1 = Convert.ToChar(fileStream.ReadByte()) == '[';

            if (!isOpenBracketMinus1)
            {
                fileStream.WriteByte(bytePoint);
            }

            fileStream.Write(bytesTelemetryItem);

            fileStream.WriteByte(byteCloseBracket);
        }
    }
    
    public void Save(TelemetryItem telemetryItem)
    {
        lock (_locker)
        {
            SaveTelemetry(telemetryItem);
        }
    }
}