using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EpsiLibraryCore.Utilitaires
{
    public class ConfigurationManager
    {
        Dictionary<string, string> configurationOptions = new Dictionary<string, string>();

        public Dictionary<string, string> Options
        {
            get { return configurationOptions; }
        }

        public ConfigurationManager()
        {
        }

        public static string MAIN_CONFIG = null;
        private static ConfigurationManager config = null;
        public static ConfigurationManager GetConfigurationManager()
        {
            if (config == null)
            {
                config = new ConfigurationManager();
                // Chargement des fichiers XML de configuration
                config.Load(MAIN_CONFIG);
            }
            return config;
        }

        public void Load(string fileName)
        {
            // Chargement du fichier XML de configuration
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(fileName);

                XmlNode rootNode = doc.DocumentElement;
                if (rootNode != null && rootNode.HasChildNodes)
                {
                    XmlNode nodeChild = doc.DocumentElement.FirstChild;
                    while (nodeChild != null)
                    {
                        Parcours(nodeChild, "");
                        nodeChild = nodeChild.NextSibling;
                    }
                }
            }
            catch (Exception ex)
            {
                EpsiLibraryCore.Utilitaires.LogManager.GetLogger().Error(ex);
            }
        }

        public string GetValue(string name)
        {
            if (configurationOptions.ContainsKey(name))
            {
                return configurationOptions[name];
            }

            return null;
        }
        public void ListAllValue()
        {
            Console.WriteLine("All key/value pairs:");
            foreach (KeyValuePair<string, string> kvp in configurationOptions)
            {
                Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
            }
        }
        public string GetAllValue()
        {
            StringBuilder values = new StringBuilder();

            values.Append("Fichier de configuration :" + MAIN_CONFIG);
            values.Append("\nAll key/value pairs:");
            foreach (KeyValuePair<string, string> kvp in configurationOptions)
            {
                values.AppendFormat("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
            }
            return values.ToString();
        }

        private void Parcours(XmlNode node, string parent)
        {
            if (node.NodeType == XmlNodeType.Element)
            {
                if (node.Attributes.Count > 0)
                {
                    XmlNamedNodeMap mapAttributes = node.Attributes;
                    XmlNode att = mapAttributes.GetNamedItem("name");
                    string name = parent + "." + (att != null ? att.Value : node.Name);
                    string value = mapAttributes.GetNamedItem("value").Value.ToString();

                    // Ajout dans le dictionnaire
                    configurationOptions.Add(name, value);
                }
            }

            if (node.HasChildNodes)
            {
                XmlNode nodeChild = node.FirstChild;
                while (nodeChild != null)
                {
                    if (parent == null || parent.Equals(string.Empty))
                        Parcours(nodeChild, node.Name);
                    else
                        Parcours(nodeChild, parent + "." + node.Name);
                    nodeChild = nodeChild.NextSibling;
                }
            }
        }
    }
}
