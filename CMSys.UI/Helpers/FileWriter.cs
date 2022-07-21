namespace CMSys.UI.Helpers
{
    public static class FileWriter
    {
        public static void WriteBytesToFile(string path, byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                using (var fs = new FileStream(path, FileMode.Create))
                {
                    ms.WriteTo(fs);
                }
            }
        }
    }
}
