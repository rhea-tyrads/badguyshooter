namespace Watermelon
{
    public class RegisteredDefine
    {
        private string define;
        public string Define => define;

        private string assemblyType;
        public string AssemblyType => assemblyType;

        private string[] linkedFiles;
        public string[] LinkedFiles => linkedFiles;

        public RegisteredDefine(string define, string assemblyType, string[] linkedFiles)
        {
            this.define = define;
            this.assemblyType = assemblyType;
            this.linkedFiles = linkedFiles;
        }

        public bool ContainsFile(string filePath)
        {
            for(int i = 0; i < linkedFiles.Length; i++)
            {
                if (linkedFiles[i] == filePath)
                    return true;
            }

            return false;
        }
    }
}