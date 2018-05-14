using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Q4_K142182
{
    public partial class Service1 : ServiceBase
    {
        Timer mytimer = null;
        private DateTime lastcheck;
        private bool flag;
        private int interval;
        private int minute = 60000;
        private int hour = 3600000;
        private int file_counter;

        public Service1()
        {
            InitializeComponent();
            this.lastcheck = DateTime.Now;
            this.flag = false;
            this.interval = 10000;
            string[] originalFiles = Directory.GetFiles(ConfigurationManager.AppSettings["folderpath"], "*", SearchOption.AllDirectories);
            this.file_counter = originalFiles.Length;
        }
        protected override void OnStart(string[] args)
        {
            
            mytimer = new Timer();
            this.mytimer.Interval = interval;
            this.mytimer.Elapsed += new ElapsedEventHandler(this.timer1_Tick);
            this.mytimer.Enabled = true;
            


        }

        public static List<DateTime> last_write(List<string> mylist)
        {
            List<String> filess = mylist;
            List<DateTime> lastwrites = new List<DateTime>();
              foreach (string f in mylist)
                {
                    DateTime lastModified = System.IO.File.GetLastWriteTime(f);
                    lastwrites.Add(lastModified);

                }
            return lastwrites;
        }
        //function to get all the file names 
        private static List<String> Get_Files(string sDir)
        {
            List<String> files = new List<String>();
            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    files.Add(f);
                }
            }
            catch (System.Exception excpt)
            {
                Console.Write(excpt.Message);
            }

            return files;
        }
        protected override void OnStop()
        {
            LogService("Service Stoped");
            mytimer.Enabled = false;

        }
        //function to Write the Log file
        private void LogService(string content)
        {
            FileStream fs = new FileStream(@"E:\monitorlog.txt", FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.BaseStream.Seek(0, SeekOrigin.End);
            sw.WriteLine(content);
            sw.Flush();
            sw.Close();
        }
        
        
          private void timer1_Tick(object sender, EventArgs e)
          {
            LogService("last check time : " + lastcheck.ToString());
            string c_path = ConfigurationManager.AppSettings["folderpath"];
            LogService(c_path);
            List<String> files = Get_Files(c_path);
            int current_file_count = files.Count();
            LogService("Files Count: "+current_file_count.ToString());
            List<DateTime> currentdates = last_write(files);
            foreach (DateTime dt in currentdates)
            {
                if (lastcheck > dt && file_counter==current_file_count)
                {
                    flag =false;
                }
                 else
                {
                    flag =true;
                    break;
                }
            
            }
            lastcheck = DateTime.Now;
            file_counter = current_file_count;
            
            if (flag == true)
            {
                LogService("Change detected");
                CopyFiles();
            }
            else
            {
                LogService("No Change detected");
                if (interval < hour)
                {
                    interval += 2*minute;
                    mytimer.Interval = interval;
                }
                    
            }
            LogService("Interval ki value: " + interval.ToString());
            
          




        }

          private void CopyFiles()
          {
              string Current_Folder =  ConfigurationManager.AppSettings["folderpath"];;
              string New_Folder = ConfigurationManager.AppSettings["newpath"]; ;
              string[] originalFiles = Directory.GetFiles(Current_Folder, "*", SearchOption.AllDirectories);

              Array.ForEach(originalFiles, (originalFileLocation) =>
              {

                  FileInfo originalFile = new FileInfo(originalFileLocation);
                  FileInfo destFile = new FileInfo(originalFileLocation.Replace(Current_Folder, New_Folder));

                  if (destFile.Exists)
                  {

                      if (originalFile.Length > destFile.Length)
                      {
                          originalFile.CopyTo(destFile.FullName, true);
                      }
                  }
                  else
                  {
                      Directory.CreateDirectory(destFile.DirectoryName);
                      originalFile.CopyTo(destFile.FullName, false);
                  }

              });
          }

        
    }
}
