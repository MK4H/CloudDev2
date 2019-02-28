using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data;

namespace WebJob
{
	class Program
	{
		static void Main(string[] args)
		{
			while (true)
			{
				Console.WriteLine("Checking for new e-mails to be sent...");

				using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["MainDatabase"].ConnectionString))
				{
					conn.Open();

					var selectCmd = new SqlCommand("SELECT * FROM EmailQueue WHERE Sent IS NULL", conn);
					var updateCmd = new SqlCommand("UPDATE [dbo].[EmailQueue] SET [Sent] = @now WHERE [ID] = @id", conn);
					var timeParam = updateCmd.Parameters.Add("@now", SqlDbType.DateTime);
					var idParam = updateCmd.Parameters.Add("@id", SqlDbType.Int);
					updateCmd.Prepare();

					using (var reader = selectCmd.ExecuteReader())
					{
						while (reader.Read())
						{
							Console.WriteLine($"Email ID:{reader["ID"]} found...");

							using (var smtpClient = new SmtpClient())
							{
								smtpClient.Host = ConfigurationManager.AppSettings["SmtpHost"];
								smtpClient.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["SmtpUsername"], ConfigurationManager.AppSettings["SmtpPassword"]);

								smtpClient.Send(
												 from: ConfigurationManager.AppSettings["SmtpFrom"],
												 recipients: reader["Recipient"].ToString(),
												 subject: reader["Subject"].ToString(),
												 body: reader["Body"].ToString());

								// TODO: Write EmailQueue.Sent to DB
								timeParam.Value = DateTime.Now;
								idParam.Value = reader["ID"];

								if (updateCmd.ExecuteNonQuery() != 1) {
									throw new InvalidOperationException("Failed to set Sent value in DB");
								}



								Console.WriteLine($"Email ID:{reader["ID"]} sent...");
							}
						}
					}
				}

				Thread.Sleep(30_000); // 30sec
			}
		}
	}
}
