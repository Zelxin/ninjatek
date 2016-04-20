using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Tasks.NetworkAnalyst;
using MySql.Data.MySqlClient;
using System.Text;
using System.Collections;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using System.Net;
using Newtonsoft.Json.Linq;
/// <summary>
/// Summary description for Job
/// </summary>
public class Job
{
    private long _id;
    public long ID
    {
        get { return _id; }
        set { _id = value; }
    }

    private Customer _customer;
    public Customer Customer
    {
        get { return _customer; }
        set { _customer = value; }
    }

    private string _details;
    /// <summary>
    /// Job details
    /// </summary>
    public string Details
    {
        get { return _details; }
        set { _details = value; }
    }

    private DateTime _requestedOn;
    public DateTime RequestedOn
    {
        get { return _requestedOn; }
        set { _requestedOn = value; }
    }

    private DateTime _completedOn;
    public DateTime CompletedOn
    {
        get { return _completedOn; }
        set { _completedOn = value; }
    }

    private Technician _AssignedTo;
    public Technician AssignedTo
    {
        get { return _AssignedTo; }
        set { _AssignedTo = value; }
    }
    
    private JobStatus _status;
    public JobStatus Status
    {
        get { return _status; }
        set { _status = value; }
    }
    
    public Job()
    {
        this.ID = -1;
        this.Status = JobStatus.NeedQuote;
        this.RequestedOn = DateTime.Now;
    }

    public Job(Customer cust) : base()
    {
        this.Customer = cust;
    }

