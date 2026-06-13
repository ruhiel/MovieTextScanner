using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieTextScanner
{
    internal class TempDirectory
    {
        private string _tempDir;
        public TempDirectory()
        {
            CreateTempdir();
        }
        
        ~TempDirectory()
        {
            try
            {
                if (System.IO.Directory.Exists(_tempDir))
                {
                    System.IO.Directory.Delete(_tempDir, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete temp directory: {ex.Message}");
            }
        }

        internal string GetTempDir() => _tempDir;

        private void CreateTempdir()
        {
            _tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
            System.IO.Directory.CreateDirectory(_tempDir);
        }
    }
}
