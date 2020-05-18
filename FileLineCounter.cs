using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace CodeCounter
{
    public class FileLineCounter
    {
        #region public and private members
        string fullName = string.Empty;
        bool isDesignerFile = false;
        protected FileInfo file;
        bool inCodeGeneratedRegion = false;
        bool inCommentBlock = false;
        public int numberLines = 0;
        public int numberBlankLines = 0;
        public int numberLinesInDesignerFiles = 0;
        public int numberCommentsLines = 0;
        public int numberCodeFiles = 0;
        public bool Success { get; set; }
        #endregion

        #region ..ctor
        public FileLineCounter(string codeFileFullName)
        {
            fullName = codeFileFullName;
            Process();
        }
        #endregion

        private void Process()
        {
            file = new FileInfo(fullName);
            isDesignerFile = IsDesignerFile();
            CountLines();
            Success = true;
        }
        internal void CountLines()
        {
            InitializeCountLines();
            if (file.Exists)
            {
                StreamReader sr = new StreamReader(file.FullName);
                try
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine().Trim();
                        IncrementLineCountsFromLine(line);
                    }
                }
                finally
                {
                    if (sr != null) sr.Close();
                }

            }
        }
        private bool IsDesignerFile()
        {
            bool isWebReferenceFile = file.FullName.Contains(@"\Web References\") && file.Name == "Reference.cs";  // Ugh
            return isWebReferenceFile || file.Name.ToLower().Contains(".designer.");
        }
        private void InitializeCountLines()
        {
            SetLineCountsToZero();
            numberCodeFiles = 1;
            inCodeGeneratedRegion = false;
            inCommentBlock = false;
        }

        private void IncrementLineCountsFromLine(string line)
        {
            SetCodeBlockFlags(line);

            this.numberLines++;
            if (inCodeGeneratedRegion || this.isDesignerFile)
                this.numberLinesInDesignerFiles++;
            else if (string.IsNullOrWhiteSpace(line))
                this.numberBlankLines++;
            else if (inCommentBlock || line.StartsWith("'") || line.StartsWith(@"//"))
                this.numberCommentsLines++;

            ResetCodeBlockFlags(line);
        }

        private void SetCodeBlockFlags(string line)
        {
            // The number of code-generated lines is an approximation at best, particularly
            // with VS 2003 code.  Change code here if you don't like the way it's working.
            // if (line.Contains("Designer generated code") // Might be cleaner
            if (line.StartsWith("#region Windows Form Designer generated code") ||
                line.StartsWith("#Region \" Windows Form Designer generated code") ||
                line.StartsWith("#region Component Designer generated code") ||
                line.StartsWith("#Region \" Component Designer generated code \"") ||
                line.StartsWith("#region Web Form Designer generated code") ||
                line.StartsWith("#Region \" Web Form Designer Generated Code \"")
                )
                inCodeGeneratedRegion = true;
            if (line.StartsWith("/*"))
                inCommentBlock = true;
        }

        private void ResetCodeBlockFlags(string line)
        {
            if (inCodeGeneratedRegion && (line.Contains("#endregion") || line.Contains("#End Region")))
                inCodeGeneratedRegion = false;
            if (inCommentBlock && line.Contains("*/"))
                inCommentBlock = false;
        }
        protected void SetLineCountsToZero()
        {
            numberLines = 0;
            numberBlankLines = 0;
            numberLinesInDesignerFiles = 0;
            numberCommentsLines = 0;
            numberCodeFiles = 0;
        }
        internal bool CheckValidExtension(string fileName)
        {
            return fileName.EndsWith(".csproj") || fileName.EndsWith(".vbproj") || fileName.EndsWith(".vcproj") ||
                fileName.EndsWith("vcxproj") || fileName.EndsWith(".fsproj") || fileName.EndsWith(".sln") ||
                fileName.EndsWith(".cs") || fileName.EndsWith(".vb") || fileName.EndsWith(".fs") ||
                fileName.EndsWith(".cpp") || fileName.EndsWith(".h") || fileName.EndsWith(".hpp");
        }
    }
}
