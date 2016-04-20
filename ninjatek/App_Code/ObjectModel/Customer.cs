using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
/// <summary>
/// Summary description for Customer
/// </summary>
public class Customer
{
    #region Properties
    private long _id;
    public long ID
    {
        get { return _id; }
        set { _id = value; }
    }

    private string _name;
    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    private string _address;
    public string Address
    {
        get { return _address; }
        set { _address = value; }
    }

    private string _city;
    public string City
    {
        get { return _city; }
        set { _city = value; }
    }

    private string _postalCode;
    public string PostalCode
    {
        get { return _postalCode; }
        set { _postalCode = value; }
    }

    private string _email;
    public string Email
    {
        get { return _email; }
        set { _email = value; }
    }

    private string _comments;
    public string Comments
    {
        get { return _comments; }
        set { _comments = value; }
    }

    private string _phoneNumber;
    public string PhoneNumber
    {
        get { return _phoneNumber; }
        set { _phoneNumber = value; }
    }

    private bool _isBlacklisted;
    public bool IsBlacklisted
    {
        get { return _isBlacklisted; }
        set { _isBlacklisted = value; }
    }
    #endregion

    public Customer()
	{
        //This is set to -1 to start so we can track if it has been changed to an actual customer
        this.ID = -1;
	}

    /// <summary>
    /// Gets all the jobs for this customer
    /// </summary>
    public List<Job> GetAllJobs()
    {
        var result = new List<Job>();
        using (var cn = new MySqlConnection(Global.ConnectionString()))
        {
            cn.Open();
            using (var cmd = cn.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@customerID", this.ID);
                cmd.CommandText = "SELECT `ID`,`Completed On`,`Customer ID`,`Details`,`Requested On`, Status,Technician FROM Jobs WHERE `Customer ID` =@customerID";

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

    public void SaveCustomer()
    {
        using (var cn = new MySqlConnection(Global.ConnectionString()))
        {
            cn.Open();
            using (var cmd = cn.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@name", this.Name);
                cmd.Parameters.AddWithValue("@addr", this.Address);
                cmd.Parameters.AddWithValue("@city", this.City);
                cmd.Parameters.AddWithValue("@phone", this.PhoneNumber);
                cmd.Parameters.AddWithValue("@email", this.Email);
                cmd.Parameters.AddWithValue("@comments", this.Comments);
                cmd.Parameters.AddWithValue("@blacklist", this.IsBlacklisted);
                cmd.CommandText = "insert into Customers (Name,Address,City,`Phone Number`,Email,Comments,Blacklisted) values(@name,@addr,@city,@phone,@email,@comments,@blacklist)";
                cmd.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>How many customer rows this affected. realistically this should always be 1 if the customer existed.</returns>
    public int UpdateCustomer()
    {
        var rowsAffected = 0; // Track this so we know if we actually updated a customer.
        using(var cn = new MySqlConnection(Global.ConnectionString()))
        {
            cn.Open();
            using(var cmd = cn.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@ID", this.ID);
                cmd.Parameters.AddWithValue("@name", this.Name);
                cmd.Parameters.AddWithValue("@addr", this.Address);
                cmd.Parameters.AddWithValue("@city", this.City);
                cmd.Parameters.AddWithValue("@phone", this.PhoneNumber);
                cmd.Parameters.AddWithValue("@email", this.Email);
                cmd.Parameters.AddWithValue("@comments", this.Comments);
                cmd.Parameters.AddWithValue("@blacklist", this.IsBlacklisted);
                cmd.CommandText = "UPDATE Customers set Name=@name, Address=@addr, city=@city, `Phone number`=@phone,email=@email,comments=@comments,Blacklisted=@blacklist where ID=@ID";
                rowsAffected = cmd.ExecuteNonQuery();
            }
        }
        return rowsAffected;
    }

    public void DeleteCustomer()
    {
        using (var cn = new MySqlConnection(Global.ConnectionString()))
        {
            cn.Open();
            using (var cmd = cn.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@ID", this.ID);
                cmd.CommandText = "DELETE FROM Customers WHERE ID=@ID";
            }
        }
    }

    public static void DeleteCustomer(long ID)
    {
        using (var cn = new MySqlConnection(Global.ConnectionString()))
        {
            cn.Open();
            using (var cmd = cn.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@ID", ID);
                cmd.CommandText = "DELETE FROM Customers WHERE ID=@ID";
                cmd.ExecuteNonQuery();
            }
        }
    }

    public static Customer LoadCustomer(long custID)
    {
        using (var cn = new MySqlConnection(Global.ConnectionString()))
        {
            cn.Open();
            using (var cmd = cn.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@ID", custID);
                cmd.CommandText = "Select `ID`,`Name`,`Address`,`City`,`Postal Code`,`Email`,`Comments`,`Phone Number`,`Blacklisted` from Customers WHERE ID = @ID";
                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        return new Customer
                        {
                            ID = dr.GetInt64("ID"),
                            Name = dr.GetString("Name"),
                            Address = dr.GetString("Address"),
                            City = dr.GetString("City"),
                            PostalCode = dr.GetString("Postal Code"),
                            Email = dr.GetString("Email"),
                            Comments = dr.GetString("Comments"),
                            PhoneNumber = dr.GetString("Phone Number"),
                            IsBlacklisted = dr.GetBoolean("Blacklisted")
                        };
                    }
                }
            }
        }
        return null;
    }

    public static List<Customer> GetAllCustomers(bool bIncludeBlacklisted)
    {
        var result = new List<Customer>();
        using (var cn = new MySqlConnection(Global.ConnectionString()))
        {
            cn.Open();
            using (var cmd = cn.CreateCommand())
            {
                if (bIncludeBlacklisted)
                {
                    cmd.CommandText = "SELECT `ID`,`Name`,`Address`,`City`,`Postal Code`,`Email`,`Comments`,`Phone Number`,`Blacklisted` FROM Customers";
                }
                else
                {
                    cmd.CommandText = "SELECT `ID`,`Name`,`Address`,`City`,`Postal Code`,`Email`,`Comments`,`Phone Number`,`Blacklisted` FROM Customers where Blacklisted = 0";
                }

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var tempCust = new Customer()
                        {
                            ID = dr.GetInt64("ID"),
                            Name = dr.GetString("Name"),
                            Address = dr.GetString("Address"),
                            City = dr.GetString("City"),
                            PostalCode = dr.GetString("Postal Code"),
                            Email = dr.GetString("Email"),
                            Comments = dr.GetString("Comments"),
                            PhoneNumber = dr.GetString("Phone Number"),
                            IsBlacklisted = dr.GetBoolean("Blacklisted")
                        };
                        result.Add(tempCust);
                    }
                }
            }
        }
        return result;
    }

    public override string ToString()
    {
        return this.Name;
    }
}