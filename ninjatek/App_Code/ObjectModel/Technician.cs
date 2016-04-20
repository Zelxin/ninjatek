using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Technician
/// </summary>
public class Technician
{

    #region Properties
    private int _ID;
    public int ID
    {
        get { return _ID; }
        set { _ID = value; }
    }

    private string _name;
    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }
    #endregion

    public Technician()
	{
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id">ID of tech</param>
    /// <returns>Tech with matching ID</returns>
    public static Technician CreateTechnician(int id)
    {
        var tech = new Technician();
        using (var cn = new MySqlConnection(Global.ConnectionString()))
        {
            cn.Open();
            using (var cmd = cn.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@techID", id);
                cmd.CommandText = "select * from Technicians where ID = @techID";
                using(var dr = cmd.ExecuteReader())
                {
                    if(dr.Read())
                    {
                        tech.ID = id;
                        tech.Name = dr["Name"].ToString();
                    }
                    else
                    {
                        //TODO: no tech found.
                    }
                }
            }
        }
        return tech;
    }

    public static Technician CreateTechnician(string id)
    {
        var tech = new Technician();
        using (var cn = new MySqlConnection(Global.ConnectionString()))
        {
            cn.Open();
            using (var cmd = cn.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@techID", id);
                cmd.CommandText = "select * from Technicians where ID = @techID";
                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        tech.ID = (int)dr["id"];
                        tech.Name = dr["Name"].ToString();
                    }
                }
            }
        }
        return tech;
    }

    public static List<Technician> GetAllTechnicians()
    {
        var result = new List<Technician>();
        using (var cn = new MySqlConnection(Global.ConnectionString()))
        {
            cn.Open();
            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = "select * from Technicians order by ID";
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var tempTech = new Technician()
                        {
                            ID = (int)dr["ID"],
                            Name = dr["Name"] as string ?? ""
                        };
                        result.Add(tempTech);
                    }
                }
            }
        }
        return result;
    }

}