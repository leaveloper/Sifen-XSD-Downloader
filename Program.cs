using HtmlAgilityPack;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report.Export;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Sifen_XSD_Downloader
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //await OpcionesConsola();

            StiReport report = new StiReport();
            report.Load(@"C:\Users\Leandro\Downloads\asd\RPT_FACTURA_MARTEL.mrt");

            DataSet dataSet = new DataSet();
            dataSet.ReadXml(@"C:\Users\Leandro\Downloads\asd\archivo.xml");

            // Establecer el objeto DataSet como origen de datos del informe
            report.RegData(dataSet);

            StiVariablesCollection variables = report.Dictionary.Variables;
            report.Render();

            StiPdfExportService pdfExport = new StiPdfExportService();
            pdfExport.ExportPdf(report, @"C:\Users\Leandro\Downloads\asd\output.pdf");
        }

        private static async Task OpcionesConsola()
        {
            Console.WriteLine("URL: ");
            string url = Console.ReadLine();
            Console.WriteLine("Carpeta de salida: ");
            string carpetaSalida = Console.ReadLine();
            Console.WriteLine("Reemplazar archivos: ");
            Console.WriteLine("0 - No");
            Console.WriteLine("1 - Si");
            bool reemplazar = Convert.ToBoolean(Convert.ToByte(Console.ReadLine()));

            await DescargarEsquemas(url, carpetaSalida, reemplazar);

            Console.WriteLine("Ubicación del archivo XSD.exe: ");
            string rutaXsdFile = Console.ReadLine();
            Console.WriteLine("Nombre del primer archivo: ");
            string nombrePrimerArchivo = Console.ReadLine();

            CrearClaseEsquema(rutaXsdFile, carpetaSalida, nombrePrimerArchivo);
        }

        private static async Task DescargarEsquemas(string url, string carpetaSalida, bool reemplazar)
        {
            url = url.TrimEnd('/');
            string contenido = await ObtenerContenido(url);

            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(contenido);
            HtmlNodeCollection nodes = html.DocumentNode.SelectNodes("//a[@href][contains(@href, '.xsd')]");
            List<string> enlaces = nodes == null ? new List<string>() : nodes.ToList().ConvertAll(
                   r => r.Attributes.ToList().ConvertAll(
                   i => i.Value)).SelectMany(j => j).ToList();

            Console.Clear();
            Console.WriteLine("Comienza a descargar");
            string contenidoEsquema;
            foreach (string esquema in enlaces)
            {
                string rutaSalida = Path.Combine(carpetaSalida, esquema);
                if (!reemplazar && File.Exists(rutaSalida))
                    continue;

                contenidoEsquema = await ObtenerContenido($"{url}/{esquema}");

                XmlDocument xml = new XmlDocument
                {
                    PreserveWhitespace = true
                };
                xml.LoadXml(contenidoEsquema);
                XmlNamespaceManager xs = new XmlNamespaceManager(xml.NameTable);
                xs.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");

                XmlNodeList includes = xml.SelectNodes("xs:schema/xs:include[@schemaLocation]", xs);
                foreach (XmlNode include in includes)
                {
                    string antiguoValor = include.Attributes[0].Value;
                    string nuevoValor = antiguoValor.Replace($"{url}/", "");
                    include.Attributes[0].Value = nuevoValor;
                }

                XmlNode signature = xml.SelectSingleNode("//xs:element[@ref='ds:Signature']", xs);
                if (signature != null)
                    signature.ParentNode.RemoveChild(signature);

                File.WriteAllText(rutaSalida, xml.OuterXml);
            }

            Console.Clear();
            Console.WriteLine("Esquemas descargados");
        }

        private static void CrearClaseEsquema(string rutaXsdFile, string nombrePrimerArchivo, string carpetaSalida)
        {
            nombrePrimerArchivo = nombrePrimerArchivo.Replace(".xsd", "");
            List<string> comandos = new List<string>()
            {
                "cd " + rutaXsdFile,
                "xsd \"" + Path.Combine(carpetaSalida, nombrePrimerArchivo + ".xsd") + "\" /outputdir:\"" + carpetaSalida + "\" -c",
            };

            Process.Start("CMD.exe", "/c " + string.Join("&", comandos));

            Console.Clear();
            Console.WriteLine($"El archivo {nombrePrimerArchivo}.cs fue creado correctamente");
            Console.WriteLine("Presione una tecla para salir");
            Console.ReadKey();
        }

        private static async Task<string> ObtenerContenido(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(url))
                {
                    using (HttpContent content = response.Content)
                    {
                        return await content.ReadAsStringAsync();
                    }
                }
            }
        }
    }
}
