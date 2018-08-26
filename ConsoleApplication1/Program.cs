using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;

namespace ConsoleApplication1
{
    public class Program
    {
        static bool failed = false;
        static bool backedUp = false;
        

        static void Main(string[] args)
        {
        }

        public static void BackupDB(Server myServer)
        {
            Database db = myServer.Databases["MemeWallStreet"];

            //return failed;
            Backup bkpDBFull = new Backup();
            /* Specify whether you want to back up database or files or log */
            bkpDBFull.Action = BackupActionType.Database;
            /* Specify the name of the database to back up */
            bkpDBFull.Database = db.Name;
            /* You can take backup on several media type (disk or tape), here I am
             * using File type and storing backup on the file system */
            bkpDBFull.Devices.AddDevice(@"D:\" + db.Name + ".bak", DeviceType.File);
            bkpDBFull.BackupSetName = db.Name + " Bakcup";
            bkpDBFull.BackupSetDescription = db.Name + "- Full Backup";
            /* You can specify the expiration date for your backup data
             * after that date backup data would not be relevant */
            bkpDBFull.ExpirationDate = DateTime.Today.AddDays(10);

            /* You can specify Initialize = false (default) to create a new 
             * backup set which will be appended as last backup set on the media. You
             * can specify Initialize = true to make the backup as first set on the
             * medium and to overwrite any other existing backup sets if the all the
             * backup sets have expired and specified backup set name matches with
             * the name on the medium */
            bkpDBFull.Initialize = false;

            /* Wiring up events for progress monitoring */
            bkpDBFull.PercentComplete += CompletionStatusInPercent;
            bkpDBFull.Complete += Backup_Completed;

            /* SqlBackup method starts to take back up
             * You can also use SqlBackupAsync method to perform the backup 
             * operation asynchronously */
            try { bkpDBFull.SqlBackup(myServer); }
            catch { failed = true; Console.WriteLine("Fail"); }
        }

        public static void DeleteDB(Server myServer)
        {
            Database db = myServer.Databases["MemeWallStreet"];
            db.Refresh();
            db.Drop();
        }

        public static void RestoreDB(Server myServer)
        {
            Database db = myServer.Databases["MemeWallStreet"];
            Restore restoreDB = new Restore();
            restoreDB.Database = db.Name;
            /* Specify whether you want to restore database, files or log */
            restoreDB.Action = RestoreActionType.Database;
            restoreDB.Devices.AddDevice(@"D:\" + db.Name + ".bak", DeviceType.File);
            //myServer.KillAllProcesses(db.Name);

            /* You can specify ReplaceDatabase = false (default) to not create a new
             * database, the specified database must exist on SQL Server
             * instance. If you can specify ReplaceDatabase = true to create new
             * database image regardless of the existence of specified database with
             * the same name */
            restoreDB.ReplaceDatabase = true;

            /* If you have a differential or log restore after the current restore,
             * you would need to specify NoRecovery = true, this will ensure no
             * recovery performed and subsequent restores are allowed. It means it
             * the database will be in a restoring state. */
            restoreDB.NoRecovery = true;

            /* Wiring up events for progress monitoring */
            restoreDB.PercentComplete += CompletionStatusInPercent;
            restoreDB.Complete += Restore_Completed;

            /* SqlRestore method starts to restore the database
             * You can also use SqlRestoreAsync method to perform restore 
             * operation asynchronously */
            try { restoreDB.SqlRestore(myServer); }
            catch { failed = true; Console.WriteLine("Fail"); }
        }

        private static void CompletionStatusInPercent(object sender, PercentCompleteEventArgs args)
        {
            Console.Clear();
            Console.WriteLine("Percent completed: {0}%.", args.Percent);
        }
        private static void Backup_Completed(object sender, ServerMessageEventArgs args)
        {
            Console.WriteLine("Hurray...Backup completed.");
            Console.WriteLine(args.Error.Message);
        }
        private static void Restore_Completed(object sender, ServerMessageEventArgs args)
        {
            Console.WriteLine("Hurray...Restore completed.");
            Console.WriteLine(args.Error.Message);
        }
    }
}
