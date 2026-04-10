using BaseClasses_V2.Model;
using MessagePack;

namespace BaseClasses_V2.Services;

public static class Serializer
{
    public static void SavePlot(Plot plot, string filePath)
    {
        var bytes = MessagePackSerializer.Serialize(plot);
        File.WriteAllBytes(filePath, bytes);
    }

    public static Plot LoadPlot(string filePath)
    {
        if (!File.Exists(filePath)) throw new FileNotFoundException();
        var bytes = File.ReadAllBytes(filePath);
        try
        {
            return MessagePackSerializer.Deserialize<Plot>(bytes);
        }
        catch (Exception)
        { 
            return null;
        }
    }

    public static string SerializeToHuman(Plot plot)
    {
        var bytes = MessagePackSerializer.Serialize(plot);
        return MessagePackSerializer.ConvertToJson(bytes);
    }

    public static Plot DeserializeFromHuman(string human)
    {
        var bytes = MessagePackSerializer.ConvertFromJson(human);
        return MessagePackSerializer.Deserialize<Plot>(bytes);
    }
}