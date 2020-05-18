using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace CodeCounter
{
    public class ProcessSolution
    {
        #region public and private members
        public List<string> ProjectList { get; set; }
        #endregion
        private FileInfo File { get; set; }
        #region ..ctor
        public ProcessSolution(string slnNameFullpath)
        {
            File = new FileInfo(slnNameFullpath);
            GetsListOfProjectFilesFromSolution();
        }
        #endregion
        #region public methods
        private List<string> GetsListOfProjectFilesFromSolution()
        {
            StreamReader sr = new StreamReader(File.FullName);
            try
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine().Trim();
                    IsValidateProjectFile(line);
                }
                return ProjectList;
            }
            finally
            {
                if (sr != null) sr.Close();
            }
        }
        #endregion

        #region private methods
        private void IsValidateProjectFile(string line)
        {
            //System.Diagnostics.Debug.WriteLine(line);
            if (line.StartsWith("Project") &&
                (line.Contains(".csproj") || line.Contains(".vbproj") || line.Contains(".vcproj") || line.Contains(".vcxproj") || line.Contains(".fsproj")))
            {
                var extLength = 7;
                int extStart = line.IndexOf(".csproj");
                if (extStart == -1) extStart = line.IndexOf(".vbproj");
                if (extStart == -1) extStart = line.IndexOf(".vcproj");
                if (extStart == -1) 
                    { 
                        extStart = line.IndexOf(".vcxproj");
                        extLength = 8;
                    }
                if (extStart == -1) extStart = line.IndexOf(".fsproj");
                string interestingLine = line.Substring(0, extStart + extLength);
                int startPosition = interestingLine.LastIndexOf('"');
                if (startPosition != -1)
                {
                    string projectRelativeName = interestingLine.Substring(startPosition + 1);
                    AddProjectFileNameTOList(projectRelativeName);
                }
            }
        }

        private void AddProjectFileNameTOList(string projectRelativeName)
        {
            string projectFullName = Path.Combine(File.Directory.FullName, projectRelativeName);
            string project = Path.Combine(projectFullName, projectRelativeName);
            ProjectList.Add(project);
        }
        #endregion

    }
}
