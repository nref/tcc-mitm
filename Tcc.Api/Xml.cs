using System.Xml.Linq;

namespace Tcc.Api;

public static class Xml
{
    public static bool TryGetNodeValue(string xml, string nodeName, out string nodeValue)
    {
        nodeValue = "";
        XDocument doc = XDocument.Parse(xml);
        XElement? node = doc.Descendants().FirstOrDefault(desc => desc.Name.LocalName == nodeName);

        if (node == null)
        {
            Log.Error($"Could not find node '{nodeName}'");
            return false;
        }

        nodeValue = node.Value;
        return true;
    }
}