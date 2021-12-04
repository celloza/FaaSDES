using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim
{
    [Serializable]
    public class SimulationRequest
    {

        public string GetBpmnXmlData()
        {
            var base64EncodedBytes = System.Convert.FromBase64String(Base64XmlData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public SimulationRequest(string xmlData)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(xmlData);
            Base64XmlData = System.Convert.ToBase64String(plainTextBytes);
        }

        public SimulationRequest()
        { }

        

        public string Base64XmlData { get; set; }
    }
}