    public void SaveJob()
    {
        using (var cn = new MySqlConnection(Global.ConnectionString()))
        {
            cn.Open();
            using (var cmd = cn.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@custID", this.Customer.ID);
                cmd.Parameters.AddWithValue("@details", this.Details);
                cmd.Parameters.AddWithValue("@requestedDate", this.RequestedOn);
                cmd.Parameters.AddWithValue("@Status", this.Status);
                cmd.Parameters.AddWithValue("@techID", this.AssignedTo == null ? -1 : this.AssignedTo.ID);
                cmd.CommandText = "Insert into Jobs (`Customer ID`,Details,`Requested On`,Status,Technician) values (@custID, @details,@requestedDate,@Status,@techID)";
                cmd.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// Updates only the Job Status
    /// </summary>
    public void UpdateJobStatus()
    {
        using (var cn = new MySqlConnection(Global.ConnectionString()))
        {
            cn.Open();
            using (var cmd = cn.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@jobID", ID);
                cmd.Parameters.AddWithValue("@Status", this.Status);

                cmd.CommandText = "update Jobs set Status=@Status where ID=@jobID";
                cmd.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// Updates only the assigned tech
    /// </summary>
    public void UpdateTechAssigned()
    {
        using (var cn = new MySqlConnection(Global.ConnectionString()))
        {
            cn.Open();
            using (var cmd = cn.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@jobID", ID);
                cmd.Parameters.AddWithValue("@techID", this.AssignedTo.ID);

                cmd.CommandText = "update Jobs set Technician=@techID where ID=@jobID";
                cmd.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// Updates all fields
    /// </summary>
    public void Update()
    {
        using (var cn = new MySqlConnection(Global.ConnectionString()))
        {
            cn.Open();
            using (var cmd = cn.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@jobID", ID);
                cmd.Parameters.AddWithValue("@Status", this.Status);
                cmd.Parameters.AddWithValue("@Details", this.Details);
                cmd.Parameters.AddWithValue("@techID", this.AssignedTo == null ? -1 : this.AssignedTo.ID);
                cmd.Parameters.AddWithValue("@CompletedOn", this.CompletedOn);
                cmd.Parameters.AddWithValue("@custID", this.Customer.ID);
                cmd.CommandText = "update Jobs set Status=@Status,Technician=@techID,Details = @Details, `Customer ID` = @custID, `Completed On` = @CompletedOn where ID=@jobID";
                cmd.ExecuteNonQuery();
            }
        }
    }

    public string GetStatusClass()
    {
        var result = "";
        switch(Status)
        {
            case JobStatus.Completed:
                result= "Completed";
                break;
            case JobStatus.NeedQuote:
                result = "NeedQuote";
                break;
            case JobStatus.QuoteDone_NeedFollowUp:
                result = "Quoted";
                break;
            case JobStatus.ReadyToDo:
                result = "Ready";
                break;
            case JobStatus.UnPaid:
                result = "Unpaid";
                break;
        }
        return result;
    }

    public static Job Load(long id)
    {
        Job result = null;
        using (var cn = new MySqlConnection(Global.ConnectionString()))
        {
            cn.Open();
            using (var cmd = cn.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@jobID", id);
                cmd.CommandText = "SELECT `ID`,`Completed On`,`Customer ID`,`Details`,`Requested On`,`Status`,`Technician` FROM Jobs Where ID = @jobID";
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var tempJob = new Job()
                        {
                            ID = dr.GetInt32("ID"),
                            Customer = Customer.LoadCustomer(dr.GetInt64("Customer ID")),
                            Details = dr.GetString("Details"),
                            RequestedOn = dr.GetDateTime("Requested On"),
                            AssignedTo = Technician.CreateTechnician(dr["Technician"].ToString()),
                            CompletedOn = dr["Completed On"] == DBNull.Value ? DateTime.MinValue : dr.GetDateTime("Completed On"),
                            Status = (JobStatus)dr.GetInt32("Status")
                        };
                        result = tempJob;
                    }
                }
            }
        }
        return result;
    }

    public static void Delete(long id)
    {
        using (var cn = new MySqlConnection(Global.ConnectionString()))
        {
            cn.Open();
            using (var cmd = cn.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@jobID", id);

                cmd.CommandText = "delete from Jobs where ID=@jobID";
                cmd.ExecuteNonQuery();
            }
        }
    }

    public static void Complete(long id)
    {
        Complete(id, DateTime.Now);
    }

    public static void Complete(long id, DateTime dt)
    {
        using (var cn = new MySqlConnection(Global.ConnectionString()))
        {
            cn.Open();
            using (var cmd = cn.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@jobID", id);
                cmd.Parameters.AddWithValue("@completedOn", dt.ToString("yyyy-MM-dd HH:mm"));
                cmd.CommandText = "update Jobs set `Completed On`=@completedOn where ID=@jobID ";
                cmd.ExecuteNonQuery();
            }
        }
    }

    public static List<Job> GetAllJobs()
    {
        var result = new List<Job>();
        using (var cn = new MySqlConnection(Global.ConnectionString()))
        {
            cn.Open();
            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = "SELECT `ID`,`Completed On`,`Customer ID`,`Details`,`Requested On`, Status,Technician FROM Jobs";
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var tempJob = new Job()
                        {
                            ID = dr.GetInt32("ID"),
                            Customer = Customer.LoadCustomer(dr.GetInt64("Customer ID")),
                            Details = dr.GetString("Details"),
                            RequestedOn = dr.GetDateTime("Requested On"),
                            AssignedTo = Technician.CreateTechnician(dr["Technician"].ToString()),
                            CompletedOn = dr["Completed On"] == DBNull.Value ? DateTime.MinValue : dr.GetDateTime("Completed On"),
                            Status = (JobStatus)dr.GetInt32("Status")
                        };
                        result.Add(tempJob);
                    }
                }
            }
        }
        return result;
    }

    public static string API_KEY = "AIzaSyDyWKMYOmSwp8zMXzOpS3b9eBu1red9u7c";

    public static void GetRoute(List<Job> jobs)
    {
        var wc = new WebClient();
        var sbURI = new StringBuilder("https://maps.googleapis.com/maps/api/directions/json?");
        sbURI.Append("origin=Toronto,ON&destination=Toronto,ON&waypoints=");

        foreach(var job in jobs)
        {
            if (job.Status != JobStatus.Completed && job.Status != JobStatus.UnPaid)
            { 
                sbURI.AppendFormat("{0},{1}|",job.Customer.Address,job.Customer.City);
            }
        }
        sbURI.Remove(sbURI.Length - 1, 1);
        sbURI.AppendFormat("&key={0}", API_KEY);

        var uri = new Uri(sbURI.ToString());
        string json = wc.DownloadString(uri);

        JObject o = JObject.Parse(json);
    }

    public static List<JobStatus> GetJobStatuses()
    {
        var result = new List<JobStatus>();
        foreach (JobStatus status in Enum.GetValues(typeof(JobStatus)))
        {
            result.Add(status);
        }
        return result;
    }

}

public class Description : Attribute
{
    public string Text;
    public Description(string text)
    {
        Text = text;
    }
}

[Flags]
public enum JobStatus
{
    None= 0,
    [Description("Ready to do")]
    ReadyToDo = 1,
    [Description("Needs Quote")]
    NeedQuote = 2,
    [Description("Needs Followup")]
    QuoteDone_NeedFollowUp=4,
    [Description("Completed, Unpaid")]
    UnPaid = 8,
    [Description("Completed")]
    Completed = 16

}
