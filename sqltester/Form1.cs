using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Data.SqlClient;
using System.Security.Principal;
using System.Threading;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace sqltester
{
    public partial class Form1 : Form
    {
        Server myServer = new Server(@"DESKTOP\MAIN");
        string conn = @"Data Source=DESKTOP\MAIN;Initial Catalog=MemeWallStreet;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        StringBuilder sb = new StringBuilder();

        public Form1()
        {
            InitializeComponent();
            statusText.Text = percentLabel.Text = "";
            progressBar1.Value = 0;
        }

        public bool BackupDB(Server myServer)
        {
            Database db = myServer.Databases["MemeWallStreet"];
            if (db != null)
            {
                Backup bkpDBFull = new Backup();
                bkpDBFull.Action = BackupActionType.Database;
                bkpDBFull.Database = db.Name;
                bkpDBFull.Devices.AddDevice(@"D:\" + db.Name + ".bak", DeviceType.File);
                bkpDBFull.BackupSetName = db.Name + " Backup";
                bkpDBFull.BackupSetDescription = db.Name + "- Full Backup";

                bkpDBFull.Initialize = false; //causes an overwrite

                bkpDBFull.PercentComplete += CompletionStatusInPercent;
                bkpDBFull.Complete += Backup_Completed;

                try
                {
                    bkpDBFull.SqlBackup(myServer);
                }
                catch (Exception ex)
                {
                    Backup_Failed(ex.Message);
                    return false;
                }

                return true;
            }

            return false;
        }


        private void btnBackup_Click(object sender, EventArgs e)
        {
            ClearUI();
            BackupDB(myServer);
        }
        private void btnRestore_Click(object sender, EventArgs e)
        {
            ClearUI();
            Database db = myServer.Databases["MemeWallStreet"];

            if (db != null)
            {
                SetMultiUser();
                if (BackupDB(myServer))
                    DeleteDB(myServer);
            }

            RestoreDB(myServer);

            richTextBox1.Text = sb.ToString();
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
            ClearUI();
            DeleteDB(myServer);
        }

        public void DeleteDB(Server myServer)
        {
            SetSingleUser();

            string dbName = myServer.Databases["MemeWallStreet"].ToString();
            string commandString = "USE [master]" + Environment.NewLine + "DROP DATABASE " + dbName;

            if (myServer.Databases["MemeWallStreet"] != null)
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(commandString, new SqlConnection(conn));
                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                    statusText.Text = "Set to single user...";
                }
                catch (Exception ex)
                {
                    sb.Append(ex.Message);
                    richTextBox1.Text = sb.ToString();
                }
            }
        }

        public bool RestoreDB(Server myServer)
        {
            Database db = myServer.Databases["MemeWallStreet"];
            Restore restoreDB = new Restore();

            if (db != null)
            {
                SetSingleUser();
                restoreDB.Database = db.Name;
            }
            else
            {
                db = new Database();
                restoreDB.Database = db.Name = "MemeWallStreet";
            }

            restoreDB.Action = RestoreActionType.Database;
            restoreDB.Devices.AddDevice(@"D:\" + db.Name + ".bak", DeviceType.File);

            restoreDB.ReplaceDatabase = true;
            restoreDB.NoRecovery = false;

            try
            {
                restoreDB.PercentComplete += CompletionStatusInPercent;
                restoreDB.Complete += Restore_Completed;
                restoreDB.SqlRestore(myServer);
                ClearUI();
                ExecuteCommand();
                ClearUI();
                DeleteDB(myServer);
                restoreDB.SqlRestore(myServer);
                ExecuteCommand();
                SetMultiUser();
                richTextBox1.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                Restore_Failed(ex.Message);
                SetMultiUser();
                return false;
            }

            return true;

        }
        private void SetSingleUser()
        {
            if (myServer.Databases["MemeWallStreet"] != null)
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("ALTER DATABASE [" + myServer.Databases["MemeWallStreet"].Name + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", new SqlConnection(conn));
                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                    statusText.Text = "Set to single user...";
                    sb.Append(statusText.Text);
                }
                catch (Exception ex)
                {
                    sb.Append(ex.Message);
                }

                richTextBox1.Text = sb.ToString();
            }
        }
        private void SetMultiUser()
        {
            try
            {
                SqlCommand cmd = new SqlCommand("ALTER DATABASE " + myServer.Databases["MemeWallStreet"].Name + " SET MULTI_USER", new SqlConnection(conn));
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                statusText.Text = "Set back to multi user...";
                sb.Append(statusText.Text);
            }
            catch (Exception ex)
            {
                sb.Append(ex.Message);
            }

            richTextBox1.Text = sb.ToString();
        }

        private void CompletionStatusInPercent(object sender, PercentCompleteEventArgs args)
        {
            percentLabel.Text = args.Percent.ToString();
            statusText.Text = args.Message;
            progressBar1.Value = args.Percent;
        }
        private void Backup_Completed(object sender, ServerMessageEventArgs args)
        {
            statusText.ForeColor = System.Drawing.Color.Green;
            statusText.Text = "Backup Complete";
        }
        private void Backup_Failed(string ex)
        {
            statusText.ForeColor = System.Drawing.Color.Red;
            statusText.Text = "Backup Failed";
            sb.Append(ex);
            richTextBox1.Text = sb.ToString();
        }
        private void Restore_Completed(object sender, ServerMessageEventArgs args)
        {
            statusText.ForeColor = System.Drawing.Color.Green;
            statusText.Text = "Restore Complete";
        }
        private void Restore_Failed(string ex)
        {
            statusText.ForeColor = System.Drawing.Color.Red;
            statusText.Text = "Restore Failed";
            sb.Append(ex);
            richTextBox1.Text = sb.ToString();
        }
        private void Delete_Completed()
        {
            statusText.ForeColor = System.Drawing.Color.Green;
            statusText.Text = "Drop Complete";
        }
        private void Delete_Failed(string ex)
        {
            statusText.ForeColor = System.Drawing.Color.Red;
            statusText.Text = "Drop Failed";
            sb.Append(ex);
            richTextBox1.Text = sb.ToString();
        }

        private string ExecuteCommand()
        {
            try
            {
                Process proc = null;

                string _batDir = string.Format(@"D:\Code\sqltester\sqltester");
                proc = new Process();
                proc.StartInfo.WorkingDirectory = _batDir;
                proc.StartInfo.FileName = "db.bat";
                proc.StartInfo.CreateNoWindow = false;
                proc.Start();
                proc.WaitForExit();
                proc.Close();
                sb.Append("Bat file executed...");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        private void ClearUI()
        {
            progressBar1.Value = 0;
            sb.Clear();
            statusText.Text = "";
            percentLabel.Text = "0";
        }
    }
}
