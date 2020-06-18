using System.Collections.Generic;
using System.IO;
using BusinessObjects;
namespace CodeCounter
{
    public class ProcessSolution
    {
        #region public and private members
        public List<ProjectNameAndSync> ProjectList { get; set; }
        private FileInfo File { get; set; }
        private bool _projectNameOnly { get; set; }
        #endregion

        #region ..ctor
        public ProcessSolution(string slnNameFullpath, bool projectNameOnly=false)
        {
            ProjectList = new List<ProjectNameAndSync>();
            _projectNameOnly = projectNameOnly;
            File = new FileInfo(slnNameFullpath);
            GetsListOfProjectFilesFromSolution();
        }
        #endregion
        #region public methods
        private List<ProjectNameAndSync> GetsListOfProjectFilesFromSolution()
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
            if (line.StartsWith("Project") &&
                (line.Contains(".csproj") || line.Contains(".vbproj") || line.Contains(".vcproj") || line.Contains(".vcxproj") || line.Contains(".fsproj")))
            {
                var extLength = 7;
                int extStart = line.IndexOf(".csproj");
                if (extStart == -1) extStart = line.IndexOf(".vbproj");
                if (extStart == -1) extStart = line.IndexOf(".vcproj");
                if (extStart == -1) extStart = line.IndexOf(".fsproj");
                if (extStart == -1) 
                { 
                    extStart = line.IndexOf(".vcxproj");
                    extLength = 8;
                }
                string interestingLine = line.Substring(0, extStart + extLength);
                int startPosition = interestingLine.LastIndexOf('"');
                if (startPosition != -1)
                {
                    string projectRelativeName = interestingLine.Substring(startPosition + 1);
                    AddProjectFileNameToList(projectRelativeName);
                }
            }
        }

        private void AddProjectFileNameToList(string projectRelativeName)
        {
            string project;
            if (!_projectNameOnly)
            {
                //string projectFullName = Path.Combine(File.Directory.FullName, projectRelativeName);
                project = Path.Combine(File.Directory.FullName, projectRelativeName);
            }
            else
                project = Path.GetFileNameWithoutExtension(projectRelativeName);

            ProjectList.Add(new ProjectNameAndSync { Name = project });
        }

        public string FindSLNFileFromProjectPath(string projectPath)
        {
            string projPath = projectPath;
            while (!string.IsNullOrWhiteSpace(projPath))
            {
                string slnName = $"{Path.GetFileNameWithoutExtension(projectPath)}.sln";
                string slnPath = Path.Combine(projPath, slnName);
                // sln normally in project path if it exists
                if (System.IO.File.Exists(slnPath))
                    return slnPath;
                // otherwise if we are to find it, it should be one or more dirs back
                projPath = Path.GetDirectoryName(projPath);
            }
            return string.Empty;
        }
        #endregion

    }
}
