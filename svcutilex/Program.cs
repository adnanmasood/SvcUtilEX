using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace svcutilex
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                string msg = @"Options:" +
                             " svcutilex <input-wsdl-file>";
                Console.WriteLine(msg);
                Environment.Exit(0);
            }

            string inputFile = args[0];
            string outFile = Path.GetFileName(inputFile) + "-AttributeGroupsReplaced.wsdl";

            XDocument wsdl = XDocument.Load(inputFile);

            IEnumerable<XElement> attributeGroupDefs =
                wsdl.Root.Descendants("{http://www.w3.org/2001/XMLSchema}attributeGroup")
                    .Where(w => w.Attribute("name") != null)
                    .Select(x => x);

            foreach (
                XElement r in
                    wsdl.Root.Descendants("{http://www.w3.org/2001/XMLSchema}attributeGroup")
                        .Where(w => w.Attribute("ref") != null))
            {
                string refValue = r.Attribute("ref").Value;

                foreach (XElement d in attributeGroupDefs)
                {
                    string defValue = d.Attribute("name").Value;
                    if (refValue == defValue)
                    {
                        IEnumerable<XElement> s =
                            d.Elements("{http://www.w3.org/2001/XMLSchema}attribute").Select(x => x);
                        foreach (XElement e in s)
                        {
                            r.AddBeforeSelf(e);
                        }
                        break;
                    }
                }
            }

            wsdl.Root.Descendants("{http://www.w3.org/2001/XMLSchema}attributeGroup")
                .Where(w => w.Attribute("ref") != null)
                .Remove();
            wsdl.Save(outFile);
            LaunchCommandLineApp(outFile);
        }

        private static void LaunchCommandLineApp(string fileName)
        {
            // Use ProcessStartInfo class
            var startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "svcutil.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = fileName;

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}