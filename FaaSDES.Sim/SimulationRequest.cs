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
            var base64EncodedBytes = Convert.FromBase64String(Base64XmlData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public SimulationRequest(string xmlData)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(xmlData);
            Base64XmlData = Convert.ToBase64String(plainTextBytes);
        }

        public SimulationRequest()
        { }

        

        public string Base64XmlData { get; set; }
    }
}
