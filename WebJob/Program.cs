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
		struct SentEmail {

			public object ID;
			public DateTime Time;

			public SentEmail(object id, DateTime time)
			{
				this.ID = id;
				this.Time = time;
			}
		}

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

					List<SentEmail> sentEmails = new List<SentEmail>();

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
								sentEmails.Add(new SentEmail(reader["ID"], DateTime.Now));

								Console.WriteLine($"Email ID:{reader["ID"]} sent...");
							}
						}
					}

					foreach (var email in sentEmails) {
						idParam.Value = email.ID;
						timeParam.Value = email.Time;
						if (updateCmd.ExecuteNonQuery() != 1) {
							throw new InvalidOperationException("Failed to update Sent field in DB");
						}
					}
				}

				Thread.Sleep(30_000); // 30sec
			}
		}
	}
}
