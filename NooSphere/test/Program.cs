using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            var str = "<s:Envelope xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:a=\"http://www.w3.org/2005/08/addressing\">" +
"<s:Header>" +
"	<a:Action s:mustUnderstand=\"1\">http://docs.oasis-open.org/ws-dd/ns/discovery/2009/01/ProbeMatches</a:Action>" +
"	<h:AppSequence InstanceId=\"1345555036\" MessageNumber=\"1\" xmlns:h=\"http://docs.oasis-open.org/ws-dd/ns/discovery/2009/01\"/>" +
"	<a:RelatesTo>urn:uuid:8f07f659-93b8-4d9f-a4d6-af457f386e14</a:RelatesTo>" +
"	<a:MessageID>urn:uuid:b24aa1f6-f290-4e0a-9ecd-c1b4772cd9ec</a:MessageID>" +
"	<ActivityId CorrelationId=\"ea733712-e3c0-4572-81e3-a793ed8547f5\" xmlns=\"http://schemas.microsoft.com/2004/09/ServiceModel/Diagnostics\">00000000-0000-0000-0000-000000000000</ActivityId>" +
"</s:Header>" +
"<s:Body>" +
"	<ProbeMatches xmlns=\"http://docs.oasis-open.org/ws-dd/ns/discovery/2009/01\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
"		<ProbeMatch>" +
"			<a:EndpointReference>" +
"				<a:Address>http://10.1.1.190:56789/</a:Address>" +
"			</a:EndpointReference>" +
"			<d:Types xmlns:d=\"http://docs.oasis-open.org/ws-dd/ns/discovery/2009/01\" xmlns:dp0=\"http://tempuri.org/\">dp0:IDiscovery</d:Types>" +
"			<XAddrs>http://10.1.1.190:56789/</XAddrs>" +
"			<MetadataVersion>0</MetadataVersion>" +
"			<string xmlns=\"\">Lenovo</string>" +
"			<string xmlns=\"\">pIT lab</string>" +
"			<string xmlns=\"\">http://10.1.1.190:56975/</string>" +
"			<string xmlns=\"\">207</string>" +
"		</ProbeMatch>" +
"	</ProbeMatches>" +
"</s:Body>" +
"</s:Envelope>";
            var xml = new XmlDocument();
            xml.LoadXml(str);
            var namespaces = new XmlNamespaceManager(xml.NameTable);
            namespaces.AddNamespace("", "http://docs.oasis-open.org/ws-dd/ns/discovery/2009/01");
            namespaces.AddNamespace("a", "http://www.w3.org/2005/08/addressing");
            namespaces.AddNamespace("i", "http://www.w3.org/2001/XMLSchema-instance");
            namespaces.AddNamespace("s", "http://www.w3.org/2003/05/soap-envelope");

            var test1 = xml.SelectNodes("//*[local-name() = 'ProbeMatch']", namespaces);
            var test1a = test1[0].SelectSingleNode("//*[local-name() = 'XAddrs']");
        }
    }
}
