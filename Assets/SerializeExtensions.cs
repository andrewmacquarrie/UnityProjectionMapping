using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace Assets
{
    public static class SerializeExtensions
    {
        public static string SerializeObject<T>(this T toSerialize)
        {
            using (StringWriter textWriter = new StringWriter())
            {
                XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }

    }
}
