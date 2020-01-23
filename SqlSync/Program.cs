using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;


namespace ConsoleApplication
{
    class Program
    {

        public static string outPutDir = (@"c:\tmp\sql_sync\tmp\");
        private static object tables;
        public static string sourceFile, staticPath, sourcePath;
        public static string sourceServer;
        public static string sourceDb;
        public static string sourceUsr;
        public static string sourcePass;
        public static string sourceSchema;
        public static string destServer;
        public static string destDb;
        public static string destUsr;
        public static string destPass;
        public static string destSchema;
        public static string targetServer;
        public static string targetDb;
        public static string answer = "n";
        public static int splitNo;

        static void Main(string[] args)
        {
            string option = "0";
            Console.WriteLine("Choose one of the following menu items:");
            Console.WriteLine("1.Cleanup output directory");
            Console.WriteLine("2.Get Table Names to compare");
            Console.WriteLine("3.Launch Table Diff and create SQL Scripts");
            Console.WriteLine("4.Remove Updates and Deletes from SQL Scripts");
            Console.WriteLine("5.Run the SQL Scripts on target DB");
            Console.WriteLine("6.Exit");
            option = Console.ReadLine();

            while (option != "6")

            {



                if (option == "1")
                {
                    Console.WriteLine("Cleaning up the output directory");
                    CleanupDir();
                    Console.WriteLine("Press any key to continue");
                    Console.ReadKey();
                    Console.Clear();
                }

                if (option == "2")
                {
                    Console.WriteLine("Getting the tablenames to use in the comparison");
                    Console.WriteLine("Enter the filename (in c:\\tmp) to the list of tables sorted by relationship levels ");
                    sourceFile = Console.ReadLine();
                    staticPath = (@"c:\tmp\");
                    sourcePath = staticPath + sourceFile;
                    Console.WriteLine("the sourcefile path is:" + sourcePath);
                    Console.WriteLine();
                    Console.WriteLine("Press any key to continue");
                    Console.ReadLine();
                    string[] tableNames = GetTableNames();
                    Console.WriteLine("The tablenames in the file is:");
                    for (int i = 0; i < tableNames.Length; i++)
                    {
                        Console.WriteLine(tableNames[i]);
                    }
                    Console.WriteLine("Press any key to continue");
                    Console.ReadKey();
                    Console.Clear();
                }

                if (option == "3")
                {
                    Console.WriteLine("Doing the compare of the tables between the DB's");
                    do
                    {
                        Console.WriteLine("Enter the source Server name ie (DCSJHB5DB01\\SQL2008R2)");
                        sourceServer = Console.ReadLine();
                        Console.WriteLine("Enter the destination Server name ie (DCSPTA1DB01\\SQL2008R2)");
                        destServer = Console.ReadLine();
                        Console.WriteLine("Enter the source DB name");
                        sourceDb = Console.ReadLine();
                        Console.WriteLine("Enter the target DB name");
                        destDb = Console.ReadLine();
                        Console.WriteLine("Enter the schema name to be synced");
                        sourceSchema = Console.ReadLine();
                        destSchema = sourceSchema;
                        Console.WriteLine("The arguments for the tablediff cmd is:");
                        Console.WriteLine("SourceServer: " + sourceServer);
                        Console.WriteLine("SourceDB: " + sourceDb);
                        Console.WriteLine("TargetServer: " + destServer);
                        Console.WriteLine("TargetDB: " + destDb);
                        Console.WriteLine("Schema: " + destSchema);
                        Console.WriteLine("Press any 'y' to continue or 'n' to re-enter data");
                        answer = Console.ReadLine();
                    }
                    while (answer == "n" || answer == "N");

                    string[] tableNames = GetTableNames();
                    for (int i = 0; i < tableNames.Length; i++)
                    {
                        LaunchTableDiff(tableNames[i]);
                    }
                    Console.WriteLine("Output Directory for sql compare script is c:\\tmp\\sql_sync\\tmp\\ .Press any key to continue");
                    Console.ReadKey();
                    Console.Clear();
                }

                if (option == "4")
                {
                    Console.WriteLine("Removing all the DELETE and UPDATE statements from the SQL scripts");
                    foreach (string table in GetTableNames())
                        StringEditing(table);
                    Console.WriteLine("Done removing UPDATES and DELETES.  Press any key to continue");
                    Console.ReadKey();
                    Console.Clear();
                }

                if (option == "5")
                {
                    string reply;
                    Console.WriteLine("Running the SQL scripts to merge the DB tables");
                    Console.WriteLine();
                    Console.WriteLine("The current target for the scripts is : " + destServer);
                    Console.WriteLine("Can we continue with this server or do you want to change the target? 'Y' - Continue / 'N' - Change target ?");
                    reply = Console.ReadLine();
                    if (reply == "y" || reply == "Y")
                    {
                        targetServer = destServer;
                    }
                    else
                    {
                        Console.WriteLine("Enter the target Server");
                        targetServer = Console.ReadLine();
                    }

                    Console.WriteLine("The current target DB for the scripts is : " + destDb);
                    Console.WriteLine("Can we continue with this DB or do you want to change the target? 'Y' - Continue / 'N' - Change DB ?");
                    reply = Console.ReadLine();
                    if (reply == "y" || reply == "Y")
                    {
                        targetDb = destDb;
                    }
                    else
                    {
                        Console.WriteLine("Enter the target DB");
                        targetDb = Console.ReadLine();
                    }
                    Console.WriteLine("Busy Processing the scripts, go have some coffee and look at the logs in c:\\tmp");

                    foreach (string table in GetTableNames())
                    {
                        LaunchSqlScripts(table);

                    }
                    Console.WriteLine("Scripts completed. Log file in c:\\tmp.   Press any key to continue");
                    Console.ReadKey();
                    Console.Clear();

                }

                else if (option == "6")
                    Console.WriteLine("Bye for now");

                Console.WriteLine("Choose one of the following menu items:");
                Console.WriteLine("1.Cleanup output directory");
                Console.WriteLine("2.Get Table Names to compare");
                Console.WriteLine("3.Launch Table Diff and create SQL Scripts");
                Console.WriteLine("4.Remove Updates and Deletes from SQL Scripts");
                Console.WriteLine("5.Run the SQL Scripts on target DB");
                Console.WriteLine("6.Exit");
                option = Console.ReadLine();

            }

        }

        public static void CleanupDir()
        {
            //Cleanup SQL Script directory

            try
            {
                Directory.Delete(outPutDir, true);
            }
            catch { }

            try
            {
                Directory.CreateDirectory(outPutDir);
            }
            catch { }

        }
        public static string[] GetTableNames()
        {

            //List<string> listtables = new List<string>();
            //// DB Connection to fetch table names to be synced
            //System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(@"Data Source=DCSJHB5DB01\SQL2008R2;Initial Catalog=IIMS_JHBMA;User ID=FDSLogin;Password=0217530;Pooling=False;MultipleActiveResultSets=False;Connect Timeout=60;Encrypt=False;TrustServerCertificate=True");
            //////System.Data.SqlClient.SqlCommand com = new System.Data.SqlClient.SqlCommand("select * from sysobjects where xtype='U' and name not in ('cdc_states', 'CentreParameters','datavaseversion','DeviceParameters', 'DiscrepancyReport','DatabaseVersion','SequenceNumber','SequenceNumberReserved','sysdiagrams','SystemParameters','TMP_GUID','TMP_TOOL','UserParameters') order by name asc");
            //System.Data.SqlClient.SqlCommand com = new System.Data.SqlClient.SqlCommand("select name from sysobjects where xtype='U' and name like 'SS_%'");
            //com.Connection = con;
            //con.Open();
            //System.Data.SqlClient.SqlDataReader reader = com.ExecuteReader();
            //while(reader.Read())
            //{
            //    listtables.Add(reader[0].ToString());
            //}
            //con.Close();

            //return listtables.ToArray();


            if (!File.Exists(sourcePath))
            {
                string[] invalidFile = { "InvalidFileName" };
                return (invalidFile);
            }

            return System.IO.File.ReadAllLines(sourcePath);
            //return System.IO.File.ReadAllLines(@"c:\tmp\tablelist.txt");
        }
        public static void LaunchTableDiff(string tableName)
        {
            string sourceTable;
            string destTable;
            string outPutFile = outPutDir + tableName + ".sql";

            sourceTable = tableName;
            destTable = sourceTable;

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "tablediff.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            //startInfo.Arguments = "-sourceserver XICENNORTJEJ\\SQL2014 -sourcedatabase IIMS_JHBMA -sourceschema dbo -sourcetable " + sourceTable + " -sourceuser FDSLogin -sourcepassword 0217530 -destinationserver XICENNORTJEJ\\SQL2014   -destinationdatabase IIMS -destinationschema dbo -destinationtable " + destTable + " -f " + outPutFile;  //whatever die arguments 
            startInfo.Arguments = "-sourceserver " + sourceServer + " -sourcedatabase " + sourceDb + " -sourceschema " + sourceSchema + " -sourcetable " + sourceTable + " -sourceuser FDSLogin -sourcepassword 0217530 -destinationserver " + destServer + "  -destinationdatabase " + destDb + " -destinationschema " + destSchema + " -destinationtable " + destTable + " -destinationuser FDSLogin -destinationpassword 0217530 -f " + outPutFile;  //whatever die arguments 

            try
            {
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch
            {

            }




        }

        public static void SplitFiles(string inputfile, string scriptPathoutput, int splitNo)
        {
            try
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(inputfile))
                {
                    int fileNumber = 0;

                    while (!sr.EndOfStream)
                    {
                        int count = 0;

                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(inputfile + ++fileNumber + ".sql"))
                        {
                            sw.AutoFlush = true;

                            while (!sr.EndOfStream && ++count < splitNo)
                            {
                                sw.WriteLine(sr.ReadLine());
                            }
                        }
                    }
                }
            }
            catch { }

        }
        public static void StringEditing(string tableName)
        {

            string scriptName = "";
            string[] bigFiles = { "AU_AUDIT_ENTITY.sql", "AU_AUDIT.sql" };
            string[] biggerFiles = { "IN_MOVEMENT_STATUS.sql" };
            string[] veryBigFiles = { "IN_PERSONAL_PHOTO.sql", "syncincoming.sql" };
            try
            {
                scriptName = tableName + ".sql";
            }
            catch { }

            string scriptPath = outPutDir + scriptName;
            try
            {
                var oldLines = File.ReadAllLines(scriptPath);


                var newLinesDeletes = oldLines.Select(line => new
                {
                    Line = line,
                    Words = line.Split(' ')
                })
            .Where(lineInfo => !lineInfo.Words.Contains("DELETE"))
            .Select(lineInfo => lineInfo.Line);
                var newLinesUpdates = newLinesDeletes.Select(line => new
                {
                    Line = line,
                    Words = line.Split(' ')
                })
            .Where(lineInfo => !lineInfo.Words.Contains("UPDATE"))
            .Select(lineInfo => lineInfo.Line);
                File.WriteAllLines(scriptPath, newLinesUpdates);
            }
            catch { }
            //Split big files into smaller chunks

            for (int i = 0; i < veryBigFiles.Length; i++)
            {
                if (scriptName == veryBigFiles[i])
                {
                    SplitFiles(scriptPath, scriptPath + "-{0}.sql", 1000);

                }
            }

            for (int i = 0; i < biggerFiles.Length; i++)
            {
                if (scriptName == biggerFiles[i])
                {
                    SplitFiles(scriptPath, scriptPath + "-{0}.sql", 20000);
                }
            }

            for (int i = 0; i < bigFiles.Length; i++)
            {
                if (scriptName == bigFiles[i])
                {
                    SplitFiles(scriptPath, scriptPath + "-{0}.sql", 50000);
                }
            }


        }



        public static void LaunchSqlScripts(string tableName)
        {


            string scriptName = "";
            string logFile = tableName + ".log";
            string outPutFile = outPutDir + tableName + ".sql";


            if (!File.Exists(outPutFile))
                return;
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.UseShellExecute = false;
                startInfo.FileName = "sqlcmd.exe";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                //scriptName = tableName + ".sql";

                startInfo.Arguments = " -S " + targetServer + " -d " + targetDb + " -i " + outPutFile + " -U FDSLogin -P 0217530 -o c:\\tmp\\" + logFile;


                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("File not found, continuing, press enter to continue");
                Console.ReadKey();

            }


        }

    }
}

