using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using System.IO;

namespace CodeCounter
{
    public class ProcessProject
    {
        #region ..ctor
        public ProcessProject(string projectFileName)
        {
            ProjectFile = new FileInfo(projectFileName);
            GetListOfCodeFilesFromProjectFile();
        }
        #endregion

        #region public and private members
            public List<string> FileList { get; set; }
            private FileInfo ProjectFile { get; set; }
        #endregion

        #region public methods
        private List<string> GetListOfCodeFilesFromProjectFile()
        {
            XPathDocument document = new XPathDocument(ProjectFile.FullName);
            XPathNavigator navigator = document.CreateNavigator();

            XPathNodeIterator iterator = GetIterator(navigator);

            while (iterator.MoveNext())
                AddCodeFileToList(iterator.Current.Value);
            return FileList;
        }

        #endregion

        #region private methods
        private void AddCodeFileToList(string codeFileRelativeName)
        {
            if (IsValidExtension(codeFileRelativeName))
            {
                if (codeFileRelativeName.StartsWith(".")) codeFileRelativeName = codeFileRelativeName.Substring(2);
                string codeFileFullName = Path.Combine(ProjectFile.Directory.FullName, codeFileRelativeName);
                string lineCountCodeFile = Path.Combine(codeFileFullName, codeFileRelativeName);
                FileList.Add(lineCountCodeFile);
            }
        }
        private XPathNodeIterator GetIterator(XPathNavigator navigator)
        {
            XPathNavigator node = navigator.SelectSingleNode("//@ProductVersion");
            bool version2003 = node != null && node.Value.StartsWith("7");
            bool isCpp = ProjectFile.Name.EndsWith(".vcproj") || ProjectFile.Name.EndsWith(".vcxproj");

            XPathNodeIterator iterator;
            if (isCpp)
            {
                iterator = navigator.Select("//Files/Filter[@Name=\"Source Files\" or @Name=\"Header Files\"]/File/@RelativePath");
            }
            else if (version2003)
            {
                // 'File' nodes with attribute BuildAction set to "Compile", RelPath attribute on that
                iterator = navigator.Select("//Files/Include/File[@BuildAction=\"Compile\"]/@RelPath");
            }
            else
            {
                XmlNamespaceManager manager = new XmlNamespaceManager(navigator.NameTable);
                manager.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");

                // 'Include' attribute on 'Compile' nodes in the namespace
                iterator = navigator.Select("//ns:ItemGroup/ns:Compile/@Include", manager);
            }
            return iterator;
        }

        private bool IsValidExtension(string fileName)
        {
            return fileName.EndsWith(".cs") || fileName.EndsWith(".vb") || fileName.EndsWith(".fs") ||
                fileName.EndsWith(".cpp") || fileName.EndsWith(".h") || fileName.EndsWith(".hpp") || fileName.EndsWith(".config");
        }

        #endregion
    }
}
