using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace KUKA
{
    class TxtFile
    {
        private StreamWriter writerFile;
        private string wFileName = null;
        private Boolean wFileOpen = false;

        public string WFileName
        {
            set { wFileName = value; }
            get { return wFileName; }
        }
        public StreamWriter WriterFile
        {
            set { writerFile = value; }
            get { return writerFile; }
        }
        public Boolean WFileOpen
        {
            get { return wFileOpen; }
        }


        public TxtFile(String w_FileName)
        {
            wFileName = w_FileName;
            wFileOpen = false;
        }
        public void OpenTxtFile()
        {
            if (wFileName != null && wFileOpen == false)
            {
                 writerFile = new StreamWriter(wFileName);
                 wFileOpen = true;
            }
        }
        public void WriteTextInFile(String a)
        {
            if (wFileOpen)
            {
                WriterFile.WriteLine(a);
            }
        }
        public void CloseWriteFile()
        {
            if (wFileOpen)
            {
                WriterFile.Close();
                wFileOpen = false; 
            }
        }
    }
}
